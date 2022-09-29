using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using AioCore.Domain;
using AioCore.Domain.AggregateModels.CategoryAggregate;
using AioCore.Domain.AggregateModels.PostAggregate;
using AioCore.Shared.Extensions;
using AioCore.Web.Helpers.HangfireHelpers;
using AioCore.Web.Jobs.DanTriJob.Models;
using HtmlAgilityPack;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace AioCore.Web.Jobs.DanTriJob;

public class DanTriJob : ICronJob
{
    private const string GoogleNewsSitemap = "https://dantri.com.vn/google-news-sitemap.xml";
    private const string CategorySitemap = "https://dantri.com.vn/sitemaps/category-sitemap.xml";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AioCoreContext _context;

    public DanTriJob(
        IHttpClientFactory httpClientFactory,
        AioCoreContext context)
    {
        _httpClientFactory = httpClientFactory;
        _context = context;
    }

    public async Task<string> Run()
    {
        var categories = await Categories();
        await UpdateCategories(categories);
        await GoogleNews(categories);
        return "OK";
    }

    private async Task<List<Category>> Categories()
    {
        var httpClient = _httpClientFactory.CreateClient();
        var xml = await httpClient.GetStringAsync(CategorySitemap);
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);
        var json = JsonConvert.SerializeXmlNode(xmlDocument);
        var danTriCategories = JsonConvert.DeserializeObject<DanTriCategories>(json);
        var updatedCategories = new List<Category>();
        foreach (var url in danTriCategories.Urlset.Url)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(x =>
                x.Active && !string.IsNullOrEmpty(x.Source) && x.Source.Equals(url.Loc));
            if (category != null)
            {
                if (category.Slug is null)
                {
                    category = await UpdateCategory(category);
                    await _context.Categories.UpdateAsync(category.Id, category);
                    if (category.Active) updatedCategories.Add(category);
                    continue;
                }

                updatedCategories.Add(category);
                continue;
            }

            category = new Category
            {
                Source = url.Loc
            };
            await _context.Categories.AddAsync(category);
            category = await UpdateCategory(category);
            await _context.Categories.UpdateAsync(category.Id, category);
            updatedCategories.Add(category);
        }

        return updatedCategories;
    }

    private async Task GoogleNews(IReadOnlyCollection<Category> categories)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var xml = await httpClient.GetStringAsync(GoogleNewsSitemap);
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);
        var json = JsonConvert.SerializeXmlNode(xmlDocument);
        var danTriGoogleNews = JsonConvert.DeserializeObject<DanTriGoogleNews>(json);
        foreach (var url in danTriGoogleNews.Urlset.Url)
        {
            if (await _context.Posts.AnyAsync(x =>
                    x.Active && !string.IsNullOrEmpty(x.Source) && x.Source.Equals(url.Loc)))
                continue;
            var post = new Post
            {
                Title = url.NewsNews.NewsTitle,
                Keyword = url.NewsNews.NewsKeywords,
                CreatedAt = url.NewsNews.NewsPublicationDate,
                Thumbnail = url.ImageImage.ImageLoc,
                Source = url.Loc,
                HashKey = url.NewsNews.NewsTitle.CreateMd5()
            };
            try
            {
                await _context.Posts.AddAsync(post);
                post = await UpdatePost(post, categories);
                await _context.Posts.UpdateAsync(post.Id, post);
            }
            catch(Exception ex)
            {
                post.Active = false;
                if (ex.Message.Contains("429"))
                    post = await UpdatePost(post, categories);
                await _context.Posts.UpdateAsync(post.Id, post);
            }
        }
    }

    private async Task UpdateCategories(IReadOnlyCollection<Category> categories)
    {
        var categoriesNullParent = categories.Where(x => x.Parents is null);
        foreach (var category in categoriesNullParent)
        {
            var slugItems = category.Slug?.Split("/");
            var categoryMatches = categories
                .Where(x => slugItems is not null && slugItems.Contains(x.Slug));
            category.Update(categoryMatches.Select(x => x.Id).ToList());
            await _context.Categories.UpdateAsync(category.Id, category);
        }
    }

    private async Task<Category> UpdateCategory(Category category)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var html = await httpClient.GetStringAsync(category.Source);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var title = htmlDocument.DocumentNode
                .SelectSingleNode("//*[@name=\"title\"]")
                .Attributes["content"]?.Value;
            var description = htmlDocument.DocumentNode
                .SelectSingleNode("//*[@name=\"description\"]")
                .Attributes["content"]?.Value;
            var thumbnail = htmlDocument.DocumentNode
                .SelectSingleNode("//*[@property=\"og:image\"]")
                .Attributes["content"]?.Value;
            var keywords = htmlDocument.DocumentNode
                .SelectSingleNode("//*[@name=\"keywords\"]")
                .Attributes["content"]?.Value;
            var slugItems = category.Source?.Replace(".htm", string.Empty).Split('/').ToList();
            var slug = slugItems.Slice(0, 3).JoinString("/");
            category.Update(title, description, thumbnail, keywords, slug);
            return category;
        }
        catch(Exception ex)
        {
            if (ex.Message.Contains("429"))
                await UpdateCategory(category);
            category.Active = false;
            return category;
        }
    }

    private async Task<Post> UpdatePost(Post news, IEnumerable<Category> categories)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var html = await httpClient.GetStringAsync(news.Source);
        html = HttpUtility.HtmlDecode(html);
        html = Regex.Replace(html, "(<style.+?</style>)|(<script.+?</script>)", string.Empty);
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);
        var slugItems = news.Source?.Replace(".htm", string.Empty).Split('/').ToList();
        var slug = slugItems?.LastOrDefault();
        var title = htmlDocument.DocumentNode
            .SelectSingleNode("//*[@class=\"title-page detail\" or @class=\"e-magazine__title\"]")
            .InnerText;
        var description = htmlDocument.DocumentNode
            .SelectSingleNode("//*[@class=\"singular-sapo\" or @class=\"e-magazine__sapo sapo-top\"]")
            .InnerText;
        var content = htmlDocument.DocumentNode
            .SelectSingleNode("//*[@class=\"singular-content\" or @class=\"e-magazine__body\"]")
            .InnerHtml;
        var categorySlug = htmlDocument.DocumentNode
            .SelectSingleNode("//*[@class=\"breadcrumbs\" or @class=\"e-magazine__maincate\"]")
            .ChildNodes
            .Where(x => x.Name.Equals("li") || x.Name.Equals("h3"))
            .SelectMany(x => x.ChildNodes)
            .Select(x => x.Attributes["href"])
            .LastOrDefault(x => x is not null)?.Value
            .Slice(1, ".htm");
        var category = categories.FirstOrDefault(x => !string.IsNullOrEmpty(x.Slug) && x.Slug.Equals(categorySlug));
        news.Update(title, description, content, slug, category?.Id);
        return news;
    }
}
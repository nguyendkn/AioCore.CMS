using System.Xml;
using AioCore.Domain;
using AioCore.Domain.AggregateModels.CategoryAggregate;
using AioCore.Domain.AggregateModels.PostAggregate;
using AioCore.Web.Helpers.HangfireHelpers;
using AioCore.Web.Jobs.DanTriJob.Models;
using HtmlAgilityPack;
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
        await Categories();
        await GoogleNews();
        return "OK";
    }

    private async Task Categories()
    {
        var httpClient = _httpClientFactory.CreateClient();
        var xml = await httpClient.GetStringAsync(CategorySitemap);
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);
        var json = JsonConvert.SerializeXmlNode(xmlDocument);
        var danTriCategories = JsonConvert.DeserializeObject<DanTriCategories>(json);
        foreach (var url in danTriCategories.Urlset.Url)
        {
            if (await _context.Categories.AnyAsync(x => x.Title.Equals(url.NewsNews.NewsTitle))) continue;
            var category = new Category
            {
                Source = url.Loc
            };
            await _context.Categories.AddAsync(category);
            category = await UpdateCategory(category);
            await _context.Categories.UpdateAsync(category.Id, category);
        }
    }

    private async Task GoogleNews()
    {
        var httpClient = _httpClientFactory.CreateClient();
        var xml = await httpClient.GetStringAsync(GoogleNewsSitemap);
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);
        var json = JsonConvert.SerializeXmlNode(xmlDocument);
        var danTriGoogleNews = JsonConvert.DeserializeObject<DanTriGoogleNews>(json);
        foreach (var url in danTriGoogleNews.Urlset.Url)
        {
            if (await _context.Posts.AnyAsync(x => x.Title.Equals(url.NewsNews.NewsTitle))) continue;
            var post = new Post
            {
                Title = url.NewsNews.NewsTitle,
                Keyword = url.NewsNews.NewsKeywords,
                CreatedAt = url.NewsNews.NewsPublicationDate,
                Thumbnail = url.ImageImage.ImageLoc,
                Source = url.Loc
            };
            await _context.Posts.AddAsync(post);
            post = await UpdatePost(post);
            await _context.Posts.UpdateAsync(post.Id, post);
        }
    }
    
    private async Task<Category> UpdateCategory(Category category)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var html = await httpClient.GetStringAsync(category.Source);
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);
        var title = htmlDocument.DocumentNode
            .SelectSingleNode("//*[@name=\"title\"]")
            .Attributes["content"].Value;
        var description = htmlDocument.DocumentNode
            .SelectSingleNode("//*[@name=\"description\"]")
            .Attributes["content"].Value;
        var thumbnail = htmlDocument.DocumentNode
            .SelectSingleNode("//*[@property=\"og:image\"]")
            .Attributes["content"].Value;
        var keywords = htmlDocument.DocumentNode
            .SelectSingleNode("//*[@name=\"keywords\"]")
            .Attributes["content"].Value;
        category.Update(title, description, thumbnail, keywords);
        return category;
    }

    private async Task<Post> UpdatePost(Post news)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var html = await httpClient.GetStringAsync(news.Source);
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);
        var title = htmlDocument.DocumentNode
            .SelectSingleNode("//*[@class=\"title-page detail\" or @class=\"e-magazine__title\"]")
            .InnerText;
        var description = htmlDocument.DocumentNode
            .SelectSingleNode("//*[@class=\"singular-sapo\" or @class=\"e-magazine__sapo sapo-top\"]")
            .InnerText;
        var content = htmlDocument.DocumentNode
            .SelectSingleNode("//*[@class=\"singular-content\" or @class=\"e-magazine__body\"]")
            .InnerHtml;
        news.Update(title, description, content);
        return news;
    }
}
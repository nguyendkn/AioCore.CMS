using Newtonsoft.Json;

namespace AioCore.Web.Jobs.DanTriJob.Models;

public class DanTriGoogleNews
{
    [JsonProperty("?xml")] public Xml Xml { get; set; } = default!;

    [JsonProperty("urlset")] public Urlset Urlset { get; set; } = default!;
}

public class DanTriCategories
{
    [JsonProperty("?xml")] public Xml Xml { get; set; } = default!;

    [JsonProperty("urlset")] public Urlset Urlset { get; set; } = default!;
}

public class ImageImage
{
    [JsonProperty("image:loc")] public string ImageLoc { get; set; } = default!;
}

public class NewsNews
{
    [JsonProperty("news:publication")] public NewsPublication NewsPublication { get; set; } = default!;

    [JsonProperty("news:publication_date")]
    public DateTime NewsPublicationDate { get; set; }

    [JsonProperty("news:title")] public string NewsTitle { get; set; } = default!;

    [JsonProperty("news:keywords")] public string NewsKeywords { get; set; } = default!;
}

public class NewsPublication
{
    [JsonProperty("news:name")] public string NewsName { get; set; } = default!;

    [JsonProperty("news:language")] public string NewsLanguage { get; set; } = default!;
}

public class Url
{
    [JsonProperty("loc")] public string Loc { get; set; } = default!;

    [JsonProperty("news:news")] public NewsNews NewsNews { get; set; } = default!;

    [JsonProperty("image:image")] public ImageImage ImageImage { get; set; } = default!;
}

public class Urlset
{
    [JsonProperty("@xmlns:image")] public string XmlnsImage { get; set; } = default!;

    [JsonProperty("@xmlns:news")] public string XmlnsNews { get; set; } = default!;

    [JsonProperty("@xmlns")] public string Xmlns { get; set; } = default!;

    [JsonProperty("url")] public List<Url> Url { get; set; } = default!;
}

public class Xml
{
    [JsonProperty("@version")] public string Version { get; set; } = default!;

    [JsonProperty("@encoding")] public string Encoding { get; set; } = default!;
}
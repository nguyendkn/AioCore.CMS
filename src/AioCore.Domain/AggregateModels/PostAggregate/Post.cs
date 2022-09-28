using AioCore.Shared.Extensions;

namespace AioCore.Domain.AggregateModels.PostAggregate;

/// <summary>
/// HashKey: Khóa chính thứ 2
/// Title: Tiêu đề
/// Parent: Category, Post, Menu
/// Thumbnail: Ảnh thu nhỏ
/// CreatedAt: Thời gian tạo
/// ModifiedAt: Thời gian cập nhật
/// Description: Mô tả ngắn
/// Content: Nội dung
/// Keyword: Từ khoá
/// </summary>
public class Post : MongoDocument
{
    public string HashKey { get; set; } = default!;

    public string Title { get; set; } = default!;

    public Guid Parent { get; set; }

    public string Slug { get; set; } = default!;

    public string Thumbnail { get; set; } = default!;

    public DateTime CreatedAt { get; set; }

    public DateTime ModifiedAt { get; set; }

    public string? Description { get; set; }

    public string? Content { get; set; }

    public string? Keyword { get; set; }

    public string? Source { get; set; }

    public void Update(string title, string description, string content, string? slug, Guid? parent)
    {
        HashKey = string.IsNullOrEmpty(title) ? Title : title.CreateMd5();
        Title = string.IsNullOrEmpty(title) ? Title : title;
        Description = string.IsNullOrEmpty(description) ? Description : description;
        Content = string.IsNullOrEmpty(content) ? Content : content;
        Slug = string.IsNullOrEmpty(slug) ? Slug : slug;
        ModifiedAt = DateTime.Now;
        Parent = parent ?? Guid.Empty;
    }
}
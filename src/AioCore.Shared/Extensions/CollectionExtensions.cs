namespace AioCore.Shared.Extensions;

public static class CollectionExtensions
{
    public static List<T> Slice<T>(this List<T>? items, int start, int count)
    {
        if (items is null || start > count) return default!;
        count = items.Count > count ? count : items.Count;
        var i = 0;
        while (i < count)
        {
            items.RemoveAt(0);
            i++;
        }

        return items;
    }
}
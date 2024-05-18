namespace DotNetDependencyExtractor;

static class Extensions
{
    /// <summary>
    ///     Removes from end.
    /// </summary>
    public static string RemoveFromEnd(this string data, string value, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        if (data.EndsWith(value, comparison))
        {
            return data.Substring(0, data.Length - value.Length);
        }

        return data;
    }

    /// <summary>
    ///     Removes from start.
    /// </summary>
    public static string RemoveFromStart(this string data, string value, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        if (data == null)
        {
            return null;
        }

        if (data.StartsWith(value, comparison))
        {
            return data.Substring(value.Length, data.Length - value.Length);
        }

        return data;
    }

    public static IReadOnlyList<T> Distinct<T>(this IReadOnlyList<T> items, Func<T, T, bool> isSame)
    {
        var newList = new List<T>();

        foreach (var item in items)
        {
            if (!newList.Any(x=>isSame(x,item)))
            {
                newList.Add(item);
            }
        }

        return newList;
    }
}
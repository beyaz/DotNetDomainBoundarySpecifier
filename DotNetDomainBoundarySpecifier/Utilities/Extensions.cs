namespace DotNetDomainBoundarySpecifier.Utilities;

static class Extensions
{
    public static IReadOnlyList<T> Distinct<T>(this IReadOnlyList<T> items, Func<T, T, bool> isSame)
    {
        var newList = new List<T>();

        foreach (var item in items)
        {
            if (!newList.Any(x => isSame(x, item)))
            {
                newList.Add(item);
            }
        }

        return newList;
    }

    public static ImmutableList<T> Toggle<T>(this ImmutableList<T> immutableList, T value, IEqualityComparer<T> comparer)
    {
        foreach (var item in immutableList)
        {
            if (comparer.Equals(item, value))
            {
                return immutableList.Remove(item);
            }
        }

        return immutableList.Add(value);
    }

    public static ImmutableList<T> Toggle<T>(this ImmutableList<T> immutableList, T value)
    {
        return immutableList.Toggle(value, EqualityComparer<T>.Default);
    }

    public static ImmutableList<T> Toggle<T>(this IReadOnlyList<T> readOnlyList, T value)
    {
        return readOnlyList.ToImmutableList().Toggle(value, EqualityComparer<T>.Default);
    }
}
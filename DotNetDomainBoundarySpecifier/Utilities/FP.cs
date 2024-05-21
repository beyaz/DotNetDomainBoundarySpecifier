global using static DotNetDomainBoundarySpecifier.Utilities.FP;

namespace DotNetDomainBoundarySpecifier.Utilities;

public sealed record Result<TValue, TError>
{
    public TError Error { get; init; }
    public TValue Value { get; init; }

    public bool HasError => !EqualityComparer<TError>.Default.Equals(Error, default);

    public static implicit operator Result<TValue, TError>(TValue value)
    {
        return new()
        {
            Value = value
        };
    }

    public static implicit operator Result<TValue, TError>(TError error)
    {
        return new()
        {
            Error = error
        };
    }

    public static implicit operator Result<TValue, TError>((TValue value, TError error) tuple)
    {
        return new()
        {
            Value = tuple.value,
            Error = tuple.error
        };
    }

    public void Match(Action<TValue> onSuccess, Action<TError> onError)
    {
        if (HasError)
        {
            onError(Error);
        }
        else
        {
            onSuccess(Value);
        }
    }

    public TResult Select<TResult>(Func<TValue, TResult> onSuccess, Func<TError, TResult> onError)
    {
        return HasError ? onError(Error) : onSuccess(Value);
    }

    //public static Result<TValue, TError> operator +(Result<TValue, TError> a, IError<TError> b)
    //    => a with
    //    {
    //        Error = b.Error
    //    };
}

public sealed record Result<TValue>
{
    public readonly Trace Trace = new();
    public Exception Error { get; init; }

    public TValue Value { get; init; }

    public bool HasError => !Success;

    public bool Success => EqualityComparer<Exception>.Default.Equals(Error, default);

    public static implicit operator Result<TValue>(TValue value)
    {
        return new()
        {
            Value = value
        };
    }

    public static implicit operator Result<TValue>(Exception error)
    {
        return new()
        {
            Error = error
        };
    }

    public static implicit operator Result<TValue>((TValue value, Exception error) tuple)
    {
        return new()
        {
            Value = tuple.value,
            Error = tuple.error
        };
    }

    public void Match(Action<TValue> onSuccess, Action<Exception> onError)
    {
        if (HasError)
        {
            onError(Error);
        }
        else
        {
            onSuccess(Value);
        }
    }

    public TResult Select<TResult>(Func<TValue, TResult> onSuccess, Func<Exception, TResult> onError)
    {
        return HasError ? onError(Error) : onSuccess(Value);
    }

    public TValue Unwrap()
    {
        if (HasError)
        {
            throw Error;
        }

        return Value;
    }
}

public sealed class Trace
{
    readonly List<string> lines = [];

    public IReadOnlyList<string> Lines => lines;

    public void Add(string line)
    {
        lines.Add(line);
    }
}

static class FP
{
    public static Exception Fail(string message)
    {
        return new(message);
    }

    public static C Flow<A, B, C>(A value, Func<A, (B, Exception)> nextFunc, Func<B, C> onSuccess)
    {
        var (b, exception) = nextFunc(value);
        if (exception is null)
        {
            return onSuccess(b);
        }

        return default;
    }

    public static Result<IReadOnlyList<TValue>> Fold<TValue>(this IEnumerable<Result<IEnumerable<TValue>>> enumerable)
    {
        var list = new List<TValue>();

        foreach (var result in enumerable)
        {
            if (result.HasError)
            {
                return result.Error;
            }

            list.AddRange(result.Value);
        }

        return list;
    }

    public static void IgnoreException(Action action)
    {
        try
        {
            action();
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public static bool IsNullOrWhiteSpaceOrEmptyJsonObject(this string jsonText)
    {
        if (string.IsNullOrWhiteSpace(jsonText) ||
            string.IsNullOrWhiteSpace(jsonText.Replace("{", string.Empty).Replace("}", string.Empty)))
        {
            return true;
        }

        return false;
    }

    public static Result<TValue> ResultFrom<TValue>(TValue value)
    {
        return new()
        {
            Value = value
        };
    }

    public static Result<TValue> ResultFrom<TValue>()
    {
        return new();
    }

    public static Unit Run(Func<Unit>[] functions)
    {
        foreach (var function in functions)
        {
            var unit = function();
            if (unit.HasError)
            {
                return unit;
            }
        }

        return Unit.Success;
    }

    public static (T value, Exception exception) SafeInvoke<T>(Func<T> func)
    {
        try
        {
            return (func(), null);
        }
        catch (Exception exception)
        {
            return (default, exception);
        }
    }

    public static Result<B> Then<A, B>(this Result<A> resultA, Func<A, Result<B>> onSuccess)
    {
        if (resultA.HasError)
        {
            return resultA.Error;
        }

        return onSuccess(resultA.Value);
    }

    public static Result<B> Then<A, B>(this Result<A> resultA, Func<A, B> onSuccess)
    {
        if (resultA.HasError)
        {
            return resultA.Error;
        }

        return onSuccess(resultA.Value);
    }

    public static void Then<T>(this (T value, Exception exception) response, Action<T> onSuccess, Action<Exception> onFail)
    {
        if (response.exception is null)
        {
            onSuccess(response.value);
        }
        else
        {
            onFail(response.exception);
        }
    }

    public static B Then<T, B>(this (T value, Exception exception) response, Func<T, B> onSuccess, Func<Exception, B> onFail)
    {
        if (response.exception is null)
        {
            return onSuccess(response.value);
        }

        return onFail(response.exception);
    }

    public static Result<TValue> Try<TValue>(Func<TValue> func)
    {
        try
        {
            return func();
        }
        catch (Exception exception)
        {
            return exception;
        }
    }
}

public sealed class Unit
{
    public static readonly Unit Success = new();
    public Exception Error { get; init; }

    public bool HasError => Error is not null;

    public static implicit operator Unit(Exception error)
    {
        return new()
        {
            Error = error
        };
    }

    public void Match(Action onSuccess, Action<Exception> onError)
    {
        if (HasError)
        {
            onError(Error);
        }
        else
        {
            onSuccess();
        }
    }

    public void Unwrap()
    {
        if (HasError)
        {
            throw Error;
        }
    }
}
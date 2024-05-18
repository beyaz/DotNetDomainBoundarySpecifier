global using static DotNetDependencyExtractor.FP;

namespace DotNetDependencyExtractor;

public sealed record Result<TValue, TError>
{
    public  TError Error { get; init; }
    public  TValue Value { get; init; }

    public bool HasError => !EqualityComparer<TError>.Default.Equals(Error, default);

    public static implicit operator Result<TValue, TError>(TValue value) => new()
    {
        Value = value
    };
    
    public static implicit operator Result<TValue, TError>(TError error) => new ()
    {
        Error = error
    };
    
    public static implicit operator Result<TValue, TError>((TValue value, TError error) tuple) => new ()
    {
        Value = tuple.value,
        Error = tuple.error
    };

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
    public Exception Error { get; init; }
    
    public TValue Value { get; init; }

    public bool HasError => !Success;

    public bool Success => EqualityComparer<Exception>.Default.Equals(Error, default);
    
    public static implicit operator Result<TValue>(TValue value) => new()
    {
        Value = value
    };

    public static implicit operator Result<TValue>(Exception error) => new()
    {
        Error = error
    };

    public static implicit operator Result<TValue>((TValue value, Exception error) tuple) => new()
    {
        Value = tuple.value,
        Error = tuple.error
    };

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

    public readonly Trace Trace = new();


   
}

public sealed class Trace
{
    readonly List<string> lines = new();

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
        return new Exception(message);
    }

    public static Result<TValue> ResultFrom<TValue>(TValue value)
    {
        return new Result<TValue>
        {
            Value = value
        };
    }
    public static Result<TValue> ResultFrom<TValue>()
    {
        return new Result<TValue>();
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
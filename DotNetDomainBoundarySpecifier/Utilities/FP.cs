global using static DotNetDomainBoundarySpecifier.Utilities.FP;

namespace DotNetDomainBoundarySpecifier.Utilities;

public sealed record Result<TValue>
{
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

static class FP
{
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
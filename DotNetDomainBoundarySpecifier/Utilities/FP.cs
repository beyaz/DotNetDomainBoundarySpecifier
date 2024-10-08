﻿global using static DotNetDomainBoundarySpecifier.Utilities.FP;

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
    
    public static implicit operator Result<TValue>(OptionNoneValue none)
    {
        return new()
        {
            Value = Activator.CreateInstance<TValue>()
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

    public static readonly OptionNoneValue None = new();

    public static Option<TValue> Some<TValue>(TValue value)
    {
        return value;
    }

    public static void Then<TValue>(this Result<Option<TValue>> result, Action<TValue> next)
    {
        if (result.Success)
        {
            result.Value.Then(next);
        }
    }
    
    public static Unit Then<A>(this Result<A> result, Func<A,Unit> next)
    {
        if (result.HasError)
        {
            return new () { Error = result.Error };
        }

        return next(result.Value);
    }
}

public sealed class OptionNoneValue;

public sealed class Option<TValue>
{
    public TValue Value { get; init; }

    public required bool IsNone { get; init; } = true;

    public bool HasValue => IsNone is false;
    
    public static implicit operator Option<TValue>(TValue value)
    {
        return new()
        {
            Value = value,
            IsNone = false
        };
    }

    public static implicit operator Option<TValue>(OptionNoneValue value)
    {
        return new()
        {
            IsNone=true
        };
    }
    
    public T Then<T>(Func<TValue, T> nextOperation)
    {
        if (IsNone)
        {
            return default;    
        }

        return nextOperation(Value);
    }
    
    public void Then(Action<TValue> nextOperation)
    {
        if (IsNone)
        {
            return;    
        }

        nextOperation(Value);

    }
    
    
    
}

class ListOf<T>: List<T>
{
    public void Add(IEnumerable<T> items) => AddRange(items);
}
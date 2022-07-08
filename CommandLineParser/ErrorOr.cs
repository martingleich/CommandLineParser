using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace CmdParse
{
	public abstract class ErrorOr<T>
	{
		private sealed class ValueT : ErrorOr<T>
		{
			public T _value;

			public ValueT(T value)
			{
				_value = value;
			}

			public override TResult Accept<TResult>(Func<T, TResult> okay, Func<ImmutableArray<Error>, TResult> error) => okay(_value);
            public override bool TryGetValue([MaybeNullWhen(false)] out T value)
            {
				value = _value;
				return true;
            }
		}
		private sealed class ErrorT : ErrorOr<T>
		{
			public ImmutableArray<Error> _errors;

			public ErrorT(ImmutableArray<Error> error)
			{
				_errors = error;
			}

			public override TResult Accept<TResult>(Func<T, TResult> okay, Func<ImmutableArray<Error>, TResult> error) => error(_errors);

            public override bool TryGetValue([MaybeNullWhen(false)] out T value)
            {
				value = default;
				return false;
            }
        }

		public abstract TResult Accept<TResult>(Func<T, TResult> okay, Func<ImmutableArray<Error>, TResult> error);

		public bool IsOkay => Accept(_ => true, _ => false);
		public abstract bool TryGetValue([MaybeNullWhen(false)] out T value);

		public static implicit operator ErrorOr<T>(Error error) => ImmutableArray.Create(error);
		public static implicit operator ErrorOr<T>(ImmutableArray<Error> errors) => new ErrorT(errors);
		public static ErrorOr<T> FromValue(T value) => new ValueT(value);

		public ImmutableArray<Error>? MaybeError => Accept<ImmutableArray<Error>?>(_ => null, e => e);
		public T Value => Accept(x => x, e => throw new InvalidOperationException());
		public ErrorOr<TResult> Apply<TResult>(Func<T, TResult> func) =>
			Accept(x => ErrorOr.FromValue(func(x)), e => e);
	}
	public static class ErrorOr
	{
		public static ErrorOr<T> FromValue<T>(T value) => ErrorOr<T>.FromValue(value);
		public static ErrorOr<T> Try<T>(Func<T> func)
		{
			try
			{
				return FromValue(func());
			}
			catch (Exception e)
			{
				return new Error(ErrorId.GenericError, e.Message);
			}
		}
	}
}

using System;

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

			public override TResult Accept<TResult>(Func<T, TResult> okay, Func<string, TResult> error) => okay(_value);
		}
		private sealed class ErrorT : ErrorOr<T>
		{
			public string _error;

			public ErrorT(string error)
			{
				_error = error;
			}

			public override TResult Accept<TResult>(Func<T, TResult> okay, Func<string, TResult> error) => error(_error);
		}

		public abstract TResult Accept<TResult>(Func<T, TResult> okay, Func<string, TResult> error);

		public bool IsOkay => Accept(_ => true, _ => false);

		public static implicit operator ErrorOr<T>(string error) => new ErrorT(error);
		public static ErrorOr<T> FromValue(T value) => new ValueT(value);

		public string? MaybeError => Accept<string?>(_ => null, e => e);
		public T Value => Accept(x => x, e => throw new InvalidOperationException());
		public ErrorOr<TResult> Apply<TResult>(Func<T, TResult> func) =>
			Accept(x => ErrorOr.FromValue(func(x)), e => e);
	}
	public static class ErrorOr
	{
		public static ErrorOr<T> FromValue<T>(T value) => ErrorOr<T>.FromValue(value);
	}
}

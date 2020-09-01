using System;

namespace CmdParse
{
	public abstract class ErrorOr<T>
	{
		private sealed class Value : ErrorOr<T>
		{
			public T _value;

			public Value(T value)
			{
				_value = value;
			}

			public override TResult Accept<TResult>(Func<T, TResult> okay, Func<string, TResult> error) => okay(_value);
		}
		private sealed class Error : ErrorOr<T>
		{
			public string _error;

			public Error(string error)
			{
				_error = error;
			}

			public override TResult Accept<TResult>(Func<T, TResult> okay, Func<string, TResult> error) => error(_error);
		}

		public abstract TResult Accept<TResult>(Func<T, TResult> okay, Func<string, TResult> error);

		public bool IsOkay => Accept(_ => true, _ => false);

		public static implicit operator ErrorOr<T>(string error) => new Error(error);
		public static ErrorOr<T> FromValue(T value) => new Value(value);

		public string? MaybeError => Accept<string?>(_ => null, e => e);
	}
	public static class ErrorOr
	{
		public static ErrorOr<T> FromValue<T>(T value) => ErrorOr<T>.FromValue(value);
	}
}

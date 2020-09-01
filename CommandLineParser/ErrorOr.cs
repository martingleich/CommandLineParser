using System;

namespace CmdParse
{
	public abstract class ErrorOr<T>
	{
		private sealed class Value : ErrorOr<T>
		{
			public T _value;
			public override TResult Accept<TResult>(Func<T, TResult> okay, Func<string, TResult> error) => okay(_value);
		}
		private sealed class Error : ErrorOr<T>
		{
			public string _error;
			public override TResult Accept<TResult>(Func<T, TResult> okay, Func<string, TResult> error) => error(_error);
		}

		public abstract TResult Accept<TResult>(Func<T, TResult> okay, Func<string, TResult> error);

		public bool IsOkay => Accept(_ => true, _ => false);

		public static implicit operator ErrorOr<T>(string error) => new Error() { _error = error };
		public static ErrorOr<T> FromValue(T value) => new Value() { _value = value };
	}
	public static class ErrorOr
	{
		public static ErrorOr<T> FromValue<T>(T value) => ErrorOr<T>.FromValue(value);
	}
}

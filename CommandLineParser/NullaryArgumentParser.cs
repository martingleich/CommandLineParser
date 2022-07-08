using System;

namespace CmdParse
{
	/// <summary>
	/// An argument parser that consumes no parameters from the stream.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal sealed class NullaryArgumentParser<T> : IArgumentParser
	{
		public NullaryArgumentParser(T value, string syntaxDescription)
		{
			Value = value;
			HumanReadableSyntaxDescription = syntaxDescription;
		}

		public T Value { get; }
		public Type ResultType => typeof(T);
		public string HumanReadableSyntaxDescription { get; }

		public ErrorOr<(ParameterStream Remainder, object? Value)> Parse(ParameterStream parameters)
            => ErrorOr.FromValue((parameters, (object?)Value));
	}
}

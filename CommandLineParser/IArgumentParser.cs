using System;

namespace CmdParse
{
	public interface IArgumentParser
	{
		ErrorOr<(ParameterStream Remainder, object? Value)> Parse(ParameterStream parameters);
		Type ResultType { get; }
		string HumanReadableSyntaxDescription { get; }
	}

	public interface IArgumentParser<T> : IArgumentParser
	{
	}
}

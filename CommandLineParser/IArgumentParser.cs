using System;
using System.Collections.Generic;

namespace CmdParse
{
	public interface IArgumentParser
	{
		ErrorOr<(int Count, object? Value)> Parse(IEnumerable<string> args);
		Type ResultType { get; }
		string HumanReadableSyntaxDescription { get; }
	}
}

using System;
using System.Collections.Generic;

namespace CmdParse
{
	public interface IArgumentParser
	{
		ErrorOr<(int Count, object? Value)> Parse(Argument arg, IEnumerable<string> parameters);
		Type ResultType { get; }
		string HumanReadableSyntaxDescription { get; }
	}
}

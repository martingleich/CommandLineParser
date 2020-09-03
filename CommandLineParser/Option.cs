using System.Collections.Generic;

namespace CmdParse
{
	public class Option : AbstractArgument
	{
		public Option(string name, string? shortName, object? defaultValue) :
			base(true, defaultValue, name, shortName, null, Arity.OneOrZero, typeof(bool))
		{
		}

		public override ErrorOr<(int Count, object? Value)> Parse(IEnumerable<string> args)
			=> ErrorOr.FromValue((0, (object?)!(bool)DefaultValue));
	}
}

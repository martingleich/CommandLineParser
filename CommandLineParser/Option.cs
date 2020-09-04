using System.Collections.Generic;

namespace CmdParse
{
	public class Option : AbstractArgument
	{
		public bool DefaultValue { get; }
		public Option(string name, string? shortName, bool defaultValue) :
			base(OptionalSettings.Optional(defaultValue), name, shortName, null, Arity.OneOrZero, typeof(bool))
		{
			DefaultValue = defaultValue;
		}

		public override ErrorOr<(int Count, object? Value)> Parse(IEnumerable<string> args)
			=> ErrorOr.FromValue((0, (object?)!DefaultValue));
	}
}

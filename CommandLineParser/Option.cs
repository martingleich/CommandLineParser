using System.Collections.Generic;
using System.Reflection;

namespace CmdParse
{
	public class Option : AbstractArgument
	{
		public Option(FieldInfo location, string name, object? defaultValue) :
			base(location, defaultValue, name, Arity.OneOrZero, typeof(bool))
		{
		}

		public override ErrorOr<(int Count, object? Value)> Parse(IEnumerable<string> args)
			=> ErrorOr.FromValue((0, (object?)!(bool)DefaultValue));
	}
}

﻿using System.Collections.Generic;

namespace CmdParse
{
	public class Option : AbstractArgument
	{
		public Option(string name, string? shortName, object? defaultValue) :
			base(defaultValue, name, shortName, Arity.OneOrZero, typeof(bool))
		{
		}

		public override ErrorOr<(int Count, object? Value)> Parse(IEnumerable<string> args)
			=> ErrorOr.FromValue((0, (object?)!(bool)DefaultValue));
	}
}

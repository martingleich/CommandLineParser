using System;
using System.Collections.Generic;

namespace CmdParse
{
	public sealed class Argument
	{
		public Argument(
			OptionalSettings optionalSettings,
			string name,
			string? shortName,
			int? freeIndex,
			Arity arity,
			IArgumentParser parser)
		{
			OptionalSettings = optionalSettings;
			Name = name;
			ShortName = shortName;
			FreeIndex = freeIndex;
			Arity = arity;
			Parser = parser;
		}

		public OptionalSettings OptionalSettings { get; }
		public string Name { get; }
		public string? ShortName { get; }
		public int? FreeIndex { get; }
		public bool IsFree => FreeIndex.HasValue;
		public Arity Arity { get; }
		public Type ResultType => Parser.ResultType;
		public IArgumentParser Parser { get; }

		public ErrorOr<(int Count, object? Value)> Parse(IEnumerable<string> args) => Parser.Parse(args);

		public override string ToString()
		{
			var result = $"{Name} : {ResultType.Name}";
			if(Arity == Arity.ZeroOrMany)
				result += "[]";
			if (OptionalSettings.GetDefaultValue(out var defaultValue))
				result += " = " + defaultValue?.ToString();
			return result;
		}
	}
}

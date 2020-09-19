using System;
using System.Collections.Generic;

namespace CmdParse
{
	public sealed class Argument
	{
		public static Argument Option(string? description, string name, string? shortName, bool valueIfPassed)
			=> new Argument(description, AritySettings.Optional(!valueIfPassed), name, shortName, null, new NullaryArgumentParser<bool>(valueIfPassed));

		public Argument(
			string? description,
			AritySettings aritySettings,
			string name,
			string? shortName,
			int? freeIndex,
			IArgumentParser parser)
		{
			Description = description;
			AritySettings = aritySettings;
			Name = name;
			ShortName = shortName;
			FreeIndex = freeIndex;
			Parser = parser;
		}
		public string? Description { get; }

		public AritySettings AritySettings { get; }
		public string Name { get; }
		public string? ShortName { get; }
		public int? FreeIndex { get; }
		public bool IsFree => FreeIndex.HasValue;
		public Type ResultType => Parser.ResultType;
		public IArgumentParser Parser { get; }

		public ErrorOr<(int Count, object? Value)> Parse(IEnumerable<string> args) => Parser.Parse(args);

		public override string ToString()
		{
			var result = $"{Name} : {ResultType.Name}{AritySettings.PostfixString}";
			if (AritySettings.GetDefaultValue(out var defaultValue))
				result += " = " + defaultValue?.ToString();
			return result;
		}
	}
}

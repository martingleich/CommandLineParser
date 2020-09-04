﻿using System;
using System.Collections.Generic;

namespace CmdParse
{
	public abstract class AbstractArgument
	{
		protected AbstractArgument(OptionalSettings optionalSettings, string name, string? shortName, int? freeIndex, Arity arity, Type resultType)
		{
			OptionalSettings = optionalSettings;
			Name = name;
			ShortName = shortName;
			FreeIndex = freeIndex;
			Arity = arity;
			ResultType = resultType;
		}

		public OptionalSettings OptionalSettings { get; }
		public string Name { get; }
		public string? ShortName { get; }
		public int? FreeIndex { get; }
		public bool IsFree => FreeIndex.HasValue;
		public Arity Arity { get; }
		public Type ResultType { get; }

		public abstract ErrorOr<(int Count, object? Value)> Parse(IEnumerable<string> args);

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

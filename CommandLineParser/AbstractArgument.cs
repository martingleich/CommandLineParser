using System;
using System.Collections.Generic;

namespace CmdParse
{
	public abstract class AbstractArgument
	{
		protected AbstractArgument(bool isOptional, object? defaultValue, string name, string? shortName, int? freeIndex, Arity arity, Type resultType)
		{
			IsOptional = isOptional;
			DefaultValue = defaultValue;
			Name = name;
			ShortName = shortName;
			FreeIndex = freeIndex;
			Arity = arity;
			ResultType = resultType;
		}

		public bool IsOptional { get; }
		public object? DefaultValue { get; }
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
			if (IsOptional)
				result += " = " + DefaultValue?.ToString();
			return result;
		}
	}
}

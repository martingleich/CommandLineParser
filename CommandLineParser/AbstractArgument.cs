using System;
using System.Collections;
using System.Collections.Generic;

namespace CmdParse
{
	public abstract class AbstractArgument
	{
		protected AbstractArgument(object? defaultValue, string name, string? shortName, Arity arity, Type resultType)
		{
			DefaultValue = defaultValue;
			Name = name;
			ShortName = shortName;
			Arity = arity;
			ResultType = resultType;
		}

		public object? DefaultValue { get; }
		public string Name { get; }
		public string? ShortName { get; }
		public Arity Arity { get; }
		public Type ResultType { get; }

		public abstract ErrorOr<(int Count, object? Value)> Parse(IEnumerable<string> args);

		public override string ToString()
		{
			var result = $"{Name} : {ResultType.Name}";
			if(Arity == Arity.ZeroOrMany)
				result += "[]";
			if (DefaultValue != null)
				result += " = " + DefaultValue.ToString();
			return result;
		}
	}
}

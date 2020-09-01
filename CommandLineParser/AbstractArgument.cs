using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace CmdParse
{
	public abstract class AbstractArgument
	{
		protected AbstractArgument(FieldInfo location, object? defaultValue, string name, Arity arity, Type resultType)
		{
			Location = location;
			DefaultValue = defaultValue;
			Name = name;
			Arity = arity;
			ResultType = resultType;
		}

		public FieldInfo Location { get; }
		public object? DefaultValue { get; }
		public string Name { get; }
		public Arity Arity { get; }
		public Type ResultType { get; }

		public abstract ErrorOr<(int Count, object? Value)> Parse(IEnumerable<string> args);

		public IList CreateList()
		{
			return (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(new[] { ResultType }));
		}
	}
}

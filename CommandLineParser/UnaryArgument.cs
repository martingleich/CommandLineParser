using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CmdParse
{
	public class UnaryArgument : AbstractArgument
	{
		public Func<string, ErrorOr<object?>> Parser { get; }
		public UnaryArgument(FieldInfo location, string name, object? defaultValue, Arity arity, Type type, Func<string, ErrorOr<object?>> parser) :
			base(location, defaultValue, name, arity, type)
		{
			Parser = parser;
		}
		public override ErrorOr<(int Count, object? Value)> Parse(IEnumerable<string> args)
		{
			var arg = args.FirstOrDefault();
			if (args == null)
				return $"Missing the value for '{arg}'.";
			return Parser(arg).Apply(x => (1, x));
		}
	}
}

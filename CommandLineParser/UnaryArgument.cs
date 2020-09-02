using System;
using System.Collections.Generic;
using System.Linq;

namespace CmdParse
{
	public class UnaryArgument : AbstractArgument
	{
		public Func<string, ErrorOr<object?>> Parser { get; }
		public UnaryArgument(object? defaultValue, string name, string? shortName, int? freeIndex, Arity arity, Type type, Func<string, ErrorOr<object?>> parser) :
			base(defaultValue, name, shortName, freeIndex, arity, type)
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

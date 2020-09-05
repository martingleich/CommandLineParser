using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CmdParse
{
	public sealed class UnaryArgumentParser : IArgumentParser
	{
		public Type ResultType { get; }
		public Func<string, ErrorOr<object?>> Parser { get; }
		public UnaryArgumentParser(Type resultType, Func<string, ErrorOr<object?>> parser)
		{
			ResultType = resultType;
			Parser = parser;
		}
		public ErrorOr<(int Count, object? Value)> Parse(IEnumerable<string> args)
		{
			var arg = args.FirstOrDefault();
			if (args == null)
				return $"Missing the value for '{arg}'.";
			else
				return Parser(arg).Apply(x => (1, x));
		}

		public static UnaryArgumentParser Bool { get; } = new UnaryArgumentParser(typeof(bool), Converters.TryParseBool);
		public static UnaryArgumentParser Int { get; } = new UnaryArgumentParser(typeof(int), Converters.TryParseInt);
		public static UnaryArgumentParser Double { get; } = new UnaryArgumentParser(typeof(double), Converters.TryParseDouble);
		public static UnaryArgumentParser String { get; } = new UnaryArgumentParser(typeof(string), Converters.TryParseString);
		public static UnaryArgumentParser DirectoryInfo { get; } = new UnaryArgumentParser(typeof(DirectoryInfo), str => ErrorOr.Try<object?>(() => new DirectoryInfo(str)));
		public static UnaryArgumentParser FileInfo { get; } = new UnaryArgumentParser(typeof(FileInfo), str => ErrorOr.Try<object?>(() => new FileInfo(str)));
	}
}

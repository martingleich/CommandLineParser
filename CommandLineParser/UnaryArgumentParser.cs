using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CmdParse
{
	public static class UnaryArgumentParser
	{
		public static UnaryArgumentParser<T> Create<T>(Func<string, ErrorOr<T>> func)
			=> new UnaryArgumentParser<T>(func);
		public static UnaryArgumentParser<bool> Bool { get; } = Create(Converters.TryParseBool);
		public static UnaryArgumentParser<int> Int { get; } = Create(Converters.TryParseInt);
		public static UnaryArgumentParser<double> Double { get; } = Create(Converters.TryParseDouble);
		public static UnaryArgumentParser<string> String { get; } = Create(ErrorOr.FromValue);
		public static UnaryArgumentParser<DirectoryInfo> DirectoryInfo { get; } = Create(str => ErrorOr.Try(() => new DirectoryInfo(str)));
		public static UnaryArgumentParser<FileInfo> FileInfo { get; } = Create(str => ErrorOr.Try(() => new FileInfo(str)));
	}

	public sealed class UnaryArgumentParser<T> : IArgumentParser
	{
		public Type ResultType => typeof(T);
		public Func<string, ErrorOr<T>> Parser { get; }
		public UnaryArgumentParser(Func<string, ErrorOr<T>> parser)
		{
			Parser = parser;
		}
		public ErrorOr<(int Count, object? Value)> Parse(IEnumerable<string> args)
		{
			var arg = args.FirstOrDefault();
			if (args == null)
				return $"Missing the value for '{arg}'.";
			else
				return Parser(arg).Apply(x => (1, (object?)x));
		}
	}
}

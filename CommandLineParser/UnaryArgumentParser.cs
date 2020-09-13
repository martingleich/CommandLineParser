using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CmdParse
{
	public static class UnaryArgumentParser
	{
		public static UnaryArgumentParser<T> Create<T>(Func<string, ErrorOr<T>> func, string description)
			=> new UnaryArgumentParser<T>(func, description);
		public static UnaryArgumentParser<bool> Bool { get; } = Create(Converters.TryParseBool, "Boolean");
		public static UnaryArgumentParser<int> Int { get; } = Create(Converters.TryParseInt, "Int32");
		public static UnaryArgumentParser<double> Double { get; } = Create(Converters.TryParseDouble, "Double");
		public static UnaryArgumentParser<string> String { get; } = Create(ErrorOr.FromValue, "String");
		public static UnaryArgumentParser<DirectoryInfo> DirectoryInfo { get; } = Create(str => ErrorOr.Try(() => new DirectoryInfo(str)), "Folderpath");
		public static UnaryArgumentParser<FileInfo> FileInfo { get; } = Create(str => ErrorOr.Try(() => new FileInfo(str)), "Filepath");
	}

	public sealed class UnaryArgumentParser<T> : IArgumentParser
	{
		public Type ResultType => typeof(T);
		public Func<string, ErrorOr<T>> Parser { get; }

		public string HumanReadableSyntaxDescription { get;}

		public UnaryArgumentParser(Func<string, ErrorOr<T>> parser, string description)
		{
			Parser = parser;
			HumanReadableSyntaxDescription = description;
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

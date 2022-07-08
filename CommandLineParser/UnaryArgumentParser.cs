using System;
using System.IO;
using System.Net;

namespace CmdParse
{
	/// <summary>
	/// Helper class to create parser from a single argument.
	/// </summary>
    public static class UnaryArgumentParser
	{
		public static IArgumentParser<T> Create<T>(Func<string, ErrorOr<T>> func, string description)
			=> new UnaryArgumentParser<T>(func, description);
		public static IArgumentParser<bool> Bool { get; } = Create(Converters.TryParseBool, "Boolean");
		public static IArgumentParser<int> Int { get; } = Create(Converters.TryParseInt, "Int32");
		public static IArgumentParser<double> Double { get; } = Create(Converters.TryParseDouble, "Double");
		public static IArgumentParser<string> String { get; } = Create(ErrorOr.FromValue, "String");
		public static IArgumentParser<DirectoryInfo> DirectoryInfo { get; } = Create(str => ErrorOr.Try(() => new DirectoryInfo(str)), "Folderpath");
		public static IArgumentParser<FileInfo> FileInfo { get; } = Create(str => ErrorOr.Try(() => new FileInfo(str)), "Filepath");
		public static IArgumentParser<IPEndPoint> IPEndPoint { get; } = Create(Converters.TryParseIPEndPoint, "IPEndPoint");
	}

	internal sealed class UnaryArgumentParser<T> : IArgumentParser<T>
	{
		public Type ResultType => typeof(T);
		public Func<string, ErrorOr<T>> Parser { get; }

		public string HumanReadableSyntaxDescription { get;}

		public UnaryArgumentParser(Func<string, ErrorOr<T>> parser, string description)
		{
			Parser = parser;
			HumanReadableSyntaxDescription = description;
		}
		public ErrorOr<(ParameterStream Remainder, object? Value)> Parse(ParameterStream parameters)
        {
			var param = parameters.TryTake(out var remainder);
			if (param == null)
				return new Error(ErrorId.MissingArgumentParameter);
			else
				return Parser(param).Apply(x => (remainder, (object?)x));
		}
	}
}

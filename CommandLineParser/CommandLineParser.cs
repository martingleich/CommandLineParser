using System;
using System.Collections.Immutable;

namespace CmdParse
{
	public static class CommandLineParser
	{
		public static T Parse<T>(params string[] args) where T : notnull
			=> ParseWithError<T>(args).Accept(
				okay: r => r,
				error: errors => throw new InvalidOperationException(string.Join(Environment.NewLine, errors)));

		public static ErrorOr<T> ParseWithError<T>(params string[] args) where T : notnull
		{
			var factory = new CommandLineParserFactory();
			var config = factory.CreateParser<T>();
			return config.Parse(args);
		}

		public static int Call<T>(string[] args, Func<T, int> main) where T : notnull
		{
			var factory = new CommandLineParserFactory();
			var config = factory.CreateParser<T>();
			var result = config.Parse(args);
			return result.Accept(main, err => OnError(config, err));
		}

		public static int OnError<T>(CommandLineParser<T> config, ImmutableArray<Error> errors)
		{
			var help = new HelpPrinter().PrintHelp(config);
			if (errors.IsEmpty)
			{
				Console.WriteLine(help);
				return 0;
			}
			else
			{
				var old = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;
				foreach(var error in errors)
					Console.WriteLine(error);
				Console.ForegroundColor = old;

				Console.WriteLine();
				Console.WriteLine(help);
				return 1;
			}
		}

		public static CommandLineParserFactory CreateFactory() => new CommandLineParserFactory();	
	}
}

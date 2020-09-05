using System;

namespace CmdParse
{
	public static class CommandLineParser
	{
		public static T Parse<T>(string[] args) where T : new()
			=> ParseWithError<T>(args).Accept(
				okay: r => r,
				error: error => throw new InvalidOperationException(error));
		public static ErrorOr<T> ParseWithError<T>(string[] args) where T : new()
		{
			var factory = new CommandLineConfigurationFactory();
			var config = factory.Create<T>();
			return config.Parse(args);
		}
	}
}

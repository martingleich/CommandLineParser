using System;

namespace CmdParse
{
	public static class CommandLineParser
	{
		public static T Parse<T>(string[] args)
			=> ParseWithError<T>(args).Accept(
				okay: r => r,
				error: error => throw new InvalidOperationException(error));
		public static ErrorOr<T> ParseWithError<T>(string[] args)
		{
			var factory = new CommandLineConfigurationFactory();
			var config = factory.Create(typeof(T));
			return config.Parse<T>(args);
		}
	}
}

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
			var config = CommandLineConfiguration.Create(typeof(T));
			return config.Parse<T>(args);
		}
	}
}

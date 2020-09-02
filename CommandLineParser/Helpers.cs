using System;

namespace CmdParse
{
	internal static class Helpers
	{
		public static T ThrowIfNull<T>(this T? value) where T : class
			=> value ?? throw new ArgumentNullException(nameof(value));
	}
}

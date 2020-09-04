using System;
using System.Linq;

namespace CmdParse
{
	internal static class Helpers
	{
		public static T ThrowIfNull<T>(this T? value) where T : class
			=> value ?? throw new ArgumentNullException(nameof(value));
		public static Type? UnpackSingleGeneric(this Type type, Type monadType)
		{
			if (type.IsGenericType && type.GetGenericTypeDefinition() == monadType)
				return type.GetGenericArguments().Single();
			else
				return null;
		}
	}
}

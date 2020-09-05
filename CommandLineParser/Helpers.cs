using System;
using System.Collections.Generic;
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
		public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> values) where T : struct
		{
			foreach (var value in values)
			{
				if (value.HasValue)
					yield return value.Value;
			}
		}
	}
}

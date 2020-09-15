using System;
using System.Collections;
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
		public static IEnumerable<T> Append<T>(this IEnumerable<T> values, T value)
		{
			foreach (var v in values)
				yield return v;
			yield return value;
		}
		public static IList CreateList(Type type)
			=> (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(new[] { type })).ThrowIfNull();
		public static IEnumerable CreateEmptyEnumerable(Type type)
			=> Array.CreateInstance(type, 0);
	}
}

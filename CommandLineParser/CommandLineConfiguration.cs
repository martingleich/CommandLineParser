using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace CmdParse
{
	public class CommandLineConfiguration
	{
		public CommandLineConfiguration(ImmutableDictionary<string, AbstractArgument> argumentLookup)
		{
			ArgumentLookup = argumentLookup;
		}

		public ImmutableDictionary<string, AbstractArgument> ArgumentLookup { get; }
		public IEnumerable<AbstractArgument> Arguments => ArgumentLookup.Values;

		public static CommandLineConfiguration Create(Type t)
		{
			// 1) Scan public writable values of t.
			var fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
			var arguments = new List<AbstractArgument>();
			foreach (var field in fields)
			{
				var nameAttribute = field.GetCustomAttribute<CmdNameAttribute>();
				var name = nameAttribute?.Name ?? field.Name;
				var shortName = nameAttribute?.ShortName;

				var defaultValue = field.GetCustomAttribute<CmdOptionDefaultAttribute>()?.DefaultValue;
				if (defaultValue != null && !field.FieldType.IsInstanceOfType(defaultValue))
					throw new ArgumentException("Wrong default type");
				Type elemType;
				Arity arity;
				if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				{
					elemType = field.FieldType.GetGenericArguments().Single();
					arity = Arity.ZeroOrMany;
				}
				else
				{
					elemType = field.FieldType;
					arity = Arity.OneOrZero;
				}

				if (elemType == typeof(bool) && arity == Arity.OneOrZero)
				{
					arguments.Add(new Option(field, name, shortName, defaultValue ?? false));
				}
				else if (elemType == typeof(bool))
				{
					arguments.Add(new UnaryArgument(field, name, shortName, defaultValue, arity, elemType, str => bool.TryParse(str, out var val)
						? ErrorOr.FromValue<object?>(val)
						: "Invalid boolean"));
				}
				else if (elemType == typeof(int))
				{
					arguments.Add(new UnaryArgument(field, name, shortName, defaultValue, arity, elemType, str => int.TryParse(str, out var val)
						? ErrorOr.FromValue<object?>(val)
						: "Invalid integer"));
				}
				else if (field.FieldType == typeof(double))
				{
					arguments.Add(new UnaryArgument(field, name, shortName, defaultValue, arity, elemType, str => double.TryParse(str, out var val)
						? ErrorOr.FromValue<object?>(val)
						: "Invalid double"));
				}
			}

			var argumentLookup = arguments.ToDictionary(a => "--" + a.Name);
			foreach (var arg in arguments)
			{
				if (arg.ShortName is string shortName)
					argumentLookup.Add("-" + shortName, arg);
			}
			return new CommandLineConfiguration(argumentLookup.ToImmutableDictionary());
		}

		public AbstractArgument? FindArgument(string arg)
		{
			ArgumentLookup.TryGetValue(arg, out var matchedArg);
			return matchedArg;
		}
		public ErrorOr<T> Parse<T>(string[] args)
		{
			var values = new Dictionary<AbstractArgument, object?>();
			for (int i = 0; i < args.Length; ++i)
			{
				var arg = args[i];
				if (FindArgument(arg) is AbstractArgument matchedArg)
				{
					var parseResult = matchedArg.Parse(args.Skip(i + 1));
					if (parseResult.MaybeError is string error)
						return error;
					var (count, value) = parseResult.Value;
					if (matchedArg.Arity != Arity.OneOrZero)
					{
						if (!values.TryGetValue(matchedArg, out object? list) || list == null)
						{
							list = matchedArg.CreateList();
							values.Add(matchedArg, list);
						}
						((IList)list).Add(value);
					}
					else if (!values.TryAdd(matchedArg, value))
						return $"Duplicate option '{arg}'.";
					i += count;
				}
				else
				{
					return $"Unknown option '{arg}'.";
				}
			}
			foreach (var arg in Arguments)
			{
				if (!values.ContainsKey(arg))
				{
					if (arg.DefaultValue is object defaultValue)
						values.Add(arg, defaultValue);
					else if (arg.Arity == Arity.ZeroOrMany)
						values.Add(arg, arg.CreateList());
					else
						return $"Missing mandatory argument '--{arg.Name}'.";
				}
			}

			var result = (T)Activator.CreateInstance(typeof(T));
			foreach (var arg in Arguments)
				arg.Location.SetValue(result, values[arg]);
			return ErrorOr.FromValue(result);
		}
	}
}

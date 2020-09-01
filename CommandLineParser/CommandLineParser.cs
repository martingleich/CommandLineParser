using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

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

	public class CmdOptionDefaultAttribute : Attribute
	{
		public CmdOptionDefaultAttribute(object defaultValue)
		{
			DefaultValue = defaultValue;
		}

		public object DefaultValue { get; }
	}

	public class CommandLineConfiguration
	{
		public CommandLineConfiguration(ImmutableArray<AbstractArgument> arguments)
		{
			Arguments = arguments;
		}

		public abstract class AbstractArgument
		{
			protected AbstractArgument(FieldInfo location, object? defaultValue, string name)
			{
				Location = location;
				DefaultValue = defaultValue;
				Name = name;
			}

			public FieldInfo Location { get; }
			public object? DefaultValue { get; }
			public string Name { get; }

			public abstract ErrorOr<(int Count, object? Value)> Parse(IEnumerable<string> args);
		}
		public class Option : AbstractArgument
		{
			public Option(FieldInfo location, string name, object? defaultValue) :
				base(location, defaultValue, name)
			{
			}

			public override ErrorOr<(int Count, object? Value)> Parse(IEnumerable<string> args)
				=> ErrorOr.FromValue((0, (object?)!(bool)DefaultValue));
		}

		public class UnaryArgument : AbstractArgument
		{
			public Func<string, ErrorOr<object?>> Parser { get; }
			public UnaryArgument(FieldInfo location, string name, object? defaultValue, Func<string, ErrorOr<object?>> parser) :
				base(location, defaultValue, name)
			{
				Parser = parser;
			}
			public override ErrorOr<(int Count, object? Value)> Parse(IEnumerable<string> args)
			{
				var arg = args.FirstOrDefault();
				if (args == null)
					return $"Missing the value for '{arg}'.";
				return Parser(arg).Apply(x => (1, x));
			}
		}

		public ImmutableArray<AbstractArgument> Arguments { get; }

		public static CommandLineConfiguration Create(Type t)
		{
			// 1) Scan public writable values of t.
			var fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
			var arguments = new List<AbstractArgument>();
			foreach (var field in fields)
			{
				var name = field.Name;
				var defaultValue = field.GetCustomAttribute<CmdOptionDefaultAttribute>()?.DefaultValue;
				if (defaultValue != null && !field.FieldType.IsInstanceOfType(defaultValue))
					throw new InvalidOperationException("Wrong default type");

				if (field.FieldType == typeof(bool))
				{
					arguments.Add(new Option(field, name, defaultValue ?? false));
				}
				else if (field.FieldType == typeof(int))
				{
					arguments.Add(new UnaryArgument(field, name, defaultValue, str => int.TryParse(str, out var val)
						? ErrorOr.FromValue<object?>(val)
						: "Invalid integer"));
				}
				else if (field.FieldType == typeof(double))
				{
					arguments.Add(new UnaryArgument(field, name, defaultValue, str => double.TryParse(str, out var val)
						? ErrorOr.FromValue<object?>(val)
						: "Invalid double"));
				}
			}

			return new CommandLineConfiguration(arguments.ToImmutableArray());
		}

		public ErrorOr<T> Parse<T>(string[] args)
		{
			var values = new Dictionary<AbstractArgument, object?>();
			for (int i = 0; i < args.Length; ++i)
			{
				var arg = args[i];
				var matchedArg = Arguments.Where(b => "--" + b.Name == arg).FirstOrDefault();
				if (matchedArg != null)
				{
					var parseResult = matchedArg.Parse(args.Skip(i + 1));
					if (parseResult.MaybeError is string error)
						return error;
					var (count, value) = parseResult.Value;
					if (!values.TryAdd(matchedArg, value))
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

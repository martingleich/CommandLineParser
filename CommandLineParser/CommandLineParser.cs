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
		public CmdOptionDefaultAttribute(bool defaultValue)
		{
			DefaultValue = defaultValue;
		}

		public bool DefaultValue { get; }
	}

	public class CommandLineConfiguration
	{
		public CommandLineConfiguration(
			ImmutableArray<Option> booleanArguments,
			ImmutableArray<IntegerArgument> integerArguments)
		{
			Options = booleanArguments;
			IntegerArguments = integerArguments;
		}

		public class Option
		{
			public Option(FieldInfo location, string name, bool defaultValue)
			{
				Location = location;
				Name = name;
				DefaultValue = defaultValue;
			}

			public FieldInfo Location { get; }
			public bool DefaultValue { get; }
			public string Name { get;}
			public string LongName => "--" + Name;
		}
		public class IntegerArgument
		{
			public IntegerArgument(FieldInfo location, string name)
			{
				Location = location;
				Name = name;
			}
			public FieldInfo Location { get; }
			public string Name { get; }
			public string LongName => "--" + Name;
		}
	
		public ImmutableArray<Option> Options { get; }
		public ImmutableArray<IntegerArgument> IntegerArguments { get; }
		
		public static CommandLineConfiguration Create(Type t)
		{
			// 1) Scan public writable values of t.
			var fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
			List<Option> options = new List<Option>();
			List<IntegerArgument> intArgs = new List<IntegerArgument>();
			foreach (var field in fields)
			{
				if (field.FieldType == typeof(bool))
				{
					var name = field.Name;
					var defaultValue = field.GetCustomAttribute<CmdOptionDefaultAttribute>()?.DefaultValue ?? false;
					options.Add(new Option(field, name, defaultValue));
				}
				else if (field.FieldType == typeof(int))
				{
					var name = field.Name;
					intArgs.Add(new IntegerArgument(field, name));
				}
			}

			return new CommandLineConfiguration(options.ToImmutableArray(), intArgs.ToImmutableArray());
		}

		public ErrorOr<T> Parse<T>(string[] args)
		{
			Dictionary<Option, bool> values = new Dictionary<Option, bool>();
			Dictionary<IntegerArgument, int> intValues = new Dictionary<IntegerArgument, int>();
			for(int i =0; i< args.Length; ++i)
			{
				var arg = args[i];
				var matchedOption = Options.Where(b => b.LongName == arg).FirstOrDefault();
				if (matchedOption != null)
				{
					if (!values.TryAdd(matchedOption, !matchedOption.DefaultValue))
						return $"Duplicate option '{arg}'.";
					continue;
				}
				var matchedIntArg = IntegerArguments.Where(b => b.LongName == arg).FirstOrDefault();
				if (matchedIntArg != null)
				{
					var valueArg = i + 1 < args.Length ? args[i + 1] : null;
					if (!int.TryParse(valueArg, out int value))
						return $"Missing the value for '{arg}'.";
					if (!intValues.TryAdd(matchedIntArg, value))
						return $"Duplicate option '{arg}'.";
					++i;
					continue;
				}
				else
				{
					return $"Unknown option '{arg}'.";
				}
			}
			foreach (var arg in Options)
			{
				if (!values.ContainsKey(arg))
					values.Add(arg, arg.DefaultValue);
			}
			foreach (var arg in IntegerArguments)
			{
				if(!intValues.ContainsKey(arg))
					return $"Missing mandatory argument '{arg.LongName}'.";
			}

			var result = (T)Activator.CreateInstance(typeof(T));
			foreach(var opt in Options)
				opt.Location.SetValue(result, values[opt]);
			foreach(var arg in IntegerArguments)
				arg.Location.SetValue(result, intValues[arg]);
			return ErrorOr.FromValue(result);
		}
	}
}

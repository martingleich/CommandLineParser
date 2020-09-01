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
			var options = CommandLineConfiguration.Create(typeof(T));
			return options.Parse<T>(args);
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
		public CommandLineConfiguration(ImmutableArray<Option> booleanArguments)
		{
			Options = booleanArguments;
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
	
		public ImmutableArray<Option> Options { get; }
		
		public static CommandLineConfiguration Create(Type t)
		{
			// 1) Scan public writable values of t.
			var fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
			List<Option> options = new List<Option>();
			foreach (var field in fields)
			{
				if (field.FieldType == typeof(bool))
				{
					var name = field.Name;
					var defaultValue = field.GetCustomAttribute<CmdOptionDefaultAttribute>()?.DefaultValue ?? false;
					options.Add(new Option(field, name, defaultValue));
				}
			}

			return new CommandLineConfiguration(options.ToImmutableArray());
		}

		public ErrorOr<T> Parse<T>(string[] args)
		{
			Dictionary<Option, bool> values = new Dictionary<Option, bool>();
			foreach (var arg in args)
			{
				var matched = Options.Where(b => b.LongName == arg).FirstOrDefault();
				if (matched == null)
					return $"Unknown option '{arg}'.";
				if (!values.TryAdd(matched, !matched.DefaultValue))
					return $"Duplicate option '{arg}'.";
			}
			foreach (var arg in Options)
			{
				if (!values.ContainsKey(arg))
					values.Add(arg, arg.DefaultValue);
			}

			var result = (T)Activator.CreateInstance(typeof(T));
			foreach(var opt in Options)
				opt.Location.SetValue(result, values[opt]);
			return ErrorOr.FromValue(result);
		}
	}
}

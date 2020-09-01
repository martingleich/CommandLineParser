using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace CmdParse
{
	public class CommandLineConfigurationFactory
	{
		public CommandLineConfiguration Create(Type t)
		{
			var fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
			var arguments = fields.ToImmutableDictionary(f => f, CreateArgument);

			var argumentLookup = CreateLookupTable(arguments.Values);
			object Factory(IDictionary<AbstractArgument, object?> values)
			{
				var result = Activator.CreateInstance(t);
				foreach (var arg in arguments)
					arg.Key.SetValue(result, values[arg.Value]);
				return result;
			}
			return new CommandLineConfiguration(argumentLookup, Factory);
		}

		private ImmutableDictionary<string, AbstractArgument> CreateLookupTable(IEnumerable<AbstractArgument> arguments)
		{
			var argumentLookup = new Dictionary<string, AbstractArgument>();
			foreach (var arg in arguments)
			{
				argumentLookup.Add("--" + arg.Name, arg);
				if (arg.ShortName is string shortName)
					argumentLookup.Add("-" + shortName, arg);
			}

			return argumentLookup.ToImmutableDictionary();
		}

		public AbstractArgument CreateArgument(FieldInfo field)
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
				return new Option(name, shortName, defaultValue ?? false);
			}
			else if (elemType == typeof(bool))
			{
				return new UnaryArgument(name, shortName, defaultValue, arity, elemType, str => bool.TryParse(str, out var val)
					? ErrorOr.FromValue<object?>(val)
					: "Invalid boolean"));
			}
			else if (elemType == typeof(int))
			{
				return new UnaryArgument(name, shortName, defaultValue, arity, elemType, str => int.TryParse(str, out var val)
					? ErrorOr.FromValue<object?>(val)
					: "Invalid integer");
			}
			else if (elemType == typeof(double))
			{
				return new UnaryArgument(name, shortName, defaultValue, arity, elemType, str => double.TryParse(str, out var val)
					? ErrorOr.FromValue<object?>(val)
					: "Invalid double");
			}
			else
			{
				throw new ArgumentException($"Unsupported type {elemType}.");
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace CmdParse
{
	public class CommandLineConfigurationFactory
	{
		private class WrittableMember
		{
			public MemberInfo Member { get; }
			public Action<object?, object?> Write { get; }
			public Type Type { get; }
			public string Name => Member.Name;
			public T? GetCustomAttribute<T>() where T : Attribute => Member.GetCustomAttribute<T>();

			public WrittableMember(MemberInfo member, Action<object?, object?> write, Type type)
			{
				Member = member;
				Write = write;
				Type = type;
			}
		}
		private IEnumerable<WrittableMember> GetWrittableMembers(Type t)
		{
			foreach (var field in t.GetFields(BindingFlags.Public | BindingFlags.Instance))
				if(!field.IsInitOnly)
					yield return new WrittableMember(field, field.SetValue, field.FieldType);
			foreach (var prop in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
				if (prop.CanWrite)
					yield return new WrittableMember(prop, prop.SetValue, prop.PropertyType);
		}

		public CommandLineConfiguration Create(Type t)
		{
			var settableMembers = GetWrittableMembers(t);
			var arguments = settableMembers.ToImmutableDictionary(f => f, CreateArgument);

			var argumentLookup = CreateLookupTable(arguments.Values);
			object Factory(IDictionary<AbstractArgument, object?> values)
			{
				var result = Activator.CreateInstance(t);
				foreach (var arg in arguments)
					arg.Key.Write(result, values[arg.Value]);
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

		private AbstractArgument CreateArgument(WrittableMember memberInfo)
		{
			var nameAttribute = memberInfo.GetCustomAttribute<CmdNameAttribute>();
			var name = nameAttribute?.Name ?? memberInfo.Name;
			var shortName = nameAttribute?.ShortName;

			var defaultValue = memberInfo.GetCustomAttribute<CmdOptionDefaultAttribute>()?.DefaultValue;
			if (defaultValue != null && !memberInfo.Type.IsInstanceOfType(defaultValue))
				throw new ArgumentException("Wrong default type");
			var (elemType, arity) = HandleEnumerable(memberInfo);

			if (elemType == typeof(bool) && arity == Arity.OneOrZero)
			{
				return new Option(name, shortName, defaultValue ?? false);
			}
			else if (elemType == typeof(bool))
			{
				return new UnaryArgument(name, shortName, defaultValue, arity, elemType, str => bool.TryParse(str, out var val)
					? ErrorOr.FromValue<object?>(val)
					: "Invalid boolean");
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
			else if (elemType == typeof(string))
			{
				return new UnaryArgument(name, shortName, defaultValue, arity, elemType, ErrorOr.FromValue<object?>);
			}
			else
			{
				throw new ArgumentException($"Unsupported type {elemType}.");
			}
		}

		private (Type elemType, Arity arity) HandleEnumerable(WrittableMember memberInfo)
		{
			if (memberInfo.Type.IsGenericType && memberInfo.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				return (memberInfo.Type.GetGenericArguments().Single(), Arity.ZeroOrMany);
			else
				return (memberInfo.Type, Arity.OneOrZero);
		}
	}
}

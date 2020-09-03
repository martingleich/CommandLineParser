using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CmdParse
{
	public class CommandLineConfigurationFactory
	{
		private static readonly Dictionary<Type, Func<string, ErrorOr<object?>>> UnaryConverters = new Dictionary<Type, Func<string, ErrorOr<object?>>>()
		{
			[typeof(bool)] = Converters.TryParseBool,
			[typeof(int)] = Converters.TryParseInt,
			[typeof(double)] = Converters.TryParseDouble,
			[typeof(string)] = Converters.TryParseString,
			[typeof(DirectoryInfo)] = str => ErrorOr.Try<object?>(() => new DirectoryInfo(str)),
			[typeof(FileInfo)] = str => ErrorOr.Try<object?>(() => new FileInfo(str)),
		};

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
				if (!field.IsInitOnly)
					yield return new WrittableMember(field, field.SetValue, field.FieldType);
			foreach (var prop in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
				if (prop.CanWrite)
					yield return new WrittableMember(prop, prop.SetValue, prop.PropertyType);
		}

		public CommandLineConfiguration Create(Type t)
		{
			var settableMembers = GetWrittableMembers(t);
			var arguments = settableMembers.ToImmutableDictionary(f => f, CreateArgument);
			CheckArguments(arguments);

			var argumentLookup = CreateLookupTable(arguments.Values);
			object Factory(IDictionary<AbstractArgument, object?> values)
			{
				var result = Activator.CreateInstance(t).ThrowIfNull();
				foreach (var arg in arguments)
					arg.Key.Write(result, values[arg.Value]);
				return result;
			}
			return new CommandLineConfiguration(argumentLookup, Factory);
		}

		private void CheckArguments(ImmutableDictionary<WrittableMember, AbstractArgument> arguments)
		{
			// Check unique ordering of free arguments
			var usedIndices = new HashSet<int>();
			AbstractArgument? longArg = null;
			var lastIndex = int.MinValue;
			foreach (var freeArg in arguments.Values.Where(arg => arg.IsFree))
			{
				if (!usedIndices.Add(freeArg.FreeIndex!.Value))
					throw new ArgumentException($"Free index {freeArg.FreeIndex!.Value} was used multiple times.");
				if (freeArg.Arity == Arity.ZeroOrMany)
				{
					if(longArg != null)
						throw new ArgumentException($"Multiple free enumerables.");
					longArg = freeArg;
				}
				lastIndex = Math.Max(lastIndex, freeArg.FreeIndex!.Value);
			}
			if (longArg != null && lastIndex != longArg.FreeIndex.Value)
				throw new ArgumentException($"The enumerable free argument must be the last free argument.");
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

			var defaultAttribute = memberInfo.GetCustomAttribute<CmdOptionDefaultAttribute>();
			bool isOptional;
			object? defaultValue;
			if (defaultAttribute != null)
			{
				isOptional = true;
				defaultValue = defaultAttribute?.DefaultValue;
				if (defaultValue != null && !memberInfo.Type.IsInstanceOfType(defaultValue))
					throw new ArgumentException("Wrong default type");
			}
			else
			{
				isOptional = false;
				defaultValue = null;
			}

			int? freeIndex = memberInfo.GetCustomAttribute<CmdFreeAttribute>()?.Index;

			if (memberInfo.Type == typeof(bool))
			{
				if(!isOptional || (isOptional && defaultValue != null))
					return new Option(name, shortName, defaultValue ?? false);
			}

			var (elemType, arity) = FlattenEnumerable(memberInfo.Type);
			if (UnaryConverters.TryGetValue(elemType, out var converter))
				return new UnaryArgument(isOptional, defaultValue, name, shortName, freeIndex, arity, elemType, converter);
			else
				throw new ArgumentException($"Unsupported type {elemType}.");
		}

		private (Type elemType, Arity arity) FlattenEnumerable(Type type)
		{
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				return (type.GetGenericArguments().Single(), Arity.ZeroOrMany);
			else
				return (type, Arity.OneOrZero);
		}
	}
}

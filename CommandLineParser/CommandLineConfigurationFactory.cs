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
					if (longArg != null)
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
			string name;
			string? shortName;
			UnpackName(memberInfo, out name, out shortName);

			bool isNullable;
			Type elemType;
			Arity arity;
			UnpackMonads(memberInfo, out isNullable, out elemType, out arity);

			var optionalSettings = UnpackDefaults(memberInfo, isNullable, elemType);

			int? freeIndex = UnpackFrees(memberInfo);

			return BuildArgument(name, shortName, elemType, arity, optionalSettings, freeIndex);
		}

		private static AbstractArgument BuildArgument(string name, string? shortName, Type elemType, Arity arity, OptionalSettings optionalSettings, int? freeIndex)
		{
			if (elemType == typeof(bool) && arity == Arity.OneOrZero)
			{
				if(!optionalSettings.IsOptional)
					return new Option(name, shortName, false);
				if(optionalSettings.GetDefaultValue(out var defaultValue) && defaultValue != null)
					return new Option(name, shortName, (bool)defaultValue);
			}

			if (UnaryConverters.TryGetValue(elemType, out var converter))
				return new UnaryArgument(optionalSettings, name, shortName, freeIndex, arity, elemType, converter);
			else
				throw new ArgumentException($"Unsupported type {elemType}.");
		}

		private static int? UnpackFrees(WrittableMember memberInfo)
		{
			return memberInfo.GetCustomAttribute<CmdFreeAttribute>()?.Index;
		}

		private static OptionalSettings UnpackDefaults(WrittableMember memberInfo, bool isNullable, Type elemType)
		{
			var defaultAttribute = memberInfo.GetCustomAttribute<CmdOptionDefaultAttribute>();
			if (defaultAttribute != null)
			{
				var defaultValue = defaultAttribute?.DefaultValue;
				if (defaultValue == null)
				{
					if (!isNullable)
						throw new ArgumentException($"Cannot use null default value for non nullable type '{elemType}'.");
				}
				else
				{
					if (!elemType.IsInstanceOfType(defaultValue))
						throw new ArgumentException($"Wrong default value '{defaultValue}' for type '{elemType}'");
				}

				return OptionalSettings.Optional(defaultValue);
			}
			else
			{
				return OptionalSettings.Excepted;
			}
		}

		private static void UnpackMonads(WrittableMember memberInfo, out bool isNullable, out Type elemType, out Arity arity)
		{
			elemType = memberInfo.Type;
			if (elemType.UnpackSingleGeneric(typeof(Nullable<>)) is Type baseType)
			{
				elemType = baseType;
				isNullable = true;
			}
			else
			{
				isNullable = !elemType.IsValueType;
			}

			if (elemType.UnpackSingleGeneric(typeof(IEnumerable<>)) is Type elemType2)
			{
				elemType = elemType2;
				arity = Arity.ZeroOrMany;
			}
			else
			{
				arity = Arity.OneOrZero;
			}
		}

		private static void UnpackName(WrittableMember memberInfo, out string name, out string? shortName)
		{
			var nameAttribute = memberInfo.GetCustomAttribute<CmdNameAttribute>();
			name = nameAttribute?.Name ?? memberInfo.Name;
			shortName = nameAttribute?.ShortName;
		}
	}
}

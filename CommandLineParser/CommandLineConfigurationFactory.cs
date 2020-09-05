using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace CmdParse
{
	public sealed class CommandLineConfigurationFactory
	{
		private static readonly ImmutableDictionary<Type, UnaryArgumentParser> Parsers = new[] {
				UnaryArgumentParser.Bool,
				UnaryArgumentParser.Int,
				UnaryArgumentParser.Double,
				UnaryArgumentParser.String,
				UnaryArgumentParser.FileInfo,
				UnaryArgumentParser.DirectoryInfo,
			}.ToImmutableDictionary(t => t.ResultType);

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
			object Factory(IDictionary<Argument, object?> values)
			{
				var result = Activator.CreateInstance(t).ThrowIfNull();
				foreach (var arg in arguments)
					arg.Key.Write(result, values[arg.Value]);
				return result;
			}
			return new CommandLineConfiguration(argumentLookup, Factory);
		}

		private (int FreeIndex, Arity Arity)? TryGetFreeArity(Argument argument)
			=> argument.FreeIndex is int idx ? (idx, argument.Arity) : default((int, Arity)?);
		private void CheckArguments(ImmutableDictionary<WrittableMember, Argument> arguments)
		{
			// Check unique ordering of free arguments
			var usedIndices = new HashSet<int>();
			int? firstLongArgIndex = null;
			var lastIndex = int.MinValue;
			foreach (var freeArg in arguments.Values.Select(TryGetFreeArity).WhereNotNull())
			{
				var indexValue = freeArg.FreeIndex;
				if (!usedIndices.Add(indexValue))
					throw new ArgumentException($"Free index {indexValue} was used multiple times.");
				if (freeArg.Arity == Arity.ZeroOrMany)
				{
					if (firstLongArgIndex != null)
						throw new ArgumentException($"Multiple free enumerables.");
					else
						firstLongArgIndex = indexValue;
				}
				lastIndex = Math.Max(lastIndex, indexValue);
			}
			if (firstLongArgIndex != null && lastIndex != firstLongArgIndex.Value)
				throw new ArgumentException($"The enumerable free argument must be the last free argument.");
		}

		private ImmutableDictionary<string, Argument> CreateLookupTable(IEnumerable<Argument> arguments)
		{
			var argumentLookup = new Dictionary<string, Argument>();
			foreach (var arg in arguments)
			{
				argumentLookup.Add("--" + arg.Name, arg);
				if (arg.ShortName is string shortName)
					argumentLookup.Add("-" + shortName, arg);
			}

			return argumentLookup.ToImmutableDictionary();
		}

		private Argument CreateArgument(WrittableMember memberInfo)
		{
			UnpackName(memberInfo, out var name, out var shortName);
			UnpackMonads(memberInfo, out var isNullable, out var elemType, out var arity);
			var optionalSettings = UnpackDefaults(memberInfo, isNullable, elemType);
			int? freeIndex = UnpackFrees(memberInfo);
			return BuildArgument(name, shortName, elemType, arity, optionalSettings, freeIndex);
		}

		private static Argument BuildArgument(string name, string? shortName, Type elemType, Arity arity, OptionalSettings optionalSettings, int? freeIndex)
		{
			if (elemType == typeof(bool) && arity == Arity.OneOrZero)
			{
				if (!optionalSettings.IsOptional)
				{
					var parser1 = new NullaryArgumentParser<bool>(true);
					var optionalSettings1 = OptionalSettings.Optional(false);
					return new Argument(
						optionalSettings1, name, shortName, null, Arity.OneOrZero, parser1);
				}
				if (optionalSettings.GetDefaultValue(out var defaultValue) && defaultValue != null)
				{
					var parser1 = new NullaryArgumentParser<bool>(!(bool)defaultValue);
					var optionalSettings1 = OptionalSettings.Optional(!parser1.Value);
					return new Argument(
						optionalSettings1, name, shortName, null, Arity.OneOrZero, parser1);
				}
			}

			if (Parsers.TryGetValue(elemType, out var parser))
				return new Argument(optionalSettings, name, shortName, freeIndex, arity, parser);
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

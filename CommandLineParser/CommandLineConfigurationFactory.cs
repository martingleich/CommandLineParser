using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace CmdParse
{
	public sealed class CommandLineConfigurationFactory
	{
		private static readonly ImmutableDictionary<Type, IArgumentParser> Parsers = new IArgumentParser[] {
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

		public CommandLineConfiguration<T> Create<T>() where T : new()
		{
			var settableMembers = GetWrittableMembers(typeof(T));
			var arguments = settableMembers.ToImmutableDictionary(f => f, CreateArgument);
			CheckArguments(arguments);

			T Factory(IDictionary<Argument, object?> values)
			{
				var result = new T();
				foreach (var arg in arguments)
					arg.Key.Write(result, values[arg.Value]);
				return result;
			}
			var argumentLookup = CreateLookupTable(arguments.Values);
			UnpackProgramDescription(typeof(T), out var programName, out var description);
			return new CommandLineConfiguration<T>(programName, description, argumentLookup, Factory);
		}

		private (int FreeIndex, Arity Arity)? TryGetFreeArity(Argument argument)
			=> argument.FreeIndex is int idx ? (idx, argument.AritySettings.Arity) : default((int, Arity)?);
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
			var (name, shortName) = UnpackName(memberInfo);
			UnpackMonads(memberInfo, out var isNullable, out var elemType, out var arity);
			var optionalSettings = UnpackDefaults(memberInfo, isNullable, arity, elemType);
			var freeIndex = UnpackFrees(memberInfo);
			var description = UnpackArgumentDescription(memberInfo);
			return BuildArgument(name, shortName, elemType, optionalSettings, freeIndex, description);
		}

		private static Argument? TryMapOption(string name, string? shortName, Type elemType, AritySettings aritySettings, string? description)
		{
			if (elemType != typeof(bool))
				return null;
			
			if (aritySettings.Arity == Arity.One)
			{
				var parser = new NullaryArgumentParser<bool>(true);
				var aritySettings1 = AritySettings.Optional(false);
				return new Argument(description, aritySettings1, name, shortName, null, parser);
			}
			if (aritySettings.Arity == Arity.ZeroOrOne && aritySettings.GetDefaultValue(out var defaultValue) && defaultValue != null)
			{
				var parser = new NullaryArgumentParser<bool>(!(bool)defaultValue);
				var aritySettings1 = AritySettings.Optional(!parser.Value);
				return new Argument(description, aritySettings1, name, shortName, null, parser);
			}

			return null;
		}
		private static Argument BuildArgument(string name, string? shortName, Type elemType, AritySettings aritySettings, int? freeIndex, string? description)
		{
			if(TryMapOption(name, shortName, elemType, aritySettings, description) is Argument optionArgument)
				return optionArgument;
			else if (Parsers.TryGetValue(elemType, out var parser))
				return new Argument(description, aritySettings, name, shortName, freeIndex, parser);
			else
				throw new ArgumentException($"Unsupported type {elemType}.");
		}

		private static void UnpackProgramDescription(Type t, out string programName, out string? description)
		{
			var attribute = t.GetCustomAttribute<CmdProgramDescriptionAttribute>();
			programName = attribute?.Name ?? System.Diagnostics.Process.GetCurrentProcess().ProcessName;
			description = attribute?.Description;
		}
		private static string? UnpackArgumentDescription(WrittableMember m)
		{
			var attribute = m.GetCustomAttribute<CmdArgumentDescriptionAttribute>();
			return attribute?.Description;
		}

		private static int? UnpackFrees(WrittableMember memberInfo)
		{
			return memberInfo.GetCustomAttribute<CmdFreeAttribute>()?.Index;
		}

		private static AritySettings UnpackDefaults(WrittableMember memberInfo, bool isNullable, Arity arity, Type elemType)
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

				return AritySettings.Optional(defaultValue);
			}
			else
			{
				if (arity == Arity.ZeroOrMany)
					return AritySettings.Many(Helpers.CreateEmptyEnumerable(elemType));
				else
					return AritySettings.Expected;
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
				arity = Arity.One;
			}
		}

		private static (string name, string? shortName) UnpackName(WrittableMember memberInfo)
		{
			var nameAttribute = memberInfo.GetCustomAttribute<CmdNameAttribute>();
			return (
				 name: nameAttribute?.Name ?? memberInfo.Name,
				 shortName: nameAttribute?.ShortName);
		}
	}
}

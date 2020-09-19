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
		public static readonly Argument HelpArgument = Argument.Option("Show this help page.", "help", "h", true);

		private class WrittableMember
		{
			public MemberInfo Member { get; }
			public Action<object?, object?> WriteFunc { get; }
			public void Write(object target, object? value) => WriteFunc(target, value);
			public Type Type { get; }
			public string Name => Member.Name;
			public T? GetCustomAttribute<T>() where T : Attribute => Member.GetCustomAttribute<T>();

			public WrittableMember(MemberInfo member, Action<object?, object?> write, Type type)
			{
				Member = member;
				WriteFunc = write;
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
			var argumentMap = settableMembers.ToImmutableDictionary(f => f, CreateArgument);
			var arguments = argumentMap.Values.Append(HelpArgument);
			CheckArguments(arguments);

			T Factory(IDictionary<Argument, object?> values)
			{
				var result = new T();
				foreach (var arg in argumentMap)
					arg.Key.Write(result, values[arg.Value]);
				return result;
			}
			var argumentLookup = CreateLookupTable(arguments);
			UnpackProgramDescription(typeof(T), out var programName, out var description);
			return new CommandLineConfiguration<T>(programName, description, argumentLookup, Factory);
		}

		private (int FreeIndex, bool Many)? TryGetFreeArity(Argument argument)
			=> argument.FreeIndex is int idx ? (idx, argument.AritySettings.IsMany) : default((int, bool)?);
		private void CheckArguments(IEnumerable<Argument> arguments)
		{
			// Check unique ordering of free arguments
			var usedIndices = new HashSet<int>();
			int? firstLongArgIndex = null;
			var lastIndex = int.MinValue;
			foreach (var (FreeIndex, Many) in arguments.Select(TryGetFreeArity).WhereNotNull())
			{
				if (!usedIndices.Add(FreeIndex))
					throw new ArgumentException($"Free index {FreeIndex} was used multiple times.");
				if (Many)
				{
					if (firstLongArgIndex != null)
						throw new ArgumentException($"Multiple free enumerables.");
					else
						firstLongArgIndex = FreeIndex;
				}
				lastIndex = Math.Max(lastIndex, FreeIndex);
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
			var elemType = memberInfo.Type;
			var isNullable = UnpackNullable(ref elemType);
			var arity = UnpackArity(memberInfo, ref elemType);
			var aritySettings = UnpackDefaults(memberInfo, isNullable, arity, elemType);
			var freeIndex = UnpackFrees(memberInfo);
			var description = UnpackArgumentDescription(memberInfo);

			if (TryGetOptionDefaultValue(elemType, aritySettings) is bool optionDefaultValue)
				return Argument.Option(description, name, shortName, optionDefaultValue);
			else if (Parsers.TryGetValue(elemType, out var parser))
				return new Argument(description, aritySettings, name, shortName, freeIndex, parser);
			else
				throw new ArgumentException($"Unsupported type {elemType}.");
		}

		private static bool? TryGetOptionDefaultValue(Type elemType, AritySettings aritySettings )
		{
			if (elemType != typeof(bool) || aritySettings.IsMany)
				return null;

			if (aritySettings.GetDefaultValue(out var defaultValue))
				return !(bool?)defaultValue;
			else
				return true;
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
			var defaultAttribute = memberInfo.GetCustomAttribute<CmdDefaultAttribute>();
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
				if(arity == Arity.OneOrMany)
					return AritySettings.OneOrMany();
				else if (arity == Arity.ZeroOrMany)
					return AritySettings.ZeroOrMany(Helpers.CreateEmptyEnumerable(elemType));
				else
					return AritySettings.Expected;
			}
		}

		private static Arity UnpackArity(WrittableMember info, ref Type elemType)
		{
			if (elemType.UnpackSingleGeneric(typeof(IEnumerable<>)) is Type baseType)
			{
				elemType = baseType;
				if (info.GetCustomAttribute<CmdAtLeastOneAttribute>() != null)
					return Arity.OneOrMany;
				else
					return Arity.ZeroOrMany;
			}
			else
			{
				return Arity.One;
			}
		}

		private static bool UnpackNullable(ref Type elemType)
		{
			if (elemType.UnpackSingleGeneric(typeof(Nullable<>)) is Type baseType)
			{
				elemType = baseType;
				return true;
			}
			else
			{
				return !elemType.IsValueType;
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

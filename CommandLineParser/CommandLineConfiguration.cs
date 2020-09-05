using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace CmdParse
{
	public sealed class CommandLineConfiguration
	{
		public CommandLineConfiguration(
			ImmutableDictionary<string, Argument> argumentLookup,
			Func<IDictionary<Argument, object?>, object> resultFactory)
		{
			ResultFactory = resultFactory;
			ArgumentLookup = argumentLookup;
		}

		private Func<IDictionary<Argument, object?>, object> ResultFactory { get; }
		private ImmutableDictionary<string, Argument> ArgumentLookup { get; }
		public IEnumerable<Argument> Arguments => ArgumentLookup.Values;
		public IEnumerable<Argument> FreeArguments => Arguments.Where(arg => arg.IsFree).OrderBy(arg => arg.FreeIndex);

		private Argument? FindArgument(string arg, ICollection<Argument> readArguments, out int argLength)
		{
			ArgumentLookup.TryGetValue(arg, out var matchedArg);
			if (matchedArg != null)
			{
				argLength = 1;
				return matchedArg;
			}
			argLength = 0;
			return FreeArguments.FirstOrDefault(freeArg => !readArguments.Contains(freeArg) || freeArg.Arity == Arity.ZeroOrMany);
		}
		private static IList CreateList(Type type)
			=> (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(new[] { type })).ThrowIfNull();

		public ErrorOr<T> Parse<T>(string[] args)
		{
			var values = new Dictionary<Argument, object?>();
			for (int i = 0; i < args.Length; ++i)
			{
				var arg = args[i];
				if (FindArgument(arg, values.Keys, out var argLength) is Argument matchedArg)
				{
					var parseResult = matchedArg.Parse(args.Skip(i + argLength));
					if (parseResult.MaybeError is string error)
						return error;
					var (count, value) = parseResult.Value;
					if (matchedArg.Arity != Arity.OneOrZero)
					{
						if (!values.TryGetValue(matchedArg, out object? list) || list == null)
						{
							list = CreateList(matchedArg.ResultType);
							values.Add(matchedArg, list);
						}
						((IList)list).Add(value);
					}
					else if (!values.TryAdd(matchedArg, value))
						return $"Duplicate option '{arg}'.";
					i += count - 1 + argLength;
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
					if (arg.OptionalSettings.GetDefaultValue(out var defaultValue))
						values.Add(arg, defaultValue);
					else if (arg.Arity == Arity.ZeroOrMany)
						values.Add(arg, CreateList(arg.ResultType));
					else
						return $"Missing mandatory argument '--{arg.Name}'.";
				}
			}

			return ErrorOr.FromValue((T)ResultFactory(values));
		}
	}
}

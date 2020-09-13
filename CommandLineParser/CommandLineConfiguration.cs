using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace CmdParse
{
	public sealed class CommandLineConfiguration<T>
	{
		public CommandLineConfiguration(
			string programName,
			string? description,
			ImmutableDictionary<string, Argument> argumentLookup,
			Func<IDictionary<Argument, object?>, T> resultFactory)
		{
			ProgramName = programName;
			Description = description;
			ResultFactory = resultFactory;
			OrderedFreeArguments = argumentLookup.Values.Where(arg => arg.IsFree).OrderBy(arg => arg.FreeIndex).ToImmutableArray();
			ArgumentLookup = argumentLookup;
		}

		public string ProgramName { get; }
		public string? Description { get; }
		public Func<IDictionary<Argument, object?>, T> ResultFactory { get; }
		public ImmutableDictionary<string, Argument> ArgumentLookup { get; }
		public ImmutableArray<Argument> OrderedFreeArguments { get; }
		public IEnumerable<Argument> OrderedMandatoryArguments => Arguments.Where(arg => arg.AritySettings.IsMandatory).OrderBy(arg => arg.FreeIndex).ThenBy(arg => arg.Name);
		public IEnumerable<Argument> Arguments => ArgumentLookup.Values;

		private Argument? FindArgument(string arg, ICollection<Argument> readArguments, out int argLength)
		{
			if (ArgumentLookup.TryGetValue(arg, out var matchedArg))
			{
				argLength = 1;
				return matchedArg;
			}
			else
			{
				argLength = 0;
				return OrderedFreeArguments.FirstOrDefault(freeArg => !readArguments.Contains(freeArg) || freeArg.AritySettings.Arity == Arity.ZeroOrMany);
			}
		}

		public ErrorOr<T> Parse(string[] args)
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
					if (matchedArg.AritySettings.Arity == Arity.ZeroOrMany)
					{
						if (!values.TryGetValue(matchedArg, out object? list) || list == null)
						{
							list = Helpers.CreateList(matchedArg.ResultType);
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
					if (arg.AritySettings.GetDefaultValue(out var defaultValue))
						values.Add(arg, defaultValue);
					else
						return $"Missing mandatory argument '--{arg.Name}'.";
				}
			}

			return ErrorOr.FromValue(ResultFactory(values));
		}
	}
}

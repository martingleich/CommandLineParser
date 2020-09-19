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
		public IEnumerable<Argument> Arguments => ArgumentLookup.Values.Distinct();

		private Argument? FindArgument(string arg, ICollection<Argument> readArguments, out int argLength)
		{
			if (IsParamaterString(arg))
			{
				if (ArgumentLookup.TryGetValue(arg, out var matchedArg))
				{
					argLength = 1;
					return matchedArg;
				}
				else
				{
					argLength = 0;
					return null;
				}
			}
			else
			{
				argLength = 0;
				return OrderedFreeArguments.FirstOrDefault(freeArg => !readArguments.Contains(freeArg) || freeArg.AritySettings.IsMany);
			}
		}

		private static bool IsParamaterString(string arg)
		{
			return arg.StartsWith("--") || arg.StartsWith("-");
		}

		public ErrorOr<T> Parse(string[] args)
		{
			var values = new Dictionary<Argument, object?>();
			for (int i = 0; i < args.Length; ++i)
			{
				var arg = args[i];
				if (FindArgument(arg, values.Keys, out var argLength) is Argument matchedArg)
				{
					if (matchedArg == CommandLineConfigurationFactory.HelpArgument)
						return ImmutableArray<Error>.Empty;
					var parseResult = matchedArg.Parse(args.Skip(i + argLength));
					if (parseResult.MaybeError is ImmutableArray<Error> errors)
						return errors;
					var (count, value) = parseResult.Value;
					if (matchedArg.AritySettings.IsMany)
					{
						if (!values.TryGetValue(matchedArg, out object? list) || list == null)
						{
							list = Helpers.CreateList(matchedArg.ResultType);
							values.Add(matchedArg, list);
						}
						((IList)list).Add(value);
					}
					else if (!values.TryAdd(matchedArg, value))
						return new Error(ErrorId.DuplicateArgument, matchedArg.Name);
					i += count - 1 + argLength;
				}
				else
				{
					return new Error(ErrorId.UnknownOption, arg);
				}
			}
			foreach (var arg in Arguments)
			{
				if (!values.ContainsKey(arg))
				{
					if (arg.AritySettings.GetDefaultValue(out var defaultValue))
						values.Add(arg, defaultValue);
					else
						return new Error(ErrorId.MissingMandatoryArgument, arg.Name);
				}
			}

			return ErrorOr.FromValue(ResultFactory(values));
		}
	}
}

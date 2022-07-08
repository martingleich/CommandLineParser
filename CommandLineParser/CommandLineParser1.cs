using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace CmdParse
{
	public sealed class CommandLineParser<T>
	{
		internal CommandLineParser(
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

		internal string ProgramName { get; }
		internal string? Description { get; }
		internal Func<IDictionary<Argument, object?>, T> ResultFactory { get; }
		internal ImmutableDictionary<string, Argument> ArgumentLookup { get; }
		internal ImmutableArray<Argument> OrderedFreeArguments { get; }
		internal IEnumerable<Argument> OrderedMandatoryArguments => Arguments.Where(arg => arg.AritySettings.IsMandatory).OrderBy(arg => arg.FreeIndex).ThenBy(arg => arg.Name);
		internal IEnumerable<Argument> Arguments => ArgumentLookup.Values.Distinct();

		public ErrorOr<T> Parse(params string[] args)
		{
			var values = new Dictionary<Argument, object?>();
			var argsStream = ParameterStream.Create(args);
            while(argsStream.TryTake(out argsStream) is string arg)
			{
				if (FindArgument(arg, values.Keys, out var argLength) is Argument matchedArg)
				{
					if (matchedArg == CommandLineParserFactory.HelpArgument)
						return ImmutableArray<Error>.Empty;
					argsStream = argsStream.Skip(argLength);
					var parseResult = matchedArg.Parser.Parse(argsStream);
					if (parseResult.MaybeError is ImmutableArray<Error> errors)
						return errors;
					var (remainderStream, value) = parseResult.Value;
					argsStream = remainderStream;
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
				}
				else
				{
					return new Error(ErrorId.UnknownOption, arg);
				}
			}
			var errorsList = new List<Error>();
			foreach (var arg in Arguments)
			{
				if (!values.ContainsKey(arg))
				{
					if (arg.AritySettings.GetDefaultValue(out var defaultValue))
						values.Add(arg, defaultValue);
					else
						errorsList.Add(new Error(ErrorId.MissingMandatoryArgument, arg.Name));
				}
			}
			if (errorsList.Any())
				return errorsList.ToImmutableArray();

			return ErrorOr.FromValue(ResultFactory(values));
		}
	
		private Argument? FindArgument(string arg, ICollection<Argument> readArguments, out int argLength)
		{
			if (IsParameterString(arg))
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

		private static bool IsParameterString(string arg) => arg.StartsWith("-");
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace CmdParse
{

	public class CommandLineConfiguration
	{
		public CommandLineConfiguration(
			ImmutableDictionary<string, AbstractArgument> argumentLookup,
			Func<IDictionary<AbstractArgument, object?>, object> resultFactory)
		{
			ResultFactory = resultFactory;
			ArgumentLookup = argumentLookup;
		}

		public Func<IDictionary<AbstractArgument, object?>, object> ResultFactory { get; }
		public ImmutableDictionary<string, AbstractArgument> ArgumentLookup { get; }
		public IEnumerable<AbstractArgument> Arguments => ArgumentLookup.Values;

		public AbstractArgument? FindArgument(string arg)
		{
			ArgumentLookup.TryGetValue(arg, out var matchedArg);
			return matchedArg;
		}
		public ErrorOr<T> Parse<T>(string[] args)
		{
			var values = new Dictionary<AbstractArgument, object?>();
			for (int i = 0; i < args.Length; ++i)
			{
				var arg = args[i];
				if (FindArgument(arg) is AbstractArgument matchedArg)
				{
					var parseResult = matchedArg.Parse(args.Skip(i + 1));
					if (parseResult.MaybeError is string error)
						return error;
					var (count, value) = parseResult.Value;
					if (matchedArg.Arity != Arity.OneOrZero)
					{
						if (!values.TryGetValue(matchedArg, out object? list) || list == null)
						{
							list = matchedArg.CreateList();
							values.Add(matchedArg, list);
						}
						((IList)list).Add(value);
					}
					else if (!values.TryAdd(matchedArg, value))
						return $"Duplicate option '{arg}'.";
					i += count;
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
					if (arg.DefaultValue is object defaultValue)
						values.Add(arg, defaultValue);
					else if (arg.Arity == Arity.ZeroOrMany)
						values.Add(arg, arg.CreateList());
					else
						return $"Missing mandatory argument '--{arg.Name}'.";
				}
			}

			return ErrorOr.FromValue((T)ResultFactory(values));
		}
	}
}

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace CmdParse
{
	public class HelpPrinter
	{
		public string PrintHelp<TResult>(CommandLineConfiguration<TResult> config)
		{
			var sb = new StringBuilder();
			if (config.Description is string description)
			{
				sb.AppendLine(description);
				sb.AppendLine();
			}

			bool isFirst = true;
			foreach (var arg in config.OrderedMandatoryArguments)
			{
				if (!isFirst)
				{
					sb.Append(" ");
				}
				else
				{
					sb.Append(config.ProgramName);
					sb.Append(" ");
					isFirst = false;
				}
				sb.Append("--" + arg.Name);
				if (arg.Parser.HumanReadableSyntaxDescription.Length > 0)
					sb.Append($" <{arg.Parser.HumanReadableSyntaxDescription}>");
				sb.Append(arg.AritySettings.Accept(
					one: () => "",
					zeroOrMany: _ => "[]",
					zeroOrOne: _ => "?"));
			}

			sb.AppendLine();
			sb.AppendLine();

			PrintTable(sb, config.Arguments.OrderBy(a => a.Name), new Func<Argument, string>[] { NameCollumn, DescriptionCollumn });
			return sb.ToString();
		}

		private string NameCollumn(Argument arg)
		{
			string result = "--" + arg.Name;
			if (arg.ShortName is string shortName)
				result += " / -" + shortName;
			if (arg.Parser.HumanReadableSyntaxDescription.Length > 0)
				result += $" : {arg.Parser.HumanReadableSyntaxDescription}";
			if (!arg.AritySettings.IsMany && !(arg.Parser is NullaryArgumentParser<bool>) && arg.AritySettings.GetDefaultValue(out var defaultValue))
				result += " = " + (defaultValue?.ToString() ?? "<null>");
			return result;
		}
		private string DescriptionCollumn(Argument arg)
			=> arg.Description ?? "";

		private void PrintTable<T1>(StringBuilder sb, IEnumerable<T1> values, params Func<T1, string>[] collumns)
		{
			var rows = values.Select(v => collumns.Select(f => f(v)).ToImmutableArray());
			var collumnWidths = rows.Select(r => r.Select(r => r.Length)).Aggregate((a, b) => a.Zip(b, Math.Max)).ToImmutableArray();
			bool isFirst = true;
			foreach (var row in rows)
			{
				if (!isFirst)
				{
					sb.AppendLine();
				}
				else
				{
					isFirst = false;
				}
				int i = 0;
				int colStart = 2;
				foreach (var col in row)
				{
					var collumnLines = col.Split('\n').Select(l => l.Trim());
					var maxCollumLength = int.MinValue;
					bool isFirstLine = true;
					foreach (var line in collumnLines)
					{
						if (isFirstLine)
						{
							sb.Append("  ");
							isFirstLine = false;
						}
						else
						{
							sb.AppendLine();
							sb.Append(' ', colStart);
						}
						sb.Append(line);
						maxCollumLength = Math.Max(maxCollumLength, line.Length);
					}
					if (i < row.Length - 1)
						sb.Append(' ', collumnWidths[i] - col.Length);
					colStart += 2 + collumnWidths[i];
					++i;
				}
			}
		}
	}
}

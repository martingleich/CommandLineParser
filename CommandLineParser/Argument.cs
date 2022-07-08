using System;

namespace CmdParse
{
	/// <summary>
	/// A single command line arguments.
	/// </summary>
	internal sealed class Argument
	{
		/// <summary>
		/// Shortcut to create a option, i.e. an argument without any parameters that is either present in the command line or not.
		/// </summary>
		/// <param name="description">The optional description of the option.</param>
		/// <param name="name">The long-name of the option</param>
		/// <param name="shortName">The optional short-name of the option</param>
		/// <param name="valueIfPassed">Should value the coresponding member is set to if the option is passed, if the option is not passed the member is set to !valueIfPassed.</param>
		/// <returns>The newly created option</returns>
		public static Argument Option(string? description, string name, string? shortName, bool valueIfPassed)
			=> new Argument(description, AritySettings.Optional(!valueIfPassed), name, shortName, null, new NullaryArgumentParser<bool>(valueIfPassed, ""));

		public Argument(
			string? description,
			AritySettings aritySettings,
			string name,
			string? shortName,
			int? freeIndex,
			IArgumentParser parser)
		{
			Description = description;
			AritySettings = aritySettings;
			Name = name;
			ShortName = shortName;
			FreeIndex = freeIndex;
			Parser = parser;
		}
		public string? Description { get; }

		public AritySettings AritySettings { get; }
		public string Name { get; }
		public string? ShortName { get; }
		public int? FreeIndex { get; }
		public bool IsFree => FreeIndex.HasValue;
		public Type ResultType => Parser.ResultType;
		public IArgumentParser Parser { get; }

		public override string ToString()
		{
			var result = $"{Name} : {ResultType.Name}{AritySettings.PostfixString}";
			if (AritySettings.GetDefaultValue(out var defaultValue))
				result += " = " + defaultValue?.ToString();
			return result;
		}
	}
}

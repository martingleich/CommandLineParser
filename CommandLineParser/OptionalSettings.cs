namespace CmdParse
{
	public sealed class OptionalSettings
	{
		public static readonly OptionalSettings Excepted = new OptionalSettings(false, null);
		public static OptionalSettings Optional(object? defaultValue)
			=> new OptionalSettings(true, defaultValue);
		private OptionalSettings(bool isOptional, object? defaultValue)
		{
			IsOptional = isOptional;
			DefaultValue = defaultValue;
		}

		public bool IsOptional { get; }
		private object? DefaultValue { get; }

		public bool GetDefaultValue(out object? value)
		{
			value = DefaultValue;
			return IsOptional;
		}
	}
}

namespace CmdParse
{
	public class AritySettings
	{
		public static AritySettings Expected => new AritySettings(Arity.One, default);
		public static AritySettings Optional(object? defaultValue) => new AritySettings(Arity.ZeroOrOne, defaultValue);
		public static AritySettings Many(object? defaultValue) => new AritySettings(Arity.ZeroOrMany, defaultValue);

		private AritySettings(Arity arity, object? @default)
		{
			Arity = arity;
			Default = @default;
		}

		public Arity Arity { get; }
		private object? Default { get; }

		public bool GetDefaultValue(out object? value)
		{
			if (Arity == Arity.ZeroOrMany || Arity == Arity.ZeroOrOne)
			{
				value = Default;
				return true;
			}
			else
			{
				value = default;
				return false;
			}
		}
	}
}

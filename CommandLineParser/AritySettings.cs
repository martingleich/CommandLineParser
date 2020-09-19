using System;

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

		private Arity Arity { get; }
		private object? Default { get; }

		public bool IsMandatory => Arity == Arity.One;
		public bool IsMany => Arity == Arity.ZeroOrMany;
		public bool GetDefaultValue(out object? value)
		{
			if (IsMandatory)
			{
				value = default;
				return false;
			}
			else
			{
				value = Default;
				return true;
			}
		}

		public T Accept<T>(
			Func<T> one,
			Func<object?, T> zeroOrMany,
			Func<object?, T> zeroOrOne)
		{
			return Arity switch
			{
				Arity.One => one(),
				Arity.ZeroOrMany => zeroOrMany(Default),
				Arity.ZeroOrOne => zeroOrOne(Default),
				_ => throw new InvalidOperationException("Unsupported arity")
			};
		}
	}
}

using System;

namespace CmdParse
{
    internal class AritySettings
	{
		public static AritySettings Expected => new AritySettings(Arity.One, default);
		public static AritySettings Optional(object? defaultValue) => new AritySettings(Arity.ZeroOrOne, defaultValue);
		public static AritySettings ZeroOrMany(object? defaultValue) => new AritySettings(Arity.ZeroOrMany, defaultValue);
		public static AritySettings OneOrMany() => new AritySettings(Arity.OneOrMany, default);

		private AritySettings(Arity arity, object? @default)
		{
			Arity = arity;
			Default = @default;
		}

		private Arity Arity { get; }
		private object? Default { get; }

		public bool IsMandatory => Arity == Arity.One || Arity == Arity.OneOrMany;
		public bool IsMany => Arity == Arity.ZeroOrMany || Arity == Arity.OneOrMany;
		public bool IsSingle => !IsMany;

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
			Func<T> oneOrMany,
			Func<object?, T> zeroOrOne)
		{
			return Arity switch
			{
				Arity.One => one(),
				Arity.ZeroOrMany => zeroOrMany(Default),
				Arity.OneOrMany => oneOrMany(),
				Arity.ZeroOrOne => zeroOrOne(Default),
				_ => throw new InvalidOperationException("Unsupported arity")
			};
		}

		public string PostfixString =>
			Arity switch
			{
				Arity.One => "",
				Arity.ZeroOrMany => "*",
				Arity.OneOrMany => "+",
				Arity.ZeroOrOne => "?",
				_ => throw new InvalidOperationException("Unsupported arity")
			};
	}
}

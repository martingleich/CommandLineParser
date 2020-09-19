using System;

namespace CmdParse
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class CmdDefaultAttribute : Attribute
	{
		public CmdDefaultAttribute(object? defaultValue)
		{
			DefaultValue = defaultValue;
		}

		public object? DefaultValue { get; }
	}
}

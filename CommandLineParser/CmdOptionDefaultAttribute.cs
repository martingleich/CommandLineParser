using System;

namespace CmdParse
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class CmdOptionDefaultAttribute : Attribute
	{
		public CmdOptionDefaultAttribute(object defaultValue)
		{
			DefaultValue = defaultValue;
		}

		public object DefaultValue { get; }
	}
}

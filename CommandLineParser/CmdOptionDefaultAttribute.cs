using System;

namespace CmdParse
{
	public class CmdOptionDefaultAttribute : Attribute
	{
		public CmdOptionDefaultAttribute(object defaultValue)
		{
			DefaultValue = defaultValue;
		}

		public object DefaultValue { get; }
	}
}

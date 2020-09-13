using System;

namespace CmdParse
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class CmdArgumentDescriptionAttribute : Attribute
	{
		public CmdArgumentDescriptionAttribute(string description)
		{
			Description = description;
		}

		public string Description { get; set; }
	}
}
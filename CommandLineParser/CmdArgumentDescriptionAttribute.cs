using System;

namespace CmdParse
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
	public class CmdArgumentDescriptionAttribute : Attribute
	{
		public CmdArgumentDescriptionAttribute(string description)
		{
			Description = description;
		}

		public string Description { get; }
	}
}
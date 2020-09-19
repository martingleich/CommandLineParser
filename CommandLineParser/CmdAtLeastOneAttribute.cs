using System;

namespace CmdParse
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class CmdAtLeastOneAttribute : Attribute
	{
		public CmdAtLeastOneAttribute()
		{
		}
	}
}

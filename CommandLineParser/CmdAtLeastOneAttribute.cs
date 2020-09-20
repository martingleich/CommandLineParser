using System;

namespace CmdParse
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
	public sealed class CmdAtLeastOneAttribute : Attribute
	{
		public CmdAtLeastOneAttribute()
		{
		}
	}
}

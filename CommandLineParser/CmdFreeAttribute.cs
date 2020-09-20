using System;

namespace CmdParse
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
	public sealed class CmdFreeAttribute : Attribute
	{
		public CmdFreeAttribute(int index)
		{
			Index = index;
		}

		public int Index { get; }
	}
}

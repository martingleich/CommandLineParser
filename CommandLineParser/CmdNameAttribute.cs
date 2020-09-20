using System;

namespace CmdParse
{

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
	public sealed class CmdNameAttribute : Attribute
	{
		public CmdNameAttribute(string name)
		{
			Name = name;
			ShortName = null;
		}
		public CmdNameAttribute(string name, string shortName)
		{
			Name = name;
			ShortName = shortName;
		}

		public string Name { get; }
		public string? ShortName { get; }
	}
}

using System;

namespace CmdParse
{

	[AttributeUsage(AttributeTargets.Class)]
	public class CmdProgramDescriptionAttribute : Attribute
	{
		public CmdProgramDescriptionAttribute(string name)
		{
			Name = name;
		}

		public string Name { get; }
		public string? Description { get; set; }
	}
}
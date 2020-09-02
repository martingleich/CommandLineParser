using CmdParse;
using System;
using Xunit;

namespace Tests
{
	public class NameAttributeTests
	{
		class NameOptionsType
		{
			[CmdName("publicValue")]
			public int InternalValue;
			[CmdName("input", "i")]
			public int Input;
		}
		[Fact]
		public void NameOptions()
		{
			var result = CommandLineParser.Parse<NameOptionsType>(new[] { "--publicValue", "23", "-i", "17" });
			Assert.Equal(23, result.InternalValue);
			Assert.Equal(17, result.Input);
		}
		class DuplicateNameErrorType
		{
			[CmdName("Name")]
			public bool InternalValue;
			[CmdName("Name")]
			public bool Input;
		}
		[Fact]
		public void DuplicateNameError()
		{
			Assert.Throws<ArgumentException>(() => CommandLineParser.Parse<DuplicateNameErrorType>(new string[0]));
		}
	}
}

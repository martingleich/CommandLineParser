using CmdParse;
using Xunit;

namespace Tests
{
	public class IntegerParse
	{
		public class IntTestType1
		{
			public int Value1;
		}
		[Fact]
		public void IntParse()
		{
			var result = CommandLineParser.Parse<IntTestType1>(new[] { "--Value1", "435" });
			Assert.Equal(435, result.Value1);
		}
		[Fact]
		public void IntParse_MissingMandatory()
		{
			var result = CommandLineParser.ParseWithError<IntTestType1>(new string[] { });

			Assert.False(result.IsOkay);
		}
		[Fact]
		public void IntParse_MissingValue()
		{
			var result = CommandLineParser.ParseWithError<IntTestType1>(new[] { "--Value1" });
			Assert.False(result.IsOkay);
		}
		[Fact]
		public void IntParse_MissingValueNoInt()
		{
			var result = CommandLineParser.ParseWithError<IntTestType1>(new[] { "--Value1", "Hi" });
			Assert.False(result.IsOkay);
		}
		[Fact]
		public void IntParse_Duplicate()
		{
			var result = CommandLineParser.ParseWithError<IntTestType1>(new[] { "--Value1", "123", "--Value1", "456" });
			Assert.False(result.IsOkay);
		}
		public class IntTestType_WithDefault
		{
			[CmdDefault(666)]
			public int Value1;
		}
		[Fact]
		public void IntParse_WithDefault()
		{
			var result = CommandLineParser.Parse<IntTestType_WithDefault>(new string[] { });
			Assert.Equal(666, result.Value1);
		}
	}
}

using CmdParse;
using Xunit;

namespace Tests
{
	public class BooleanParseTest
	{
		public class BooleanTestType1
		{
			public bool Value1;
			public bool Value2;
			private bool HiddenValue;
		}
		[Fact]
		public void BooleanParse()
		{
			var result = CommandLineParser.Parse<BooleanTestType1>(new[] { "--Value1" });

			Assert.True(result.Value1);
			Assert.False(result.Value2);
		}
		[Fact]
		public void BooleanParse_UnknownArgument()
		{
			var result = CommandLineParser.ParseWithError<BooleanTestType1>(new[] { "--blub" });
			Assert.False(result.IsOkay);
		}
		[Fact]
		public void BooleanParse_DuplicatOption()
		{
			var result = CommandLineParser.ParseWithError<BooleanTestType1>(new[] { "--Value1", "--Value1" });
			Assert.False(result.IsOkay);
		}
		public class BooleanTestType_WithDefault
		{
			public bool Value1;
			[CmdOptionDefault(true)]

			public bool Value2;
			[CmdOptionDefault(true)]
			public bool Value3;
			private bool HiddenValue;
		}
		[Fact]
		public void BooleanParse_WithDefault()
		{
			var result = CommandLineParser.Parse<BooleanTestType_WithDefault>(new[] { "--Value1", "--Value3" });
			Assert.True(result.Value1);
			Assert.True(result.Value2);
			Assert.False(result.Value3);
		}
	}
}

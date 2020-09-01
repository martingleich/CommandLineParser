using CmdParse;
using System;
using Xunit;

namespace Tests
{
	public class Tests
	{
		public class EmptyType
		{
		}
		[Fact]
		public void EmptyArgs()
		{
			var result = CommandLineParser.Parse<EmptyType>(Array.Empty<string>());
			Assert.NotNull(result);
		}

		public class BooleanTestType1
		{
			public bool Value1;
			public bool Value2;
			private bool HiddenValue;
		}
		[Fact]
		public void BooleanConfigCreate()
		{
			var config = CommandLineConfiguration.Create(typeof(BooleanTestType1));
			Assert.Contains(config.Options, a => a.Name == "Value1");
			Assert.Contains(config.Options, a => a.Name == "Value2");
			Assert.DoesNotContain(config.Options, a => a.Name == "HiddenValue");
		}
		[Fact]
		public void BooleanParse()
		{
			var result = CommandLineParser.Parse<BooleanTestType1>(new[] { "--Value1" });
			Assert.True(result.Value1);
			Assert.False(result.Value2);
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

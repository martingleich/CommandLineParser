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
			[CmdOptionDefault(666)]
			public int Value1;
		}
		[Fact]
		public void IntParse_WithDefault()
		{
			var result = CommandLineParser.Parse<IntTestType_WithDefault>(new string[] { });
			Assert.Equal(666, result.Value1);
		}
		public class WrongDefaultType
		{
			[CmdOptionDefault(true)]
			public int Value1;
		}
		[Fact]
		public void WrongDefaultTypeTest()
		{
			Assert.Throws<InvalidOperationException>(() => CommandLineParser.Parse<WrongDefaultType>(new string[] { }));
		}

		class DoubleTypeTest
		{
			public double Value;
		}
		[Fact]
		public void ParseDouble()
		{
			var result = CommandLineParser.Parse<DoubleTypeTest>(new[] { "--Value", "3,145" });
			Assert.Equal(3.145, result.Value);
		}
	}
}

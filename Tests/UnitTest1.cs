using CmdParse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

		public class ReadonlyTestType
		{
			public readonly int Value;
		}
		[Fact]
		public void ReadonlyTest()
		{
			var config = new CommandLineConfigurationFactory().Create(typeof(ReadonlyTestType));
			Assert.DoesNotContain("Value", config.Arguments.Select(a => a.Name));
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
			Assert.Throws<ArgumentException>(() => CommandLineParser.Parse<WrongDefaultType>(new string[] { }));
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

		class ManyIntsType
		{
			public IEnumerable<int> Value;
		}
		[Fact]
		public void ParseManyInts_None()
		{
			var result = CommandLineParser.Parse<ManyIntsType>(new string[0]);
			Assert.Empty(result.Value);
		}
		[Fact]
		public void ParseManyInts_Single()
		{
			var result = CommandLineParser.Parse<ManyIntsType>(new[] { "--Value", "1" });
			Assert.Equal(new[] { 1 }, result.Value);
		}
		[Fact]
		public void ParseManyInts_Many()
		{
			var result = CommandLineParser.Parse<ManyIntsType>(new[] { "--Value", "1", "--Value", "2", "--Value", "3" });
			Assert.Equal(new[] { 1, 2, 3 }, result.Value);
		}
		class ManyBoolsType
		{
			public IEnumerable<bool> Value;
		}
		[Fact]
		public void ParseManyBooleans_Many()
		{
			var result = CommandLineParser.Parse<ManyBoolsType>(new[] { "--Value", "true", "--Value", "false", "--Value", "true" });
			Assert.Equal(new[] { true, false, true }, result.Value);
		}
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
			var result = CommandLineParser.Parse<NameOptionsType>(new[] { "--publicValue", "23", "-i", "17"});
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
		class UnsupportedTypeErrorType
		{
			public KeyValuePair<float, float> UnsupportedType;
		}
		[Fact]
		public void UnsupportedTypeError()
		{
			Assert.Throws<ArgumentException>(() => CommandLineParser.Parse<UnsupportedTypeErrorType>(new string[0]));
		}
		class PropertyType
		{
			public bool Value { get; set; }
			public bool IgnoredValue { get; }
		}
		[Fact]
		public void Property()
		{
			var config = new CommandLineConfigurationFactory().Create(typeof(PropertyType));
			Assert.Contains("Value", config.Arguments.Select(a => a.Name));
			Assert.DoesNotContain("IgnoredValue", config.Arguments.Select(a => a.Name));
		}
		class StringType
		{
			public string Value;
		}
		[Fact]
		public void String()
		{
			var result = CommandLineParser.Parse<StringType>(new[] { "--Value", "Hello"});
			Assert.Equal("Hello", result.Value);
		}
	}
}

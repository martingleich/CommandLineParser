using CmdParse;
using System;
using Xunit;

namespace Tests
{
	public class OptionalTest
	{
		class OptionalReferenceType
		{
			[CmdDefault(null)]
			public string? Value;
		}
		[Fact]
		public void OptionalReference_Available()
		{
			var result = CommandLineParser.Parse<OptionalReferenceType>(new[] { "--Value", "Hello" });
			Assert.Equal("Hello", result.Value);
		}
		[Fact]
		public void OptionalReferenceMissing()
		{
			var result = CommandLineParser.Parse<OptionalReferenceType>(new string[0]);
			Assert.Null(result.Value);
		}

		class OptionalError_NonOptionalType
		{
			[CmdDefault(null)]
			public int Value;
		}

		[Fact]
		public void OptionalError_NonOptional()
		{
			Assert.Throws<ArgumentException>(() => CommandLineParser.Parse<OptionalError_NonOptionalType>(new string[0]));
		}

		class OptionalNullableValue_Type
		{
			[CmdDefault(null)]
			public int? Value;
		}

		[Fact]
		public void OptionalValue_Available()
		{
			var result = CommandLineParser.Parse<OptionalNullableValue_Type>(new[] { "--Value", "123" });
			Assert.Equal(123, result.Value);
		}
		[Fact]
		public void OptionalValueMissing()
		{
			var result = CommandLineParser.Parse<OptionalNullableValue_Type>(new string[0]);
			Assert.Null(result.Value);
		}

		class OptionalNullableValue_WithoutDefault_Type
		{
			public int? Value;
		}
		[Fact]
		public void OptionalValueNoDefault_Available()
		{
			var result = CommandLineParser.Parse<OptionalNullableValue_WithoutDefault_Type>(new[] { "--Value", "123" });
			Assert.Equal(123, result.Value);
		}
		[Fact]
		public void OptionalValueNoDefault_Missing()
		{
			var result = CommandLineParser.ParseWithError<OptionalNullableValue_WithoutDefault_Type>(new string[0]);
			Assert.False(result.IsOkay);
		}
		class OptionalNullableValue_WithDefault_Type
		{
			[CmdDefault(456)]
			public int? Value;
		}
		[Fact]
		public void OptionalValueWithDefault_Available()
		{
			var result = CommandLineParser.Parse<OptionalNullableValue_WithDefault_Type>(new[] { "--Value", "123" });
			Assert.Equal(123, result.Value);
		}
		[Fact]
		public void OptionalValueWithDefault_Missing()
		{
			var result = CommandLineParser.Parse<OptionalNullableValue_WithDefault_Type>(new string[0]);
			Assert.Equal(456, result.Value);
		}
	}
} 

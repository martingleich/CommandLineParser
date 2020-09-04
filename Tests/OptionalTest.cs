﻿using CmdParse;
using System;
using Xunit;

namespace Tests
{
	public class OptionalTest
	{
		class OptionalReferenceType
		{
			[CmdOptionDefault(null)]
			public string? Value;
		}
		[Fact]
		public void OptionalReference_Avaiable()
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
			[CmdOptionDefault(null)]
			public int Value;
		}

		[Fact]
		public void OptionalError_NonOptional()
		{
			Assert.Throws<ArgumentException>(() => CommandLineParser.Parse<OptionalError_NonOptionalType>(new string[0]));
		}

		class OptionalNullableValue_Type
		{
			[CmdOptionDefault(null)]
			public int? Value;
		}

		[Fact]
		public void OptionalValue_Avaiable()
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

	}
} 

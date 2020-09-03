using CmdParse;
using System;
using System.Collections.Generic;
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
	}

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
	}
}

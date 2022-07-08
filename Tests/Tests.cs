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
			var config = new CommandLineParserFactory().CreateParser<ReadonlyTestType>();
			Assert.DoesNotContain("Value", config.Arguments.Select(a => a.Name));
		}

		public class WrongDefaultType

		{
			[CmdDefault(true)]
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
		public void UnsupWrongtypeportedTypeError()
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
			var config = new CommandLineParserFactory().CreateParser<PropertyType>();
			Assert.Contains("Value", config.Arguments.Select(a => a.Name));
			Assert.DoesNotContain("IgnoredValue", config.Arguments.Select(a => a.Name));
		}
		class ConstructorType
		{
			public ConstructorType(int arg, int arg2)
			{
				SynthValue = arg * arg2;
			}
			public bool arg { get; set; } // The constructor arg will overrride this
			public int MyPropertyValue { get; set; }
			public int SynthValue { get; }
		}
		[Fact]
		public void Constructor()
		{
			var config = new CommandLineParserFactory().CreateParser<ConstructorType>();
			Assert.Contains("arg", config.Arguments.Select(a => a.Name));
			Assert.Contains("arg2", config.Arguments.Select(a => a.Name));
			Assert.Contains("MyPropertyValue", config.Arguments.Select(a => a.Name));
			Assert.DoesNotContain("SynthValue", config.Arguments.Select(a => a.Name));
			var value = config.Parse("--arg2", "8", "--arg", "4", "--MyPropertyValue", "7").Value;
			Assert.Equal(32, value.SynthValue);
			Assert.Equal(7, value.MyPropertyValue);
			Assert.Equal(default(bool), value.arg);
		}
	}
}

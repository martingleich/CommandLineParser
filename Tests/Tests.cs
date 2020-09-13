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
			var config = new CommandLineConfigurationFactory().Create<ReadonlyTestType>();
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
			var config = new CommandLineConfigurationFactory().Create<PropertyType>();
			Assert.Contains("Value", config.Arguments.Select(a => a.Name));
			Assert.DoesNotContain("IgnoredValue", config.Arguments.Select(a => a.Name));
		}
	}

	public class HelpTests
	{
		[CmdProgramDescription("MyProgram", Description = @"A cool program todo things.
With multiline.")]
		class BasicType
		{
			[CmdArgumentDescription(@"A argument.
Another line of comments.")]
			public int myArgument;
			[CmdArgumentDescription("A option.")]
			public bool myOption;
		}

		[Fact]
		public void ShowBasicHelp()
		{
			var config = new CommandLineConfigurationFactory().Create<BasicType>();
			var help = new HelpPrinter().PrintHelp(config);
			Assert.Equal(@"A cool program todo things.
With multiline.

MyProgram --myArgument <Int32>

  --myArgument : Int32  A argument.
                        Another line of comments.
  --myOption            A option.", help);
		}
	}
}

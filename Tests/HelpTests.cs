using CmdParse;
using Xunit;

namespace Tests
{
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
			[CmdName("myOption", "o")]
			public bool myOption;

			[CmdOptionDefault(123.0)]
			public double optionalReal;
		}

		[Fact]
		public void ShowBasicHelp()
		{
			var config = new CommandLineConfigurationFactory().Create<BasicType>();
			var help = new HelpPrinter().PrintHelp(config);
			Assert.Equal(@"A cool program todo things.
With multiline.

MyProgram --myArgument <Int32>

  --help / -h                    Show this help page.
  --myArgument : Int32           A argument.
                                 Another line of comments.
  --myOption / -o                A option.
  --optionalReal : Double = 123  "
, help);
		}
	}
}

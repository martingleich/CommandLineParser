using CmdParse;
using Xunit;

namespace Tests
{
	public class DoubleParse
	{
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

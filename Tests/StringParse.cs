using CmdParse;
using Xunit;

namespace Tests
{
	public class StringParse
	{
		class StringType
		{
			public string Value;
		}
		[Fact]
		public void String()
		{
			var result = CommandLineParser.Parse<StringType>(new[] { "--Value", "Hello" });
			Assert.Equal("Hello", result.Value);
		}
	}
}

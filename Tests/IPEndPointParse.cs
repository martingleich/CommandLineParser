using CmdParse;
using System.Net;
using Xunit;

namespace Tests
{
	public class IPEndPointParse
	{
		class IPEndPointTypeTest
		{
			public IPEndPoint Value;
		}
		[Theory]
		[InlineData("127.0.0.1")]
		[InlineData("127.0.0.1:4840")]
		public void ParseDouble(string str)
		{
			var result = CommandLineParser.Parse<IPEndPointTypeTest>(new[] { "--Value", str });
			Assert.Equal(IPEndPoint.Parse(str), result.Value);
		}

		public class IPEndPointTypeWithDefaultTest
		{
            [CmdDefault("123.123.123")]
			public IPEndPoint Value;
		}
		[Fact]
		public void Parse()
		{
			var result = CommandLineParser.Parse<IPEndPointTypeWithDefaultTest>();
			Assert.Equal(IPEndPoint.Parse("123.123.123"), result.Value);
		}
	}
}

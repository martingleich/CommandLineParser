using CmdParse;
using Xunit;

namespace Tests
{
    public class RecordTests
	{
		public record Vector3(double X, double Y, double Z)
		{
		}

        [Fact]
        public void RecordTest()
        {
            var parser = CommandLineParser.CreateFactory().CreateParser<Vector3>();
            var vector = parser.Parse("--X", "1", "--Y", "2", "--Z", "3").Value;
            Assert.Equal(1, vector.X);
            Assert.Equal(2, vector.Y);
            Assert.Equal(3, vector.Z);
        }
		public record Vector3WithDefault(double X, [CmdDefault(777.0)] double Y, double Z)
		{
		}

        [Fact]
        public void RecordWithDefault()
        {
            var parser = CommandLineParser.CreateFactory().CreateParser<Vector3WithDefault>();
            var vector = parser.Parse("--X", "1", "--Z", "3").Value;
            Assert.Equal(1, vector.X);
            Assert.Equal(777, vector.Y);
            Assert.Equal(3, vector.Z);
        }
	}
}

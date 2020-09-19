using CmdParse;
using System.Collections.Generic;
using Xunit;

namespace Tests
{
	public class ArityTests
	{
		class ManyIntsType
		{
			public IEnumerable<int> Value;
		}
		[Fact]
		public void ParseManyInts_None()
		{
			var result = CommandLineParser.Parse<ManyIntsType>(new string[0]);
			Assert.Empty(result.Value);
		}
		[Fact]
		public void ParseManyInts_Single()
		{
			var result = CommandLineParser.Parse<ManyIntsType>(new[] { "--Value", "1" });
			Assert.Equal(new[] { 1 }, result.Value);
		}
		[Fact]
		public void ParseManyInts_Many()
		{
			var result = CommandLineParser.Parse<ManyIntsType>(new[] { "--Value", "1", "--Value", "2", "--Value", "3" });
			Assert.Equal(new[] { 1, 2, 3 }, result.Value);
		}

		class OneOrManyIntsType
		{
			[CmdAtLeastOne]
			public IEnumerable<int> Value;
		}
		[Fact]
		public void ParseOneOrManyInts_None()
		{
			var result = CommandLineParser.ParseWithError<OneOrManyIntsType>(new string[0]);
			Assert.False(result.IsOkay);
		}
		[Fact]
		public void ParseOneOrManyInts_Single()
		{
			var result = CommandLineParser.Parse<OneOrManyIntsType>(new[] { "--Value", "1" });
			Assert.Equal(new[] { 1 }, result.Value);
		}
		[Fact]
		public void ParseOneOrManyInts_Many()
		{
			var result = CommandLineParser.Parse<OneOrManyIntsType>(new[] { "--Value", "1", "--Value", "2", "--Value", "3" });
			Assert.Equal(new[] { 1, 2, 3 }, result.Value);
		}

		class ManyBoolsType
		{
			public IEnumerable<bool> Value;
		}
		[Fact]
		public void ParseManyBooleans_Many()
		{
			var result = CommandLineParser.Parse<ManyBoolsType>(new[] { "--Value", "true", "--Value", "false", "--Value", "true" });
			Assert.Equal(new[] { true, false, true }, result.Value);
		}
	}
}

using CmdParse;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests
{
	public class FreeArgumentsTests
	{
		class FreeArgumentType
		{
			[CmdFree(0)]
			public int FreeArgument;
		}
		[Fact]
		public void FreeArgument()
		{
			var parsed = CommandLineParser.Parse<FreeArgumentType>(new[] { "123" });
			Assert.Equal(123, parsed.FreeArgument);
		}
		[Fact]
		public void FreeArgument_Explicit()
		{
			var parsed = CommandLineParser.Parse<FreeArgumentType>(new[] { "--FreeArgument", "123" });
			Assert.Equal(123, parsed.FreeArgument);
		}
		[Fact]
		public void FreeArgument_Missing()
		{
			var parsed = CommandLineParser.ParseWithError<FreeArgumentType>(new string[0]);
			Assert.False(parsed.IsOkay);
		}
		class FreeArgument_Mixed_Type
		{
			[CmdFree(0)]
			public int FreeArgument;
			public int Value;
		}
		[Fact]
		public void FreeArgument_Mixed()
		{
			var parsed = CommandLineParser.Parse<FreeArgument_Mixed_Type>(new[] { "--Value", "456", "123" });
			Assert.Equal(123, parsed.FreeArgument);
			Assert.Equal(456, parsed.Value);
		}
		class FreeArgument_Multiple_Type
		{
			[CmdFree(0)]
			public int FreeArgument1;
			[CmdFree(1)]
			public int FreeArgument2;
		}
		[Fact]
		public void FreeArgument_Multiple()
		{
			var parsed = CommandLineParser.Parse<FreeArgument_Multiple_Type>(new[] { "456", "123" });
			Assert.Equal(456, parsed.FreeArgument1);
			Assert.Equal(123, parsed.FreeArgument2);
		}
		[Fact]
		public void FreeArgument_Multiple_Multiple()
		{
			var parsed = CommandLineParser.ParseWithError<FreeArgument_Multiple_Type>(new[] { "456", "--FreeArgument1", "123" });
			Assert.False(parsed.IsOkay);
		}

		class FreeArgument_NoOrdering_Error_Type
		{
			[CmdFree(0)]
			public int FreeArgument1;
			[CmdFree(0)]
			public int FreeArgument2;
		}
		[Fact]
		public void FreeArgument_NoOrdering()
		{
			Assert.Throws<ArgumentException>(() => new CommandLineParserFactory().CreateParser<FreeArgument_NoOrdering_Error_Type>());
		}
		class FreeArgument_TrailingFreeArity_Type
		{
			[CmdFree(0)]
			public int FreeArgument1;
			[CmdFree(1)]
			public IEnumerable<int> FreeArgument2;
		}
		[Fact]
		public void FreeArgument_TrailingFreeArity()
		{
			var parsed = CommandLineParser.Parse<FreeArgument_TrailingFreeArity_Type>(new[] { "456", "1", "2", "3" });
			Assert.Equal(456, parsed.FreeArgument1);
			Assert.Equal(new[] { 1, 2, 3 }, parsed.FreeArgument2);
		}
		class FreeArgument_LongNonTrailing_Error_Type
		{
			[CmdFree(0)]
			public IEnumerable<int> FreeArgument2;
			[CmdFree(1)]
			public int FreeArgument1;
		}
		[Fact]
		public void FreeArgument_LongNonTrailing_Error()
		{
			Assert.Throws<ArgumentException>(() => new CommandLineParserFactory().CreateParser<FreeArgument_LongNonTrailing_Error_Type>());
		}
		class FreeArgument_ManyLong_Error_Type
		{
			[CmdFree(0)]
			public IEnumerable<int> FreeArgument1;
			[CmdFree(1)]
			public IEnumerable<int> FreeArgument2;
		}
		[Fact]
		public void FreeArgument_ManyLong_Error()
		{
			Assert.Throws<ArgumentException>(() => new CommandLineParserFactory().CreateParser<FreeArgument_ManyLong_Error_Type>());
		}
	}
}

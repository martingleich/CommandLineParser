using CmdParse;
using System.IO;
using Xunit;

namespace Tests
{
	public class FileInfoParse
	{
		class FileInfoType
		{
			public FileInfo Value;
			public DirectoryInfo Path;
		}
		[Fact]
		public void FileInfo()
		{
			var result = CommandLineParser.Parse<FileInfoType>(new[] { "--Value", "C:/Path", "--Path", "path" });
			Assert.Equal(new FileInfo("C:/Path").FullName, result.Value.FullName);
			Assert.Equal(new DirectoryInfo("path").FullName, result.Path.FullName);
		}
		[Fact]
		public void FileInfoError()
		{
			var result = CommandLineParser.ParseWithError<FileInfoType>(new[] { "--Value", "   " });
			Assert.False(result.IsOkay);
		}
	}
}

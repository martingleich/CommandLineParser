using System.Collections.Immutable;
using System.Linq;

namespace CmdParse
{
	public class ErrorId
	{
		private ErrorId(int index, string formatString)
		{
			Index = index;
			FormatString = formatString;
		}

		public int Index { get; }
		public string FormatString { get; }

		public static readonly ErrorId GenericError = new ErrorId(0, "GenericError: {0}.");
		public static readonly ErrorId MissingMandatoryArgument = new ErrorId(1, "Missing mandatory argument '--{0}'.");
	}

	public class Error
	{
		public Error(ErrorId id, params string[] payload)
		{
			Id = id;
			Payload = payload.ToImmutableArray();
		}

		public ErrorId Id { get; }
		public ImmutableArray<string> Payload { get; }

		public override string ToString() => string.Format(Id.FormatString, Payload.ToArray<object>());
	}
}

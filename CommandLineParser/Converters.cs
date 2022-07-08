namespace CmdParse
{
	internal static class Converters
	{
		public static ErrorOr<bool> TryParseBool(string value) => bool.TryParse(value, out var val)
				? ErrorOr.FromValue(val)
				: new Error(ErrorId.InvalidParseFormat, value, "bool");
		public static ErrorOr<int> TryParseInt(string value) => int.TryParse(value, out var val)
				? ErrorOr.FromValue(val)
				: new Error(ErrorId.InvalidParseFormat, value, "int");
		public static ErrorOr<double> TryParseDouble(string value) => double.TryParse(value, out var val)
				? ErrorOr.FromValue(val)
				: new Error(ErrorId.InvalidParseFormat, value, "double");
		public static ErrorOr<System.Net.IPEndPoint> TryParseIPEndPoint(string value) => System.Net.IPEndPoint.TryParse(value, out var val)
				? ErrorOr.FromValue(val)
				: new Error(ErrorId.InvalidParseFormat, value, "IPEndpoint");
	}
}

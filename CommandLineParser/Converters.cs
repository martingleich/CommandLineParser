namespace CmdParse
{
	internal static class Converters
	{
		public static ErrorOr<bool> TryParseBool(string value) => bool.TryParse(value, out var val)
				? ErrorOr.FromValue(val)
				: $"Invalid boolean '{value}'";
		public static ErrorOr<int> TryParseInt(string value) => int.TryParse(value, out var val)
				? ErrorOr.FromValue(val)
				: $"Invalid integer '{value}'";
		public static ErrorOr<double> TryParseDouble(string value) => double.TryParse(value, out var val)
				? ErrorOr.FromValue(val)
				: $"Invalid double '{value}'";
	}
}

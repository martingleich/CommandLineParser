namespace CmdParse
{
	internal static class Converters
	{
		public static ErrorOr<object?> TryParseBool(string value) => bool.TryParse(value, out var val)
				? ErrorOr.FromValue<object?>(val)
				: "Invalid boolean";
		public static ErrorOr<object?> TryParseInt(string value) => int.TryParse(value, out var val)
				? ErrorOr.FromValue<object?>(val)
				: "Invalid integer";
		public static ErrorOr<object?> TryParseDouble(string value) => double.TryParse(value, out var val)
				? ErrorOr.FromValue<object?>(val)
				: "Invalid double";
		public static ErrorOr<object?> TryParseString(string value) => ErrorOr.FromValue<object?>(value);
	}
}

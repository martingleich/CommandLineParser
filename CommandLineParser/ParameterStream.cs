using System;

namespace CmdParse
{
    /// <summary>
    /// A immutable stream of string values.
    /// </summary>
    public readonly struct ParameterStream
	{
		private readonly string[] _args;
		private readonly int _offset;

        private ParameterStream(string[] args, int offset)
        {
			if (offset > args.Length)
				throw new ArgumentException($"{nameof(offset)}({offset}) must be less or equal than {nameof(args.Length)}({args.Length}).");
            _args = args ?? throw new ArgumentNullException(nameof(args));
            _offset = offset;
        }

		/// <summary>
		/// Create a new parameter stream
		/// </summary>
		/// <param name="args">The underlying array of parameters.</param>
		/// <param name="offset">The offset of the first element in the stream</param>
		/// <returns>The newly created stream</returns>
		public static ParameterStream Create(string[] args, int offset = 0) => new ParameterStream(args, offset);

		/// <summary>
		/// Try to take a single element from the stream.
		/// </summary>
		/// <param name="remainder">The remaining values.</param>
		/// <returns>The taken value, or null if no value is available.</returns>
		public string? TryTake(out ParameterStream remainder)
		{
			if (_offset != _args.Length)
			{
				remainder = new ParameterStream(_args, _offset + 1);
				return _args[_offset];
			}
			else
			{
				remainder = this;
				return null;
			}
		}
		public ParameterStream Skip(int count) => new ParameterStream(_args, _offset + count);
    }
}

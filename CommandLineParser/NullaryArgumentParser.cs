﻿using System;
using System.Collections.Generic;

namespace CmdParse
{
	public sealed class NullaryArgumentParser<T> : IArgumentParser
	{
		public NullaryArgumentParser(T value)
		{
			Value = value;
		}

		public T Value { get; }

		public Type ResultType => typeof(T);

		public ErrorOr<(int Count, object? Value)> Parse(IEnumerable<string> args)
			=> ErrorOr.FromValue((0, (object?)Value));
	}
}

using System;

namespace Gibbed.FarCry2.FileFormats
{
	public class FileFormatException : Exception
	{
		public FileFormatException()
			: base()
		{
		}

		public FileFormatException(string message)
			: base(message)
		{
		}

		public FileFormatException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}

	public class NotAnBigFileException : FileFormatException
	{
		public NotAnBigFileException()
			: base()
		{
		}

		public NotAnBigFileException(string message)
			: base(message)
		{
		}

		public NotAnBigFileException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}

	public class UnsupportedBigFileVersionException : FileFormatException
	{
		public UnsupportedBigFileVersionException()
			: base()
		{
		}

		public UnsupportedBigFileVersionException(string message)
			: base(message)
		{
		}

		public UnsupportedBigFileVersionException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}

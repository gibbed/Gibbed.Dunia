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

	public class NotAnArchiveException : FileFormatException
	{
		public NotAnArchiveException()
			: base()
		{
		}

		public NotAnArchiveException(string message)
			: base(message)
		{
		}

		public NotAnArchiveException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}

	public class UnsupportedArchiveVersionException : FileFormatException
	{
		public UnsupportedArchiveVersionException()
			: base()
		{
		}

		public UnsupportedArchiveVersionException(string message)
			: base(message)
		{
		}

		public UnsupportedArchiveVersionException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}

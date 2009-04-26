using System;

namespace Gibbed.FarCry2.FileFormats
{
	public class ArchiveFileException : Exception
	{
		public ArchiveFileException()
			: base()
		{
		}

		public ArchiveFileException(string message)
			: base(message)
		{
		}

		public ArchiveFileException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}

	public class NotAnArchiveException : ArchiveFileException
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

	public class UnsupportedArchiveVersionException : ArchiveFileException
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

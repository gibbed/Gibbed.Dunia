using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.FarCry2.Helpers;

namespace Gibbed.FarCry2.Archive
{
	public class ArchiveFileException : Exception
	{
	}

	public class NotAnArchiveException : ArchiveFileException
	{
	}

	public class UnsupportedArchiveVersionException : ArchiveFileException
	{
	}

	public class ArchiveIndex
	{
		public UInt32 Hash;
		public UInt32 UncompressedSize;
		public UInt32 CompressedSize;
		public UInt64 Offset;
		public byte Flags;
	}

	public class ArchiveFile
	{
		public UInt32 Version;
		public List<ArchiveIndex> Indices = new List<ArchiveIndex>();

		public void Read(Stream stream)
		{
			UInt32 indexCount;

			uint magic = stream.ReadU32();
			if (magic != 0x46415432) // FAT2
			{
				throw new NotAnArchiveException();
			}

			uint version = stream.ReadU32();
			if (version != 5)
			{
				throw new UnsupportedArchiveVersionException();
			}

			stream.ReadU32();
			indexCount = stream.ReadU32();

			this.Indices = new List<ArchiveIndex>();

			for (int i = 0; i < indexCount; i++)
			{
				// hhhhhhhh hhhhhhhh hhhhhhhh hhhhhhhh
				// uuuuuuuu uuuuuuuu uuuuuuuu uuuuuuff
				// oooooooo oooooooo oooooooo oooooooo
				// oocccccc cccccccc cccccccc cccccccc

				// hash = 32 bits
				// flags = 2 bits
				// uncompressed size = 30 bits
				// compressed size = 30 bits
				// offset = 34 bits

				ArchiveIndex index = new ArchiveIndex();
				index.Hash = stream.ReadU32();
				index.UncompressedSize = stream.ReadU32();
				index.Flags = (byte)(index.UncompressedSize & 2);
				index.UncompressedSize >>= 2;
				index.Offset = stream.ReadU64();
				index.CompressedSize = (UInt32)(index.Offset & 0x3FFFFFFF);
				index.Offset >>= 30;

				this.Indices.Add(index);
			}

			// There's a dword at the end of the file past the index entries, all observed
			// Far Cry 2 archives all have it as 0, I assume it's another table for something,
			// but until we can observe it in action there's no point in reading it.
		}
	}
}

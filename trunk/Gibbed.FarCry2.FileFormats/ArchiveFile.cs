using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.Helpers;
using Gibbed.FarCry2.Helpers;

namespace Gibbed.FarCry2.FileFormats
{
	public class ArchiveFile
	{
		public UInt32 Version;
		public List<ArchiveEntry> Entries = new List<ArchiveEntry>();

		public void Read(Stream input)
		{
			uint magic = input.ReadU32();
			if (magic != 0x46415432) // FAT2
			{
				throw new NotAnArchiveException();
			}

			uint version = input.ReadU32();
			if (version != 5)
			{
				throw new UnsupportedArchiveVersionException();
			}

			input.ReadU32();
			UInt32 indexCount = indexCount = input.ReadU32();

			this.Entries = new List<ArchiveEntry>();

			for (int i = 0; i < indexCount; i++)
			{
				ArchiveEntry index = new ArchiveEntry();
				index.Read(input);
				this.Entries.Add(index);
			}

			// There's a dword at the end of the file past the index entries, all observed
			// Far Cry 2 archives all have it as 0, I assume it's another table for something.

			if (input.ReadU32() != 0)
			{
				throw new Exception();
			}
		}

		public void Write(Stream output)
		{
			output.WriteU32(0x46415432);
			output.WriteU32(5);
			output.WriteU32(0x0301);
			output.WriteU32((uint)this.Entries.Count);

			foreach (ArchiveEntry entry in this.Entries)
			{
				entry.Write(output);
			}

			output.WriteU32(0);
		}
	}
}

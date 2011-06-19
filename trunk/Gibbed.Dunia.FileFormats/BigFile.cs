using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.Helpers;
using Gibbed.Dunia.Helpers;

namespace Gibbed.Dunia.FileFormats
{
	public class BigFile
	{
		public UInt32 Version;
		public List<BigEntry> Entries = new List<BigEntry>();

		public void Read(Stream input)
		{
			uint magic = input.ReadValueU32();
			if (magic != 0x46415432) // FAT2
			{
				throw new NotAnBigFileException();
			}

			uint version = input.ReadValueU32();
			if (version != 5)
			{
				throw new UnsupportedBigFileVersionException();
			}

			input.ReadValueU32();
			UInt32 indexCount = indexCount = input.ReadValueU32();

			this.Entries = new List<BigEntry>();

			for (int i = 0; i < indexCount; i++)
			{
				BigEntry index = new BigEntry();
				index.Read(input);
				this.Entries.Add(index);
			}

			// There's a dword at the end of the file past the index entries, all observed
			// Far Cry 2 archives all have it as 0, I assume it's another table for something.

			if (input.ReadValueU32() != 0)
			{
				throw new Exception();
			}
		}

		public void Write(Stream output)
		{
			output.WriteValueU32(0x46415432);
			output.WriteValueU32(5);
			output.WriteValueU32(0x0301);
			output.WriteValueU32((uint)this.Entries.Count);

			foreach (BigEntry entry in this.Entries)
			{
				entry.Write(output);
			}

			output.WriteValueU32(0);
		}
	}
}

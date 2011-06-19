using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.Helpers;
using Gibbed.Dunia.Helpers;

namespace Gibbed.Dunia.FileFormats
{
	public class BigEntry
	{
		public UInt32 Hash;
		public UInt32 UncompressedSize;
		public UInt32 CompressedSize;
		public UInt64 Offset;
		public byte Flags;

		// hhhhhhhh hhhhhhhh hhhhhhhh hhhhhhhh
		// uuuuuuuu uuuuuuuu uuuuuuuu uuuuuuff
		// oooooooo oooooooo oooooooo oooooooo
		// oocccccc cccccccc cccccccc cccccccc

		// hash = 32 bits
		// flags = 2 bits
		// uncompressed size = 30 bits
		// compressed size = 30 bits
		// offset = 34 bits

		public void Read(Stream input)
		{
			this.Hash = input.ReadValueU32();
			this.UncompressedSize = input.ReadValueU32();
			this.Flags = (byte)(this.UncompressedSize & 2);
			this.UncompressedSize >>= 2;
			this.Offset = input.ReadValueU64();
			this.CompressedSize = (UInt32)(this.Offset & 0x3FFFFFFF);
			this.Offset >>= 30;

			// File flags are currently unknown, bit 1 appears to say that the data is
			// compressed with zlib instead of lzo1x, but it's unconfirmed.
			// Bit 2 is unknown.
			if (this.Flags != 0)
			{
				throw new FileFormatException("got unsupported flags " + this.Flags.ToString());
			}
		}

		public void Write(Stream output)
		{
			output.WriteValueU32(this.Hash);
			output.WriteValueU32((UInt32)(this.UncompressedSize << 2) | (UInt32)(this.Flags & 2));
			output.WriteValueU64(this.Offset << 30 | (this.CompressedSize & 0x3FFFFFFF));
		}

		public override string ToString()
		{
			return this.Hash.ToString("X8") + " @ " + this.Offset.ToString("X16");
		}
	}
}

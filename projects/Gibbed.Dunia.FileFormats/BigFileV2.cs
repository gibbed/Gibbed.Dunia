/* Copyright (c) 2021 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Dunia.FileFormats
{
	public class BigFileV2
	{
		public uint Version;
        public List<Big.Entry> Entries = new List<Big.Entry>();

		public void Deserialize(Stream input)
		{
			var magic = input.ReadValueU32(Endian.Little);
			if (magic != 0x46415432) // FAT2
			{
				throw new FormatException("not a big file");
			}
            var endian = Endian.Little;

            var version = input.ReadValueU32(endian);
			if (version != 5)
			{
                throw new FormatException("unsupported big file version");
			}

            input.ReadValueU32(endian);
            var indexCount = input.ReadValueU32(endian);

            this.Entries.Clear();
			for (int i = 0; i < indexCount; i++)
			{
				var index = new Big.Entry();
				index.Deserialize(input, endian);
				this.Entries.Add(index);
			}

			// There's a dword at the end of the file past the index entries, all observed
			// Far Cry 2 archives all have it as 0, I assume it's another table for something.

            if (input.ReadValueU32(endian) != 0)
			{
				throw new FormatException("unexpected value");
			}
		}

		public void Serialize(Stream output)
        {
            var endian = Endian.Little;

            output.WriteValueU32(0x46415432, endian);
            output.WriteValueU32(5, endian);
            output.WriteValueU32(0x0301, endian);
            output.WriteValueU32((uint)this.Entries.Count, endian);

			foreach (var entry in this.Entries)
			{
                entry.Serialize(output, endian);
			}

            output.WriteValueU32(0, endian);
		}
	}
}

/* Copyright (c) 2012 Rick (rick 'at' gibbed 'dot' us)
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

using System.IO;
using Gibbed.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.FarCry2.FileFormats.Map
{
    public class Archive
    {
        public uint Version;
        public CompressedData DAT;
        public CompressedData FAT;
        public CompressedData XML;

        public void Deserialize(Stream input, Endian endian)
        {
            var baseOffset = input.Position;

            var magic = input.ReadValueU32(endian);
            if (magic != 0x4D324346) // FC2M
            {
                throw new FormatException();
            }

            this.Version = input.ReadValueU32(endian);
            if (this.Version != 1)
            {
                throw new FormatException();
            }

            uint offsetA = input.ReadValueU32(endian);
            uint offsetB = input.ReadValueU32(endian);
            uint offsetC = input.ReadValueU32(endian);

            if (offsetA != 20)
            {
                throw new FormatException();
            }

            this.DAT = new CompressedData();
            this.DAT.Deserialize(input, endian);

            if (baseOffset + offsetB != input.Position)
            {
                throw new FormatException();
            }

            this.FAT = new CompressedData();
            this.FAT.Deserialize(input, endian);

            if (baseOffset + offsetC != input.Position)
            {
                throw new FormatException();
            }

            this.XML = new CompressedData();
            this.XML.Deserialize(input, endian);
        }

        public void Serialize(Stream output, Endian endian)
        {
            output.WriteValueU32(0x4D324346, endian); // FC2M
            output.WriteValueU32(1, endian);

            uint offset = 0;

            offset += 20;
            output.WriteValueU32(offset, endian);

            offset += 4 + (uint)this.DAT.Data.Length + 4 + ((uint)this.DAT.Blocks.Count * 8);
            output.WriteValueU32(offset, endian);

            offset += 4 + (uint)this.FAT.Data.Length + 4 + ((uint)this.FAT.Blocks.Count * 8);
            output.WriteValueU32(offset, endian);

            this.DAT.Serialize(output, endian);
            this.FAT.Serialize(output, endian);
            this.XML.Serialize(output, endian);
        }
    }
}

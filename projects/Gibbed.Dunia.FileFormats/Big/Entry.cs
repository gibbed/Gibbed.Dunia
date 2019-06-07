/* Copyright (c) 2019 Rick (rick 'at' gibbed 'dot' us)
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
using System.IO;
using Gibbed.IO;

namespace Gibbed.Dunia.FileFormats.Big
{
    public class Entry
    {
        public uint NameHash;
        public uint UncompressedSize;
        public uint CompressedSize;
        public long Offset;
        public CompressionScheme CompressionScheme;

        // hhhhhhhh hhhhhhhh hhhhhhhh hhhhhhhh
        // uuuuuuuu uuuuuuuu uuuuuuuu uuuuuuss
        // oooooooo oooooooo oooooooo oooooooo
        // oocccccc cccccccc cccccccc cccccccc

        // hash = 32 bits
        // compression scheme = 2 bits
        // uncompressed size = 30 bits
        // compressed size = 30 bits
        // offset = 34 bits

        public void Deserialize(Stream input, Endian endian)
        {
            var a = input.ReadValueU32(endian);
            var b = input.ReadValueU32(endian);
            var c = input.ReadValueU64(endian);

            this.NameHash = a;
            this.UncompressedSize = (uint)((b & 0xFFFFFFFCu) >> 2);
            this.CompressionScheme = (CompressionScheme)((b & 0x00000003u) >> 0);
            this.Offset = (long)((c & 0xFFFFFFFFC0000000ul) >> 30);
            this.CompressedSize = (uint)((c & 0x000000003FFFFFFFul) >> 0);

            if (this.CompressionScheme == CompressionScheme.None)
            {
                if (this.UncompressedSize != 0)
                {
                    throw new FormatException();
                }
            }
            else if (this.CompressionScheme == CompressionScheme.LZO1x)
            {
                if (this.CompressedSize == 0 &&
                    this.UncompressedSize > 0)
                {
                    throw new FormatException();
                }
            }
            else
            {
                throw new FormatException();
            }
        }

        public void Serialize(Stream output, Endian endian)
        {
            uint a = this.NameHash;
            uint b = 0;
            b |= ((this.UncompressedSize << 2) & 0xFFFFFFFCu);
            b |= (uint)(((byte)this.CompressionScheme << 0) & 0x00000003u);
            ulong c = 0;
            c |= ((ulong)(this.Offset << 30) & 0xFFFFFFFFC0000000ul);
            c |= (ulong)((this.CompressedSize << 0) & 0x000000003FFFFFFFul);

            output.WriteValueU32(a, endian);
            output.WriteValueU32(b, endian);
            output.WriteValueU64(c, endian);
        }

        public override string ToString()
        {
            return this.NameHash.ToString("X8") + " @ " + this.Offset.ToString("X16");
        }
    }
}

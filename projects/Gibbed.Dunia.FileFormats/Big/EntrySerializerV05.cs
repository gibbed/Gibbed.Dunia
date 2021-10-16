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

using System.IO;
using Gibbed.IO;

namespace Gibbed.Dunia.FileFormats.Big
{
    internal class EntrySerializerV05 : IEntrySerializer<uint>
    {
        //          24       16        8        0
        //    76543210 76543210 76543210 76543210
        //    -------- -------- -------- --------
        // a: hhhhhhhh hhhhhhhh hhhhhhhh hhhhhhhh
        // b: uuuuuuuu uuuuuuuu uuuuuuuu uuuuuuss
        // c: oocccccc cccccccc cccccccc cccccccc
        // d: oooooooo oooooooo oooooooo oooooooo

        // [h] hash = 32 bits
        // [s] compression scheme = 2 bits
        // [u] uncompressed size = 30 bits
        // [c] compressed size = 30 bits
        // [o] offset = 34 bits

        public int Size { get { return 16; } }

        public void Serialize(Stream output, Entry<uint> entry, Endian endian)
        {
            uint a = entry.NameHash;
            uint b = 0;
            b |= ((uint)entry.UncompressedSize & 0x3FFFFFFFu) << 2;
            b |= (entry.CompressionScheme & 0x3u) << 0;
            uint c = 0;
            c |= (uint)(entry.CompressedSize & 0x3FFFFFFFu) << 0;
            c |= (uint)(entry.Offset & 0x3u) << 30;
            uint d = 0;
            d |= (uint)(entry.Offset >> 2);

            output.WriteValueU32(a, endian);
            output.WriteValueU32(b, endian);
            output.WriteValueU32(c, endian);
            output.WriteValueU32(d, endian);
        }

        public void Deserialize(Stream input, Endian endian, out Entry<uint> entry)
        {
            var a = input.ReadValueU32(endian);
            var b = input.ReadValueU32(endian);
            var c = input.ReadValueU32(endian);
            var d = input.ReadValueU32(endian);

            entry = new Entry<uint>()
            {
                NameHash = a,
                UncompressedSize = (int)((b >> 2) & 0x3FFFFFFFu),
                CompressionScheme = (byte)((b >> 0) & 0x3u),
                CompressedSize = (int)((c >> 0) & 0x3FFFFFFFu),
                Offset = (long)(((c >> 30) & 0x3u) | (ulong)d << 2),
            };
        }
    }
}

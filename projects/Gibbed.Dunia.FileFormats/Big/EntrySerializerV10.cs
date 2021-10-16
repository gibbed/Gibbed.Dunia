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
using System.IO;
using Gibbed.IO;

namespace Gibbed.Dunia.FileFormats.Big
{
    internal class EntrySerializerV10 : IEntrySerializer<ulong>
    {
        //          24       16        8        0
        //    76543210 76543210 76543210 76543210
        //    -------- -------- -------- --------
        // a: hhhhhhhh hhhhhhhh hhhhhhhh hhhhhhhh
        // b: HHHHHHHH HHHHHHHH HHHHHHHH HHHHHHHH
        // c: uuuuuuuu uuuuuuuu uuuuuuuu uuuuuuss
        // d: oooooooo oooooooo oooooooo oooooooo
        // e: OOOccccc cccccccc cccccccc cccccccc
        //
        // [h] hash (upper)       = 32 bits
        // [H] hash (lower)       = 32 bits
        // [s] compression scheme =  2 bits
        // [u] uncompressed size  = 30 bits
        // [o] offset (upper)     = 32 bits
        // [O] offset (lower)     =  3 bits
        // [c] compressed size    = 29 bits

        public int Size { get { return 20; } }

        public void Serialize(Stream output, Entry<ulong> entry, Endian endian)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input, Endian endian, out Entry<ulong> entry)
        {
            var a = input.ReadValueU32(endian);
            var b = input.ReadValueU32(endian);
            var c = input.ReadValueU32(endian);
            var d = input.ReadValueU32(endian);
            var e = input.ReadValueU32(endian);

            entry = new Entry<ulong>()
            {
                NameHash = ((ulong)a << 32) | b,
                UncompressedSize = (int)((c >> 2) & 0x3FFFFFFFu),
                IsEncrypted = ((c >> 0) & 0x1u) != 0,
                CompressionScheme = (byte)((c >> 1) & 0x1u),
                Offset = (long)(((ulong)d << 3) | (e >> 29)),
                CompressedSize = (int)((e >> 0) & 0x1FFFFFFFu),
            };
        }
    }
}

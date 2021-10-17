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
using Entry = Gibbed.Dunia.FileFormats.Big.Entry<uint>;

namespace Gibbed.Dunia.FileFormats.Big
{
    internal class EntrySerializerV1 : IEntrySerializer<uint>
    {
        //          24       16        8        0
        //    76543210 76543210 76543210 76543210
        //    -------- -------- -------- --------
        // a: hhhhhhhh hhhhhhhh hhhhhhhh hhhhhhhh
        // b: ???????? ???????? ???????? ????????
        // c: uuuuuuuu uuuuuuuu uuuuuuuu uuuuuuss
        // d: oooooooo oooooooo oooooooo oooooooo
        // e: OOcccccc cccccccc cccccccc cccccccc
        // f: ???????? ???????? ???????? ????????

        // [h] hash               = 32 bits
        // [?] unknown            = 32 bits
        // [u] uncompressed size  = 30 bits
        // [s] compression scheme =  2 bits
        // [o] offset (upper)     = 32 bits
        // [O] offset (lower)     =  2 bits
        // [c] compressed size    = 30 bits
        // [?] unknown            = 32 bits

        public int Size { get { return 24; } }

        public void Serialize(Stream output, Entry entry, Endian endian)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input, Endian endian, out Entry entry)
        {
            var a = input.ReadValueU32(endian);
            /*var b =*/ input.ReadValueU32(endian);
            var c = input.ReadValueU32(endian);
            var d = input.ReadValueU32(endian);
            var e = input.ReadValueU32(endian);
            /*var f =*/ input.ReadValueU32(endian);

            entry = new()
            {
                NameHash = a,
                UncompressedSize = (int)((e >> 2) & 0x3FFFFFFFu),
                CompressionScheme = (byte)((e >> 0) & 0x3u),
                Offset = (long)(((ulong)d << 2) | ((c >> 30) & 0x3u)),
                CompressedSize = (int)((c >> 0) & 0x3FFFFFFFu),
            };

            throw new NotImplementedException();
        }
    }
}

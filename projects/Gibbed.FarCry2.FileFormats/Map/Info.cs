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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.FarCry2.FileFormats.Map
{
    public class Info
    {
        public uint Unknown2;
        public uint Unknown3;
        public uint Unknown4;
        public ulong Unknown5;
        public string Creator;
        public ulong Unknown7;
        public string Author;
        public string Name;
        public ulong Unknown10;
        public byte[] Unknown11;
        public byte[] Unknown12;
        public Size Size;
        public Players Players;
        public uint Unknown15;

        public void Deserialize(Stream input, Endian endian)
        {
            this.Unknown2 = input.ReadValueU32(endian);
            this.Unknown3 = input.ReadValueU32(endian);
            this.Unknown4 = input.ReadValueU32(endian);
            this.Unknown5 = input.ReadValueU64(endian);
            this.Creator = input.ReadString((int)input.ReadValueU32(endian), Encoding.UTF8);
            this.Unknown7 = input.ReadValueU64(endian);
            this.Author = input.ReadString((int)input.ReadValueU32(endian), Encoding.UTF8);
            this.Name = input.ReadString((int)input.ReadValueU32(endian), Encoding.UTF8);
            this.Unknown10 = input.ReadValueU64(endian);

            this.Unknown11 = new byte[36];
            input.Read(this.Unknown11, 0, this.Unknown11.Length);

            this.Unknown12 = new byte[36];
            input.Read(this.Unknown12, 0, this.Unknown12.Length);

            this.Size = (Size)input.ReadValueU32(endian);
            this.Players = (Players)input.ReadValueU32(endian);
            this.Unknown15 = input.ReadValueU32(endian);
        }

        public void Serialize(Stream output, Endian endian)
        {
            output.WriteValueU32(this.Unknown2, endian);
            output.WriteValueU32(this.Unknown3, endian);
            output.WriteValueU32(this.Unknown4, endian);
            output.WriteValueU64(this.Unknown5, endian);

            output.WriteValueS32(this.Creator.Length, endian);
            output.WriteString(this.Creator, Encoding.UTF8);

            output.WriteValueU64(this.Unknown7, endian);

            output.WriteValueS32(this.Author.Length, endian);
            output.WriteString(this.Author, Encoding.UTF8);

            output.WriteValueS32(this.Name.Length, endian);
            output.WriteString(this.Name, Encoding.UTF8);

            output.WriteValueU64(this.Unknown10, endian);

            output.Write(this.Unknown11, 0, 36);
            output.Write(this.Unknown12, 0, 36);

            output.WriteValueU32((uint)this.Size, endian);
            output.WriteValueU32((uint)this.Players, endian);
            output.WriteValueU32(this.Unknown15, endian);
        }
    }
}

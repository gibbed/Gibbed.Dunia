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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using Gibbed.IO;

namespace Gibbed.Dunia.FileFormats
{
    public class AIWorkspaceResourceFile
    {
        public List<UnknownData0> Unknown0 = new List<UnknownData0>();
        public byte[] Unknown1;
        public List<uint> VariableNameHashes = new List<uint>();
        public List<UnknownData3> Unknown3 = new List<UnknownData3>();
        public XmlResourceFile XmlResource;

        public void Deserialize(Stream input)
        {
            var version = input.ReadValueU32(Endian.Little);
            if (version < 1 || version > 4)
            {
                throw new FormatException();
            }
            var endian = Endian.Little;

            var unknownLength = input.ReadValueU32(endian);
            var rmlLength = input.ReadValueU32(endian);

            if (input.Position + unknownLength + rmlLength > input.Length)
            {
                throw new FormatException();
            }

            using (var data = input.ReadToMemoryStream((int)unknownLength))
            {
                var unk0count = data.ReadValueU32(endian);
                this.Unknown0.Clear();
                for (uint i = 0; i < unk0count; i++)
                {
                    var id = data.ReadValueU32(endian);
                    var length = data.ReadValueU32(endian);
                    var xml = new XmlResourceFile();
                    using (var data2 = data.ReadToMemoryStream((int)length))
                    {
                        xml.Deserialize(data2);
                    }
                    this.Unknown0.Add(new UnknownData0()
                    {
                        TypeHash = id,
                        XmlResource = xml,
                    });
                }

                var unk1length = data.ReadValueU32(endian);
                this.Unknown1 = new byte[unk1length];
                if (data.Read(this.Unknown1, 0, this.Unknown1.Length) != this.Unknown1.Length)
                {
                    throw new FormatException();
                }

                this.VariableNameHashes.Clear();
                var variableNameCount = data.ReadValueU32(endian);
                for (uint i = 0; i < variableNameCount; i++)
                {
                    this.VariableNameHashes.Add(data.ReadValueU32(endian));
                }

                this.Unknown3.Clear();
                var unk3count = data.ReadValueU32(endian);
                for (uint i = 0; i < unk3count; i++)
                {
                    var unknown3 = new UnknownData3();
                    unknown3.NameHash = data.ReadValueU32(endian);
                    var unk1 = data.ReadValueU32(endian);
                    unknown3.Name = data.ReadString((int)unk1, Encoding.UTF8);
                    unknown3.IndexIntoUnknown0 = data.ReadValueU32(endian);
                    unknown3.IndexIntoUnknown1 = data.ReadValueU32(endian);
                    unknown3.Unknown4 = data.ReadValueU32(endian);
                    this.Unknown3.Add(unknown3);
                }
            }

            this.XmlResource = new XmlResourceFile();
            using (var data = input.ReadToMemoryStream((int)rmlLength))
            {
                this.XmlResource.Deserialize(data);
                if (data.Position != data.Length)
                {
                    throw new FormatException();
                }
            }

            var test_u2 = this.Unknown3.Max(u => u.IndexIntoUnknown0);
            var test_u3 = this.Unknown3.Max(u => u.IndexIntoUnknown1);
            var test_u4 = this.Unknown3.Max(u => u.Unknown4);
        }

        public class UnknownData0
        {
            public uint TypeHash;
            public XmlResourceFile XmlResource;
        }

        public class UnknownData3
        {
            public uint NameHash;
            public string Name;
            public uint IndexIntoUnknown0;
            public uint IndexIntoUnknown1;
            public uint Unknown4;
        }
    }
}

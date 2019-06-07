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

using System.IO;
using Gibbed.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.FarCry2.FileFormats
{
    public class MapFile
    {
        public uint Version = 11;
        public uint TypeHash = 0xD2FD0A6B;
        public Map.Info Info;
        public Map.Snapshot Snapshot;
        public Map.Data Data;
        public Map.Archive Archive;

        public void Deserialize(Stream input)
        {
            var endian = Endian.Little;

            this.Version = input.ReadValueU32(endian);
            if (this.Version != 11)
            {
                throw new FormatException();
            }

            this.TypeHash = input.ReadValueU32(endian);
            if (this.TypeHash != 0xD2FD0A6B) // crc32(CCustomMapGameFile)
            {
                throw new FormatException();
            }

            this.Info = new Map.Info();
            this.Info.Deserialize(input, endian);

            this.Snapshot = new Map.Snapshot();
            this.Snapshot.Deserialize(input, endian);

            this.Data = new Map.Data();
            this.Data.Deserialize(input, endian);

            this.Archive = new Map.Archive();
            this.Archive.Deserialize(input, endian);
        }

        public void Serialize(Stream output)
        {
            var endian = Endian.Little;

            output.WriteValueU32(this.Version, endian);
            output.WriteValueU32(this.TypeHash, endian);
            this.Info.Serialize(output, endian);
            this.Snapshot.Serialize(output, endian);
            this.Data.Serialize(output, endian);
            this.Archive.Serialize(output, endian);
        }
    }
}

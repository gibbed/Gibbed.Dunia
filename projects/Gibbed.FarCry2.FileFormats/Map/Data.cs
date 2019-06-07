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

namespace Gibbed.FarCry2.FileFormats.Map
{
    public class Data
    {
        public string Unknown1;
        public Snapshot Unknown2;

        public void Deserialize(Stream input, Endian endian)
        {
            this.Unknown1 = input.ReadString((int)input.ReadValueU32(endian), Encoding.UTF8);
            this.Unknown2 = new Snapshot();
            this.Unknown2.Deserialize(input, endian);

            var unknown3 = input.ReadValueU32(endian);
            for (uint i = 0; i < unknown3; i++)
            {
                throw new NotSupportedException();
                var unknown4 = input.ReadString((int)input.ReadValueU32(endian), Encoding.UTF8);
            }
        }

        public void Serialize(Stream output, Endian endian)
        {
            output.WriteValueS32(this.Unknown1.Length, endian);
            output.WriteString(this.Unknown1, Encoding.UTF8);

            this.Unknown2.Serialize(output, endian);

            output.WriteValueU32(0, endian); // unknown3
        }
    }
}

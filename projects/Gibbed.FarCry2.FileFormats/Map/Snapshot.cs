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
    public class Snapshot
    {
        public uint Width;
        public uint Height;
        public uint BytesPerPixel;
        public uint Unknown4;
        public byte[] Data;

        public void Deserialize(Stream input, Endian endian)
        {
            this.Width = input.ReadValueU32(endian);
            this.Height = input.ReadValueU32(endian);
            this.BytesPerPixel = input.ReadValueU32(endian);
            this.Unknown4 = input.ReadValueU32(endian);

            var size =
                (this.Unknown4 *
                this.BytesPerPixel *
                this.Height *
                this.Width) / 8;
            this.Data = new byte[size];
            input.Read(this.Data, 0, this.Data.Length);

            var unknown6 = input.ReadValueU32(endian);
            for (uint i = 0; i < unknown6; i++)
            {
                throw new NotSupportedException();
            }
        }

        public void Serialize(Stream output, Endian endian)
        {
            output.WriteValueU32(this.Width, endian);
            output.WriteValueU32(this.Height, endian);
            output.WriteValueU32(this.BytesPerPixel, endian);
            output.WriteValueU32(this.Unknown4, endian);

            var size =
                (this.Unknown4 *
                this.BytesPerPixel *
                this.Height *
                this.Width) / 8;

            if (size > 0)
            {
                output.Write(this.Data, 0, (int)size);
            }

            output.WriteValueU32(0, endian); // unknown6
        }
    }
}

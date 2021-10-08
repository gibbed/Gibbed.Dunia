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
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Dunia.FileFormats.Geometry
{
    // Refer to common\engine\providerdescriptors.xml
    public class MaterialDescriptor : IBlock
    {
        public BlockType Type
        {
            get { return BlockType.MaterialDescriptor; }
        }

        public string Name;
        public string Unknown1;
        public string Unknown2;
        public Dictionary<string, string> TextureProperties
            = new Dictionary<string, string>();
        public Dictionary<string, float> Float1Properties
            = new Dictionary<string, float>();
        public Dictionary<string, Float2> Float2Properties
            = new Dictionary<string, Float2>();
        public Dictionary<string, Float3> Float3Properties
            = new Dictionary<string, Float3>();
        public Dictionary<string, Float4> Float4Properties
            = new Dictionary<string, Float4>();
        public Dictionary<string, int> IntProperties
            = new Dictionary<string, int>();
        public Dictionary<string, bool> BoolProperties
            = new Dictionary<string, bool>();

        public void Deserialize(IBlock parent, Stream input, Endian endian)
        {
            uint length;
            int count;

            length = input.ReadValueU32(endian);
            this.Name = input.ReadString((int)length);
            input.Seek(1, SeekOrigin.Current); // skip null

            length = input.ReadValueU32(endian);
            this.Unknown1 = input.ReadString((int)length);
            input.Seek(1, SeekOrigin.Current); // skip null

            length = input.ReadValueU32(endian);
            this.Unknown2 = input.ReadString((int)length);
            input.Seek(1, SeekOrigin.Current); // skip null

            this.TextureProperties.Clear();
            count = input.ReadValueS32(endian);
            for (int i = 0; i < count; i++)
            {
                length = input.ReadValueU32(endian);
                var value = input.ReadString((int)length);
                input.Seek(1, SeekOrigin.Current); // skip null

                length = input.ReadValueU32(endian);
                var key = input.ReadString((int)length);
                input.Seek(1, SeekOrigin.Current); // skip null

                this.TextureProperties[key] = value;
            }

            this.Float1Properties.Clear();
            count = input.ReadValueS32(endian);
            for (int i = 0; i < count; i++)
            {
                length = input.ReadValueU32(endian);
                var key = input.ReadString((int)length);
                input.Seek(1, SeekOrigin.Current); // skip null

                this.Float1Properties[key] = input.ReadValueF32(endian);
            }

            this.Float2Properties.Clear();
            count = input.ReadValueS32(endian);
            for (int i = 0; i < count; i++)
            {
                length = input.ReadValueU32(endian);
                var key = input.ReadString((int)length);
                input.Seek(1, SeekOrigin.Current); // skip null

                var value = new Float2();
                value.X = input.ReadValueF32(endian);
                value.Y = input.ReadValueF32(endian);

                this.Float2Properties[key] = value;
            }

            this.Float3Properties.Clear();
            count = input.ReadValueS32(endian);
            for (int i = 0; i < count; i++)
            {
                length = input.ReadValueU32(endian);
                var key = input.ReadString((int)length);
                input.Seek(1, SeekOrigin.Current); // skip null

                var value = new Float3();
                value.X = input.ReadValueF32(endian);
                value.Y = input.ReadValueF32(endian);
                value.Z = input.ReadValueF32(endian);

                this.Float3Properties[key] = value;
            }

            this.Float4Properties.Clear();
            count = input.ReadValueS32(endian);
            for (int i = 0; i < count; i++)
            {
                length = input.ReadValueU32(endian);
                var key = input.ReadString((int)length);
                input.Seek(1, SeekOrigin.Current); // skip null

                var value = new Float4();
                value.X = input.ReadValueF32(endian);
                value.Y = input.ReadValueF32(endian);
                value.Z = input.ReadValueF32(endian);
                value.W = input.ReadValueF32(endian);

                this.Float4Properties[key] = value;
            }

            this.IntProperties.Clear();
            count = input.ReadValueS32(endian);
            for (int i = 0; i < count; i++)
            {
                length = input.ReadValueU32(endian);
                var key = input.ReadString((int)length);
                input.Seek(1, SeekOrigin.Current); // skip null

                this.IntProperties[key] = input.ReadValueS32(endian);
            }

            this.BoolProperties.Clear();
            count = input.ReadValueS32(endian);
            for (int i = 0; i < count; i++)
            {
                length = input.ReadValueU32(endian);
                var key = input.ReadString((int)length);
                input.Seek(1, SeekOrigin.Current); // skip null

                this.BoolProperties[key] = input.ReadValueB8();
            }
        }

        public void Serialize(IBlock parent, Stream output, Endian endian)
        {
            throw new NotImplementedException();
        }

        public IBlock CreateBlock(BlockType type)
        {
            throw new NotSupportedException();
        }

        public void AddChild(IBlock child)
        {
            throw new NotSupportedException();
        }

        public IEnumerable<IBlock> GetChildren()
        {
            throw new NotSupportedException();
        }
    }
}

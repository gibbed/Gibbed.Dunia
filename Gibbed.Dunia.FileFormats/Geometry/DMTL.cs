/* Copyright (c) 2011 Rick (rick 'at' gibbed 'dot' us)
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
using Gibbed.Helpers;

namespace Gibbed.Dunia.FileFormats.Geometry
{
    public class DMTL : IBlock
    {
        public BlockType Type
        {
            get { return BlockType.DMTL; }
        }

        public string Unknown0;
        public string Unknown1;
        public string Unknown2;
        public Dictionary<string, string> Unknown3 = new Dictionary<string, string>();
        public Dictionary<string, float> Unknown4 = new Dictionary<string, float>();
        public Dictionary<string, Vector2> Unknown5 = new Dictionary<string, Vector2>();
        public Dictionary<string, Vector3> Unknown6 = new Dictionary<string, Vector3>();
        public Dictionary<string, Vector4> Unknown7 = new Dictionary<string, Vector4>();
        public Dictionary<string, int> Unknown8 = new Dictionary<string, int>();
        public Dictionary<string, byte> Unknown9 = new Dictionary<string, byte>();

        public void Deserialize(IBlock parent, Stream input)
        {
            uint length;
            int count;

            length = input.ReadValueU32();
            this.Unknown0 = input.ReadString(length);
            input.Seek(1, SeekOrigin.Current); // skip null

            length = input.ReadValueU32();
            this.Unknown1 = input.ReadString(length);
            input.Seek(1, SeekOrigin.Current); // skip null

            length = input.ReadValueU32();
            this.Unknown2 = input.ReadString(length);
            input.Seek(1, SeekOrigin.Current); // skip null

            this.Unknown3.Clear();
            count = input.ReadValueS32();
            for (int i = 0; i < count; i++)
            {
                length = input.ReadValueU32();
                var value = input.ReadString(length);
                input.Seek(1, SeekOrigin.Current); // skip null

                length = input.ReadValueU32();
                var key = input.ReadString(length);
                input.Seek(1, SeekOrigin.Current); // skip null

                this.Unknown3[key] = value;
            }

            this.Unknown4.Clear();
            count = input.ReadValueS32();
            for (int i = 0; i < count; i++)
            {
                length = input.ReadValueU32();
                var key = input.ReadString(length);
                input.Seek(1, SeekOrigin.Current); // skip null

                this.Unknown4[key] = input.ReadValueF32();
            }

            this.Unknown5.Clear();
            count = input.ReadValueS32();
            for (int i = 0; i < count; i++)
            {
                length = input.ReadValueU32();
                var key = input.ReadString(length);
                input.Seek(1, SeekOrigin.Current); // skip null

                var value = new Vector2();
                value.X = input.ReadValueF32();
                value.Y = input.ReadValueF32();

                this.Unknown5[key] = value;
            }

            this.Unknown6.Clear();
            count = input.ReadValueS32();
            for (int i = 0; i < count; i++)
            {
                length = input.ReadValueU32();
                var key = input.ReadString(length);
                input.Seek(1, SeekOrigin.Current); // skip null

                var value = new Vector3();
                value.X = input.ReadValueF32();
                value.Y = input.ReadValueF32();
                value.Z = input.ReadValueF32();

                this.Unknown6[key] = value;
            }

            this.Unknown7.Clear();
            count = input.ReadValueS32();
            for (int i = 0; i < count; i++)
            {
                length = input.ReadValueU32();
                var key = input.ReadString(length);
                input.Seek(1, SeekOrigin.Current); // skip null

                var value = new Vector4();
                value.X = input.ReadValueF32();
                value.Y = input.ReadValueF32();
                value.Z = input.ReadValueF32();
                value.W = input.ReadValueF32();

                this.Unknown7[key] = value;
            }

            this.Unknown8.Clear();
            count = input.ReadValueS32();
            for (int i = 0; i < count; i++)
            {
                length = input.ReadValueU32();
                var key = input.ReadString(length);
                input.Seek(1, SeekOrigin.Current); // skip null

                this.Unknown8[key] = input.ReadValueS32();
            }

            this.Unknown9.Clear();
            count = input.ReadValueS32();
            for (int i = 0; i < count; i++)
            {
                length = input.ReadValueU32();
                var key = input.ReadString(length);
                input.Seek(1, SeekOrigin.Current); // skip null

                this.Unknown9[key] = input.ReadValueU8();
            }
        }

        public void Serialize(IBlock parent, Stream output)
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

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

using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Dunia.FileFormats.Geometry
{
    public class LODs : IBlock
    {
        public BlockType Type
        {
            get { return BlockType.LODs; }
        }

        public List<LevelOfDetail> Items = new List<LevelOfDetail>();

        public void Deserialize(IBlock parent, Stream input, Endian endian)
        {
            var count = input.ReadValueS32(endian);
            this.Items.Clear();
            for (int i = 0; i < count; i++)
            {
                var lod = new LevelOfDetail();
                lod.Deserialize(input, endian);
                this.Items.Add(lod);
            }
        }

        public void Serialize(IBlock parent, Stream output, Endian endian)
        {
            throw new NotImplementedException();
        }

        public class LevelOfDetail
        {
            public float Unknown0; // seems to be a distance value for determining which LOD to use
            public List<Buffer> Buffers = new List<Buffer>();
            public List<Primitive> Primitives = new List<Primitive>();
            public byte[] VertexData;
            public short[] Indices;

            public void Deserialize(Stream input, Endian endian)
            {
                this.Unknown0 = input.ReadValueF32(endian);

                var bufferCount = input.ReadValueS32(endian);
                this.Buffers.Clear();
                for (int j = 0; j < bufferCount; j++)
                {
                    var dataInfo = new Buffer();
                    dataInfo.Format = input.ReadValueU32(endian);
                    dataInfo.Size = input.ReadValueU32(endian);
                    dataInfo.Count = input.ReadValueU32(endian);
                    dataInfo.Offset = input.ReadValueU32(endian);
                    this.Buffers.Add(dataInfo);
                }

                var primitiveCount = input.ReadValueS32(endian);
                this.Primitives.Clear();
                for (int j = 0; j < primitiveCount; j++)
                {
                    var primitive = new Primitive();
                    primitive.BufferIndex = input.ReadValueS32(endian);
                    primitive.SkeletonIndex = input.ReadValueS32(endian);
                    primitive.MaterialIndex = input.ReadValueS32(endian);
                    primitive.IndicesStartIndex = input.ReadValueS32(endian);
                    primitive.Unknown4 = input.ReadValueU32(endian);
                    primitive.Unknown5 = input.ReadValueU32(endian);
                    primitive.Unknown6 = input.ReadValueU32(endian);
                    this.Primitives.Add(primitive);
                }

                var vertexDataSize = input.ReadValueU32(endian);
                // data is aligned to 16 bytes, ugh
                input.Seek(input.Position.Align(16), SeekOrigin.Begin);
                this.VertexData = new byte[vertexDataSize];
                input.Read(this.VertexData, 0, this.VertexData.Length);

                var indexCount = input.ReadValueU32(endian);
                // data is aligned to 16 bytes, ugh
                input.Seek(input.Position.Align(16), SeekOrigin.Begin);
                this.Indices = new short[indexCount];
                for (int i = 0; i < indexCount; i++)
                {
                    this.Indices[i] = input.ReadValueS16(endian);
                }
            }
        }

        public class Buffer
        {
            public uint Format;
            public uint Size;
            public uint Count;
            public uint Offset;
        }

        public class Primitive
        {
            public int BufferIndex;
            public int SkeletonIndex;
            public int MaterialIndex;
            public int IndicesStartIndex;
            public uint Unknown4;
            public uint Unknown5;
            public uint Unknown6;
        }

        public IBlock CreateBlock(BlockType type)
        {
            throw new NotImplementedException();
        }

        public void AddChild(IBlock child)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IBlock> GetChildren()
        {
            throw new NotImplementedException();
        }
    }
}

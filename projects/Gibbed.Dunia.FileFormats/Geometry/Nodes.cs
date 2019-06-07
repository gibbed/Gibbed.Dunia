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
using Gibbed.IO;

namespace Gibbed.Dunia.FileFormats.Geometry
{
    public class Nodes : IBlock
    {
        public List<Node> Items
            = new List<Node>();

        public BlockType Type
        {
            get { return BlockType.Nodes; }
        }

        public void Deserialize(IBlock parent, Stream input, Endian endian)
        {
            var count = input.ReadValueU32(endian);

            this.Items.Clear();
            for (uint i = 0; i < count; i++)
            {
                var node = new Node();

                node.NameHash = input.ReadValueU32(endian);
                node.NextSiblingIndex = input.ReadValueS32(endian);
                node.FirstChildIndex = input.ReadValueS32(endian);
                node.PreviousSiblingIndex = input.ReadValueS32(endian);
                node.Unknown10 = input.ReadValueF32(endian);
                node.Unknown14 = input.ReadValueF32(endian);
                node.Unknown18 = input.ReadValueF32(endian);
                node.Unknown1C = input.ReadValueF32(endian);
                node.Unknown20 = input.ReadValueF32(endian);
                node.Unknown24 = input.ReadValueF32(endian);
                node.Unknown28 = input.ReadValueF32(endian);
                node.Unknown2C = input.ReadValueF32(endian);
                node.Unknown30 = input.ReadValueF32(endian);
                node.Unknown34 = input.ReadValueF32(endian);
                node.O2BMIndex = input.ReadValueS32(endian);
                node.Unknown3C = input.ReadValueF32(endian);
                node.Unknown40 = input.ReadValueF32(endian);

                var length = input.ReadValueU32(endian);
                node.Name = input.ReadString((int)length);
                input.Seek(1, SeekOrigin.Current); // skip null

                this.Items.Add(node);
            }
        }

        public void Serialize(IBlock parent, Stream output, Endian endian)
        {
            throw new NotImplementedException();
        }

        public class Node
        {
            public uint NameHash;
            public int NextSiblingIndex;
            public int FirstChildIndex;
            public int PreviousSiblingIndex;
            public float Unknown10;
            public float Unknown14;
            public float Unknown18;
            public float Unknown1C;
            public float Unknown20;
            public float Unknown24;
            public float Unknown28;
            public float Unknown2C;
            public float Unknown30;
            public float Unknown34;
            public int O2BMIndex;
            public float Unknown3C;
            public float Unknown40;
            public string Name;

            public override string ToString()
            {
                return this.Name ?? base.ToString();
            }
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

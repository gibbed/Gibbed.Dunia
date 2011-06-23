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
    public class CLUS : IBlock
    {
        public List<List<UnknownData0>> Unknown0 = new List<List<UnknownData0>>();

        public BlockType Type
        {
            get { return BlockType.CLUS; }
        }

        public void Deserialize(IBlock parent, Stream input)
        {
            var sknd = (SKND)parent;

            this.Unknown0.Clear();
            for (int i = 0; i < sknd.Unknown0.Count; i++)
            {
                uint count = input.ReadValueU32();
                List<UnknownData0> unknowns = new List<UnknownData0>();
                for (int j = 0; j < count; j++)
                {
                    var unknown = new UnknownData0();
                    unknown.Unknown0 = new byte[108];
                    input.Read(unknown.Unknown0, 0, unknown.Unknown0.Length);
                    unknown.Unknown1 = input.ReadValueU16();
                    unknowns.Add(unknown);
                }
                this.Unknown0.Add(unknowns);
            }
        }

        public void Serialize(IBlock parent, Stream output)
        {
            throw new NotImplementedException();
        }

        public class UnknownData0
        {
            public byte[] Unknown0;
            public ushort Unknown1;
        }

        public IBlock CreateBlock(BlockType type)
        {
            return null;
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

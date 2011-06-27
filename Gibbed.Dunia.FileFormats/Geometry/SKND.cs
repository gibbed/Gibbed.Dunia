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
using System.Linq;
using Gibbed.Helpers;

namespace Gibbed.Dunia.FileFormats.Geometry
{
    public class SKND : IBlock
    {
        public BlockType Type
        {
            get { return BlockType.SKND; }
        }

        public List<UnknownData0> Unknown0 = new List<UnknownData0>();
        public List<CLUS> Unknown1 = new List<CLUS>();

        public void Deserialize(IBlock parent, Stream input)
        {
            var count = input.ReadValueU32();

            this.Unknown0.Clear();
            this.Unknown1.Clear();
            for (uint i = 0; i < count; i++)
            {
                var unknown = new UnknownData0();

                unknown.Unknown00 = input.ReadValueF32();
                unknown.Unknown04 = input.ReadValueF32();
                unknown.Unknown08 = input.ReadValueF32();
                unknown.Unknown0C = input.ReadValueF32();
                unknown.Unknown10 = input.ReadValueF32();
                unknown.Unknown14 = input.ReadValueF32();
                unknown.Unknown18 = input.ReadValueF32();
                unknown.Unknown1C = input.ReadValueF32();
                unknown.Unknown20 = input.ReadValueF32();
                unknown.Unknown24 = input.ReadValueF32();
                unknown.Unknown28 = input.ReadValueF32();
                unknown.Unknown2C = input.ReadValueU32();
                unknown.Unknown30 = input.ReadValueU32();

                var length = input.ReadValueU32();
                unknown.Name = input.ReadString(length);

                input.Seek(1, SeekOrigin.Current); // skip null

                this.Unknown0.Add(unknown);
            }
        }

        public void Serialize(IBlock parent, Stream output)
        {
            throw new NotImplementedException();
        }

        public class UnknownData0
        {
            public float Unknown00;
            public float Unknown04;
            public float Unknown08;
            public float Unknown0C;
            public float Unknown10;
            public float Unknown14;
            public float Unknown18;
            public float Unknown1C;
            public float Unknown20;
            public float Unknown24;
            public float Unknown28;
            public uint Unknown2C;
            public uint Unknown30;
            public string Name;
        }

        public IBlock CreateBlock(BlockType type)
        {
            switch (type)
            {
                case BlockType.CLUS: return new CLUS();
            }

            return null;
        }

        public void AddChild(IBlock child)
        {
            this.Unknown1.Add((CLUS)child);
        }

        public IEnumerable<IBlock> GetChildren()
        {
            return this.Unknown1.Cast<IBlock>();
        }
    }
}

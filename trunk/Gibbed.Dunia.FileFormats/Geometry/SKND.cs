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

                unknown.Unknown0 = new byte[52];
                input.Read(unknown.Unknown0, 0, unknown.Unknown0.Length);

                var length = input.ReadValueU32();
                unknown.Unknown1 = input.ReadString(length);

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
            public byte[] Unknown0;
            public string Unknown1;
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

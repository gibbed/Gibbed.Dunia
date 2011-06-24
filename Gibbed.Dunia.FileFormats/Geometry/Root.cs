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

namespace Gibbed.Dunia.FileFormats.Geometry
{
    public class Root : IBlock
    {
        public List<IBlock> Blocks = new List<IBlock>();

        public BlockType Type
        {
            get { return BlockType.Root; }
        }

        public void Deserialize(IBlock parent, Stream input)
        {
        }

        public void Serialize(IBlock parent, Stream output)
        {
        }

        public IBlock CreateBlock(BlockType type)
        {
            switch (type)
            {
                case BlockType.RMTL: return new RMTL();
                case BlockType.Node: return new Node();
                case BlockType.O2BM: return new O2BM();
                case BlockType.SKID: return new SKID();
                case BlockType.SKND: return new SKND();
                case BlockType.LODS: return new LODS();
                case BlockType.BoundingBox: return new BoundingBox();
                case BlockType.BSPH: return new BSPH();
                case BlockType.LOD: return new LOD();
                case BlockType.PCMP: return new PCMP();
                case BlockType.UCMP: return new UCMP();
                case BlockType.IKDA: return new IKDA();
                case BlockType.MaterialDescriptor: return new MaterialDescriptor();
            }

            throw new NotSupportedException();
        }

        public void AddChild(IBlock child)
        {
            this.Blocks.Add(child);
        }

        public IEnumerable<IBlock> GetChildren()
        {
            return this.Blocks;
        }
    }
}

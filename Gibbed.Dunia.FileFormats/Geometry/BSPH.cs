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
    public class BSPH : IBlock
    {
        public BlockType Type
        {
            get { return BlockType.BSPH; }
        }

        public float X;
        public float Y;
        public float Z;
        public float W;

        public void Deserialize(IBlock parent, Stream input)
        {
            this.X = input.ReadValueF32();
            this.Y = input.ReadValueF32();
            this.Z = input.ReadValueF32();
            this.W = input.ReadValueF32();
        }

        public void Serialize(IBlock parent, Stream output)
        {
            throw new NotImplementedException();
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

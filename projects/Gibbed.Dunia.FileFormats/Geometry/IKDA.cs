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
    public class IKDA : IBlock
    {
        public BlockType Type
        {
            get { return BlockType.IKDA; }
        }

        public List<byte[]> Unknown = new List<byte[]>();

        public void Deserialize(IBlock parent, Stream input, Endian endian)
        {
            var count = input.ReadValueU32(endian);

            this.Unknown.Clear();
            for (uint i = 0; i < count; i++)
            {
                var data = new byte[52];
                input.Read(data, 0, data.Length);
                this.Unknown.Add(data);
            }
        }

        public void Serialize(IBlock parent, Stream output, Endian endian)
        {
            throw new NotImplementedException();
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

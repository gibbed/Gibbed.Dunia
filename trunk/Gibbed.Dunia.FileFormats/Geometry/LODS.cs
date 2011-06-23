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
    public class LODS : IBlock
    {
        public BlockType Type
        {
            get { return BlockType.LODS; }
        }

        public void Deserialize(IBlock parent, Stream input)
        {
            var count1 = input.ReadValueS32();
            for (int i = 0; i < count1; i++)
            {
                var unk0 = input.ReadValueF32();
                var count2 = input.ReadValueS32();
                for (int j = 0; j < count2; j++)
                {
                    var unk1 = input.ReadValueU32();
                    var unk2 = input.ReadValueU32();
                    var unk3 = input.ReadValueU32();
                    var unk4 = input.ReadValueU32();
                }

                var count3 = input.ReadValueS32();
                for (int j = 0; j < count3; j++)
                {
                    var unk5 = new byte[28];
                    input.Read(unk5, 0, unk5.Length);
                }

                var unk6 = input.ReadValueU32();
                // data is aligned to 16 bytes, ugh
                input.Seek(input.Position.Align(16), SeekOrigin.Begin);
                var unk7 = new byte[unk6];
                input.Read(unk7, 0, unk7.Length);

                var unk8 = input.ReadValueU32();
                // data is aligned to 16 bytes, ugh
                input.Seek(input.Position.Align(16), SeekOrigin.Begin);
                var unk9 = new byte[unk8 * 2];
                input.Read(unk9, 0, unk9.Length);
            }
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

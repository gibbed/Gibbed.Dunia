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
    public class O2BM : IBlock
    {
        public BlockType Type
        {
            get { return BlockType.O2BM; }
        }

        public List<Matrix> Items = new List<Matrix>();

        public void Deserialize(IBlock parent, Stream input)
        {
            var count = input.ReadValueU32();

            this.Items.Clear();
            for (uint i = 0; i < count; i++)
            {
                var item = new Matrix();
                item.M11 = input.ReadValueF32();
                item.M12 = input.ReadValueF32();
                item.M13 = input.ReadValueF32();
                item.M14 = input.ReadValueF32();
                item.M21 = input.ReadValueF32();
                item.M22 = input.ReadValueF32();
                item.M23 = input.ReadValueF32();
                item.M24 = input.ReadValueF32();
                item.M31 = input.ReadValueF32();
                item.M32 = input.ReadValueF32();
                item.M33 = input.ReadValueF32();
                item.M34 = input.ReadValueF32();
                item.M41 = input.ReadValueF32();
                item.M42 = input.ReadValueF32();
                item.M43 = input.ReadValueF32();
                item.M44 = input.ReadValueF32();
                this.Items.Add(item);
            }
        }

        public void Serialize(IBlock parent, Stream output)
        {
            throw new NotImplementedException();
        }

        public struct Matrix
        {
            public float M11;
            public float M12;
            public float M13;
            public float M14;
            public float M21;
            public float M22;
            public float M23;
            public float M24;
            public float M31;
            public float M32;
            public float M33;
            public float M34;
            public float M41;
            public float M42;
            public float M43;
            public float M44;
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

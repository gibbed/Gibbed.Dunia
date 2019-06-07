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
using System.IO;
using Gibbed.IO;

namespace Gibbed.Dunia.FileFormats
{
    public class GeometryResourceFile : Geometry.IBlockFactory
    {
        public ushort MajorVersion;
        public ushort MinorVersion;
        public uint Unknown08;
        public Geometry.Root Root;

        public void Deserialize(Stream input)
        {
            if (input.Position + 32 > input.Length)
            {
                throw new FormatException();
            }

            if (input.ReadValueU32(Endian.Little) != 0x4D455348)
            {
                throw new FormatException();
            }
            var endian = Endian.Little;

            this.MajorVersion = input.ReadValueU16(endian);
            if (this.MajorVersion != 42)
            {
                throw new FormatException();
            }

            this.MinorVersion = input.ReadValueU16(endian);
            this.Unknown08 = input.ReadValueU32(endian);

            this.Root = (Geometry.Root)DeserializeBlock(null, this, input, endian);
        }

        public Geometry.IBlock CreateBlock(Geometry.BlockType type)
        {
            return type != Geometry.BlockType.Root ? null : new Geometry.Root();
        }

        private static Geometry.IBlock DeserializeBlock(
            Geometry.IBlock parent, Geometry.IBlockFactory factory, Stream input, Endian endian)
        {
            var baseOffset = input.Position;

            var type = (Geometry.BlockType)input.ReadValueU32(endian);
            var block = factory.CreateBlock(type);
            if (block == null || block.Type != type)
            {
                throw new FormatException();
            }

            var unknown04 = input.ReadValueU32(endian);
            var size = input.ReadValueU32(endian);
            var dataSize = input.ReadValueU32(endian);
            var childCount = input.ReadValueU32(endian);

            if (dataSize > size)
            {
                throw new FormatException();
            }

            var childOffset = input.Position;
            var childEnd = childOffset + (size - dataSize - 20);
            var blockOffset = childEnd;
            var blockEnd = blockOffset + dataSize;

            if (blockEnd != baseOffset + size)
            {
                throw new FormatException();
            }

            input.Seek(blockOffset, SeekOrigin.Begin);
            block.Deserialize(parent, input, endian);

            if (input.Position != blockEnd)
            {
                throw new FormatException();
            }

            input.Seek(childOffset, SeekOrigin.Begin);
            for (uint i = 0; i < childCount; i++)
            {
                block.AddChild(DeserializeBlock(block, block, input, endian));
            }

            if (input.Position != childEnd)
            {
                throw new FormatException();
            }

            input.Seek(blockEnd, SeekOrigin.Begin);
            return block;
        }
    }
}

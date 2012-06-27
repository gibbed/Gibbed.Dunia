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

namespace Gibbed.Dunia.FileFormats
{
    public class BinaryResourceFile
    {
        public ushort Flags;
        public Object Root;

        public void Deserialize(Stream input)
        {
            var magic = input.ReadValueU32(Endian.Little);
            if (magic != 0x4643626E) // FCbn
            {
                throw new FormatException();
            }
            var endian = Endian.Little;

            var version = input.ReadValueU16(endian);
            if (version != 2)
            {
                throw new FormatException();
            }

            this.Flags = input.ReadValueU16(endian);
            if (this.Flags != 0)
            {
                throw new FormatException();
            }

            var totalObjectCount = input.ReadValueU32(endian);
            var totalValueCount = input.ReadValueU32(endian);

            var pointers = new List<Object>();
            /*this.Root = new Object();
            this.Root.Deserialize(input, pointers, endian);*/
            this.Root = Object.Deserialize(input, pointers, endian);
        }

        public void Serialize(Stream output)
        {
            var endian = Endian.Little;

            using (var data = new MemoryStream())
            {
                uint totalObjectCount = 0, totalValueCount = 0;

                this.Root.Serialize(
                    data,
                    ref totalObjectCount,
                    ref totalValueCount,
                    endian);
                data.Flush();
                data.Position = 0;

                output.WriteValueU32(0x4643626E, endian);
                output.WriteValueU16(2, endian);
                output.WriteValueU16(0, endian);
                output.WriteValueU32(totalObjectCount, endian);
                output.WriteValueU32(totalValueCount, endian);
                output.WriteFromStream(data, data.Length);
            }
        }

        public class Object
        {
            public long Position;
            public uint TypeHash;
            public Dictionary<uint, byte[]> Values
                = new Dictionary<uint, byte[]>();
            public List<Object> Children
                = new List<Object>();

            public static Object Deserialize(Stream input, List<Object> pointers, Endian endian)
            {
                long position = input.Position;

                bool isOffset;
                var childCount = input.ReadCount(out isOffset, endian);

                if (isOffset == true)
                {
                    return pointers[(int)childCount];
                }

                var child = new Object();
                child.Position = position;
                pointers.Add(child);

                child.Deserialize(input, childCount, pointers, endian);
                return child;
            }

            private void Deserialize(
                Stream input, uint childCount, List<Object> pointers, Endian endian)
            {
                long position;
                bool isOffset;

                /*
                var childCount = input.ReadCount(out isOffset);
                if (isOffset == true)
                {
                    var other = pointers[(int)childCount];

                    this.TypeHash = other.TypeHash;
                    
                    foreach (var kv in other.Values)
                    {
                        this.Values.Add(kv.Key, (byte[])kv.Value.Clone());
                    }
                    
                    this.Children.AddRange(other.Children);
                    return;
                }

                pointers.Add(this);
                */

                this.TypeHash = input.ReadValueU32(endian);

                var valueCount = input.ReadCount(out isOffset, endian);
                if (isOffset == true)
                {
                    throw new NotImplementedException();
                }

                for (var i = 0; i < valueCount; i++)
                {
                    var nameHash = input.ReadValueU32(endian);
                    byte[] value;

                    uint size;
                    position = input.Position;

                    size = input.ReadCount(out isOffset, endian);
                    if (isOffset == true)
                    {
                        input.Seek(position - size, SeekOrigin.Begin);

                        size = input.ReadCount(out isOffset, endian);
                        if (isOffset == true)
                        {
                            throw new FormatException();
                        }

                        value = new byte[size];
                        input.Read(value, 0, value.Length);

                        input.Seek(position, SeekOrigin.Begin);
                        input.ReadCount(out isOffset, endian);
                    }
                    else
                    {
                        value = new byte[size];
                        input.Read(value, 0, value.Length);
                    }

                    this.Values.Add(nameHash, value);
                }

                for (var i = 0; i < childCount; i++)
                {
                    this.Children.Add(Deserialize(input, pointers, endian));
                    /*var child = new Object();
                    child.Deserialize(input, pointers);
                    this.Children.Add(child);*/
                }
            }

            public void Serialize(
                Stream output,
                ref uint totalObjectCount,
                ref uint totalValueCount,
                Endian endian)
            {
                totalObjectCount += (uint)this.Children.Count;
                totalValueCount += (uint)this.Values.Count;

                output.WriteCount(this.Children.Count, false, endian);

                output.WriteValueU32(this.TypeHash, endian);

                output.WriteCount(this.Values.Count, false, endian);
                foreach (var kv in this.Values)
                {
                    output.WriteValueU32(kv.Key, endian);
                    output.WriteCount(kv.Value.Length, false, endian);
                    output.Write(kv.Value, 0, kv.Value.Length);
                }

                foreach (var child in this.Children)
                {
                    child.Serialize(
                        output,
                        ref totalObjectCount,
                        ref totalValueCount,
                        endian);
                }
            }
        }
    }
}

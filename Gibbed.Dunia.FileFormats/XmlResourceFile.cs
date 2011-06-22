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
using System.Text;
using Gibbed.Helpers;

namespace Gibbed.Dunia.FileFormats
{
	public class XmlResourceFile
	{
        public byte Unknown1;
		public Node Root;

		public void Deserialize(Stream input)
		{
			if (input.ReadValueU8() != 0)
			{
				throw new FormatException("not an xml resource file");
			}

			this.Unknown1 = input.ReadValueU8();
            var stringTableSize = input.ReadValuePackedU32();
            var totalNodeCount = input.ReadValuePackedU32();
            var totalAttributeCount = input.ReadValuePackedU32();

            uint actualNodeCount = 1, actualAttributeCount = 0;

			this.Root = new Node();
			this.Root.Deserialize(
                input, ref actualNodeCount, ref actualAttributeCount);

            if (actualNodeCount != totalNodeCount ||
                actualAttributeCount != totalAttributeCount)
            {
                throw new FormatException();
            }

            var stringTableData = new byte[stringTableSize];
            input.Read(stringTableData, 0, stringTableData.Length);
            var stringTable = new StringTable();
            stringTable.Deserialize(stringTableData);

            this.Root.ReadStringTable(stringTable);
		}

        public void Serialize(Stream output)
        {
            var stringTable = new StringTable();
            this.Root.WriteStringTable(stringTable);
            var stringTableData = stringTable.Serialize();

            output.WriteValueU8(0);
            output.WriteValueU8(0);

            using (var data = new MemoryStream())
            {
                uint totalNodeCount = 1, totalAttributeCount = 0;
                this.Root.Serialize(
                    data,
                    ref totalNodeCount,
                    ref totalAttributeCount);

                output.WriteValuePackedU32((uint)stringTableData.Length);
                output.WriteValuePackedU32(totalNodeCount);
                output.WriteValuePackedU32(totalAttributeCount);

                data.Position = 0;
                output.WriteFromStream(data, data.Length);

                output.Write(stringTableData, 0, stringTableData.Length);
            }
        }

        public class Node
        {
            public string Name;
            public string Value;

            internal uint _NameIndex;
            internal uint _ValueIndex;

            public List<Attribute> Attributes = new List<Attribute>();
            public List<Node> Children = new List<Node>();

            public void Deserialize(
                Stream input,
                ref uint totalNodeCount,
                ref uint totalAttributeCount)
            {
                this._NameIndex = input.ReadValuePackedU32();
                this._ValueIndex = input.ReadValuePackedU32();

                var attributeCount = input.ReadValuePackedU32();
                var childCount = input.ReadValuePackedU32();

                totalNodeCount += childCount;
                totalAttributeCount += attributeCount;

                this.Attributes.Clear();
                for (uint i = 0; i < attributeCount; i++)
                {
                    var attribute = new Attribute();
                    attribute.Deserialize(input);
                    this.Attributes.Add(attribute);
                }

                this.Children.Clear();
                for (uint i = 0; i < childCount; i++)
                {
                    var child = new Node();
                    child.Deserialize(
                        input,
                        ref totalNodeCount,
                        ref totalAttributeCount);
                    this.Children.Add(child);
                }
            }

            public void Serialize(
                Stream output,
                ref uint totalNodeCount,
                ref uint totalAttributeCount)
            {
                output.WriteValuePackedU32(this._NameIndex);
                output.WriteValuePackedU32(this._ValueIndex);

                totalAttributeCount += (uint)this.Attributes.Count;
                totalNodeCount += (uint)this.Children.Count;

                output.WriteValuePackedU32((uint)this.Attributes.Count);
                output.WriteValuePackedU32((uint)this.Children.Count);

                foreach (var attribute in this.Attributes)
                {
                    attribute.Serialize(output);
                }

                foreach (var child in this.Children)
                {
                    child.Serialize(
                        output,
                        ref totalNodeCount,
                        ref totalAttributeCount);
                }
            }

            internal void ReadStringTable(StringTable stringTable)
            {
                this.Name = stringTable.Read(this._NameIndex);
                this.Value = stringTable.Read(this._ValueIndex);

                foreach (var attribute in this.Attributes)
                {
                    attribute.ReadStringTable(stringTable);
                }

                foreach (var child in this.Children)
                {
                    child.ReadStringTable(stringTable);
                }
            }

            internal void WriteStringTable(StringTable stringTable)
            {
                this._NameIndex = stringTable.Write(this.Name);
                this._ValueIndex = stringTable.Write(this.Value);

                foreach (var attribute in this.Attributes)
                {
                    attribute.WriteStringTable(stringTable);
                }

                foreach (var child in this.Children)
                {
                    child.WriteStringTable(stringTable);
                }
            }
        }

        public class Attribute
        {
            public uint Unknown;
            public string Name;
            public string Value;

            internal uint _NameIndex;
            internal uint _ValueIndex;

            public void Deserialize(Stream input)
            {
                this.Unknown = input.ReadValuePackedU32();

                if (this.Unknown != 0)
                {
                    throw new FormatException();
                }

                this._NameIndex = input.ReadValuePackedU32();
                this._ValueIndex = input.ReadValuePackedU32();
            }

            public void Serialize(Stream output)
            {
                output.WriteValuePackedU32(this.Unknown);
                output.WriteValuePackedU32(this._NameIndex);
                output.WriteValuePackedU32(this._ValueIndex);
            }

            internal void ReadStringTable(StringTable stringTable)
            {
                this.Name = stringTable.Read(this._NameIndex);
                this.Value = stringTable.Read(this._ValueIndex);
            }

            internal void WriteStringTable(StringTable stringTable)
            {
                this._NameIndex = stringTable.Write(this.Name);
                this._ValueIndex = stringTable.Write(this.Value);
            }
        }

        internal class StringTable
        {
            private MemoryStream Data = new MemoryStream();

            // this is dumb :effort:
            private Dictionary<uint, string> Offsets = new Dictionary<uint, string>();
            private Dictionary<string, uint> Values = new Dictionary<string, uint>();

            public string Read(uint index)
            {
                if (this.Offsets.ContainsKey(index) == false)
                {
                    throw new KeyNotFoundException();
                }

                return this.Offsets[index];
            }

            public uint Write(string value)
            {
                if (this.Values.ContainsKey(value) == true)
                {
                    return this.Values[value];
                }

                var offset = (uint)this.Data.Position;
                this.Offsets.Add(offset, value);
                this.Values.Add(value, offset);
                this.Data.WriteStringZ(value, Encoding.UTF8);
                return offset;
            }

            public void Deserialize(byte[] buffer)
            {
                this.Offsets.Clear();
                this.Values.Clear();

                this.Data = new MemoryStream(buffer);

                while (this.Data.Position < this.Data.Length)
                {
                    var offset = (uint)this.Data.Position;
                    var value = this.Data.ReadStringZ(Encoding.UTF8);
                    this.Offsets.Add(offset, value);
                    this.Values.Add(value, offset);
                }
            }

            public byte[] Serialize()
            {
                var buffer = new byte[this.Data.Length];
                Array.Copy(this.Data.GetBuffer(), buffer, buffer.Length);
                return buffer;
            }
        }
	}
}

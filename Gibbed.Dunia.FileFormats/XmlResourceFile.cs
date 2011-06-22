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

namespace Gibbed.Dunia.FileFormats
{
	public class XmlResourceStringTable
	{
		protected byte[] Data;

		public void Deserialize(Stream input, uint size)
		{
			this.Data = new byte[size];
			input.Read(this.Data, 0, (int)size);
		}

        public string this[uint index]
		{
			get
			{
                throw new NotImplementedException(); //return this.Data.ToStringUTF8Z(index);
			}
		}
	}

	public class XmlResourceAttribute
	{
		public uint Unknown;
		public string Name;
        public uint NameIndex;
		public string Value;
        public uint ValueIndex;

		public void Read(Stream stream)
		{
			this.Unknown = stream.ReadCount();
			this.NameIndex = stream.ReadCount();
			this.ValueIndex = stream.ReadCount();
		}

		public void Resolve(XmlResourceStringTable strings)
		{
			this.Name = strings[this.NameIndex];
			this.Value = strings[this.ValueIndex];
		}
	}

	public class XmlResourceNode
	{
		public string Name;
        public uint NameIndex;
		public string Value;
        public uint ValueIndex;

		public List<XmlResourceAttribute> Attributes;
		public List<XmlResourceNode> Children;

		public void Deserialize(Stream stream)
		{
			this.NameIndex = stream.ReadCount();
			this.ValueIndex = stream.ReadCount();
			
			var attributeCount = stream.ReadCount();
			var childCount = stream.ReadCount();

			this.Attributes = new List<XmlResourceAttribute>();
			for (int i = 0; i < attributeCount; i++)
			{
				XmlResourceAttribute attribute = new XmlResourceAttribute();
				attribute.Read(stream);
				this.Attributes.Add(attribute);
			}

			this.Children = new List<XmlResourceNode>();
			for (int i = 0; i < childCount; i++)
			{
				XmlResourceNode child = new XmlResourceNode();
				child.Deserialize(stream);
				this.Children.Add(child);
			}
		}

		public void Resolve(XmlResourceStringTable strings)
		{
			this.Name = strings[this.NameIndex];
			this.Value = strings[this.ValueIndex];

			foreach (XmlResourceAttribute attribute in this.Attributes)
			{
				attribute.Resolve(strings);
			}

			foreach (XmlResourceNode child in this.Children)
			{
				child.Resolve(strings);
			}
		}
	}

	public class XmlResourceFile
	{
		public XmlResourceNode Root;

		public void Deserialize(Stream stream)
		{
			if (stream.ReadValueU8() != 0)
			{
				throw new FormatException("not an xml resource file");
			}

			var unknown1 = stream.ReadValueU8();
            var stringTableSize = stream.ReadCount();
			var unknown3 = stream.ReadCount();
			var unknown4 = stream.ReadCount();

			this.Root = new XmlResourceNode();
			this.Root.Deserialize(stream);

			var strings = new XmlResourceStringTable();
			strings.Deserialize(stream, stringTableSize);

			this.Root.Resolve(strings);
		}
	}
}

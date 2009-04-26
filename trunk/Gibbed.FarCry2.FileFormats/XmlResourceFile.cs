using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gibbed.Helpers;
using Gibbed.FarCry2.Helpers;

namespace Gibbed.FarCry2.FileFormats
{
	public static class XmlResourceFileHelpers
	{
		public static UInt32 ReadPackedU32(this Stream stream)
		{
			byte value = stream.ReadU8();

			if (value < 0xFE)
			{
				return value;
			}

			return stream.ReadU32();
		}
	}

	public class XmlResourceStringTable
	{
		protected byte[] Data;

		public void Read(Stream input, uint size)
		{
			this.Data = new byte[size];
			input.Read(this.Data, 0, (int)size);
		}

		public string this[uint index]
		{
			get
			{
				return this.Data.ReadUTF8Z(index);
			}
		}
	}

	public class XmlResourceAttribute
	{
		public UInt32 Unknown;
		public string Name;
		public UInt32 NameIndex;
		public string Value;
		public UInt32 ValueIndex;

		public void Read(Stream stream)
		{
			this.Unknown = stream.ReadPackedU32();
			this.NameIndex = stream.ReadPackedU32();
			this.ValueIndex = stream.ReadPackedU32();
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
		public UInt32 NameIndex;
		public string Value;
		public UInt32 ValueIndex;

		public List<XmlResourceAttribute> Attributes;
		public List<XmlResourceNode> Children;

		public void Read(Stream stream)
		{
			this.NameIndex = stream.ReadPackedU32();
			this.ValueIndex = stream.ReadPackedU32();
			
			uint attributeCount = stream.ReadPackedU32();
			uint childCount = stream.ReadPackedU32();

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
				child.Read(stream);
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

		public void Read(Stream stream)
		{
			if (stream.ReadU8() != 0)
			{
				throw new FileFormatException("not an xml resource file");
			}

			byte unknown1 = stream.ReadU8();
			uint stringTableSize = stream.ReadPackedU32();
			uint unknown3 = stream.ReadPackedU32();
			uint unknown4 = stream.ReadPackedU32();

			this.Root = new XmlResourceNode();
			this.Root.Read(stream);

			XmlResourceStringTable strings = new XmlResourceStringTable();
			strings.Read(stream, stringTableSize);

			this.Root.Resolve(strings);
		}
	}
}

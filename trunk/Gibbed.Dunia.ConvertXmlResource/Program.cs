using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Gibbed.Dunia.FileFormats;

namespace Gibbed.Dunia.ConvertXmlResource
{
	class Program
	{
		private static void WriteNode(XmlWriter writer, XmlResourceNode node)
		{
			writer.WriteStartElement(node.Name);

			foreach (XmlResourceAttribute attribute in node.Attributes)
			{
				writer.WriteAttributeString(attribute.Name, attribute.Value);
			}

			foreach (XmlResourceNode child in node.Children)
			{
				WriteNode(writer, child);
			}

			if (node.Value != null && node.Value.Length > 0)
			{
				writer.WriteValue(node.Value);
			}

			writer.WriteEndElement();
		}

		public static void Main(string[] args)
		{
			if (args.Length != 2)
			{
				Console.WriteLine("{0} [input_binary.rml] [output.xml]", Path.GetFileName(Application.ExecutablePath));
				return;
			}

			Stream input = File.OpenRead(args[0]);
			XmlResourceFile resource = new XmlResourceFile();
			resource.Read(input);
			input.Close();

			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Encoding = Encoding.UTF8;
			settings.Indent = true;
			settings.OmitXmlDeclaration = true;

			XmlWriter writer = XmlWriter.Create(args[1], settings);
			writer.WriteStartDocument();
			WriteNode(writer, resource.Root);
			writer.WriteEndDocument();
			writer.Flush();
			writer.Close();
		}
	}
}

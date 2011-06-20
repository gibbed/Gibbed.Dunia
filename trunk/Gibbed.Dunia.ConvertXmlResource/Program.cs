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
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Gibbed.Dunia.FileFormats;

namespace Gibbed.Dunia.ConvertXmlResource
{
	internal class Program
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

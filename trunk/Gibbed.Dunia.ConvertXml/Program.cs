﻿/* Copyright (c) 2011 Rick (rick 'at' gibbed 'dot' us)
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
using System.Xml;
using System.Xml.XPath;
using Gibbed.Dunia.FileFormats;
using NDesk.Options;

namespace Gibbed.Dunia.ConvertXml
{
	public class Program
	{
		private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }

        public static void Main(string[] args)
        {
            var mode = Mode.Unknown;
            bool showHelp = false;

            OptionSet options = new OptionSet()
            {
                {
                    "rml",
                    "convert XML to RML", 
                    v => mode = v != null ? Mode.ToRML : mode
                },
                {
                    "xml",
                    "convert RML to XML", 
                    v => mode = v != null ? Mode.ToXML : mode
                },
                {
                    "h|help",
                    "show this message and exit", 
                    v => showHelp = v != null
                },
            };

            List<string> extras;

            try
            {
                extras = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("{0}: ", GetExecutableName());
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `{0} --help' for more information.", GetExecutableName());
                return;
            }

            if (mode == Mode.Unknown &&
                extras.Count >= 1)
            {
                var extension = Path.GetExtension(extras[0]);
                
                if (extension == ".rml")
                {
                    mode = Mode.ToXML;
                }
                else if (extension == ".xml")
                {
                    mode = Mode.ToRML;
                }
            }

            if (showHelp == true ||
                mode == Mode.Unknown ||
                extras.Count < 1 ||
                extras.Count > 2)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ input [output]", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            if (mode == Mode.ToRML)
            {
                string inputPath = extras[0];
                string outputPath = extras.Count > 1 ? extras[1] : Path.ChangeExtension(Path.ChangeExtension(inputPath, null) + "_converted", ".rml");

                var rez = new XmlResourceFile();
                using (var input = File.OpenRead(inputPath))
                {
                    var doc = new XPathDocument(input);
                    var nav = doc.CreateNavigator();

                    if (nav.MoveToFirstChild() == false)
                    {
                        throw new FormatException();
                    }

                    rez.Root = ReadNode(nav);
                }

                using (var output = File.Create(outputPath))
                {
                    rez.Serialize(output);
                }
            }
            else if (mode == Mode.ToXML)
            {
                string inputPath = extras[0];
                string outputPath = extras.Count > 1 ? extras[1] : Path.ChangeExtension(Path.ChangeExtension(inputPath, null) + "_converted", ".xml");

                var rez = new XmlResourceFile();
                using (var input = File.OpenRead(inputPath))
                {
                    rez.Deserialize(input);
                }
                
                var settings = new XmlWriterSettings();
                settings.Encoding = Encoding.UTF8;
                settings.Indent = true;
                settings.OmitXmlDeclaration = true;

                using (var writer = XmlWriter.Create(outputPath, settings))
                {
                    writer.WriteStartDocument();
                    WriteNode(writer, rez.Root);
                    writer.WriteEndDocument();
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
		}

        public static XmlResourceFile.Node ReadNode(XPathNavigator nav)
        {
            var node = new XmlResourceFile.Node();
            node.Name = nav.Name;
            node.Value = nav.Value;

            if (nav.MoveToFirstAttribute() == true)
            {
                node.Attributes = new List<XmlResourceFile.Attribute>();

                do
                {
                    node.Attributes.Add(new XmlResourceFile.Attribute()
                        {
                            Name = nav.Name,
                            Value = nav.Value,
                        });
                }
                while (nav.MoveToNextAttribute() == true);
                nav.MoveToParent();
            }

            var children = nav.SelectChildren(XPathNodeType.Element);
            if (children.Count > 0)
            {
                node.Children = new List<XmlResourceFile.Node>();
                while (children.MoveNext() == true)
                {
                    node.Children.Add(ReadNode(children.Current));
                }
            }

            return node;
        }

        public static void WriteNode(XmlWriter writer, XmlResourceFile.Node node)
        {
            writer.WriteStartElement(node.Name);

            foreach (var attribute in node.Attributes)
            {
                writer.WriteAttributeString(attribute.Name, attribute.Value);
            }

            foreach (var child in node.Children)
            {
                WriteNode(writer, child);
            }

            if (node.Value != null && node.Value.Length > 0)
            {
                writer.WriteValue(node.Value);
            }

            writer.WriteEndElement();
        }
	}
}

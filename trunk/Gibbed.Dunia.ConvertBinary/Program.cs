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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Gibbed.Dunia.FileFormats;
using NDesk.Options;

namespace Gibbed.Dunia.ConvertBinary
{
    internal class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }

        public static void Main(string[] args)
        {
            var mode = Mode.Unknown;
            bool multiExport = true;
            bool showHelp = false;

            var options = new OptionSet()
            {
                {
                    "fcb",
                    "convert XML to FCB", 
                    v => mode = v != null ? Mode.ToFCB : mode
                },
                {
                    "xml",
                    "convert FCB to XML", 
                    v => mode = v != null ? Mode.ToXML : mode
                },
                {
                    "m|multi-export",
                    "when converting FCB to XML, export to many files when possible",
                    v => multiExport = v != null
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
                
                if (extension == ".fcb")
                {
                    mode = Mode.ToXML;
                }
                else if (extension == ".xml")
                {
                    mode = Mode.ToFCB;
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

            Console.WriteLine("Loading project...");

            var manager = ProjectData.Manager.Load();
            IDefinitionProvider defs = null;

            if (manager.ActiveProject == null)
            {
                Console.WriteLine("Warning: no active project loaded.");
                return;
            }
            else
            {
                defs = Definitions.Load(Path.Combine(manager.ActiveProject.ListsPath, "binary_classes.xml"));
            }

            if (mode == Mode.ToFCB)
            {
                string inputPath = extras[0];
                string outputPath;
                string basePath;

                if (extras.Count > 1)
                {
                    outputPath = extras[1];
                }
                else
                {
                    outputPath = Path.ChangeExtension(inputPath, null);
                    outputPath += "_converted.fcb";
                }

                basePath = Path.ChangeExtension(inputPath, null);

                inputPath = Path.GetFullPath(inputPath);
                outputPath = Path.GetFullPath(outputPath);
                basePath = Path.GetFullPath(basePath);

                var bf = new BinaryResourceFile();

                using (var input = File.OpenRead(inputPath))
                {
                    Console.WriteLine("Loading XML...");
                    var doc = new XPathDocument(input);
                    var nav = doc.CreateNavigator();

                    var root = nav.SelectSingleNode("/object");
                    if (root == null)
                    {
                        throw new FormatException();
                    }

                    Console.WriteLine("Reading XML...");
                    bf.Root = ReadNode(basePath, root);
                }

                Console.WriteLine("Writing FCB...");
                using (var output = File.Create(outputPath))
                {
                    bf.Serialize(output);
                }
            }
            else if (mode == Mode.ToXML)
            {
                string inputPath = extras[0];
                string outputPath;
                string basePath;
                if (extras.Count > 1)
                {
                    outputPath = extras[1];
                    basePath = Path.ChangeExtension(outputPath, null);
                }
                else
                {
                    outputPath = Path.ChangeExtension(inputPath, null);
                    outputPath += "_converted";
                    basePath = outputPath;
                    outputPath += ".xml";
                }

                inputPath = Path.GetFullPath(inputPath);
                outputPath = Path.GetFullPath(outputPath);
                basePath = Path.GetFullPath(basePath);

                Console.WriteLine("Reading binary...");

                var bf = new BinaryResourceFile();
                using (var input = File.OpenRead(inputPath))
                {
                    bf.Deserialize(input);
                }
                
                var settings = new XmlWriterSettings();
                settings.Encoding = Encoding.UTF8;
                settings.Indent = true;
                settings.CheckCharacters = false;
                settings.OmitXmlDeclaration = true;

                Console.WriteLine("Writing XML...");

                if (multiExport == true &&
                    bf.Root.Values.Count == 0 &&
                    bf.Root.TypeHash == 0xBCDD10B4 &&
                    bf.Root.Children.Where(c => c.TypeHash != 0xE0BDB3DB).Count() == 0)
                {
                    using (var writer = XmlWriter.Create(outputPath, settings))
                    {
                        writer.WriteStartDocument();

                        var root = bf.Root;
                        {
                            writer.WriteStartElement("object");

                            var def = defs.GetClassDefinition(root.TypeHash);

                            if (def.Name != null)
                            {
                                writer.WriteAttributeString("type", def.Name);
                            }

                            writer.WriteAttributeString("hash", root.TypeHash.ToString("X8"));

                            int counter = 0;
                            int padLength = root.Children.Count.ToString().Length;
                            foreach (var child in root.Children)
                            {
                                counter++;

                                string childName = counter.ToString().PadLeft(padLength, '0');

                                // name
                                if (child.Values.ContainsKey(0xFE11D138) == true)
                                {
                                    var value = child.Values[0xFE11D138];
                                    childName += "_" + Encoding.UTF8.GetString(value, 0, value.Length - 1);
                                }

                                Directory.CreateDirectory(basePath);

                                var childPath = Path.Combine(basePath, childName + ".xml");
                                using (var childWriter = XmlWriter.Create(childPath, settings))
                                {
                                    childWriter.WriteStartDocument();
                                    WriteNode(childWriter, child, defs);
                                    childWriter.WriteEndDocument();
                                }

                                writer.WriteStartElement("object");
                                writer.WriteAttributeString("external", Path.GetFileName(childPath));
                                writer.WriteEndElement();
                            }

                            writer.WriteEndElement();
                        }

                        writer.WriteEndDocument();
                    }
                }
                else
                {
                    using (var writer = XmlWriter.Create(outputPath, settings))
                    {
                        writer.WriteStartDocument();
                        WriteNode(writer, bf.Root, defs);
                        writer.WriteEndDocument();
                    }
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private static BinaryResourceFile.Object LoadNode(string basePath, XPathNavigator node)
        {
            string external = node.GetAttribute("external", "");
            if (string.IsNullOrWhiteSpace(external) == true)
            {
                return ReadNode(basePath, node);
            }

            var inputPath = Path.Combine(basePath, external);

            using (var input = File.OpenRead(inputPath))
            {
                Console.WriteLine("Loading object from '{0}'...", Path.GetFileName(inputPath));
                var doc = new XPathDocument(input);
                var nav = doc.CreateNavigator();

                var root = nav.SelectSingleNode("/object");
                if (root == null)
                {
                    throw new InvalidOperationException();
                }

                return LoadNode(Path.ChangeExtension(inputPath, null), root);
            }
        }

        private static BinaryResourceFile.Object ReadNode(string basePath, XPathNavigator node)
        {
            string className;
            uint classHash;

            Definitions.LoadTypeAndHash(node, out className, out classHash);

            var parent = new BinaryResourceFile.Object();
            parent.TypeHash = classHash;

            var values = node.Select("value");
            while (values.MoveNext() == true)
            {
                string valueName;
                uint valueHash;

                Definitions.LoadNameAndHash(values.Current, out valueName, out valueHash);

                ValueType valueType;
                string _valueType;
                _valueType = values.Current.GetAttribute("type", "");
                if (string.IsNullOrWhiteSpace(_valueType) == true ||
                    Enum.IsDefined(typeof(ValueType), _valueType) == false)
                {
                    throw new FormatException();
                }
                valueType = (ValueType)Enum.Parse(typeof(ValueType), _valueType);

                byte[] valueData;
                switch (valueType)
                {
                    case ValueType.BinHex:
                    {
                        using (var reader = new XmlTextReader(new StringReader(values.Current.OuterXml)))
                        {
                            reader.MoveToContent();
                            valueData = new byte[0];
                            int read = 0;
                            do
                            {
                                Array.Resize(ref valueData, valueData.Length + 4096);
                                read += reader.ReadBinHex(valueData, read, 4096);
                            }
                            while (reader.EOF == false);
                            Array.Resize(ref valueData, read);
                        }

                        break;
                    }

                    case ValueType.Hash:
                    {
                        valueData = BitConverter.GetBytes(uint.Parse(values.Current.Value, NumberStyles.AllowHexSpecifier));
                        break;
                    }

                    case ValueType.String:
                    {
                        valueData = Encoding.UTF8.GetBytes(values.Current.Value);
                        Array.Resize(ref valueData, valueData.Length + 1);
                        break;
                    }

                    case ValueType.Bool:
                    {
                        valueData = new byte[1];
                        valueData[0] = (byte)(bool.Parse(values.Current.Value) == true ? 1 : 0);
                        break;
                    }

                    case ValueType.UInt32:
                    {
                        valueData = BitConverter.GetBytes(uint.Parse(values.Current.Value));
                        break;
                    }

                    case ValueType.UInt64:
                    {
                        valueData = BitConverter.GetBytes(ulong.Parse(values.Current.Value));
                        break;
                    }

                    case ValueType.Float:
                    {
                        valueData = BitConverter.GetBytes(float.Parse(values.Current.Value));
                        break;
                    }

                    case ValueType.Vector2:
                    {
                        valueData = new byte[8];
                        Array.Copy(BitConverter.GetBytes(float.Parse(values.Current.SelectSingleNode("x").Value)), 0, valueData, 0, 4);
                        Array.Copy(BitConverter.GetBytes(float.Parse(values.Current.SelectSingleNode("y").Value)), 0, valueData, 4, 4);
                        break;
                    }

                    case ValueType.Vector3:
                    {
                        valueData = new byte[12];
                        Array.Copy(BitConverter.GetBytes(float.Parse(values.Current.SelectSingleNode("x").Value)), 0, valueData, 0, 4);
                        Array.Copy(BitConverter.GetBytes(float.Parse(values.Current.SelectSingleNode("y").Value)), 0, valueData, 4, 4);
                        Array.Copy(BitConverter.GetBytes(float.Parse(values.Current.SelectSingleNode("z").Value)), 0, valueData, 8, 4);
                        break;
                    }

                    default:
                    {
                        throw new FormatException();
                    }
                }

                parent.Values.Add(valueHash, valueData);
            }

            var children = node.Select("object");
            while (children.MoveNext() == true)
            {
                parent.Children.Add(LoadNode(basePath, children.Current));
            }

            return parent;
        }

        private static void WriteNode(
            XmlWriter writer,
            BinaryResourceFile.Object node,
            IDefinitionProvider provider)
        {
            var def = provider.GetClassDefinition(node.TypeHash);

            writer.WriteStartElement("object");

            if (def.Name != null)
            {
                writer.WriteAttributeString("type", def.Name);
            }
            else
            {
                writer.WriteAttributeString("hash", node.TypeHash.ToString("X8"));
            }

            if (node.Values != null &&
                node.Values.Count > 0)
            {
                foreach (var kv in node.Values)
                {
                    writer.WriteStartElement("value");

                    var member = def.GetMemberDefinition(kv.Key);

                    if (member != null && member.Name != null)
                    {
                        writer.WriteAttributeString("name", member.Name);
                    }
                    else
                    {
                        writer.WriteAttributeString("hash", kv.Key.ToString("X8"));
                    }

                    if (member == null)
                    {
                        writer.WriteAttributeString("type", ValueType.BinHex.ToString());
                        writer.WriteBinHex(kv.Value, 0, kv.Value.Length);
                    }
                    else
                    {
                        writer.WriteAttributeString("type", member.Type.ToString());

                        switch (member.Type)
                        {
                            case ValueType.BinHex:
                            {
                                writer.WriteBinHex(kv.Value, 0, kv.Value.Length);
                                break;
                            }

                            case ValueType.Hash:
                            {
                                if (kv.Value.Length != 4)
                                {
                                    throw new FormatException();
                                }

                                writer.WriteValue(BitConverter.ToUInt32(kv.Value, 0).ToString("X8"));
                                break;
                            }

                            case ValueType.String:
                            {
                                if (kv.Value.Length < 1)
                                {
                                    throw new FormatException();
                                }
                                else if (kv.Value[kv.Value.Length - 1] != 0)
                                {
                                    throw new FormatException();
                                }

                                writer.WriteValue(Encoding.UTF8.GetString(kv.Value, 0, kv.Value.Length - 1));
                                break;
                            }

                            case ValueType.Bool:
                            {
                                if (kv.Value.Length != 1)
                                {
                                    throw new FormatException();
                                }

                                writer.WriteValue(kv.Value[0] != 0 ? true : false);
                                break;
                            }

                            case ValueType.Float:
                            {
                                if (kv.Value.Length != 4)
                                {
                                    throw new FormatException();
                                }

                                writer.WriteValue(BitConverter.ToSingle(kv.Value, 0));
                                break;
                            }

                            case ValueType.UInt32:
                            {
                                if (kv.Value.Length != 4)
                                {
                                    throw new FormatException();
                                }

                                writer.WriteValue(BitConverter.ToUInt32(kv.Value, 0).ToString());
                                break;
                            }

                            case ValueType.UInt64:
                            {
                                if (kv.Value.Length != 8)
                                {
                                    throw new FormatException();
                                }

                                writer.WriteValue(BitConverter.ToUInt64(kv.Value, 0).ToString());
                                break;
                            }

                            case ValueType.Vector2:
                            {
                                if (kv.Value.Length != 4 * 2)
                                {
                                    throw new FormatException();
                                }

                                writer.WriteElementString("x", BitConverter.ToSingle(kv.Value, 0).ToString());
                                writer.WriteElementString("y", BitConverter.ToSingle(kv.Value, 4).ToString());
                                break;
                            }

                            case ValueType.Vector3:
                            {
                                if (kv.Value.Length != 4 * 3)
                                {
                                    throw new FormatException();
                                }

                                writer.WriteElementString("x", BitConverter.ToSingle(kv.Value, 0).ToString());
                                writer.WriteElementString("y", BitConverter.ToSingle(kv.Value, 4).ToString());
                                writer.WriteElementString("z", BitConverter.ToSingle(kv.Value, 8).ToString());
                                break;
                            }

                            default:
                            {
                                throw new NotSupportedException();
                            }
                        }
                    }

                    writer.WriteEndElement();
                }
            }

            if (node.Children != null &&
                node.Children.Count > 0)
            {
                foreach (var child in node.Children)
                {
                    WriteNode(writer, child, def);
                }
            }

            writer.WriteEndElement();
        }
    }
}

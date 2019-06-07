/* Copyright (c) 2019 Rick (rick 'at' gibbed 'dot' us)
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
using System.Globalization;
using Gibbed.Dunia.FileFormats;
using Big = Gibbed.Dunia.FileFormats.Big;
using Gibbed.FarCry2.FileFormats;
using System.Xml;
using System.Linq;
using Gibbed.IO;
using System.Text;
using NDesk.Options;
using System.Xml.XPath;
using Map = Gibbed.FarCry2.FileFormats.Map;
using CompressionScheme = Gibbed.Dunia.FileFormats.Big.CompressionScheme;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace Gibbed.FarCry2.MapPack
{
    public class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }

        private static Map.CompressedData MakeCompressedData(MemoryStream input)
        {
            var cd = new Map.CompressedData();

            using (var data = new MemoryStream())
            {
                input.Position = 0;
                uint virtualOffset = 0;
                uint realOffset = 4;
                while (input.Position < input.Length)
                {
                    var length = (uint)Math.Min(0x40000, input.Length - input.Position);

                    using (var block = new MemoryStream())
                    {
                        var zlib = new DeflaterOutputStream(block, new Deflater(9, false));
                        zlib.WriteFromStream(input, length);
                        zlib.Finish();

                        cd.Blocks.Add(new Map.CompressedData.Block()
                        {
                            VirtualOffset = virtualOffset,
                            FileOffset = realOffset,
                            IsCompressed = true,
                        });

                        block.Position = 0;
                        data.WriteFromStream(block, block.Length);

                        realOffset += (uint)block.Length;
                    }

                    virtualOffset += length;
                }

                data.Position = 0;
                cd.Data = new byte[data.Length];
                data.Read(cd.Data, 0, cd.Data.Length);

                cd.Blocks.Add(new Map.CompressedData.Block()
                {
                    VirtualOffset = virtualOffset,
                    FileOffset = realOffset,
                    IsCompressed = true,
                });
            }

            return cd;
        }

        public static void Main(string[] args)
        {
            bool showHelp = false;
            bool overwriteFiles = false;
            bool verbose = false;

            var options = new OptionSet()
            {
                {
                    "v|verbose",
                    "be verbose",
                    v => verbose = v != null
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

            if (extras.Count < 1 || extras.Count > 2 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_fat [output_dir]", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            string inputPath = extras[0];
            string outputPath = extras.Count > 1 ? extras[1] : Path.ChangeExtension(inputPath, null) + ".fc2map";

            var map = new MapFile();
            map.Info = new Map.Info();
            map.Snapshot = new Map.Snapshot();
            map.Data = new Map.Data();
            map.Data.Unknown2 = new Map.Snapshot();
            map.Archive = new Map.Archive();

            using (var input = File.OpenRead(Path.Combine(inputPath, "map.xml")))
            {
                var doc = new XPathDocument(input);
                var nav = doc.CreateNavigator();

                var info = nav.SelectSingleNode("/map/info");
                map.Info.Name = info.SelectSingleNode("name").Value;
                map.Info.Creator = info.SelectSingleNode("creator").Value;
                map.Info.Author = info.SelectSingleNode("author").Value;
                map.Info.Size = (Map.Size)Enum.Parse(typeof(Map.Size), info.SelectSingleNode("size").Value);
                map.Info.Players = (Map.Players)Enum.Parse(typeof(Map.Players), info.SelectSingleNode("players").Value);
                map.Info.Unknown2 = uint.Parse(info.SelectSingleNode("unknown2").Value, CultureInfo.InvariantCulture);
                map.Info.Unknown3 = uint.Parse(info.SelectSingleNode("unknown3").Value, CultureInfo.InvariantCulture);
                map.Info.Unknown4 = uint.Parse(info.SelectSingleNode("unknown4").Value, CultureInfo.InvariantCulture);
                map.Info.Unknown5 = ulong.Parse(info.SelectSingleNode("unknown5").Value, CultureInfo.InvariantCulture);
                map.Info.Unknown7 = ulong.Parse(info.SelectSingleNode("unknown7").Value, CultureInfo.InvariantCulture);
                map.Info.Unknown10 = ulong.Parse(info.SelectSingleNode("unknown10").Value, CultureInfo.InvariantCulture);

                using (var reader = new XmlTextReader(new StringReader(info.SelectSingleNode("unknown11").OuterXml)))
                {
                    reader.MoveToContent();
                    map.Info.Unknown11 = new byte[0];
                    int read = 0;
                    do
                    {
                        Array.Resize(ref map.Info.Unknown11, map.Info.Unknown11.Length + 4096);
                        read += reader.ReadBinHex(map.Info.Unknown11, read, 4096);
                    }
                    while (reader.EOF == false);
                    Array.Resize(ref map.Info.Unknown11, read);
                }

                using (var reader = new XmlTextReader(new StringReader(info.SelectSingleNode("unknown12").OuterXml)))
                {
                    reader.MoveToContent();
                    map.Info.Unknown12 = new byte[0];
                    int read = 0;
                    do
                    {
                        Array.Resize(ref map.Info.Unknown12, map.Info.Unknown12.Length + 4096);
                        read += reader.ReadBinHex(map.Info.Unknown12, read, 4096);
                    }
                    while (reader.EOF == false);
                    Array.Resize(ref map.Info.Unknown12, read);
                }

                map.Info.Unknown15 = uint.Parse(info.SelectSingleNode("unknown15").Value, CultureInfo.InvariantCulture);

                var snapshot = nav.SelectSingleNode("/map/snapshot");
                map.Snapshot.Width = uint.Parse(snapshot.SelectSingleNode("width").Value, CultureInfo.InvariantCulture);
                map.Snapshot.Height = uint.Parse(snapshot.SelectSingleNode("height").Value, CultureInfo.InvariantCulture);
                map.Snapshot.BytesPerPixel = uint.Parse(snapshot.SelectSingleNode("bpp").Value, CultureInfo.InvariantCulture);
                map.Snapshot.Unknown4 = uint.Parse(snapshot.SelectSingleNode("unknown4").Value, CultureInfo.InvariantCulture);

                var data = nav.SelectSingleNode("/map/data");
                map.Data.Unknown1 = data.SelectSingleNode("unknown1").Value;
            }

            using (var input = File.OpenRead(Path.Combine(inputPath, "snapshot.bin")))
            {
                map.Snapshot.Data = new byte[input.Length];
                input.Read(map.Snapshot.Data, 0, map.Snapshot.Data.Length);
            }

            var paths = new SortedDictionary<uint, string>();

            if (verbose == true)
            {
                Console.WriteLine("Finding files...");
            }

            var dataPath = Path.Combine(inputPath, "archive");
            dataPath = Path.GetFullPath(dataPath);

            if (dataPath.EndsWith(Path.DirectorySeparatorChar.ToString()) == true)
            {
                dataPath = dataPath.Substring(0, dataPath.Length - 1);
            }

            foreach (string path in Directory.GetFiles(dataPath, "*", SearchOption.AllDirectories))
            {
                string fullPath = Path.GetFullPath(path);
                string partPath = fullPath.Substring(dataPath.Length + 1).ToLowerInvariant();

                uint hash = 0xFFFFFFFFu;
                if (partPath.ToUpper().StartsWith("__UNKNOWN") == true)
                {
                    string partName;
                    partName = Path.GetFileNameWithoutExtension(partPath);
                    if (partName.Length > 8)
                    {
                        partName = partName.Substring(0, 8);
                    }

                    hash = uint.Parse(
                        partName,
                        System.Globalization.NumberStyles.AllowHexSpecifier);
                }
                else
                {
                    hash = partPath.HashFileNameCRC32();
                }

                if (paths.ContainsKey(hash) == true)
                {
                    continue;
                }

                paths[hash] = fullPath;
            }

            var big = new BigFile();

            using (var output = new MemoryStream())
            {
                foreach (var value in paths)
                {
                    var hash = value.Key;
                    var path = value.Value;

                    if (verbose == true)
                    {
                        Console.WriteLine(path);
                    }

                    var entry = new Big.Entry();
                    entry.NameHash = hash;
                    entry.Offset = output.Position;

                    using (var input = File.OpenRead(path))
                    {
                        entry.CompressionScheme = Big.CompressionScheme.None;
                        entry.UncompressedSize = 0;
                        entry.CompressedSize = (uint)input.Length;
                        output.WriteFromStream(input, input.Length);
                    }

                    big.Entries.Add(entry);
                }

                map.Archive.DAT = MakeCompressedData(output);
            }

            using (var output = new MemoryStream())
            {
                big.Serialize(output);
                map.Archive.FAT = MakeCompressedData(output);
            }

            using (var output = new MemoryStream())
            {
                var settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.OmitXmlDeclaration = true;
                settings.IndentChars = "\t";
                settings.Encoding = Encoding.ASCII;

                using (var writer = XmlWriter.Create(output, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("FatInfo");

                    foreach (var entry in big.Entries.OrderBy(e => e.NameHash))
                    {
                        writer.WriteStartElement("File");
                        writer.WriteAttributeString("Path", entry.NameHash.ToString(CultureInfo.InvariantCulture));
                        writer.WriteAttributeString("Crc", entry.NameHash.ToString(CultureInfo.InvariantCulture));
                        writer.WriteAttributeString("FileTime", "0");
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }

                output.Position = 0;
                map.Archive.XML = MakeCompressedData(output);
            }

            using (var output = File.Create(outputPath))
            {
                map.Serialize(output);
            }
        }
    }
}

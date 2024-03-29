﻿/* Copyright (c) 2021 Rick (rick 'at' gibbed 'dot' us)
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
using System.Xml;
using Gibbed.Dunia.FileFormats;
using Gibbed.Dunia.Packing;
using Gibbed.FarCry2.FileFormats;
using Gibbed.IO;
using NDesk.Options;

namespace Gibbed.FarCry2.MapUnpack
{
    public class Program
    {
        private static string GetExecutablePath()
        {
            using var process = System.Diagnostics.Process.GetCurrentProcess();
            var path = Path.GetFullPath(process.MainModule.FileName);
            return Path.GetFullPath(path);
        }

        private static string GetExecutableName()
        {
            return Path.GetFileName(GetExecutablePath());
        }

        private static string GetProjectPath(string projectName)
        {
            var executablePath = GetExecutablePath();
            var binPath = Path.GetDirectoryName(executablePath);
            return Path.Combine(binPath, "..", "configs", projectName, "project.json");
        }

        public static void Main(string[] args)
        {
            bool showHelp = false;
            bool overwriteFiles = false;
            bool verbose = false;

            var options = new OptionSet()
            {
                { "v|verbose", "be verbose", v => verbose = v != null },
                { "h|help", "show this message and exit",  v => showHelp = v != null },
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
            string outputPath = extras.Count > 1 ? extras[1] : Path.ChangeExtension(inputPath, null) + "_unpack";

            var projectPath = GetProjectPath("Far Cry 2");
            if (File.Exists(projectPath) == false)
            {
                Console.WriteLine($"Project file '{projectPath}' is missing!");
                return;
            }

            Console.WriteLine("Loading project...");
            var project = ProjectData.Project.Load(projectPath);
            if (project == null)
            {
                Console.WriteLine("Failed to load project!");
                return;
            }

            var hashes = project.LoadLists(
                "*.filelist",
                s => s.HashFileNameCRC32(),
                s => s.ToLowerInvariant());

            var map = new MapFile();
            using (var input = File.OpenRead(inputPath))
            {
                map.Deserialize(input);
            }

            Directory.CreateDirectory(outputPath);
            using (var output = File.Create(Path.Combine(outputPath, "map.xml")))
            {
                var settings = new XmlWriterSettings();
                settings.Indent = true;

                using (var writer = XmlWriter.Create(output, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("map");
                    
                    writer.WriteStartElement("info");
                    writer.WriteElementString("name", map.Info.Name);
                    writer.WriteElementString("creator", map.Info.Creator);
                    writer.WriteElementString("author", map.Info.Author);
                    writer.WriteElementString("size", map.Info.Size.ToString());
                    writer.WriteElementString("players", map.Info.Players.ToString());
                    writer.WriteElementString("unknown2", map.Info.Unknown2.ToString());
                    writer.WriteElementString("unknown3", map.Info.Unknown3.ToString());
                    writer.WriteElementString("unknown4", map.Info.Unknown4.ToString());
                    writer.WriteElementString("unknown5", map.Info.Unknown5.ToString());
                    writer.WriteElementString("unknown7", map.Info.Unknown7.ToString());
                    writer.WriteElementString("unknown10", map.Info.Unknown10.ToString());
                    writer.WriteStartElement("unknown11");
                    writer.WriteBinHex(map.Info.Unknown11, 0, map.Info.Unknown11.Length);
                    writer.WriteEndElement();
                    writer.WriteStartElement("unknown12");
                    writer.WriteBinHex(map.Info.Unknown12, 0, map.Info.Unknown12.Length);
                    writer.WriteEndElement();
                    writer.WriteElementString("unknown15", map.Info.Unknown15.ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("snapshot");
                    writer.WriteElementString("width", map.Snapshot.Width.ToString());
                    writer.WriteElementString("height", map.Snapshot.Height.ToString());
                    writer.WriteElementString("bpp", map.Snapshot.BytesPerPixel.ToString());
                    writer.WriteElementString("unknown4", map.Snapshot.Unknown4.ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("data");
                    writer.WriteElementString("unknown1", map.Data.Unknown1);
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }

            using (var input = map.Archive.XML.Unpack())
            {
                using (var output = File.Create(Path.Combine(outputPath, "archive.xml")))
                {
                    output.WriteFromStream(input, input.Length);
                }
            }

            using (var output = File.Create(Path.Combine(outputPath, "snapshot.bin")))
            {
                output.Write(map.Snapshot.Data, 0, map.Snapshot.Data.Length);
            }

            var big = new BigFileV2_32();
            using (var input = map.Archive.FAT.Unpack())
            {
                big.Deserialize(input);
            }

            var dataPath = Path.Combine(outputPath, "archive");
            Directory.CreateDirectory(dataPath);

            using (var input = map.Archive.DAT.Unpack())
            {
                long current = 0;
                long total = big.Entries.Count;

                foreach (var entry in big.Entries)
                {
                    current++;

                    string name = hashes[entry.NameHash];
                    if (name == null)
                    {
                        name = entry.NameHash.ToString("X8");
                        name = Path.ChangeExtension(name, ".unknown");
                        name = Path.Combine("__UNKNOWN", name);
                    }
                    else
                    {
                        name = name.Replace("/", "\\");
                        if (name.StartsWith("\\") == true)
                        {
                            name = name.Substring(1);
                        }
                    }

                    var entryPath = Path.Combine(dataPath, name);
                    Directory.CreateDirectory(Path.GetDirectoryName(entryPath));

                    if (overwriteFiles == false &&
                        File.Exists(entryPath) == true)
                    {
                        continue;
                    }

                    if (verbose == true)
                    {
                        Console.WriteLine("[{0}/{1}] {2}",
                            current, total, name);
                    }

                    using (var output = File.Create(entryPath))
                    {
                        EntryDecompression.Decompress(big, entry, input, output);
                    }
                }
            }
        }
    }
}

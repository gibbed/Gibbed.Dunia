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
using System.Linq;
using Gibbed.Dunia.FileFormats;
using Gibbed.Helpers;
using NDesk.Options;
using Big = Gibbed.Dunia.FileFormats.Big;

namespace Gibbed.Dunia.Pack
{
    internal class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }

        public static void Main(string[] args)
        {
            bool showHelp = false;
            bool verbose = false;
            bool compress = false;

            var options = new OptionSet()
            {
                {
                    "v|verbose",
                    "be verbose",
                    v => verbose = v != null
                },
                {
                    "lzo",
                    "compress data with LZO1x",
                    v => compress = v != null
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

            if (extras.Count < 1 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ output_fat input_directory+", GetExecutableName());
                Console.WriteLine("Pack files from input directories into a Encapsulated Resource File.");
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            var inputPaths = new List<string>();
            string fatPath, datPath;

            if (extras.Count == 1)
            {
                inputPaths.Add(extras[0]);
                fatPath = Path.ChangeExtension(extras[0], ".fat");
                datPath = Path.ChangeExtension(extras[0], ".dat");
            }
            else
            {
                fatPath = extras[0];

                if (Path.GetExtension(fatPath) != ".fat")
                {
                    datPath = fatPath;
                    fatPath = Path.ChangeExtension(datPath, ".fat");
                }
                else
                {
                    datPath = Path.ChangeExtension(fatPath, ".dat");
                }

                inputPaths.AddRange(extras.Skip(1));
            }

            var paths = new SortedDictionary<uint, string>();

            if (verbose == true)
            {
                Console.WriteLine("Finding files...");
            }

            foreach (var relPath in inputPaths)
            {
                string inputPath = Path.GetFullPath(relPath);

                if (inputPath.EndsWith(Path.DirectorySeparatorChar.ToString()) == true)
                {
                    inputPath = inputPath.Substring(0, inputPath.Length - 1);
                }

                foreach (string path in Directory.GetFiles(inputPath, "*", SearchOption.AllDirectories))
                {
                    string fullPath = Path.GetFullPath(path);
                    string partPath = fullPath.Substring(inputPath.Length + 1).ToLowerInvariant();

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
                        hash = partPath.FileNameCRC32();
                    }

                    if (paths.ContainsKey(hash) == true)
                    {
                        Console.WriteLine("Ignoring {0} duplicate.", partPath);
                        continue;
                    }

                    paths[hash] = fullPath;
                    Console.WriteLine(fullPath);
                }
            }

            var big = new BigFile();

            using (var output = File.Create(datPath))
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
                    entry.CompressionScheme = compress == false ?
                        Big.CompressionScheme.None :
                        Big.CompressionScheme.LZO1x;
                    entry.Offset = output.Position;

                    using (var input = File.OpenRead(path))
                    {
                        if (compress == false)
                        {
                            entry.UncompressedSize = (uint)input.Length;
                            entry.CompressedSize = (uint)input.Length;
                            output.WriteFromStream(input, input.Length);
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }

                        output.Seek(output.Position.Align(16), SeekOrigin.Begin);
                    }

                    big.Entries.Add(entry);
                }
            }

            using (var output = File.Create(fatPath))
            {
                big.Serialize(output);
            }
        }
    }
}

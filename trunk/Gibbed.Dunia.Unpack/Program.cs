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
using Gibbed.Dunia.FileFormats;
using Gibbed.Helpers;
using NDesk.Options;
using CompressionScheme = Gibbed.Dunia.FileFormats.Big.CompressionScheme;

namespace Gibbed.Dunia.Unpack
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
            bool extractUnknowns = true;
            bool overwriteFiles = false;

            OptionSet options = new OptionSet()
            {
                {
                    "o|overwrite",
                    "overwrite existing files",
                    v => overwriteFiles = v != null
                },
                {
                    "u|no-unknowns",
                    "don't extract unknown files",
                    v => extractUnknowns = v == null
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

            string fatPath = extras[0];
            string outputPath = extras.Count > 1 ? extras[1] : Path.ChangeExtension(fatPath, null) + "_unpack";
            string datPath;

            if (Path.GetExtension(fatPath) == ".dat")
            {
                datPath = fatPath;
                fatPath = Path.ChangeExtension(fatPath, ".fat");
            }
            else
            {
                datPath = Path.ChangeExtension(fatPath, ".dat");
            }

            var manager = ProjectData.Manager.Load();
            if (manager.ActiveProject == null)
            {
                Console.WriteLine("Warning: no active project loaded.");
            }

            var hashes = manager.LoadLists(
                "*.filelist",
                s => s.HashFileNameCRC32(),
                s => s.ToLowerInvariant());

            var big = new BigFile();
            using (var input = File.OpenRead(fatPath))
            {
                big.Deserialize(input);
            }

            using (var input = File.OpenRead(datPath))
            {
                long current = 1;
                long total = big.Entries.Count;

                foreach (var entry in big.Entries)
                {
                    bool isUnknown = false;

                    string name = hashes[entry.NameHash];
                    if (name == null)
                    {
                        if (extractUnknowns == false)
                        {
                            continue;
                        }

                        isUnknown = true;

                        string extension;

                        // detect type
                        {
                            var guess = new byte[16];
                            int read = 0;

                            if (entry.CompressionScheme == CompressionScheme.None)
                            {
                                if (entry.CompressedSize > 0)
                                {
                                    input.Seek(entry.Offset, SeekOrigin.Begin);
                                    read = input.Read(guess, 0, (int)Math.Min(
                                        entry.CompressedSize, guess.Length));
                                }
                            }
                            else if (entry.CompressionScheme == CompressionScheme.LZO1x)
                            {
                                input.Seek(entry.Offset, SeekOrigin.Begin);

                                var compressedData = new byte[entry.CompressedSize];
                                if (input.Read(compressedData, 0, compressedData.Length) != compressedData.Length)
                                {
                                    throw new EndOfStreamException();
                                }

                                var uncompressedData = new byte[entry.UncompressedSize];
                                uint uncompressedSize = entry.UncompressedSize;

                                var result = LZO1x.Decompress(
                                    compressedData,
                                    entry.CompressedSize,
                                    uncompressedData,
                                    ref uncompressedSize);
                                if (result != 0)
                                {
                                    throw new InvalidOperationException("decompression error: " + result.ToString());
                                }
                                else if (uncompressedSize != entry.UncompressedSize)
                                {
                                    throw new InvalidOperationException("did not decompress correct amount of data");
                                }

                                Array.Copy(uncompressedData, 0, guess, 0, Math.Min(16, uncompressedData.Length));
                                read = uncompressedData.Length;
                            }
                            else
                            {
                                throw new NotSupportedException();
                            }

                            extension = FileExtensions.Detect(guess, read);
                        }

                        name = entry.NameHash.ToString("X8");
                        name = Path.ChangeExtension(name, "." + extension);
                        name = Path.Combine(extension, name);
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

                    Console.WriteLine("[{0}/{1}] {2}",
                        current, total, name);
                    current++;

                    /*
                    var ext = Path.GetExtension(name);
                    if (ext == ".xbt" ||
                        ext == ".xbg" ||
                        ext == ".xbm" ||
                        ext == ".spk" ||
                        ext == ".mab" ||
                        ext == ".lfe" ||
                        ext == ".apm")
                    {
                        continue;
                    }
                    */

                    var entryPath = Path.Combine(outputPath, name);
                    Directory.CreateDirectory(Path.GetDirectoryName(entryPath));

                    if (overwriteFiles == false &&
                        File.Exists(entryPath) == true)
                    {
                        continue;
                    }

                    using (var output = File.Create(entryPath))
                    {
                        if (entry.CompressionScheme == CompressionScheme.None)
                        {
                            if (entry.CompressedSize > 0)
                            {
                                input.Seek(entry.Offset, SeekOrigin.Begin);
                                output.WriteFromStream(input, entry.CompressedSize);
                            }
                        }
                        else if (entry.CompressionScheme == CompressionScheme.LZO1x)
                        {
                            if (entry.UncompressedSize > 0)
                            {
                                input.Seek(entry.Offset, SeekOrigin.Begin);

                                var compressedData = new byte[entry.CompressedSize];
                                if (input.Read(compressedData, 0, compressedData.Length) != compressedData.Length)
                                {
                                    throw new EndOfStreamException();
                                }

                                var uncompressedData = new byte[entry.UncompressedSize];
                                uint uncompressedSize = entry.UncompressedSize;

                                var result = LZO1x.Decompress(
                                    compressedData,
                                    entry.CompressedSize,
                                    uncompressedData,
                                    ref uncompressedSize);
                                if (result != 0)
                                {
                                    throw new InvalidOperationException("decompression error: " + result.ToString());
                                }
                                else if (uncompressedSize != entry.UncompressedSize)
                                {
                                    throw new InvalidOperationException("did not decompress correct amount of data");
                                }

                                output.Write(uncompressedData, 0, uncompressedData.Length);
                            }
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }
                }
            }
        }
    }
}

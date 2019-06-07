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
using System.Linq;
using Gibbed.Dunia.FileFormats;
using Gibbed.IO;
using NDesk.Options;
using ProjectData = Gibbed.ProjectData;
using Big = Gibbed.Dunia.FileFormats.Big;

namespace Bootstrap
{
    internal class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }

        public static void Main(string[] args)
        {
            var options = new OptionSet()
            {
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

            if (extras.Count < 1 || extras.Count > 2)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ output_dir", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            var manager = ProjectData.Manager.Load();
            if (manager.ActiveProject == null ||
                string.IsNullOrWhiteSpace(manager.ActiveProject.InstallPath) == true)
            {
                Console.WriteLine("Could not detect Far Cry 2 install directory. Bootstrap failed.");
                return;
            }
            var project = manager.ActiveProject;

            var outputPath = Path.GetFullPath(extras[0]);
            var basePath = Path.GetFullPath(project.InstallPath);

            var checks = new string[]
            {
                @"patch.fat",
                @"patch.dat",
                @"worlds\worlds.dat",
                @"worlds\worlds.fat",
            };

            foreach (var check in checks)
            {
                if (File.Exists(Path.Combine(basePath, check)) == false)
                {
                    Console.WriteLine("You seem to be missing '{0}' in your Data_Win32 directory. Bootstrap failed.", check);
                    return;
                }
            }

            if (FileExistsInBig(
                Path.Combine(basePath, @"patch.fat"),
                @"generated\entitylibrarypatchoverride.fcb") == false)
            {
                Console.WriteLine(@"Your patch.fat doesn't seem to have generated\entitylibrarypatchoverride.fcb in it! Bootstraip failed.");
                return;
            }

            var mymodPath = Path.Combine(outputPath, "mymod");
            Directory.CreateDirectory(mymodPath);

            var mypatchPath = Path.Combine(outputPath, "mypatch");
            Directory.CreateDirectory(mypatchPath);
            Directory.CreateDirectory(Path.Combine(mypatchPath, "generated"));

            var librariesPath = Path.Combine(outputPath, "libraries");
            Directory.CreateDirectory(librariesPath);

            var originalPath = Path.Combine(outputPath, "original");
            Directory.CreateDirectory(originalPath);

            var patchPath = Path.Combine(originalPath, "patch");
            Directory.CreateDirectory(patchPath);

            Console.WriteLine("Unpacking patch.fat... (this'll take a moment)");

            Gibbed.Dunia.Unpack.Program.Main(new string[]
                {
                    Path.Combine(basePath, "patch.fat"),
                    patchPath,
                });

            Console.WriteLine(@"Unpacking worlds.fat\world1\generated\entitylibrary_full.fcb...");
            
            if (UnpackFileFromBig(
                Path.Combine(basePath, @"worlds\worlds.fat"),
                @"worlds\world1\generated\entitylibrary_full.fcb",
                Path.Combine(librariesPath, "world1.fcb")) == false)
            {
                Console.WriteLine("Error unpacking! Bootstraip failed.");
                return;
            }

            Console.WriteLine(@"Unpacking worlds.fat\world2\generated\entitylibrary_full.fcb...");

            if (UnpackFileFromBig(
                Path.Combine(basePath, @"worlds\worlds.fat"),
                @"worlds\world2\generated\entitylibrary_full.fcb",
                Path.Combine(librariesPath, "world2.fcb")) == false)
            {
                Console.WriteLine("Error unpacking! Bootstraip failed.");
                return;
            }

            Console.WriteLine("Converting entity libraries... (this'll take a moment)");

            Gibbed.Dunia.ConvertBinary.Program.Main(new string[]
                {
                    "-q",
                    "-m",
                    Path.Combine(patchPath, @"generated\entitylibrarypatchoverride.fcb"),
                    Path.Combine(librariesPath, "patch.xml"),
                });

            Gibbed.Dunia.ConvertBinary.Program.Main(new string[]
                {
                    "-q",
                    "-m",
                    Path.Combine(patchPath, @"generated\entitylibrarypatchoverride.fcb"),
                    Path.Combine(mymodPath, "patch.xml"),
                });

            Console.WriteLine("Still converting entity libraries...");

            Gibbed.Dunia.ConvertBinary.Program.Main(new string[]
                {
                    "-q",
                    "-m",
                    Path.Combine(librariesPath, "world1.fcb"),
                    Path.Combine(librariesPath, "world1.xml"),
                });

            Gibbed.Dunia.ConvertBinary.Program.Main(new string[]
                {
                    "-q",
                    "-m",
                    Path.Combine(librariesPath, "world2.fcb"),
                    Path.Combine(librariesPath, "world2.xml"),
                });

            using (var writer = new StreamWriter(Path.Combine(outputPath, "build_patch.bat")))
            {
                writer.WriteLine(@"@echo off");
                writer.WriteLine();
                writer.WriteLine("echo Converting patch.xml...");
                writer.WriteLine(@"tools\Gibbed.Dunia.ConvertBinary.exe --fcb mymod\patch.xml mypatch\generated\entitylibrarypatchoverride.fcb");
                writer.WriteLine();
                writer.WriteLine(@"echo Creating patch.fat/dat...");
                writer.WriteLine(@"tools\Gibbed.Dunia.Pack.exe -c patch.fat mypatch original\patch");
                writer.WriteLine();
                writer.WriteLine(@"rem This one will directly overwrite the real one :)");
                writer.WriteLine(@"rem tools\Gibbed.Dunia.Pack.exe -c ..\Data_Win32\patch.fat mypatch original\patch");
                writer.WriteLine();
                writer.WriteLine(@"echo Done.");
                writer.WriteLine(@"pause");
            }

            Console.WriteLine("All done! Be sure to read the README!");
        }

        private static bool FileExistsInBig(string fatPath, string fileName)
        {
            if (File.Exists(fatPath) == false)
            {
                return false;
            }

            var big = new BigFile();
            using (var input = File.OpenRead(fatPath))
            {
                big.Deserialize(input);
            }

            var entries = big.Entries.Where(e => e.NameHash == fileName.HashFileNameCRC32());
            if (entries.Count() == 0)
            {
                return false;
            }

            return true;
        }

        private static bool UnpackFileFromBig(string fatPath, string fileName, string outputPath)
        {
            var datPath = Path.ChangeExtension(fatPath, ".dat");

            if (File.Exists(fatPath) == false ||
                File.Exists(datPath) == false)
            {
                return false;
            }

            var big = new BigFile();
            using (var input = File.OpenRead(fatPath))
            {
                big.Deserialize(input);
            }

            var entries = big.Entries.Where(e => e.NameHash == fileName.HashFileNameCRC32());
            if (entries.Count() == 0)
            {
                return false;
            }

            var entry = entries.First();
            using (var input = File.OpenRead(datPath))
            {
                using (var output = File.Create(outputPath))
                {
                    if (entry.CompressionScheme == Big.CompressionScheme.None)
                    {
                        if (entry.CompressedSize > 0)
                        {
                            input.Seek(entry.Offset, SeekOrigin.Begin);
                            output.WriteFromStream(input, entry.CompressedSize);
                        }
                    }
                    else if (entry.CompressionScheme == Big.CompressionScheme.LZO1x)
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
                            int uncompressedSize = (int)entry.UncompressedSize;

                            var result = MiniLZO.LZO.DecompressSafe(
                                compressedData,
                                0,
                                (int)entry.CompressedSize,
                                uncompressedData,
                                0,
                                ref uncompressedSize);
                            if (result != MiniLZO.ErrorCode.Success)
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

            return true;
        }
    }
}

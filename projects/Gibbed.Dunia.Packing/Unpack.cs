/* Copyright (c) 2021 Rick (rick 'at' gibbed 'dot' us)
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
using System.Text.RegularExpressions;
using Gibbed.Dunia.FileFormats;
using NDesk.Options;
using Big = Gibbed.Dunia.FileFormats.Big;

namespace Gibbed.Dunia.Packing
{
    public static class Unpack<TArchive, THash>
        where TArchive : Big.IArchive<THash>, new()
    {
        public static void Main(string[] args, string projectName)
        {
            Main(args, projectName, null);
        }

        public static void Main(string[] args, string projectName, Big.TryGetHashOverride<THash> tryGetHashOverride)
        {
            if (string.IsNullOrEmpty(projectName) == true)
            {
                throw new ArgumentNullException(nameof(projectName));
            }

            bool multiple = false;
            bool recursive = false;
            bool showHelp = false;
            var options = new UnpackOptions()
            {
                ExtractUnknowns = true,
                OnlyUnknowns = false,
                Filter = null,
                ExtractFiles = true,
                OverwriteFiles = false,
                DifferencePath = null,
                Verbose = false,
                CreateSubDirectory = false,
            };

            var optionSet = new OptionSet()
            {
                { "o|overwrite", "overwrite existing files", v => options.OverwriteFiles = v != null },
                { "nf|no-files", "don't extract files", v => options.ExtractFiles = v == null },
                { "nu|no-unknowns", "don't extract unknown files", v => options.ExtractUnknowns = v == null },
                { "ou|only-unknowns", "only extract unknown files", v => options.OnlyUnknowns = v != null },
                { "f|filter=", "only extract files using pattern", v => options.Filter = !string.IsNullOrEmpty(v) ? new Regex(v) : null },
                { "if|invert-filter", "only extract files not using pattern", v => options.InvertFilter = v != null },
                { "d|difference=", "only extract files aren't in specified archive", v => options.DifferencePath = v },
                { "m|multiple", "extract all archives from [input_path]", v => multiple = v != null },
                { "r|recursive", "extract all archives from [input_path] and its sub-directories", v => recursive = v != null },
                { "sd|subdir", "create a sub-directory per archive", v => options.CreateSubDirectory = v != null },
                { "v|verbose", "be verbose", v => options.Verbose = v != null },
                { "h|help", "show this message and exit", v => showHelp = v != null },
            };

            List<string> extras;

            try
            {
                extras = optionSet.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("{0}: ", Helpers.GetExecutableName());
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `{0} --help' for more information.", Helpers.GetExecutableName());
                return;
            }

            if (extras.Count < 1 || extras.Count > 2 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_path [output_dir]", Helpers.GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Unpack files from a Big File (FAT/DAT pair).");
                Console.WriteLine();
                Console.WriteLine("Options:");
                optionSet.WriteOptionDescriptions(Console.Out);
                return;
            }

            var projectPath = Helpers.GetProjectPath(projectName);
            if (File.Exists(projectPath) == false)
            {
                Console.WriteLine($"Project file '{projectPath}' is missing!");
                return;
            }

            if (options.Verbose == true)
            {
                Console.WriteLine("Loading project...");
            }

            var project = ProjectData.Project.Load(projectPath);
            if (project == null)
            {
                Console.WriteLine("Failed to load project!");
                return;
            }

            string[] fatPaths;
            string sourcePath = extras[0];

            if (recursive == true && multiple == false)
            {
                Console.WriteLine("Warning: [recursive] has no effect when [multiple] is not set");
            }

            if (multiple)
            {
                if (!Directory.Exists(sourcePath))
                {
                    Console.WriteLine($"Input path \"{sourcePath}\" should be a directory path when [multiple] is set.");
                    return;
                }
                fatPaths = Directory.GetFiles(sourcePath, "*.fat", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            }
            else
            {
                if (Path.GetExtension(sourcePath) == ".dat")
                {
                    sourcePath = Path.ChangeExtension(sourcePath, ".fat");
                }
                fatPaths = new string[] { sourcePath };
            }

            if (options.Verbose)
            {
                Console.WriteLine($"Loading {(fatPaths.Length == 1 ? "FAT" : $"{fatPaths.Length} FATs")}...");
            }

            if (verbose == true)
            {
                Console.WriteLine("Loading project...");
            }

            var project = ProjectData.Project.Load(projectPath);
            if (project == null)
            {
                Console.WriteLine("Failed to load project!");
                return;
            }

            if (verbose == true)
            {
                Console.WriteLine("Reading FAT...");
            }

            TArchive fat;
            using (var input = File.OpenRead(fatPath))
            {
                fat = new TArchive();
                fat.Deserialize(input);
            }

            THash[] hashDifference = null;
            if (string.IsNullOrEmpty(differencePath) == false)
            {
                TArchive fatDifference;
                using (var input = File.OpenRead(differencePath))
                {
                    fatDifference = new TArchive();
                    fatDifference.Deserialize(input);
                }
                hashDifference = fat.Entries
                    .Select(e => e.NameHash)
                    .Except(fatDifference.Entries.Select(e => e.NameHash))
                    .ToArray();
                //Console.WriteLine("{0} not in parent.", hashDifference.Length);
            }

            if (options.ExtractFiles == false)
            {
                return;
            }

            var entries = fat.Entries.OrderBy(e => e.Offset).ToArray();
            if (entries.Length == 0)
            {
                return;
            }

            if (verbose == true)
            {
                Console.WriteLine("Loading file lists...");
            }

            THash wrappedComputeNameHash(string s) =>
                fat.ComputeNameHash(s, tryGetHashOverride);
            project.LoadListsFileNames(wrappedComputeNameHash, out var hashes);

            if (verbose == true)
            {
                Console.WriteLine("Unpacking files...");
            }

            using (var input = File.OpenRead(datPath))
            {
                long current = 0;
                long total = entries.Length;
                var padding = total.ToString(CultureInfo.InvariantCulture).Length;
                var duplicates = new Dictionary<THash, int>();
                foreach (var entry in entries)
                {
                    current++;

                    if (hashDifference != null &&
                        Array.IndexOf(hashDifference, entry.NameHash) < 0)
                    {
                        continue;
                    }

                    if (GetEntryName(
                        input,
                        fat,
                        entry,
                        hashes,
                        options,
                        out var entryName) == false)
                    {
                        continue;
                    }

                    if (duplicates.TryGetValue(entry.NameHash, out var duplicateCount) == true)
                    {
                        duplicates[entry.NameHash] = duplicateCount++;
                        var entryBaseName = Path.ChangeExtension(entryName, null);
                        var entryExtension = Path.GetExtension(entryName);
                        entryName = Path.Combine(
                            "__DUPLICATE",
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "{0}__DUPLICATE_{1}{2}",
                                entryBaseName,
                                duplicateCount,
                                entryExtension ?? ""));
                    }
                    else
                    {
                        duplicates[entry.NameHash] = 0;
                    }

                    if (filter != null && filter.IsMatch(entryName) == invertFilter)
                    {
                        continue;
                    }

                    var entryPath = Path.Combine(outputPath, entryName);
                    if (options.OverwriteFiles == false && File.Exists(entryPath) == true)
                    {
                        continue;
                    }

                    if (verbose == true)
                    {
                        Console.WriteLine(
                            "[{0}/{1}] {2}",
                            current.ToString(CultureInfo.InvariantCulture).PadLeft(padding),
                            total,
                            entryName);
                    }

                    input.Seek(entry.Offset, SeekOrigin.Begin);

                    var entryParent = Path.GetDirectoryName(entryPath);
                    if (string.IsNullOrEmpty(entryParent) == false)
                    {
                        Directory.CreateDirectory(entryParent);
                    }

                    using var output = File.Create(entryPath);
                    EntryDecompression.Decompress(fat, entry, input, output);
                }
            }
        }

        private static bool GetEntryName(
            Stream input,
            Big.IArchive<THash> archive,
            Big.Entry<THash> entry,
            ProjectData.HashList<THash> hashes,
            UnpackOptions options,
            out string entryName)
        {
            entryName = hashes[entry.NameHash];
            if (entryName != null)
            {
                if (options.OnlyUnknowns == true)
                {
                    return false;
                }

                entryName = FilterEntryName(entryName);
                return true;
            }

            if (options.ExtractUnknowns == false)
            {
                return false;
            }

            string type;
            string extension;
            {
                var guess = new byte[64];
                int read = 0;

                var compressionScheme = archive.ToCompressionScheme(entry.CompressionScheme);
                if (compressionScheme == Big.CompressionScheme.None)
                {
                    if (entry.UncompressedSize > 0)
                    {
                        input.Position = entry.Offset;
                        read = input.Read(guess, 0, (int)Math.Min(entry.UncompressedSize, guess.Length));
                    }
                }
                else
                {
                    using (var temp = new MemoryStream(guess))
                    {
                        input.Position = entry.Offset;
                        EntryDecompression.Decompress(archive, entry, input, temp, guess.Length);
                        read = (int)temp.Position;
                    }
                }

                if (FileDetection.TryDetect(guess, Math.Min(guess.Length, read), out var info) == true)
                {
                    type = info.Type;
                    extension = info.Extension;
                }
                else
                {
                    type = "unknown";
                    extension = null;
                }
            }

            entryName = archive.RenderNameHash(entry.NameHash);

            if (string.IsNullOrEmpty(extension) == false)
            {
                entryName = Path.ChangeExtension(entryName, "." + extension);
            }

            if (string.IsNullOrEmpty(type) == false)
            {
                entryName = Path.Combine(type, entryName);
            }

            entryName = Path.Combine("__UNKNOWN", entryName);
            return true;
        }

        private static string FilterEntryName(string entryName)
        {
            entryName = entryName.Replace(@"/", @"\");
            if (entryName.StartsWith(@"\") == true)
            {
                entryName = entryName.Substring(1);
            }
            return entryName;
        }

        private static void ExtractArchive(
            BoundArchive<TArchive, THash> archive,
            ExtractionContext<TArchive, THash> context,
            UnpackOptions options)
        {
            THash[] hashDifference = null;
            if (string.IsNullOrEmpty(options.DifferencePath) == false)
            {
                TArchive fatDifference;
                using (var input = File.OpenRead(options.DifferencePath))
                {
                    fatDifference = new TArchive();
                    fatDifference.Deserialize(input);
                }
                hashDifference = archive.Fat.Entries
                    .Select(e => e.NameHash)
                    .Except(fatDifference.Entries.Select(e => e.NameHash))
                    .ToArray();
                //Console.WriteLine("{0} not in parent.", hashDifference.Length);
            }

            if (options.ExtractFiles == false)
            {
                return;
            }

            var entries = archive.Fat.Entries.OrderBy(e => e.Offset).ToArray();
            if (entries.Length == 0)
            {
                return;
            }

            long extractedCount = 0;
            long ignoredCount = 0;
            long excludedCount = 0;
            long existingCount = 0;

            using (var input = File.OpenRead(archive.DatPath))
            {
                var padding = context.TotalEntryCount.ToString(CultureInfo.InvariantCulture).Length;
                var duplicates = new Dictionary<THash, int>();
                foreach (var entry in entries)
                {
                    context.IncrementProcessedEntryCount();

                    if (hashDifference != null &&
                        Array.IndexOf(hashDifference, entry.NameHash) < 0)
                    {
                        continue;
                    }

                    if (GetEntryName(
                        input,
                        archive.Fat,
                        entry,
                        context.Hashes,
                        options,
                        out var entryName) == false)
                    {
                        ignoredCount++;
                        continue;
                    }

                    if (duplicates.TryGetValue(entry.NameHash, out var duplicateCount) == true)
                    {
                        duplicates[entry.NameHash] = duplicateCount++;
                        var entryBaseName = Path.ChangeExtension(entryName, null);
                        var entryExtension = Path.GetExtension(entryName);
                        entryName = Path.Combine(
                            "__DUPLICATE",
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "{0}__DUPLICATE_{1}{2}",
                                entryBaseName,
                                duplicateCount,
                                entryExtension ?? ""));
                    }
                    else
                    {
                        duplicates[entry.NameHash] = 0;
                    }

                    if (options.Filter != null && options.Filter.IsMatch(entryName) == options.InvertFilter)
                    {
                        excludedCount++;
                        continue;
                    }

                    var entryPath = Path.Combine(
                        context.OutputPath,
                        options.CreateSubDirectory ? Path.GetFileName(archive.FatPath) : "",
                        entryName);

                    if (options.OverwriteFiles == false && File.Exists(entryPath) == true)
                    {
                        existingCount++;
                        continue;
                    }

                    if (options.Verbose == true)
                    {
                        Console.WriteLine(
                            $"[{context.ProcessedEntryCount.ToString(CultureInfo.InvariantCulture).PadLeft(padding)}/{context.TotalEntryCount}] {entryName}");
                    }

                    input.Seek(entry.Offset, SeekOrigin.Begin);

                    var entryParent = Path.GetDirectoryName(entryPath);
                    if (string.IsNullOrEmpty(entryParent) == false)
                    {
                        Directory.CreateDirectory(entryParent);
                    }

                    using var output = File.Create(entryPath);
                    EntryDecompression.Decompress(archive.Fat, entry, input, output);
                    extractedCount++;
                }
            }

            context.Tally(
                archive.FatPath,
                archive.Fat.Entries.Count,
                extractedCount,
                ignoredCount,
                excludedCount,
                existingCount);
        }

    }
}

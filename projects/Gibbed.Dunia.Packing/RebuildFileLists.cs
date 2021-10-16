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
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.Dunia.FileFormats;
using Gibbed.ProjectData;
using NDesk.Options;
using Big = Gibbed.Dunia.FileFormats.Big;

namespace Gibbed.Dunia.Packing
{
    public static class RebuildFileLists<TArchive, THash>
        where TArchive : Big.IArchive<THash>, new()
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        private static string GetListPath(string installPath, string inputPath)
        {
            installPath = installPath.ToLowerInvariant();
            inputPath = inputPath.ToLowerInvariant();

            if (inputPath.StartsWith(installPath) == false)
            {
                return null;
            }

            var baseName = inputPath.Substring(installPath.Length + 1);

            string outputPath;
            outputPath = Path.Combine("files", baseName);
            outputPath = Path.ChangeExtension(outputPath, ".filelist");
            return outputPath;
        }

        public static void Main(string[] args, string projectName)
        {
            Main(args, projectName, null);
        }

        public static void Main(
            string[] args,
            string projectName,
            Big.TryGetHashOverride<THash> tryGetHashOverride)
        {
            bool showHelp = false;

            var options = new OptionSet()
            {
                { "h|help", "show this message and exit", v => showHelp = v != null },
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

            if (showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ [extra_install_path]+", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            Console.WriteLine("Loading project...");

            var manager = Manager.Load(projectName);
            if (manager.ActiveProject == null)
            {
                Console.WriteLine("Nothing to do: no active project loaded.");
                return;
            }

            var project = manager.ActiveProject;

            var listsPath = project.ListsPath;
            if (string.IsNullOrEmpty(listsPath) == true)
            {
                Console.WriteLine("Could not detect lists path.");
                return;
            }

            HashList<THash> previousHashes = null;

            var installPaths = new List<string>();
            if (string.IsNullOrEmpty(project.InstallPath) == false)
            {
                installPaths.Add(project.InstallPath);
            }
            installPaths.AddRange(extras);

            if (installPaths.Count == 0)
            {
                Console.WriteLine("Could not detect install path.");
                return;
            }

            var fatPaths = new Dictionary<string, string>();

            Console.WriteLine("Searching for archives...");

            foreach (var installPath in installPaths)
            {
                foreach (var fatPath in Directory.GetFiles(installPath, "*.fat", SearchOption.AllDirectories))
                {
                    if (fatPath.EndsWith(".fat") == false)
                    {
                        continue;
                    }

                    var listPath = GetListPath(installPath, fatPath);
                    if (fatPaths.ContainsKey(listPath) == true)
                    {
                        continue;
                    }

                    var bakPath = fatPath + ".bak";
                    fatPaths[listPath] = File.Exists(bakPath) == false
                        ? fatPath
                        : bakPath;
                }
            }

            if (fatPaths.Count == 0)
            {
                Console.WriteLine("No archives, aborting.");
                return;
            }

            var breakdown = new Breakdown();
            var tracking = new Tracking();

            var fats = new Dictionary<string, TArchive>();
            var fatHashes = new Dictionary<string, THash[]>();

            Console.WriteLine("Loading archives...");
            foreach (var kv in fatPaths)
            {
                var listPath = kv.Key;
                var fatPath = kv.Value;

                var fat = new TArchive();
                using (var input = File.OpenRead(fatPath))
                {
                    fat.Deserialize(input);
                }

                fats[listPath] = fat;
                fatHashes[listPath] = fat.Entries
                    .Select(e => e.NameHash)
                    .Concat(GetDependentHashes(fat))
                    .Distinct()
                    .ToArray();
            }

            Console.WriteLine("Loading file lists...");
            THash wrappedComputeNameHash(string s) =>
                fats.Values.First().ComputeNameHash(s, tryGetHashOverride);
            manager.LoadListsFileNames(wrappedComputeNameHash, out previousHashes);

            Console.WriteLine("Processing names for archives...");
            foreach (var kv in fatPaths)
            {
                var listPath = kv.Key;
                var fatPath = kv.Value;

                Console.WriteLine(listPath);

                var fat = fats[listPath];

                var parentListPath = GetParentListPath(listPath);
                THash[] parentHashes = null;
                if (string.IsNullOrEmpty(parentListPath) == false)
                {
                    fatHashes.TryGetValue(parentListPath, out parentHashes);
                }

                var outputPath = Path.Combine(listsPath, listPath);
                HandleEntries(fatHashes[listPath], parentHashes, previousHashes, tracking, fat, outputPath);
            }

            var fcbconverterPath = Path.Combine(listsPath, "files", "FCBConverterFileNames.list");
            var allHashes = tracking.Hashes.Distinct().ToArray();
            var allNames = tracking.Names.Distinct().ToArray();
            var totalBreakdown = new Breakdown()
            {
                Known = allNames.Length,
                Total = allHashes.Length,
            };
            WriteList(default, null, allNames, totalBreakdown, null, fcbconverterPath);

            var statusPath = Path.Combine(listsPath, "files", "status.txt");
            using (var output = new StreamWriter(statusPath, false, new UTF8Encoding(false)))
            {
                output.WriteLine("{0}", totalBreakdown);
                // TODO(gibbed): breakdown all archives individually
            }
        }

        private static string GetParentListPath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (path.EndsWith("_hd.filelist") == false)
            {
                return null;
            }
            return path.Substring(0, path.Length - 12) + ".filelist";
        }

        private static THash[] HandleEntries(
            THash[] hashes,
            THash[] parentHashes,
            HashList<THash> previousHashes,
            Tracking tracking,
            TArchive fat,
            string outputPath)
        {
            var breakdown = new Breakdown();

            var names = new List<string>();
            foreach (var hash in hashes)
            {
                var name = previousHashes[hash];
                if (name != null)
                {
                    names.Add(name);
                }
                breakdown.Total++;
            }

            tracking.Hashes.AddRange(hashes);
            tracking.Names.AddRange(names);

            int? differenceCount = null;
            if (parentHashes != null)
            {
                differenceCount = hashes.Except(parentHashes).Count();
            }

            names = names.Distinct().ToList();
            breakdown.Known += names.Count;

            WriteList(fat, previousHashes, names, breakdown, differenceCount, outputPath);

            return hashes;
        }

        private static void WriteList(
            TArchive fat,
            HashList<THash> previousHashes,
            IEnumerable<string> names,
            Breakdown breakdown,
            int? differenceCount,
            string outputPath)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                writer.WriteLine("; {0}", breakdown);

                if (differenceCount != null && differenceCount != 0)
                {
                    writer.WriteLine("; {0} not in parent", differenceCount.Value);
                }

                if (fat is Big.IDependentArchive<THash> dependentFat)
                {
                    if (dependentFat.HasArchiveHash == true || dependentFat.Dependencies.Count > 0)
                    {
                        writer.WriteLine(";");
                    }
                    if (dependentFat.HasArchiveHash == true)
                    {
                        writer.WriteLine("; archive={0}",
                            previousHashes?[dependentFat.ArchiveHash] ??
                                fat.RenderNameHash(dependentFat.ArchiveHash));
                    }
                    foreach (var dependency in dependentFat.Dependencies)
                    {
                        writer.WriteLine("; dependency={0} @ {1}",
                            previousHashes?[dependency.ArchiveHash] ??
                                fat.RenderNameHash(dependency.ArchiveHash),
                            previousHashes?[dependency.NameHash] ??
                                fat.RenderNameHash(dependency.NameHash));
                    }
                }

                foreach (string name in names.OrderBy(dn => dn))
                {
                    writer.WriteLine(name);
                }

                writer.Flush();
            }

            var outputParent = Path.GetDirectoryName(outputPath);
            if (string.IsNullOrEmpty(outputParent) == false)
            {
                Directory.CreateDirectory(outputParent);
            }

            File.WriteAllText(outputPath, sb.ToString(), new UTF8Encoding(false));
        }

        private static IEnumerable<THash> GetDependentHashes(TArchive fat)
        {
            if (fat is Big.IDependentArchive<THash> dependentFat)
            {
                if (dependentFat.HasArchiveHash == true)
                {
                    yield return dependentFat.ArchiveHash;
                }

                foreach (var dependency in dependentFat.Dependencies)
                {
                    yield return dependency.ArchiveHash;
                    yield return dependency.NameHash;
                }
            }
        }

        internal class Tracking
        {
            public readonly List<THash> Hashes = new List<THash>();
            public readonly List<string> Names = new List<string>();
        }

        internal class Breakdown
        {
            public long Known = 0;
            public long Total = 0;

            public int Percent
            {
                get
                {
                    return this.Total == 0
                        ? 0
                        : (int)Math.Floor(((float)this.Known / this.Total) * 100.0f);
                }
            }

            public override string ToString()
            {
                return $"{this.Known}/{this.Total} ({this.Percent}%)";
            }
        }
    }
}

/* Copyright (c) 2012 Rick (rick 'at' gibbed 'dot' us)
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gibbed.Dunia.FileFormats;
using Gibbed.IO;
using Big = Gibbed.Dunia.FileFormats.Big;

namespace Gibbed.Dunia.ArchiveViewer
{
    public partial class Viewer : Form
    {
        public Viewer()
        {
            this.InitializeComponent();
        }

        private ProjectData.HashList<uint> Hashes = null;
        private ProjectData.Manager Manager;

        private void OnLoad(object sender, EventArgs e)
        {
            this.LoadProject();
        }

        private void LoadProject()
        {
            try
            {
                this.Manager = ProjectData.Manager.Load();
                this.projectComboBox.Items.AddRange(this.Manager.ToArray());
                this.SetProject(this.Manager.ActiveProject);
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    "There was an error while loading project data." +
                    Environment.NewLine + Environment.NewLine +
                    e.ToString() +
                    Environment.NewLine + Environment.NewLine +
                    "(You can press Ctrl+C to copy the contents of this dialog)",
                    "Critical Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void SetProject(ProjectData.Project project)
        {
            if (project != null)
            {
                try
                {
                    this.Hashes = project.LoadLists(
                        "*.filelist",
                        s => s.HashFileNameCRC32(),
                        s => s.ToLowerInvariant());
                    
                    this.openDialog.InitialDirectory = project.InstallPath;
                    this.saveKnownFileListDialog.InitialDirectory = project.ListsPath;
                }
                catch (Exception e)
                {
                    MessageBox.Show(
                        "There was an error while loading project data." +
                        Environment.NewLine + Environment.NewLine +
                        e.ToString() +
                        Environment.NewLine + Environment.NewLine +
                        "(You can press Ctrl+C to copy the contents of this dialog)",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    project = null;
                }
            }

            if (project != this.Manager.ActiveProject)
            {
                this.Manager.ActiveProject = project;
            }

            this.projectComboBox.SelectedItem = project;
        }

        private BigFile Archive;
        private void BuildFileTree()
        {
            this.fileList.Nodes.Clear();
            this.fileList.BeginUpdate();

            if (this.Archive != null)
            {
                var dirNodes = new Dictionary<string, TreeNode>();

                var baseNode = new TreeNode(Path.GetFileName(this.openDialog.FileName), 0, 0);
                var knownNode = new TreeNode("Known", 1, 1);
                var unknownNode = new TreeNode("Unknown", 1, 1);

                foreach (var entry in this.Archive.Entries
                    .OrderBy(k => k.NameHash, new FileNameHashComparer(this.Hashes)))
                {
                    TreeNode node = null;

                    if (this.Hashes[entry.NameHash] != null)
                    {
                        var fileName = this.Hashes[entry.NameHash];
                        var pathName = Path.GetDirectoryName(fileName);
                        var parentNodes = knownNode.Nodes;

                        if (pathName.Length > 0)
                        {
                            var dirs = pathName.Split(new char[] { '\\' });

                            foreach (string dir in dirs)
                            {
                                if (parentNodes.ContainsKey(dir))
                                {
                                    parentNodes = parentNodes[dir].Nodes;
                                }
                                else
                                {
                                    var parentNode = parentNodes.Add(dir, dir, 2, 2);
                                    parentNodes = parentNode.Nodes;
                                }
                            }
                        }

                        node = parentNodes.Add(null, Path.GetFileName(fileName) /*+ " [" + entry.Name.ToString("X8") + "]"*/, 3, 3);
                    }
                    else
                    {
                        node = unknownNode.Nodes.Add(null, entry.NameHash.ToString("X8"), 3, 3);
                    }

                    node.Tag = entry;
                }

                if (knownNode.Nodes.Count > 0)
                {
                    baseNode.Nodes.Add(knownNode);
                }

                if (unknownNode.Nodes.Count > 0)
                {
                    baseNode.Nodes.Add(unknownNode);
                    unknownNode.Text = "Unknown (" + unknownNode.Nodes.Count.ToString() + ")";
                }

                if (knownNode.Nodes.Count > 0)
                {
                    knownNode.Expand();
                }
                else if (unknownNode.Nodes.Count > 0)
                {
                    //unknownNode.Expand();
                }

                baseNode.Expand();
                this.fileList.Nodes.Add(baseNode);
            }

            //this.fileList.Sort();
            this.fileList.EndUpdate();
        }

        private void OnOpen(object sender, EventArgs e)
        {
            if (this.openDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if (this.openDialog.InitialDirectory != null)
            {
                this.openDialog.InitialDirectory = null;
            }

            BigFile archive;
            using (var input = this.openDialog.OpenFile())
            {
                archive = new BigFile();
                archive.Deserialize(input);
            }
            this.Archive = archive;

            this.BuildFileTree();

            var exists = File.Exists(Path.ChangeExtension(this.openDialog.FileName, ".dat"));
            this.saveAllButton.Enabled = exists;
            this.saveToolStripMenuItem.Enabled = exists;
        }

        private void OnSave(object sender, EventArgs e)
        {
            if (this.fileList.SelectedNode == null)
            {
                return;
            }

            var root = this.fileList.SelectedNode;
            if (root.Nodes.Count == 0)
            {
                this.saveFileDialog.FileName = root.Text;

                if (this.saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var entry = (Big.Entry)root.Tag; 
                var inputPath = Path.ChangeExtension(this.openDialog.FileName, ".dat");
                using (var input = File.OpenRead(inputPath))
                {
                    using (var output = this.saveFileDialog.OpenFile())
                    {
                        ExtractFile(input, entry, output);
                    }
                }
            }
            else
            {
                if (this.saveFilesDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var entries = new List<Big.Entry>();
                
                var nodes = new List<TreeNode>();
                nodes.Add(root);

                while (nodes.Count > 0)
                {
                    var node = nodes[0];
                    nodes.RemoveAt(0);

                    if (node.Nodes.Count > 0)
                    {
                        foreach (TreeNode child in node.Nodes)
                        {
                            if (child.Nodes.Count > 0)
                            {
                                nodes.Add(child);
                            }
                            else
                            {
                                entries.Add((Big.Entry)child.Tag);
                            }
                        }
                    }
                }

                this.SaveFiles(entries, this.saveFilesDialog.SelectedPath);
            }
        }

        private void OnSaveAll(object sender, EventArgs e)
        {
            if (this.saveFilesDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            this.SaveFiles(null, this.saveFilesDialog.SelectedPath);
        }

        private static string DetectExtension(Stream input, Big.Entry entry)
        {
            var guess = new byte[64];
            int read = 0;

            if (entry.CompressionScheme == Big.CompressionScheme.None)
            {
                if (entry.CompressedSize > 0)
                {
                    input.Seek(entry.Offset, SeekOrigin.Begin);
                    read = input.Read(guess, 0, (int)Math.Min(
                        entry.CompressedSize, guess.Length));
                }
            }
            else if (entry.CompressionScheme == Big.CompressionScheme.LZO1x)
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

                Array.Copy(uncompressedData, 0, guess, 0, Math.Min(guess.Length, uncompressedData.Length));
                read = uncompressedData.Length;
            }
            else
            {
                throw new NotSupportedException();
            }

            return FileExtensions.Detect(guess, Math.Min(guess.Length, read));
        }

        private static void ExtractFile(Stream input, Big.Entry entry, Stream output)
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

        private void SaveFiles(
            IEnumerable<Big.Entry> entries,
            string basePath)
        {
            var extractUnknowns = this.saveOnlyKnownFilesMenuItem.Checked == false;
            var overwriteFiles = false;

            var inputPath = Path.ChangeExtension(this.openDialog.FileName, ".dat");
            var outputPath = this.saveFilesDialog.SelectedPath;
            var big = this.Archive;
            var hashes = this.Hashes;

            if (entries == null)
            {
                entries = this.Archive.Entries;
            }

            using (var progress = new SaveProgress())
            {
                var gong = new CancellationTokenSource();

                progress.Cancel += new EventHandler(delegate(object foo, EventArgs bar)
                {
                    gong.Cancel();
                });

                var task = Task.Factory.StartNew((object baz) =>
                {
                    using (var input = File.OpenRead(inputPath))
                    {
                        int current = 0;
                        int total = entries.Count();

                        foreach (var entry in entries)
                        {
                            current++;

                            if (gong.Token.IsCancellationRequested == true)
                            {
                                break;
                            }

                            string name = hashes[entry.NameHash];
                            progress.SetStatus(name ?? entry.NameHash.ToString("X8"), current, total);

                            if (name == null)
                            {
                                var extension = DetectExtension(input, entry);

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

                            var entryPath = Path.Combine(outputPath, name);
                            Directory.CreateDirectory(Path.GetDirectoryName(entryPath));

                            if (overwriteFiles == false &&
                                File.Exists(entryPath) == true)
                            {
                                continue;
                            }

                            using (var output = File.Create(entryPath))
                            {
                                ExtractFile(input, entry, output);
                            }
                        }
                    }
                },
                    gong.Token, TaskCreationOptions.LongRunning)
                    .ContinueWith(_ => progress.WorkDone());
                if (task.IsCompleted != true &&
                    task.IsCanceled != true &&
                    task.IsFaulted != true)
                {
                    progress.ShowDialog(this);
                    task.Wait();
                }
            }
        }

        private void OnReloadLists(object sender, EventArgs e)
        {
            if (this.Manager.ActiveProject != null)
            {
                this.Hashes = this.Manager.ActiveProject.LoadLists(
                        "*.filelist",
                        s => s.HashFileNameCRC32(),
                        s => s.ToLowerInvariant());
            }

            this.BuildFileTree();
        }

        private void OnProjectSelected(object sender, EventArgs e)
        {
            this.projectComboBox.Invalidate();
            var project = this.projectComboBox.SelectedItem as ProjectData.Project;
            if (project == null)
            {
                this.projectComboBox.Items.Remove(this.projectComboBox.SelectedItem);
            }
            this.SetProject(project);
            this.BuildFileTree();
        }
    }
}

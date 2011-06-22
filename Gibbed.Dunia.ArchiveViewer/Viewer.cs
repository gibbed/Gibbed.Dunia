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
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Gibbed.Dunia.FileFormats;
using Big = Gibbed.Dunia.FileFormats.Big;

namespace Gibbed.Dunia.ArchiveViewer
{
	public partial class Viewer : Form
	{
		public Viewer()
		{
			InitializeComponent();
		}

		private Font MonospaceFont = new Font(FontFamily.GenericMonospace, 9.0f);

		// File names
		private Dictionary<uint, string> FileNames;

		private void LoadFileNames(string path)
		{
			if (File.Exists(path))
			{
				TextReader reader = new StreamReader(path);

				while (true)
				{
					string line = reader.ReadLine();
					if (line == null)
					{
						break;
					}

					if (line.Length <= 0)
					{
						continue;
					}

					uint hash = line.HashFileNameCRC32();
					this.FileNames[hash] = line.ToLower();
				}

				reader.Close();
			}
		}

		private void OnLoad(object sender, EventArgs e)
		{
			string path;
			path = (string)Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Ubisoft\\Far Cry 2", "InstallDir", "");
			if (path != null && path.Length > 0)
			{
				this.openDialog.InitialDirectory = Path.Combine(path, "Data_Win32");
			}

			this.FileNames = new Dictionary<uint, string>();
			if (Directory.Exists(Path.Combine(Application.StartupPath, "filelists")))
			{
				this.saveFileListDialog.InitialDirectory = Path.Combine(Application.StartupPath, "filelists");

				foreach (string listPath in Directory.GetFiles(Path.Combine(Application.StartupPath, "filelists"), "*.filelist", SearchOption.AllDirectories))
				{
					this.LoadFileNames(listPath);
				}
			}
		}

        private int SortByFileNames(Big.Entry a, Big.Entry b)
		{
			if (a == null || b == null)
			{
				return 0;
			}

			if (this.FileNames.ContainsKey(a.NameHash) == false)
			{
				if (this.FileNames.ContainsKey(b.NameHash) == false)
				{
					if (a.NameHash == b.NameHash)
					{
						return 0;
					}

					return a.NameHash < b.NameHash ? -1 : 1;
				}
				else
				{
					return -1;
				}
			}
			else
			{
				if (this.FileNames.ContainsKey(b.NameHash) == false)
				{
					return 1;
				}
				else
				{
					return String.Compare(this.FileNames[a.NameHash], this.FileNames[b.NameHash]);
				}
			}
		}


		// A stupid way to do it but it's for the Save All function.
		private Big.Entry[] ArchiveFiles;

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

			var input = this.openDialog.OpenFile();
			var db = new BigFile();
			db.Deserialize(input);

			db.Entries.Sort(SortByFileNames);

			this.ArchiveFiles = db.Entries.ToArray();

			Dictionary<string, TreeNode> dirNodes = new Dictionary<string, TreeNode>();

			this.fileList.Nodes.Clear();
			this.fileList.BeginUpdate();

			TreeNode baseNode = new TreeNode(Path.GetFileName(this.openDialog.FileName), 0, 0);
			TreeNode knownNode = new TreeNode("Known", 1, 1);
			TreeNode unknownNode = new TreeNode("Unknown", 1, 1);
			
			for (int i = 0; i < this.ArchiveFiles.Length; i++)
			{
				var index = this.ArchiveFiles[i];
				TreeNode node = null;

				if (this.FileNames.ContainsKey(index.NameHash) == true)
				{
					string fileName = this.FileNames[index.NameHash];
					string pathName = Path.GetDirectoryName(fileName);
					TreeNodeCollection parentNodes = knownNode.Nodes;

					if (pathName.Length > 0)
					{
						string[] dirs = pathName.Split(new char[] { '\\' });

						foreach (string dir in dirs)
						{
							if (parentNodes.ContainsKey(dir))
							{
								parentNodes = parentNodes[dir].Nodes;
							}
							else
							{
								TreeNode parentNode = parentNodes.Add(dir, dir, 2, 2);
								parentNodes = parentNode.Nodes;
							}
						}
					}

					node = parentNodes.Add(null, Path.GetFileName(fileName), 3, 3);
				}
				else
				{
					node = unknownNode.Nodes.Add(null, index.NameHash.ToString("X8"), 3, 3);
				}

				node.Tag = index;

				if (index.CompressionScheme != 0)
				{
					throw new Exception();
				}

				if (index.UncompressedSize != 0)
				{
					node.ForeColor = Color.Blue;
				}
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
				unknownNode.Expand();
			}

			baseNode.Expand();

			this.fileList.Nodes.Add(baseNode);
			//this.fileList.Sort();
			this.fileList.EndUpdate();
		}

		private void OnSave(object sender, EventArgs e)
		{

		}

		private void OnSaveAll(object sender, EventArgs e)
		{
			if (this.saveAllFolderDialog.ShowDialog() != DialogResult.OK)
			{
				return;
			}

			Stream input = File.OpenRead(Path.ChangeExtension(this.openDialog.FileName, ".dat"));

			if (input == null)
			{
				return;
			}

			string basePath = this.saveAllFolderDialog.SelectedPath;

			SaveAllProgress progress = new SaveAllProgress();
			progress.ShowSaveProgress(this, input, this.ArchiveFiles, this.FileNames, basePath, this.saveOnlyknownFilesMenuItem.Checked);

			input.Close();
		}

		private void OnSaveKnownFileList(object sender, EventArgs e)
		{
			if (this.saveFileListDialog.ShowDialog() != DialogResult.OK)
			{
				return;
			}

			List<string> names = new List<string>();

			foreach (var index in this.ArchiveFiles)
			{
				if (this.FileNames.ContainsKey(index.NameHash))
				{
					names.Add(this.FileNames[index.NameHash]);
				}
			}

			names.Sort();

			TextWriter output = new StreamWriter(this.saveFileListDialog.OpenFile());

			foreach (string name in names)
			{
				output.WriteLine(name);
			}

			output.Close();
		}
	}
}

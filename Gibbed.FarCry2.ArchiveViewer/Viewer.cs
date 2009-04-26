using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Gibbed.FarCry2.Helpers;
using Gibbed.FarCry2.FileFormats;

namespace Gibbed.FarCry2.ArchiveViewer
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

					uint hash = line.FileNameCRC32();
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

		private int SortByFileNames(ArchiveEntry a, ArchiveEntry b)
		{
			if (a == null || b == null)
			{
				return 0;
			}

			if (this.FileNames.ContainsKey(a.Hash) == false)
			{
				if (this.FileNames.ContainsKey(b.Hash) == false)
				{
					if (a.Hash == b.Hash)
					{
						return 0;
					}

					return a.Hash < b.Hash ? -1 : 1;
				}
				else
				{
					return -1;
				}
			}
			else
			{
				if (this.FileNames.ContainsKey(b.Hash) == false)
				{
					return 1;
				}
				else
				{
					return String.Compare(this.FileNames[a.Hash], this.FileNames[b.Hash]);
				}
			}
		}


		// A stupid way to do it but it's for the Save All function.
		private ArchiveEntry[] ArchiveFiles;

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

			Stream input = this.openDialog.OpenFile();
			ArchiveFile db = new ArchiveFile();
			db.Read(input);

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
				ArchiveEntry index = this.ArchiveFiles[i];
				TreeNode node = null;

				if (this.FileNames.ContainsKey(index.Hash) == true)
				{
					string fileName = this.FileNames[index.Hash];
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
					node = unknownNode.Nodes.Add(null, index.Hash.ToString("X8"), 3, 3);
				}

				node.Tag = index;

				if (index.Flags != 0)
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

			foreach (ArchiveEntry index in this.ArchiveFiles)
			{
				if (this.FileNames.ContainsKey(index.Hash))
				{
					names.Add(this.FileNames[index.Hash]);
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

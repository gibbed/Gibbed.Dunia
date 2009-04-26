using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Gibbed.FarCry2.FileFormats;

namespace Gibbed.FarCry2.ArchiveViewer
{
	public partial class SaveAllProgress : Form
	{
		public SaveAllProgress()
		{
			InitializeComponent();
		}

		delegate void SetStatusDelegate(string status, int percent);
		private void SetStatus(string status, int percent)
		{
			if (this.progressBar.InvokeRequired || this.statusLabel.InvokeRequired)
			{
				SetStatusDelegate callback = new SetStatusDelegate(SetStatus);
				this.Invoke(callback, new object[] { status, percent });
				return;
			}

			this.statusLabel.Text = status;
			this.progressBar.Value = percent;
		}

		delegate void SaveDoneDelegate();
		private void SaveDone()
		{
			if (this.InvokeRequired)
			{
				SaveDoneDelegate callback = new SaveDoneDelegate(SaveDone);
				this.Invoke(callback);
				return;
			}

			this.Close();
		}

		[DllImport("Gibbed.FarCry2.Compression.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "Decompress")]
		static internal extern int Decompress([MarshalAs(UnmanagedType.LPArray)] byte[] in_buffer, UInt32 in_length, [MarshalAs(UnmanagedType.LPArray)] byte[] out_buffer, ref UInt32 out_length);

		public void SaveAll(object oinfo)
		{
			SaveAllInformation info = (SaveAllInformation)oinfo;
			Dictionary<uint, string> UsedNames = new Dictionary<uint, string>();

			for (int i = 0; i < info.Files.Length; i++)
			{
				BigEntry index = info.Files[i];

				string fileName = null;

				if (info.FileNames.ContainsKey(index.Hash))
				{
					fileName = info.FileNames[index.Hash];
					UsedNames[index.Hash] = info.FileNames[index.Hash];
				}
				else
				{
					if (info.SaveOnlyKnownFiles)
					{
						continue;
					}

					fileName = Path.Combine("__UNKNOWN", index.Hash.ToString("X8"));
				}

				Directory.CreateDirectory(Path.Combine(info.BasePath, Path.GetDirectoryName(fileName)));

				string path = Path.Combine(info.BasePath, fileName);

				this.SetStatus(path, i);

				path = Path.Combine(info.BasePath, path);

				info.Archive.Seek((long)index.Offset, SeekOrigin.Begin);
				byte[] data = new byte[index.CompressedSize];
				info.Archive.Read(data, 0, data.Length);
				
				if (index.UncompressedSize != 0)
				{
					byte[] decompressedData = new byte[index.UncompressedSize];
					UInt32 decompressedSize = index.UncompressedSize;
					int rez = Decompress(data, (uint)data.Length, decompressedData, ref decompressedSize);

					if (rez != 0)
					{
						throw new FileFormatException("decompress returned " + rez.ToString());
					}
					else if (decompressedSize != index.UncompressedSize)
					{
						throw new FileFormatException("decompress size mismatch (" + decompressedSize.ToString() + " vs " + index.UncompressedSize.ToString());
					}

					data = decompressedData;
				}

				FileStream output = new FileStream(path, FileMode.Create);
				output.Write(data, 0, data.Length);
				output.Close();
			}

			this.SaveDone();
		}

		private struct SaveAllInformation
		{
			public string BasePath;
			public Stream Archive;
			public BigEntry[] Files;
			public Dictionary<uint, string> FileNames;
			public bool SaveOnlyKnownFiles;
		}

		private Thread SaveThread;
		public void ShowSaveProgress(IWin32Window owner, Stream archive, BigEntry[] files, Dictionary<uint, string> fileNames, string basePath, bool saveOnlyKnown)
		{
			SaveAllInformation info;
			info.BasePath = basePath;
			info.Archive = archive;
			info.Files = files;
			info.FileNames = fileNames;
			info.SaveOnlyKnownFiles = saveOnlyKnown;

			this.progressBar.Value = 0;
			this.progressBar.Maximum = files.Length;

			this.SaveThread = new Thread(new ParameterizedThreadStart(SaveAll));
			this.SaveThread.Start(info);
			this.ShowDialog(owner);
		}

		private void OnCancel(object sender, EventArgs e)
		{
			if (this.SaveThread != null)
			{
				this.SaveThread.Abort();
			}

			this.Close();
		}
	}
}

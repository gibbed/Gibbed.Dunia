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
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Big = Gibbed.Dunia.FileFormats.Big;

namespace Gibbed.Dunia.ArchiveViewer
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

		[DllImport("Gibbed.Dunia.Compression.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "Decompress")]
		static internal extern int Decompress([MarshalAs(UnmanagedType.LPArray)] byte[] in_buffer, UInt32 in_length, [MarshalAs(UnmanagedType.LPArray)] byte[] out_buffer, ref UInt32 out_length);

		public void SaveAll(object oinfo)
		{
			SaveAllInformation info = (SaveAllInformation)oinfo;
			Dictionary<uint, string> UsedNames = new Dictionary<uint, string>();

			for (int i = 0; i < info.Files.Length; i++)
			{
				var index = info.Files[i];

				string fileName = null;

				if (info.FileNames.ContainsKey(index.NameHash))
				{
					fileName = info.FileNames[index.NameHash];
					UsedNames[index.NameHash] = info.FileNames[index.NameHash];
				}
				else
				{
					if (info.SaveOnlyKnownFiles)
					{
						continue;
					}

					fileName = Path.Combine("__UNKNOWN", index.NameHash.ToString("X8"));
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
						throw new FormatException("decompress returned " + rez.ToString());
					}
					else if (decompressedSize != index.UncompressedSize)
					{
                        throw new FormatException("decompress size mismatch (" + decompressedSize.ToString() + " vs " + index.UncompressedSize.ToString());
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
			public Big.Entry[] Files;
			public Dictionary<uint, string> FileNames;
			public bool SaveOnlyKnownFiles;
		}

		private Thread SaveThread;
		public void ShowSaveProgress(IWin32Window owner, Stream archive, Big.Entry[] files, Dictionary<uint, string> fileNames, string basePath, bool saveOnlyKnown)
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

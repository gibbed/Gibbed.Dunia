using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Gibbed.FarCry2.FileFormats;
using Gibbed.FarCry2.Helpers;
using Gibbed.Helpers;

namespace Gibbed.SaintsRow2.BuildPackage
{
	public class MyArchiveEntry : BigEntry
	{
		public Stream FileStream;
	}

	public class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("{0} <package.fat> <directory> [<directory>[, <directory>[, ...]]]", Path.GetFileName(Application.ExecutablePath));
				return;
			}

			Stream fatStream = File.Create(Path.ChangeExtension(args[0], ".fat"));
			Stream datStream = File.Create(Path.ChangeExtension(args[0], ".dat"));

			SortedDictionary<UInt32, string> paths = new SortedDictionary<UInt32, string>();
			Dictionary<UInt32, MyArchiveEntry> files = new Dictionary<UInt32, MyArchiveEntry>();

			long offset = 0;
			for (int i = 1; i < args.Length; i++)
			{
				string directory = Path.GetFullPath(args[i]);

				foreach (string path in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
				{
					string fullPath = Path.GetFullPath(path);
					string partPath = fullPath.Substring(directory.Length + 1);

					UInt32 hash = 0xFFFFFFFF;
					if (partPath.ToUpper().StartsWith("__UNKNOWN") == true)
					{
						hash = ("0x" + Path.GetFileNameWithoutExtension(fullPath)).GetHexNumber();
					}
					else
					{
						hash = partPath.FileNameCRC32();
					}

					if (paths.ContainsKey(hash) == true)
					{
						continue;
					}

					paths[hash] = fullPath;
					Console.WriteLine(fullPath);
				}
			}

			foreach (KeyValuePair<uint, string> value in paths)
			{
				UInt32 hash = value.Key;
				string path = value.Value;

				MyArchiveEntry entry = new MyArchiveEntry();
				entry.FileStream = File.OpenRead(path);

				entry.Hash = hash;

				entry.Flags = 0;
				entry.Offset = (UInt64)offset;
				entry.UncompressedSize = 0;
				entry.CompressedSize = (uint)entry.FileStream.Length;

				files[hash] = entry;

				offset += entry.FileStream.Length;
			}

			BigFile package = new BigFile();

			foreach (MyArchiveEntry entry in files.Values)
			{
				package.Entries.Add(entry);
			}

			package.Write(fatStream);

			foreach (MyArchiveEntry entry in package.Entries)
			{
				long size = entry.CompressedSize;
				byte[] block = new byte[4096];

				while (size > 0)
				{
					int read = entry.FileStream.Read(block, 0, 4096);
					if (read == 0)
					{
						break;
					}

					datStream.Write(block, 0, read);
					size -= read;
				}

				/*
				long align = entry.UncompressedSize.Align(16) - entry.UncompressedSize;
				if (align > 0)
				{
					byte[] block = new byte[align];
					stream.Write(block, 0, (int)align);
				}
				*/
			}

			fatStream.Close();
			datStream.Close();

			foreach (MyArchiveEntry entry in files.Values)
			{
				entry.FileStream.Close();
			}
		}
	}
}

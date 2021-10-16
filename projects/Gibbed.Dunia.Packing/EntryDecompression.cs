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
using System.IO;
using Gibbed.Dunia.FileFormats.Big;
using Gibbed.IO;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using K4os.Compression.LZ4;
using LZO = MiniLZO.LZO;
using LZOErrorCode = MiniLZO.ErrorCode;

namespace Gibbed.Dunia.Packing
{
    public static class EntryDecompression
    {
        public static void Decompress<T>(
            IArchive<T> archive,
            IEntry entry,
            Stream input,
            Stream output,
            int maxSize = -1)
        {
            if (entry.UncompressedSize <= 0)
            {
                return;
            }

            var compressionScheme = archive.ToCompressionScheme(entry.CompressionScheme);

            input.Position = entry.Offset;

            Stream temp;
            if (entry.IsEncrypted == true)
            {
                var entrySize = compressionScheme != CompressionScheme.None
                    ? entry.CompressedSize
                    : entry.UncompressedSize;
                var encryptedSize = Math.Min(0x100000, entrySize);
                encryptedSize &= ~7;
                if (encryptedSize > 0)
                {
                    var entryBytes = input.ReadBytes(entrySize);
                    var entryKey = Crypto.GenerateXTEAKey((uint)encryptedSize);
                    FileFormats.Crypto.XTEA.Decrypt(entryBytes, 0, encryptedSize, entryKey);
                    temp = new MemoryStream(entryBytes, false);
                }
                else
                {
                    temp = input;
                }
            }
            else
            {
                temp = input;
            }

            using (temp != input ? temp : null)
            {
                var decompress = GetDecompressor(compressionScheme);
                decompress(entry, input, output, maxSize);
            }
        }

        private delegate void DecompressDelegate(IEntry entry, Stream input, Stream output, int maxSize);

        private static DecompressDelegate GetDecompressor(CompressionScheme compressionScheme) =>
            compressionScheme switch
            {
                CompressionScheme.None => DecompressNone,
                CompressionScheme.LZO1x => DecompressLZO1x,
                CompressionScheme.Zlib => DecompressZlib,
                CompressionScheme.LZ4 => DecompressLZ4,
                _ => throw new NotSupportedException("unsupported compression scheme"),
            };

        private static void DecompressNone(IEntry entry, Stream input, Stream output, int maxSize)
        {
            output.WriteFromStream(input, entry.UncompressedSize);
        }

        private static void DecompressLZO1x(IEntry entry, Stream input, Stream output, int maxSize)
        {
            var compressedBytes = new byte[entry.CompressedSize];
            if (input.Read(compressedBytes, 0, compressedBytes.Length) != compressedBytes.Length)
            {
                throw new EndOfStreamException("could not read all compressed bytes");
            }

            var uncompressedBytes = new byte[entry.UncompressedSize];
            int actualUncompressedLength = uncompressedBytes.Length;

            var result = LZO.Decompress(
                compressedBytes,
                0,
                compressedBytes.Length,
                uncompressedBytes,
                0,
                ref actualUncompressedLength);
            if (result != LZOErrorCode.Success)
            {
                throw new InvalidOperationException($"LZO decompression failure ({result})");
            }
            if (actualUncompressedLength != uncompressedBytes.Length)
            {
                throw new InvalidOperationException("LZO decompression failure (uncompressed size mismatch)");
            }

            output.Write(uncompressedBytes, 0, uncompressedBytes.Length);
        }

        private static void DecompressZlib(IEntry entry, Stream input, Stream output, int maxSize)
        {
            if (entry.CompressedSize < 16)
            {
                throw new EndOfStreamException("not enough data for zlib compressed data");
            }

            var sizes = new ushort[8];
            for (int i = 0; i < 8; i++)
            {
                sizes[i] = input.ReadValueU16(Endian.Little);
            }

            var blockCount = sizes[0];
            var maximumUncompressedBlockSize = 16 * (sizes[1] + 1);

            long left = entry.UncompressedSize;
            for (int i = 0, c = 2; i < blockCount; i++, c++)
            {
                if (c == 8)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        sizes[j] = input.ReadValueU16(Endian.Little);
                    }

                    c = 0;
                }

                uint compressedBlockSize = sizes[c];
                if (compressedBlockSize != 0)
                {
                    var uncompressedBlockSize = i + 1 < blockCount
                        ? Math.Min(maximumUncompressedBlockSize, left)
                        : left;
                    //var uncompressedBlockSize = Math.Min(maximumUncompressedBlockSize, left);

                    using (var temp = input.ReadToMemoryStream((int)compressedBlockSize))
                    {
                        var zlib = new InflaterInputStream(temp, new Inflater(true));
                        output.WriteFromStream(zlib, uncompressedBlockSize);
                        left -= uncompressedBlockSize;
                    }

                    var padding = (16 - (compressedBlockSize % 16)) % 16;
                    if (padding > 0)
                    {
                        input.Seek(padding, SeekOrigin.Current);
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            if (left > 0)
            {
                throw new InvalidOperationException("did not decompress enough data");
            }
        }

        private static void DecompressLZ4(IEntry entry, Stream input, Stream output, int maxSize)
        {
            // K4os.Compression.LZ4 does not currently support partial decoding.
            // https://github.com/MiloszKrajewski/K4os.Compression.LZ4/issues/61
#if K4OS_WAS_FIXED
            var uncompressedSize = size < 0 ? entry.UncompressedSize : Math.Min(maxSize, entry.UncompressedSize);
            var compressedBytes = input.ReadBytes(entry.CompressedSize);
            var uncompressedBytes = new byte[new byte[uncompressedSize];
            var decoded = LZ4Codec.Decode(compressedBytes, uncompressedBytes);
            if (decoded != uncompressedSize)
            {
                throw new InvalidOperationException();
            }
            output.WriteBytes(uncompressedBytes);
#else
            var uncompressedSize = maxSize < 0
                ? entry.UncompressedSize
                : Math.Min(maxSize, entry.UncompressedSize);
            var compressedBytes = input.ReadBytes(entry.CompressedSize);
            var uncompressedBytes = new byte[entry.UncompressedSize];
            var decoded = LZ4Codec.Decode(compressedBytes, uncompressedBytes);
            if (decoded != entry.UncompressedSize)
            {
                throw new InvalidOperationException();
            }
            output.Write(uncompressedBytes, 0, uncompressedSize);
#endif
        }
    }
}

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
using Gibbed.Dunia.FileFormats.Big;
using Gibbed.IO;
using Version = Gibbed.Dunia.FileFormats.Big.Version;

namespace Gibbed.Dunia.FileFormats
{
    public abstract class BigFileV2<T>
    {
        public const uint Signature = 0x46415432; // 'FAT2'

        #region Fields
        private Endian _Endian;
        private Version _Version;
        private readonly List<Entry<T>> _Entries;
        #endregion

        public BigFileV2()
        {
            this._Endian = Endian.Little;
            this._Entries = new List<Entry<T>>();
        }

        #region Properties
        public Endian Endian
        {
            get { return this._Endian; }
            set { this._Endian = value; }
        }

        public Version Version
        {
            get { return this._Version; }
            set { this._Version = value; }
        }

        public List<Entry<T>> Entries => this._Entries;
        #endregion

        public static bool VersionSupportsEncryption(int fileVersion)
        {
            return fileVersion >= 11;
        }

        public void Serialize(Stream output)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input)
        {
            var magic = input.ReadValueU32(Endian.Little);
            if (magic != Signature && magic.Swap() != Signature)
            {
                throw new FormatException("bad magic");
            }
            var endian = magic == Signature ? Endian.Little : Endian.Big;

            var fileVersionAndEncryptionFlag = input.ReadValueU32(endian);
            var fileVersion = (int)(fileVersionAndEncryptionFlag & ~0x80000000u);
            var indexIsEncrypted = (fileVersionAndEncryptionFlag & 0x80000000u) != 0;

            if (indexIsEncrypted == true && VersionSupportsEncryption(fileVersion) == false)
            {
                throw new FormatException("encryption flag set when unsupported");
            }

            if (fileVersion > 11)
            {
                throw new FormatException("unsupported version");
            }

            var flags = fileVersion >= 3 ? input.ReadValueU32(endian) : 0u;
            var unknown0C = fileVersion >= 9 ? input.ReadValueU32(endian) : 0u;
            var unknown10 = fileVersion >= 9 ? input.ReadValueU32(endian) : 0u;

            var platform = ToPlatform((byte)(flags >> 0));
            var compressionVersion = (byte)(flags >> 8);

            if ((flags & 0xFFFF0000u) != 0)
            {
                throw new FormatException("unknown flags");
            }

            var version = new Version(fileVersion, platform, compressionVersion);

            if (this.IsKnownVersion(version) == false)
            {
                throw new FormatException("unknown version/platform/CV combination");
            }

            if (unknown0C != 0 || unknown10 != 0)
            {
                throw new NotImplementedException();
            }

            var entryCount = input.ReadValueS32(endian);
            if (entryCount < 0)
            {
                throw new FormatException();
            }

            var entrySerializer = GetEntrySerializer(fileVersion);

            Stream index;
            if (indexIsEncrypted == true)
            {
                var indexSize = entrySerializer.Size * entryCount;
                var indexBytes = input.ReadBytes(indexSize);
                indexSize &= ~7;
                var indexKey = Big.Crypto.GenerateXTEAKey((uint)indexSize);
                Crypto.XTEA.Decrypt(indexBytes, 0, indexSize, indexKey);
                File.WriteAllBytes("index.bin", indexBytes);
                index = new MemoryStream(indexBytes, false);
            }
            else
            {
                index = input;
            }

            var entries = new List<Entry<T>>();
            using (index != input ? index : null)
            {
                for (uint i = 0; i < entryCount; i++)
                {
                    entrySerializer.Deserialize(index, endian, out var entry);
                    entries.Add(entry);
                }
            }

            uint localizationCount = input.ReadValueU32(endian);
            for (uint i = 0; i < localizationCount; i++)
            {
                var nameLength = input.ReadValueU32(endian);
                if (nameLength > 32)
                {
                    throw new FormatException("bad length for localization name");
                }
                var nameBytes = input.ReadBytes((int)nameLength);
                var unknownValue = input.ReadValueU64(endian);
                throw new NotImplementedException();
            }

            foreach (var entry in this.Entries)
            {
                SanityCheckEntry(entry, version);
            }

            this._Endian = endian;
            this._Version = version;
            this._Entries.Clear();
            this._Entries.AddRange(entries);
        }

        protected abstract IEntrySerializer<T> GetEntrySerializer(int version);

        internal static void SanityCheckEntry(Entry<T> entry, Version version)
        {
            var compressionScheme = ToCompressionScheme(version, entry.CompressionScheme);
            switch (compressionScheme)
            {
                case CompressionScheme.None:
                {
                    if (version.Platform != Platform.Xenon && entry.UncompressedSize != 0)
                    {
                        throw new FormatException("no compression with a non-zero uncompressed size");
                    }
                    break;
                }
                case CompressionScheme.LZO1x:
                case CompressionScheme.Zlib:
                {
                    if (entry.CompressedSize == 0 && entry.UncompressedSize > 0)
                    {
                        throw new FormatException("compression with zero compressed size and a non-zero uncompressed size");
                    }
                    break;
                }
                case CompressionScheme.XMemCompress:
                {
                    if (version.Platform != Platform.Xenon)
                    {
                        throw new FormatException("XMemCompress on non-Xenon");
                    }
                    if (entry.CompressedSize == 0 && entry.UncompressedSize > 0)
                    {
                        throw new FormatException("compression with zero compressed size and a non-zero uncompressed size");
                    }
                    break;
                }
                default:
                {
                    throw new FormatException("unsupported compression scheme");
                }
            }
        }

        public static Platform ToPlatform(byte id)
        {
            return id switch
            {
                0 => Platform.Any,
                1 => Platform.Windows,
                _ => throw new NotSupportedException("unknown platform"),
            };
        }

        public static byte FromPlatform(Platform platform)
        {
            return platform switch
            {
                Platform.Any => 0,
                Platform.Windows => 1,
                _ => throw new NotSupportedException("unknown platform"),
            };
        }

        private static CompressionScheme ToCompressionScheme(Version version, byte id)
        {
            return version.FileVersion switch
            {
                10 => CompressionSchemeV10_V11.ToCompressionScheme(version, id),
                11 => CompressionSchemeV10_V11.ToCompressionScheme(version, id),
                _ => throw new NotSupportedException(),
            };
        }

        public CompressionScheme ToCompressionScheme(byte id)
        {
            return ToCompressionScheme(this._Version, id);
        }

        private static byte FromCompressionScheme(Version version, CompressionScheme compressionScheme)
        {
            return version.FileVersion switch
            {
                10 => CompressionSchemeV10_V11.FromCompressionScheme(version, compressionScheme),
                11 => CompressionSchemeV10_V11.FromCompressionScheme(version, compressionScheme),
                _ => throw new NotSupportedException(),
            };
        }

        public byte FromCompressionScheme(CompressionScheme compressionScheme)
        {
            return FromCompressionScheme(this._Version, compressionScheme);
        }

        protected abstract bool IsKnownVersion(Version version);
    }
}

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

namespace Gibbed.Dunia.FileFormats.Big
{
    public struct Entry<T> : IEquatable<Entry<T>>, IEntry
    {
        public T NameHash { get; set; }
        public int UncompressedSize { get; set; }
        public long Offset { get; set; }
        public int CompressedSize { get; set; }
        public byte CompressionScheme { get; set; }
        public bool IsEncrypted { get; set; }

        public override string ToString()
        {
            var encryption = this.IsEncrypted ? ", encrypted" : "";
            return this.CompressionScheme == 0
                ? $"{this.NameHash:X} @{this.Offset}, {this.CompressedSize} bytes{encryption}"
                : $"{this.NameHash:X} @{this.Offset}, {this.UncompressedSize} bytes ({this.CompressedSize} compressed bytes, scheme #{this.CompressionScheme}){encryption}";
        }

        public bool Equals(Entry<T> other)
        {
            return
                EqualityComparer<T>.Default.Equals(this.NameHash, other.NameHash) &&
                this.UncompressedSize == other.UncompressedSize &&
                this.Offset == other.Offset &&
                this.CompressedSize == other.CompressedSize &&
                this.CompressionScheme == other.CompressionScheme &&
                this.IsEncrypted == other.IsEncrypted;
        }

        public override bool Equals(object obj)
        {
            return obj is Entry<T> other && this.Equals(other) == true;
        }

        public static bool operator ==(Entry<T> left, Entry<T> right)
        {
            return left.Equals(right) == true;
        }

        public static bool operator !=(Entry<T> left, Entry<T> right)
        {
            return left.Equals(right) == false;
        }

        public override int GetHashCode()
        {
            int hashCode = 1805868928;
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(this.NameHash);
            hashCode = hashCode * -1521134295 + this.UncompressedSize.GetHashCode();
            hashCode = hashCode * -1521134295 + this.Offset.GetHashCode();
            hashCode = hashCode * -1521134295 + this.CompressedSize.GetHashCode();
            hashCode = hashCode * -1521134295 + this.CompressionScheme.GetHashCode();
            hashCode = hashCode * -1521134295 + this.IsEncrypted.GetHashCode();
            return hashCode;
        }
    }
}

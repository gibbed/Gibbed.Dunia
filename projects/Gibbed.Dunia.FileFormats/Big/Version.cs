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

namespace Gibbed.Dunia.FileFormats.Big
{
    public struct Version : IEquatable<Version>
    {
        public readonly int FileVersion;
        public readonly Platform Platform;
        public readonly byte CompressionVersion;

        public Version(int fileVersion, Platform platform, byte compressionVersion)
        {
            this.FileVersion = fileVersion;
            this.Platform = platform;
            this.CompressionVersion = compressionVersion;
        }

        public void Deconstruct(out int fileVersion, out Platform platform, out byte compressionVersion)
        {
            fileVersion = this.FileVersion;
            platform = this.Platform;
            compressionVersion = this.CompressionVersion;
        }

        public static implicit operator (int fileVersion, Platform platform, byte compressionVersion)(Version value)
        {
            return (value.FileVersion, value.Platform, value.CompressionVersion);
        }

        public static implicit operator Version((int fileVersion, Platform platform, byte compressionVersion) value)
        {
            return new Version(value.fileVersion, value.platform, value.compressionVersion);
        }

        public bool Equals(Version other)
        {
            return this.FileVersion == other.FileVersion &&
                   this.Platform == other.Platform &&
                   this.CompressionVersion == other.CompressionVersion;
        }

        public override bool Equals(object obj)
        {
            return
                obj is Version version &&
                this.Equals(version) == true;
        }

        public override int GetHashCode()
        {
            int hashCode = -2033215402;
            hashCode = hashCode * -1521134295 + this.FileVersion.GetHashCode();
            hashCode = hashCode * -1521134295 + this.Platform.GetHashCode();
            hashCode = hashCode * -1521134295 + this.CompressionVersion.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Version left, Version right)
        {
            return left.Equals(right) == true;
        }

        public static bool operator !=(Version left, Version right)
        {
            return left.Equals(right) == false;
        }
    }
}

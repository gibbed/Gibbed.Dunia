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

namespace Gibbed.Dunia.FileFormats
{
    public struct FileDetectionInfo : IEquatable<FileDetectionInfo>
    {
        public readonly string Type;
        public readonly string Extension;

        public FileDetectionInfo(string type, string extension)
        {
            this.Type = type;
            this.Extension = extension;
        }

        public void Deconstruct(out string type, out string extension)
        {
            type = this.Type;
            extension = this.Extension;
        }

        public static implicit operator (string directory, string extension)(FileDetectionInfo value)
        {
            return (value.Type, value.Extension);
        }

        public static implicit operator FileDetectionInfo((string type, string extension) value)
        {
            return new FileDetectionInfo(value.type, value.extension);
        }

        public bool Equals(FileDetectionInfo other)
        {
            return
                this.Type == other.Type &&
                this.Extension == other.Extension;
        }

        public override bool Equals(object obj)
        {
            return
                obj is FileDetectionInfo info &&
                this.Equals(info) == true;
        }

        public static bool operator ==(FileDetectionInfo left, FileDetectionInfo right)
        {
            return left.Equals(right) == true;
        }

        public static bool operator !=(FileDetectionInfo left, FileDetectionInfo right)
        {
            return left.Equals(right) == false;
        }

        public override int GetHashCode()
        {
            int hashCode = -993150302;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Type);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Extension);
            return hashCode;
        }
    }
}

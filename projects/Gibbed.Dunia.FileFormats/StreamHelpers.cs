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
using Gibbed.IO;

namespace Gibbed.Dunia.FileFormats
{
    public static class StreamHelpers
    {
        public static uint ReadValuePackedU32(this Stream stream, Endian endian)
        {
            var value = stream.ReadValueU8();
            if (value < 0xFE)
            {
                return value;
            }

            if (value == 0xFE)
            {
                throw new FormatException();
            }

            return stream.ReadValueU32(endian);
        }

        public static void WriteValuePackedU32(this Stream stream, uint value, Endian endian)
        {
            if (value >= 0xFE)
            {
                stream.WriteValueU8(0xFF);
                stream.WriteValueU32(value, endian);
                return;
            }

            stream.WriteValueU8((byte)(value & 0xFF));
        }

        public static uint ReadCount(this Stream stream, out bool isOffset, Endian endian)
        {
            var value = stream.ReadValueU8();
            isOffset = false;

            if (value < 0xFE)
            {
                return value;
            }

            isOffset = value != 0xFF;
            return stream.ReadValueU32(endian);
        }

        public static void WriteCount(this Stream stream, int value, bool isOffset, Endian endian)
        {
            stream.WriteCount((uint)value, isOffset, endian);
        }

        public static void WriteCount(this Stream stream, uint value, bool isOffset, Endian endian)
        {
            if (isOffset == true || value >= 0xFE)
            {
                stream.WriteValueU8((byte)(isOffset == true ? 0xFE : 0xFF));
                stream.WriteValueU32(value, endian);
                return;
            }

            stream.WriteValueU8((byte)(value & 0xFF));
        }
    }
}

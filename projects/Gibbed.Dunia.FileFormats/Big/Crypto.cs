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
    public static class Crypto
    {
        public static uint[] GenerateXTEAKey(uint value)
        {
            var xorBytes = new byte[]
            {
                0x76, 0x41, 0x74, 0x1E,
                0x4E, 0x16, 0x1E, 0x02,
                0x6A, 0x5B, 0x72, 0x0B,
                0x60, 0x4F, 0x72, 0x25,
            };
            var key = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                byte b = (byte)((value >> i) + 0x39);
                byte x = xorBytes[i];
                key[i] = b != x
                  ? (byte)(b ^ x)
                  : (byte)0xFF;
            }
            return new uint[]
            {
                BitConverter.ToUInt32(key, 0),
                BitConverter.ToUInt32(key, 4),
                BitConverter.ToUInt32(key, 8),
                BitConverter.ToUInt32(key, 12),
            };
        }
    }
}

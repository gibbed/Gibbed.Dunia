/* Copyright (c) 2012 Rick (rick 'at' gibbed 'dot' us)
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

namespace Gibbed.Dunia.FileFormats
{
    public static class StringHelpers
    {
        public static uint HashFNV32(this string input)
        {
            return input.HashFNV32(0x811C9DC5);
        }

        public static uint HashFNV32(this string input, uint hash)
        {
            if (input.Length == 0)
            {
                return 0;
            }

            string lower = input.ToLowerInvariant();

            for (int i = 0; i < lower.Length; i++)
            {
                hash *= 0x1000193;
                hash ^= (char)(lower[i]);
            }

            return hash;
        }

        public static uint HashCRC32(this string input)
        {
            return CRC32.Hash(input);
        }

        public static uint HashFileNameCRC32(this string input)
        {
            return input.ToLowerInvariant().HashCRC32();
        }

        
    }
}

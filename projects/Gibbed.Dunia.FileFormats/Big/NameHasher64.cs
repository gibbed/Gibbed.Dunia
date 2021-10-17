﻿/* Copyright (c) 2021 Rick (rick 'at' gibbed 'dot' us)
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

using System.Globalization;
using static Gibbed.Dunia.FileFormats.InvariantShorthand;

namespace Gibbed.Dunia.FileFormats.Big
{
    public class NameHasher64 : INameHasher<ulong>
    {
        public static ulong Compute(string s, TryGetHashOverride<ulong> tryGetOverride)
        {
            if (s == null || s.Length == 0)
            {
                return 0xFFFFFFFFFFFFFFFFul;
            }

            var hash = Hashing.CRC64.Compute(s.ToLowerInvariant());
            if (tryGetOverride != null && tryGetOverride(hash, out var hashOverride) == true)
            {
                return hashOverride;
            }
            return hash;
        }

        public static bool TryParse(string s, out ulong value)
        {
            return ulong.TryParse(s, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value);
        }

        public static string Render(ulong value)
        {
            return _($"{value:X16}");
        }

        ulong INameHasher<ulong>.Compute(string s, TryGetHashOverride<ulong> tryGetOverride)
        {
            return Compute(s, tryGetOverride);
        }

        bool INameHasher<ulong>.TryParse(string s, out ulong value)
        {
            return TryParse(s, out value);
        }

        string INameHasher<ulong>.Render(ulong value)
        {
            return Render(value);
        }
    }
}

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
using Big = Gibbed.Dunia.FileFormats.Big;

namespace Gibbed.Dunia.Packing
{
    internal sealed class BoundArchive<TArchive, THash>
        where TArchive : Big.IArchive<THash>, new()
    {
        public TArchive Fat { get; private set; }
        public string FatPath { get; private set; }
        public string DatPath { get; private set; }

        public BoundArchive(string fatPath, string datPath = null)
        {
            Fat = new TArchive();
            using var input = File.OpenRead(fatPath);
            Fat.Deserialize(input);
            FatPath = fatPath;
            DatPath = datPath ?? Path.ChangeExtension(fatPath, ".dat");
        }
    }
}

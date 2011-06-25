/* Copyright (c) 2011 Rick (rick 'at' gibbed 'dot' us)
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

namespace Gibbed.Dunia.ArchiveViewer
{
    internal class FileNameHashComparer : IComparer<uint>
    {
        private ProjectData.HashList<uint> Hashes;

        public FileNameHashComparer(ProjectData.HashList<uint> hashes)
        {
            this.Hashes = hashes;
        }

        public int Compare(uint x, uint y)
        {
            if (this.Hashes[x] == null)
            {
                if (this.Hashes[y] == null)
                {
                    if (x == y)
                    {
                        return 0;
                    }

                    return x < y ? -1 : 1;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (this.Hashes[y] == null)
                {
                    return 1;
                }
                else
                {
                    return String.Compare(this.Hashes[x], this.Hashes[y]);
                }
            }
        }
    }
}

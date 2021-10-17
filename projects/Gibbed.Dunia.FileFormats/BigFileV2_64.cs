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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Gibbed.Dunia.FileFormats.Big;
using Version = Gibbed.Dunia.FileFormats.Big.Version;

namespace Gibbed.Dunia.FileFormats
{
    public class BigFileV2_64 : BigFileV2<ulong>, IArchive<ulong>
    {
        protected override IEntrySerializer<ulong> GetEntrySerializer(int version)
        {
            return _EntrySerializers.TryGetValue(version, out var entrySerializer) == true
                ? entrySerializer
                : throw new InvalidOperationException("entry serializer is missing");
        }

        protected override bool IsKnownVersion(Version version)
        {
            return _KnownVersions.Contains(version) == true;
        }

        private static readonly ReadOnlyCollection<Version> _KnownVersions;
        private static readonly ReadOnlyDictionary<int, IEntrySerializer<ulong>> _EntrySerializers;

        static BigFileV2_64()
        {
            _KnownVersions = new ReadOnlyCollection<Version>(new Version[]
            {
                // Far Cry 3
                // Far Cry 3 Blood Dragon
                // Far Cry 4
                // Far Cry Primal
                (9, Platform.Any, 0),

                // Far Cry 3
                // Far Cry 3 Blood Dragon
                // Far Cry 4
                (9, Platform.Windows, 3),

                // Far Cry Primal
                (9, Platform.Windows, 4),

                // Far Cry 5
                // Far Cry New Dawn
                (10, Platform.Windows, 0),

                // Far Cry 6
                (11, Platform.Windows, 0),
            });

            _EntrySerializers = new ReadOnlyDictionary<int, IEntrySerializer<ulong>>(
                new Dictionary<int, IEntrySerializer<ulong>>()
                {
                    [9] = new EntrySerializerV09(),
                    [10] = new EntrySerializerV10(),
                    [11] = new EntrySerializerV11(),
                });
        }
    }
}

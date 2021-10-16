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
using System.Collections.ObjectModel;
using System.Globalization;
using Gibbed.Dunia.FileFormats.Big;
using Version = Gibbed.Dunia.FileFormats.Big.Version;

namespace Gibbed.Dunia.FileFormats
{
	public class BigFileV2_32 : BigFileV2<uint>, IArchive<uint>
	{
		public static uint ComputeNameHash(string s, TryGetHashOverride<uint> tryGetOverride)
		{
			if (s == null || s.Length == 0)
			{
				return 0xFFFFFFFFu;
			}

			var hash = Hashing.CRC32.Compute(s.ToLowerInvariant());
			if (tryGetOverride != null && tryGetOverride(hash, out var hashOverride) == true)
			{
				return hashOverride;
			}
			return hash;
		}

		public static bool TryParseNameHash(string s, out uint value)
		{
			return uint.TryParse(s, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value);
		}

		public static string RenderNameHash(uint value)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0:X8}", value);
		}

		uint IArchive<uint>.ComputeNameHash(string s, TryGetHashOverride<uint> tryGetOverride)
		{
			return ComputeNameHash(s, tryGetOverride);
		}

		bool IArchive<uint>.TryParseNameHash(string s, out uint value)
		{
			return TryParseNameHash(s, out value);
		}

		string IArchive<uint>.RenderNameHash(uint value)
		{
			return RenderNameHash(value);
		}

		protected override IEntrySerializer<uint> GetEntrySerializer(int version)
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
		private static readonly ReadOnlyDictionary<int, IEntrySerializer<uint>> _EntrySerializers;

		static BigFileV2_32()
		{
			_KnownVersions = new ReadOnlyCollection<Version>(new Version[]
			{
                // Far Cry 2
                (5, Platform.Any, 0),
				(5, Platform.Windows, 3),
				(5, Platform.PS3, 4),

				// Far Cry 3
				(9, Platform.Any, 3),
				(9, Platform.Windows, 3),
			});

			_EntrySerializers = new ReadOnlyDictionary<int, IEntrySerializer<uint>>(
				new Dictionary<int, IEntrySerializer<uint>>()
				{
					[5] = new EntrySerializerV05(),
				});
		}
	}
}

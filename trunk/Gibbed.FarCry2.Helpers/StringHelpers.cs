using System;
using System.Text;

namespace Gibbed.FarCry2.Helpers
{
	public static class StringHelpers
	{
		public static uint CRC32(this string input)
		{
			return BitConverter.ToUInt32(new CRC32().ComputeHash(Encoding.ASCII.GetBytes(input)), 0).Swap();
		}

		public static uint FileNameCRC32(this string input)
		{
			return input.Replace('/', '\\').ToLower().CRC32();
		}

		public static uint GetHexNumber(this string input)
		{
			if (input.StartsWith("0x"))
			{
				return uint.Parse(input.Substring(2), System.Globalization.NumberStyles.AllowHexSpecifier);
			}

			return uint.Parse(input);
		}
	}
}

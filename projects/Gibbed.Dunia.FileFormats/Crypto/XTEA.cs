using System;

namespace Gibbed.Dunia.FileFormats.Crypto
{
    public static class XTEA
    {
        private const uint _Delta = 0x9E3779B9u;
        private const uint _Rounds = 32;
        private const uint _Sum = unchecked(_Delta * _Rounds);

        private static void Decrypt(ref uint v0, ref uint v1, uint[] key, uint sum)
        {
            for (uint i = 0; i < _Rounds; i++)
            {
                v1 -= (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
                sum -= _Delta;
                v0 -= (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
            }
        }

        public static void Decrypt(byte[] data, int offset, int count, uint[] keys, uint sum)
        {
            for (int i = offset; i + 8 <= offset + count; i += 8)
            {
                uint v0 = BitConverter.ToUInt32(data, i + 0);
                uint v1 = BitConverter.ToUInt32(data, i + 4);
                Decrypt(ref v0, ref v1, keys, sum);
                Array.Copy(BitConverter.GetBytes(v0), 0, data, i + 0, 4);
                Array.Copy(BitConverter.GetBytes(v1), 0, data, i + 4, 4);
            }
        }

        public static void Decrypt(byte[] data, int offset, int count, uint[] keys)
        {
            Decrypt(data, offset, count, keys, _Sum);
        }
    }
}

using System;
using System.IO;
using System.Text;

namespace Gibbed.FarCry2.Helpers
{
	public static class StreamHelpers
	{
		/// <summary>
		/// Read an 8-bit boolean value.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static bool ReadBoolean(this Stream stream)
		{
			return stream.ReadU8() > 0 ? true : false;
		}

		public static void WriteBoolean(this Stream stream, bool value)
		{
			stream.WriteU8((byte)(value == true ? 1 : 0));
		}

		/// <summary>
		/// Read an unsigned 8-bit integer.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static byte ReadU8(this Stream stream)
		{
			return (byte)stream.ReadByte();
		}

		public static void WriteU8(this Stream stream, byte value)
		{
			stream.WriteByte(value);
		}

		/// <summary>
		/// Read a signed 8-bit integer;
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static char ReadS8(this Stream stream)
		{
			return (char)stream.ReadByte();
		}

		public static void WriteS8(this Stream stream, char value)
		{
			stream.WriteByte((byte)value);
		}

		/// <summary>
		/// Read a signed 16-bit integer.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static Int16 ReadS16(this Stream stream)
		{
			byte[] data = new byte[2];
			stream.Read(data, 0, 2);
			return BitConverter.ToInt16(data, 0);
		}

		public static void WriteS16(this Stream stream, Int16 value)
		{
			byte[] data = BitConverter.GetBytes(value);
			stream.Write(data, 0, 2);
		}

		/// <summary>
		/// Read a signed 16-bit big-endian integer.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static Int16 ReadS16BE(this Stream stream)
		{
			byte[] data = new byte[2];
			stream.Read(data, 0, 2);
			return BitConverter.ToInt16(data, 0).Swap();
		}

		public static void WriteS16BE(this Stream stream, Int16 value)
		{
			byte[] data = BitConverter.GetBytes(value.Swap());
			stream.Write(data, 0, 2);
		}

		/// <summary>
		/// Read an unsigned 16-bit integer.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static UInt16 ReadU16(this Stream stream)
		{
			byte[] data = new byte[2];
			stream.Read(data, 0, 2);
			return BitConverter.ToUInt16(data, 0);
		}

		public static void WriteU16(this Stream stream, UInt16 value)
		{
			byte[] data = BitConverter.GetBytes(value);
			stream.Write(data, 0, 2);
		}

		/// <summary>
		/// Read an unsigned 16-bit big-endian integer.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static UInt16 ReadU16BE(this Stream stream)
		{
			byte[] data = new byte[2];
			stream.Read(data, 0, 2);
			return BitConverter.ToUInt16(data, 0).Swap();
		}

		public static void WriteU16BE(this Stream stream, UInt16 value)
		{
			byte[] data = BitConverter.GetBytes(value.Swap());
			stream.Write(data, 0, 2);
		}

		public static UInt32 ReadU24BE(this Stream stream)
		{
			byte[] data = new byte[4];
			stream.Read(data, 1, 3);
			return BitConverter.ToUInt32(data, 0).Swap();
		}

		public static void WriteU24BE(this Stream stream, UInt32 value)
		{
			byte[] data = BitConverter.GetBytes(value.Swap());
			stream.Write(data, 1, 3);
		}

		/// <summary>
		/// Read a signed 32-bit integer.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static Int32 ReadS32(this Stream stream)
		{
			byte[] data = new byte[4];
			stream.Read(data, 0, 4);
			return BitConverter.ToInt32(data, 0);
		}

		public static void WriteS32(this Stream stream, Int32 value)
		{
			byte[] data = BitConverter.GetBytes(value);
			stream.Write(data, 0, 4);
		}

		/// <summary>
		/// Read a signed 32-bit big-endian integer.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static Int32 ReadS32BE(this Stream stream)
		{
			byte[] data = new byte[4];
			stream.Read(data, 0, 4);
			return BitConverter.ToInt32(data, 0).Swap();
		}

		public static void WriteS32BE(this Stream stream, Int32 value)
		{
			byte[] data = BitConverter.GetBytes(value.Swap());
			stream.Write(data, 0, 4);
		}

		/// <summary>
		/// Read an unsigned 32-bit integer.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static UInt32 ReadU32(this Stream stream)
		{
			byte[] data = new byte[4];
			stream.Read(data, 0, 4);
			return BitConverter.ToUInt32(data, 0);
		}

		public static void WriteU32(this Stream stream, UInt32 value)
		{
			byte[] data = BitConverter.GetBytes(value);
			stream.Write(data, 0, 4);
		}

		/// <summary>
		/// Read an unsigned 32-bit big-endian integer.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static UInt32 ReadU32BE(this Stream stream)
		{
			byte[] data = new byte[4];
			stream.Read(data, 0, 4);
			return BitConverter.ToUInt32(data, 0).Swap();
		}

		public static void WriteU32BE(this Stream stream, UInt32 value)
		{
			byte[] data = BitConverter.GetBytes(value.Swap());
			stream.Write(data, 0, 4);
		}

		/// <summary>
		/// Read a signed 64-bit integer.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static Int64 ReadS64(this Stream stream)
		{
			byte[] data = new byte[8];
			stream.Read(data, 0, 8);
			return BitConverter.ToInt64(data, 0);
		}

		/// <summary>
		/// Read a signed 64-bit big-endian integer.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static Int64 ReadS64BE(this Stream stream)
		{
			byte[] data = new byte[8];
			stream.Read(data, 0, 8);
			return BitConverter.ToInt64(data, 0).Swap();
		}

		/// <summary>
		/// Read an unsigned 64-bit integer.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static UInt64 ReadU64(this Stream stream)
		{
			byte[] data = new byte[8];
			stream.Read(data, 0, 8);
			return BitConverter.ToUInt64(data, 0);
		}

		/// <summary>
		/// Read an unsigned 64-bit big-endian integer.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static UInt64 ReadU64BE(this Stream stream)
		{
			byte[] data = new byte[8];
			stream.Read(data, 0, 8);
			return BitConverter.ToUInt64(data, 0).Swap();
		}

		/// <summary>
		/// Read a 32-bit floating point number.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static Single ReadF32(this Stream stream)
		{
			byte[] data = new byte[4];
			stream.Read(data, 0, 4);
			return BitConverter.ToSingle(data, 0);
		}

		public static void WriteF32(this Stream stream, Single value)
		{
			byte[] data = BitConverter.GetBytes(value);
			stream.Write(data, 0, 4);
		}

		/// <summary>
		/// Read a 32-bit floating point number.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static Single ReadF32BE(this Stream stream)
		{
			byte[] data = new byte[4];
			stream.Read(data, 0, 4);
			UInt32 value = BitConverter.ToUInt32(data, 0).Swap();
			data = BitConverter.GetBytes(value);
			return BitConverter.ToSingle(data, 0);
		}

		public static void WriteF32BE(this Stream stream, Single value)
		{
			byte[] data = BitConverter.GetBytes(value);
			UInt32 swappedvalue = BitConverter.ToUInt32(data, 0).Swap();
			data = BitConverter.GetBytes(swappedvalue);
			stream.Write(data, 0, 4);
		}

		/// <summary>
		/// Read a 64-bit floating point number.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static Double ReadF64(this Stream stream)
		{
			byte[] data = new byte[8];
			stream.Read(data, 0, 8);
			return BitConverter.ToDouble(data, 0);
		}

		public static void WriteF64(this Stream stream, Double value)
		{
			byte[] data = BitConverter.GetBytes(value);
			stream.Write(data, 0, 8);
		}

		/// <summary>
		/// Read a 64-bit floating point number.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static Double ReadF64BE(this Stream stream)
		{
			return BitConverter.Int64BitsToDouble((long)stream.ReadU64BE());
		}

		public static void WriteF64BE(this Stream stream, Double value)
		{
			byte[] data = BitConverter.GetBytes(value);
			UInt64 swappedvalue = BitConverter.ToUInt64(data, 0).Swap();
			data = BitConverter.GetBytes(swappedvalue);
			stream.Write(data, 0, 8);
		}

		public static string ReadASCII(this Stream stream, uint size)
		{
			byte[] data = new byte[size];
			stream.Read(data, 0, data.Length);
			return Encoding.ASCII.GetString(data);
		}

		public static void WriteASCII(this Stream stream, string value)
		{
			byte[] data = Encoding.ASCII.GetBytes(value);
			stream.Write(data, 0, data.Length);
		}
	}
}

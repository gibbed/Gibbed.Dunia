using System;
using System.Runtime.InteropServices;

namespace Gibbed.FarCry2.Helpers
{
	public static class ByteHelpers
	{
		public static object BytesToStructure(this byte[] data, Type type)
		{
			if (data.Length != Marshal.SizeOf(type))
			{
				throw new Exception("structure size is not the same as the data size");
			}

			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			object structure = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), type);
			handle.Free();
			return structure;
		}
	}
}

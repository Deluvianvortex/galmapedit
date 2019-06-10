using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalMapEdit
{
	class Thing
	{
		public string name { get; set; }
		public string desc { get; set; }
		public int x { get; set; }
		public int y { get; set; }
		public int type { get; set; }
		public int mass { get; set; }
		public object host { get; set; }

		public byte[] CombineByteArrays(byte[] arr1, byte[] arr2)
		{
			byte[] combined = new byte[arr1.Length + arr2.Length];

			for (int i = 0; i < combined.Length; ++i)
			{
				combined[i] = i < arr1.Length ? arr1[i] : arr2[i - arr1.Length];
			}
			return combined;
		}

		public byte[] Export()
		{
			byte[] spacer = new byte[3] { 0xFF, 0xFF, 0xFF };
			byte[] data = new byte[0];
			byte[] buffer = new byte[0];

			byte[] Name = Encoding.ASCII.GetBytes(name);
			byte[] Desc = Encoding.ASCII.GetBytes(desc);

			byte X = Convert.ToByte(x);
			byte Y = Convert.ToByte(y);
			byte Type = Convert.ToByte(type);
			byte Mass = Convert.ToByte(mass);

			for (int i = 0; i < 3; i++)
			{
				if (i == 0)
				{
					buffer = CombineByteArrays(Name, spacer);
				}
				if (i == 1)
				{
					buffer = CombineByteArrays(Desc, spacer);
				}
				if (i == 2)
				{
					buffer = new byte[4];
					buffer[0] = X;
					buffer[1] = Y;
					buffer[2] = Type;
					buffer[3] = Mass;

					buffer = CombineByteArrays(buffer, spacer);
				}
				data = CombineByteArrays(data, buffer);
			}
			return data;
		}
	}
}

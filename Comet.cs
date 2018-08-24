using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalMapEdit
{
	[Serializable]
	class Comet
	{
		public int x, y;

		public string name;
		public UInt16 type;
		public int mass;

		private Sector host;

		public Comet(int X, int Y, string n, UInt16 Type, Sector Host)
		{
			x = X;
			y = Y;
			name = n;
			type = Type;
			host = Host;
		}

		public void Destroy()
		{
			host.comets.Remove(this);
		}
	}
}

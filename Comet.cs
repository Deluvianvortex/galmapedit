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
		public int type;
		public int mass;
		public string desc;

		private Sector host;

		public Comet(int X, int Y, string n, string d, int Type, int Mass, Sector Host)
		{
			x = X;
			y = Y;
			name = n;
			type = Type;
			host = Host;
			mass = Mass;
			desc = d;
		}

		public void Destroy()
		{
			host.comets.Remove(this);
		}
	}
}

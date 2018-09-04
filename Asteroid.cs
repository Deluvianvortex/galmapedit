using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalMapEdit
{
	[Serializable]
	class Asteroid
	{
		public int x, y;
		public string name, desc;
		public int type, mass;

		public object host;

		public Asteroid(int X, int Y, int t, int m, string n, string d, object h)
		{
			x = X;
			y = Y;
			type = t;
			mass = m;
			name = n;
			desc = d;
			host = h;
		}

		public void Destroy()
		{
			if (host is Sector s)
			{
				s.asteroids.Remove(this);
			}
			else if (host is Star st)
			{
				st.asteroids.Remove(this);
			}
			else if (host is Planet p)
			{
				p.asteroids.Remove(this);
			}
			else if (host is Moon m)
			{
				m.asteroids.Remove(this);
			}
		}

	}
}

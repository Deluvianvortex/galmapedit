using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalMapEdit
{
	[Serializable]
	class Moon : Thing
	{
		public List<Asteroid> asteroids;

		public Moon(string Name, string Desc, int dist, int ang, int Mass, int Type, Planet Host )
		{
			name = Name;
			desc = Desc;
			x = dist;
			y = ang;
			mass = Mass;
			type = Type;
			host = Host;
			asteroids = new List<Asteroid>();
		}

		public void Destroy()
		{
			if (host is Planet p)
			{
				p.moons.Remove(this);
			}
		}
	}
}

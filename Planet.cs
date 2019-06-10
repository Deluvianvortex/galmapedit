using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalMapEdit
{
	[Serializable]
	class Planet : Thing
	{
		public List<Moon> moons = new List<Moon>();
		public List<Asteroid> asteroids = new List<Asteroid>();

		public Planet(string Name, string Desc, int dist, int ang, int Type, int Mass, Star Host)
		{
			name = Name;
			desc = Desc;
			x = dist;
			y = ang;
			type = Type;
			host = Host;
			mass = Mass;
		}

		public void Destroy()
		{
			if (host is Star s)
			{
				s.planets.Remove(this);
			}
		}
	}
}
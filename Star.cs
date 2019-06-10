using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalMapEdit
{
	[Serializable]
	class Star : Thing
	{
		public List<Planet> planets = new List<Planet>();
		public List<Asteroid> asteroids = new List<Asteroid>();

		public Star(string n, string d, int x1, int y1, int Type, int Mass, Sector Host)
		{
			name = n;
			x = x1;
			y = y1;
			desc = d;
			type = Type;
			host = Host;
			mass = Mass;
		}

		public void addAsteroid(Asteroid ast)
		{
			asteroids.Add(ast);
		}
		public void addPlanet(Planet pla)
		{
			planets.Add(pla);
		}

		public void Destroy()
		{
			if (host is Sector s)

			s.stars.Remove(this);
		}
	}
}

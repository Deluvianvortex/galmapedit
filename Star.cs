using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalMapEdit
{
	[Serializable]
	class Star
	{
		public string name;
		public int x;
		public int y;

		public int mass;

		public UInt32 type;

		public List<Planet> planets = new List<Planet>();
		public List<Asteroid> asteroids = new List<Asteroid>();

		private Sector host;

		public Star(string n, int x1, int y1, UInt16 Type, Sector Host)
		{
			name = n;
			x = x1;
			y = y1;
			type = Type;
			host = Host;
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
			host.stars.Remove(this);
		}
	}
}

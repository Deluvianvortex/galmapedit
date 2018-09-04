using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalMapEdit
{
	[Serializable]
	class Planet
	{
		private string name;                     // planet name
		private string desc;

		private int distance;					// distance from host star
		private int angle;                      // angle from host star
		private int mass;

		public int type;

		private Star host;						// star in which this planet is in orbit around

		public List<Moon> moons = new List<Moon>();
		public List<Asteroid> asteroids = new List<Asteroid>();

		public Planet(string Name, string Desc, int dist, int ang, int Type, int Mass, Star Host)
		{
			name = Name;
			desc = Desc;
			distance = dist;
			angle = ang;
			type = Type;
			host = Host;
			mass = Mass;
		}

		public string getName()
		{
			return name;
		}

		public string getDescription()
		{
			return desc;
		}

		public int getDistance()
		{
			return distance;
		}

		public int getAngle()
		{
			return angle;
		}

		public int getMass()
		{
			return mass;
		}

		public void Destroy()
		{
			host.planets.Remove(this);
		}
	}
}
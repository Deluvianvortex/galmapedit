using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalMapEdit
{
	[Serializable]
	class Moon
	{
		private string name;
		private string desc;

		private int distance;
		private int angle;

		private int mass;

		public int type;

		public List<Asteroid> asteroids;


		private Planet host;

		public Moon(string Name, string Desc, int dist, int ang, int Mass, int Type, Planet Host )
		{
			name = Name;
			desc = Desc;
			distance = dist;
			angle = ang;
			mass = Mass;
			type = Type;
			host = Host;
			asteroids = new List<Asteroid>();
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
			host.moons.Remove(this);
		}

	}
}

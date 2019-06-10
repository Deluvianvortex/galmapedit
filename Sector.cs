using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalMapEdit
{
	// sector classes hold all the information on a particular cell.
	// sectors are 100x100 tiles ingame. There are 181x181 sectors in a galaxy.
	[Serializable]
	class Sector
	{
		public string name;
		public int x;
		public int y;
		
		public List<Star> stars;
		public List<Comet> comets;
		public List<Asteroid> asteroids;


		public Sector(string n, int xcoord, int ycoord)
		{
			stars = new List<Star>();
			comets = new List<Comet>();
			asteroids = new List<Asteroid>();

			name = n;

			x = xcoord;
			y = ycoord;
		}

		public void AddStar(Star st)
		{
			stars.Add(st);
		}
		public void AddComet(Comet com)
		{
			comets.Add(com);
		}
		public void AddAsteroid(Asteroid ast)
		{
			asteroids.Add(ast);
		}

		public byte[] Export()
		{
			byte[] data = new byte[0];
			// lol this doesn't do anything right now
			return data; 
		}

	}
}

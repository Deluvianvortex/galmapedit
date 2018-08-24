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
		public string name;
		public UInt16 type;

		public Asteroid(string n)
		{
			name = n;
		}

	}
}

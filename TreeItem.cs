using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalMapEdit
{
	class TreeItem
	{
		public string name;
		public int ID;
		public int parentID;

		public object tag;

		public TreeItem(string Name, int id, int parent)
		{
			parentID = parent;
			ID = id;
			name = Name;
		}
	}
}

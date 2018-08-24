using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GalMapEdit
{
	public partial class Form1 : Form
	{
		private const int GRIDSIZE = 5;																								// size of each grid square in pixels
		private const int GRID_MIN = 0;																								// just zero
		private const int GRID_MAX = 181;                                                                                           // number of squares in the grid

		private const int SECTORSIZE = 100;

		private const int GALAXY_SIZE = 181;                                                                                        // galaxy_size should equal grid_max 


		private const int PLANET_MAXDIST = 32000;                                                                                   // maximum distance a planet can be from its host star

		private int xcoord, ycoord;																									// these are the sector coords when the mouse moves
		private int selectionX, selectionY;                                                                                         // this is the sector coords when you click the mouse

		private string filename;

		private StatusBar bar;
		private StatusBarPanel statusPanel;
		private StatusBarPanel xyPanel;

		private Sector[,] galaxy;																									// this is the currently loaded galaxy
		private Sector selected;																									// this is the currently selected sector

		private Comet newComet;
		private Star newStar;
		private Planet newPlanet;
		private Moon newMoon;

		private Random rnd;

		private List<TreeItem> treeQueue;																							// this caused so much headache to get to work (but it works!)

		public Form1()
		{
			InitializeComponent();
			SetupGUI();																												// boilerplate bullshit
			selected = null;																										// start with no sector selected
			rnd = new Random();
			SpoolNewGalaxy();																										// lets get this shit started!
		}

		private void SetupGUI()
		{
			bar = new StatusBar();
			statusPanel = new StatusBarPanel();
			xyPanel = new StatusBarPanel();

			massBox.Hide();
			nameBox.Hide();
			typeBox.Hide();
			distBox.Hide();
			angleBox.Hide();
			list1.Hide();
			list2.Hide();
			descBox.Hide();

			statusPanel.BorderStyle = StatusBarPanelBorderStyle.Sunken;
			statusPanel.Text = "Program Started.";
			statusPanel.ToolTipText = "Latest Activity.";
			statusPanel.AutoSize = StatusBarPanelAutoSize.Spring;

			bar.Panels.Add(statusPanel);

			xyPanel.BorderStyle = StatusBarPanelBorderStyle.Sunken;
			xyPanel.Text = String.Format("X: {0} / Y: {1}", xcoord, ycoord);
			xyPanel.ToolTipText = "The x/y coordinate of the mouse cursor.";
			xyPanel.AutoSize = StatusBarPanelAutoSize.Contents;

			bar.Panels.Add(xyPanel);

			bar.ShowPanels = true;
			Controls.Add(bar);
			
			Name = "GalMapEdit";
			Text = "Galaxy Map Editor";
			FormBorderStyle = FormBorderStyle.FixedSingle;


		}

		private void SpoolNewGalaxy()
		{
			galaxy = new Sector[GALAXY_SIZE, GALAXY_SIZE];                                                                          // creates the map but doesn't populate it

			for (int y = 0; y < GALAXY_SIZE; y++)
			{
				for (int x = 0; x < GALAXY_SIZE; x++)
				{
					galaxy[x, y] = new Sector("Sector", x, y);																		// populates the galaxy with empty sectors
				}
			}
		}

		private void refreshTree()
		{
			treeView1.Nodes.Clear();																								// Clear the nodes
			treeView1.BeginUpdate();																								// Start the update for the tree
			int i = 4;																												// has to be 4 because 3 nodes exist in the list already
			treeQueue = new List<TreeItem>()
			{
				new TreeItem("Sector", 1, 0),																						// sector is the root node
				new TreeItem("Stars", 2, 1),																						// stars is first child node
				new TreeItem("Comets", 3, 1)																						// comets is second child node
			};
			if (selected.comets.Count > 0)																							// don't do this shit if there's no comets in the sector
			{
				foreach (Comet c in selected.comets)																				
				{
					treeQueue.Add(new TreeItem("Comet", i, 3) { tag = c });                                                         // add a comet to the queue and set its parent to the root comet node
					i++;																											
				}
			}
			if (selected.stars.Count > 0)																							// So, there is definitely a way to do this recursively, but the																						
			{                                                                                                                       // node structure is only, at most, 5 levels deep, so why bother?
				foreach (Star s in selected.stars)
				{
					treeQueue.Add(new TreeItem("Star", i, 2) { tag = s });
					i++;
					treeQueue.Add(new TreeItem("Planets", i, i - 1) { tag = s });													// the only real issue with doing it this way is making sure the nodes
					i++;																											// align with their parents correctly, but the fix is just to run 2 
					int j = 1;																										// reverse iterators and subtract those numbers. Simple.
					foreach (Planet p in s.planets)
					{
						treeQueue.Add(new TreeItem("Planet", i, i - j) { tag = p });
						i++;
						j++;
						treeQueue.Add(new TreeItem("Moons", i, i - 1) { tag = p });
						i++;
						j++;
						int k = 1;
						foreach (Moon m in p.moons)
						{
							treeQueue.Add(new TreeItem("Moon", i, i - k) { tag = m });
							i++;
							j++;
							k++;
						}
					}
				}
			}

			PopulateTreeView(0, null);																								// this took entirely too long to figure out
			treeView1.EndUpdate();                                                                                                  // End the update cycle
			treeView1.Refresh();                                                                                                    // refresh the treeview.
			treeQueue.Clear();
		}

		private void PopulateTreeView(int parentId, TreeNode parentNode)
		{
			var filteredItems = treeQueue.Where(item =>																				// some linq bullshit to filter out the correct nodes
										item.parentID == parentId);

			TreeNode childNode;																										// make a child node
			foreach (var i in filteredItems.ToList())																				// iterate through the list of the filtered nodes
			{
				if (parentNode == null)                                                                                             // if the current node's parent node is null, its a root node, so,
				{
					childNode = treeView1.Nodes.Add(i.name);                                                                        // add it to the root of the treeview
					childNode.Tag = i.tag;																							// add a pointer to the object the node references.
				}
				else
				{
					childNode = parentNode.Nodes.Add(i.name);                                                                       // Otherwise, add it to the parent node that was referenced
					childNode.Tag = i.tag;																							// add a pointer to the object the node references.
				}
				PopulateTreeView(i.ID, childNode);																					// then, recurse with the child node we just made unil no nodes remain.
			}
		}

		private void pictureBox1_Click(object sender, EventArgs e)
		{
			selectionX = xcoord;
			selectionY = ycoord;

			selected = galaxy[selectionX, selectionY];
			pictureBox1.Refresh();
			if (selected != null)
			{
				refreshTree();
			}
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			pictureBox1.BackColor = Color.Black;
			pictureBox1.Paint += new PaintEventHandler(this.pictureBox1_Paint);
			pictureBox1.MouseMove += new MouseEventHandler(this.pictureBox1_MouseMove);
			pictureBox1.Click += new EventHandler(this.pictureBox1_Click);
			
			Controls.Add(pictureBox1);
		}

		private void pictureBox1_Paint(object sender, PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			Pen scribble = new Pen(Color.Gray);
			Pen selection = new Pen(Color.Wheat);

			g.DrawRectangle(selection, selectionX * GRIDSIZE, selectionY * GRIDSIZE, GRIDSIZE, GRIDSIZE);

			scribble.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

			for (int i = 0; i < 181; i++)
			{
				// Vertical
				g.DrawLine(scribble, i * GRIDSIZE, GRID_MIN, i * GRIDSIZE, GRID_MAX * GRIDSIZE);
				// Horizontal
				g.DrawLine(scribble, GRID_MIN, i * GRIDSIZE, GRID_MAX * GRIDSIZE, i * GRIDSIZE);
			}
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{
			// x box

			int temp = 0;
			string error1 = "Enter only numbers.";
			string error3 = "Number cannot be greater than 180 or less than 0.";
			MessageBoxButtons button = MessageBoxButtons.OK;
			DialogResult result;

			try
			{
				Int32.TryParse(textBox1.Text, out temp);
			}
			catch (FormatException)
			{
				result = MessageBox.Show(error1, "Error", button);
			}
			finally
			{
				if (temp > 180 || temp < 0)
				{
					result = MessageBox.Show(error3, "Error", button);
				}
				else selectionX = temp;
			}
			selected = galaxy[selectionX, selectionY];
			pictureBox1.Refresh();

			if (selected != null)
			{
				refreshTree();
			}
		}

		private void treeView1_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			// loads the doubleclicked object into the editor

			object select = treeView1.SelectedNode.Tag;

			List<string> onelist = new List<string>();
			List<string> twolist = new List<string>();

			if (select is Sector s)
			{
				nameBox.Show();
				nameBox.Text = s.name;
				typeBox.Hide();
				massBox.Hide();
				distBox.Hide();

				list1.Show();
				list2.Show();
			
				foreach (Star fire in s.stars)
				{
					onelist.Add(fire.name.ToString());	
				}
				foreach (Comet comet in s.comets)
				{
					twolist.Add(comet.name.ToString());
				}

				list1.DataSource = onelist;
				list2.DataSource = twolist;
				
			}
			if (select is Star st)
			{
				nameBox.Show();
				nameBox.Text = st.name;
				massBox.Show();
				massBox.Text = st.mass.ToString();
				typeBox.Show();
				typeBox.Text = st.type.ToString();
				distBox.Hide();
				angleBox.Hide();
				list1.Show();
				foreach (Planet pl in st.planets)
				{
					onelist.Add(pl.getName());
				}
				list1.DataSource = onelist;


			}
			if (select is Comet c)
			{
				nameBox.Show();
				nameBox.Text = c.name;
				massBox.Show();
				massBox.Text = c.mass.ToString();
				typeBox.Show();
				typeBox.Text = c.type.ToString();
				distBox.Hide();
				angleBox.Hide();
				list1.Hide();
				list2.Hide();
			}
			if (select is Planet p)
			{
				nameBox.Show();
				nameBox.Text = p.getName();
				massBox.Show();
				massBox.Text = p.getMass().ToString();
				typeBox.Show();
				typeBox.Text = p.type.ToString();
				distBox.Show();
				distBox.Text = p.getDistance().ToString();
				angleBox.Show();
				angleBox.Text = p.getAngle().ToString();

				list1.Show();
				foreach (Moon mo in p.moons)
				{
					onelist.Add(mo.getName());
				}
				list1.DataSource = onelist;
				list2.Hide();
			}
			if (select is Moon m)
			{
				nameBox.Show();
				nameBox.Text = m.getName();
				massBox.Show();
				massBox.Text = m.getMass().ToString();
				typeBox.Show();
				typeBox.Text = m.type.ToString();
				distBox.Show();
				distBox.Text = m.getDistance().ToString();
				angleBox.Show();
				angleBox.Text = m.getAngle().ToString();
				list1.Hide();
				list2.Hide();
			}

			
			
			
		}

		private void textBox2_TextChanged(object sender, EventArgs e)
		{
			// y box
			int temp = 0;
			string error1 = "Enter only numbers.";
			string error3 = "Number cannot be greater than 180 or less than 0.";
			MessageBoxButtons button = MessageBoxButtons.OK;
			DialogResult result;
			
			try
			{
				Int32.TryParse(textBox2.Text, out temp);
			}
			catch (FormatException)
			{
				result = MessageBox.Show(error1, "Error", button);
			}
			finally
			{
				if (temp > 180 || temp < 0)
				{
					result = MessageBox.Show(error3, "Error", button);
				}
				else selectionY = temp;
			}
			selected = galaxy[selectionX, selectionY];
			pictureBox1.Refresh();
			if (selected != null)
			{
				refreshTree();
			}

		}

		private void addToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// I don't know why this is here..
		}

		private void removeAsteroidToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// remove asteroid menu <todo>
		}

		private void treeView1_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)																						// All right clicks in the treeview add/remove objects to the sector
			{
				// Select the clicked node
				treeView1.SelectedNode = treeView1.GetNodeAt(e.X, e.Y);

				if (treeView1.SelectedNode != null)
				{

					if (treeView1.SelectedNode.Text == "Asteroids")
					{
						// add Asteroid
						contextMenuStrip2.Show(treeView1, e.Location);
					}
					if (treeView1.SelectedNode.Text == "Stars")
					{
						// add Star
						contextMenuStrip5.Show(treeView1, e.Location);
					}
					if (treeView1.SelectedNode.Text == "Asteroid")
					{
						// remove asteroid
						contextMenuStrip3.Show(treeView1, e.Location);
					}
					if (treeView1.SelectedNode.Text == "Star")
					{
						// remove star
						contextMenuStrip6.Show(treeView1, e.Location);
					}
					if (treeView1.SelectedNode.Text == "Planets")
					{
						// add planet
						contextMenuStrip1.Show(treeView1, e.Location);
					}
					if (treeView1.SelectedNode.Text == "Planet")
					{
						// remove planet
						contextMenuStrip4.Show(treeView1, e.Location);
					}
					if (treeView1.SelectedNode.Text == "Moons")
					{
						// add moon
						contextMenuStrip7.Show(treeView1, e.Location);
					}
					if (treeView1.SelectedNode.Text == "Moon")
					{
						// remove moon
						contextMenuStrip8.Show(treeView1, e.Location);
					}
					if (treeView1.SelectedNode.Text == "Comets")
					{
						// add comet
						contextMenuStrip9.Show(treeView1, e.Location);
					}
					if (treeView1.SelectedNode.Text == "Comet")
					{
						// remove comet
						contextMenuStrip10.Show(treeView1, e.Location);
					}
				}
			}
		}

		private void contextMenuStrip1_Click(object sender, EventArgs e)
		{
			if (treeView1.SelectedNode.Tag is Star tempStar)
			{
				newPlanet = new Planet("Planet", "", 23, 42, 55, tempStar);
				tempStar.addPlanet(newPlanet);
				statusPanel.Text = "Created New Planet!";
			}
			refreshTree();
		}

		private void addAsteroidToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// add asteroid context menu button <todo>
		}

		private void removePlanetToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (treeView1.SelectedNode.Tag is Planet tempPlanet)
			{
				tempPlanet.Destroy();
				statusPanel.Text = "Destroyed a Planet!";
				refreshTree();
			}
		}

		private void addStarToolStripMenuItem_Click(object sender, EventArgs e)
		{
			newStar = new Star("Star", 27, 27, 32, selected);
			selected.stars.Add(newStar);
			refreshTree();
			statusPanel.Text = "Created New Star!";
		}

		private void addCometToolStripMenuItem_Click(object sender, EventArgs e)
		{
			newComet = new Comet(0, 0, "Comet", 52, selected);
			selected.comets.Add(newComet);
			refreshTree();
			statusPanel.Text = "Created New Comet!";
		}

		private void addMoonToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (treeView1.SelectedNode.Tag is Planet tempPlanet)
			{
				newMoon = new Moon("Moon", "A Moon", 12, 31, 430000, 12, tempPlanet);
				tempPlanet.moons.Add(newMoon);
			}
			refreshTree();
			statusPanel.Text = "Created New Moon!";
		}

		private void removeStarToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (treeView1.SelectedNode.Tag is Star tempStar)
			{
				tempStar.Destroy();
				refreshTree();
				statusPanel.Text = "Destroyed a Star!";
			}
		}

		private void removeCometToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (treeView1.SelectedNode.Tag is Comet tempComet)
			{
				tempComet.Destroy();
				refreshTree();
				statusPanel.Text = "Destroyed a Comet!";
			}
		}

		private void blankToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// create new blank galaxy (just sectors)

			
			if (galaxy != null)
			{
				// ask to save current galaxy y/n/cancel
				DialogResult result;
				MessageBoxButtons buttons = MessageBoxButtons.YesNoCancel;
				string message = "Would you like to save the current galaxy?";

				result = MessageBox.Show(message, "Save Map?", buttons);

				if (result == DialogResult.Yes)
				{
					saveCheck();
					SpoolNewGalaxy();
				}
				if (result == DialogResult.No)
				{
					SpoolNewGalaxy();
				}
			}
			
		}

		private void randomToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// create new random galaxy (random everything)

			//ask to save current galaxy y/n/cancel

			DialogResult result;
			MessageBoxButtons buttons = MessageBoxButtons.YesNoCancel;
			string message = "Would you like to save the current galaxy?";

			result = MessageBox.Show(message, "Save Map?", buttons);

			if (result == DialogResult.Yes)
			{
				saveCheck();
				SpoolNewRandomGalaxy();
			}
			if (result == DialogResult.No)
			{
				SpoolNewRandomGalaxy();
			}
		}


		private void SpoolNewRandomGalaxy()
		{
			galaxy = new Sector[GALAXY_SIZE, GALAXY_SIZE];                                                                          // creates the map but doesn't populate it

			for (int y = 0; y < GALAXY_SIZE; y++)
			{
				for (int x = 0; x < GALAXY_SIZE; x++)
				{
					galaxy[x, y] = new Sector("Sector", x, y);

					for (int i = 0; i < (int)(rnd.NextDouble() * 10); i++)
					{
						galaxy[x, y].stars.Add(new Star("Star", (int)(rnd.NextDouble() * SECTORSIZE), (int)(rnd.NextDouble() * SECTORSIZE), 32, galaxy[x, y])); 
					}
					foreach (Star s in galaxy[x, y].stars)
					{
						for (int i = 0; i < (int)(rnd.NextDouble() * 10); i++)
						{
							s.planets.Add(new Planet("Planet", "", (int)(rnd.NextDouble() * 32000), (int)(rnd.NextDouble() * 360), 32, s));
						}
						foreach (Planet p in s.planets)
						{
							for (int i = 0; i < (int)(rnd.NextDouble() * 10); i++)
							{
								p.moons.Add(new Moon("Moon", "", (int)(rnd.NextDouble() * 24000), (int)(rnd.NextDouble() * 360), 43000000, 24, p));
							}
						}
					}
					for (int i = 0; i < (int)(rnd.NextDouble() * 10); i++)
					{
						galaxy[x, y].comets.Add(new Comet((int)rnd.NextDouble()*SECTORSIZE, (int)rnd.NextDouble()*SECTORSIZE, "Comet", 8, galaxy[x, y]));
					}
				}
			}
		}
		private void importFromFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// create galaxy from image file <todo>
		}

		private void contentsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// help -- contents
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// help -- about
		}

		private void saveCheck()
		{
			if (filename != "" || filename != null)
			{
				Export();
			}
			else
			{
				GetFilename();
				Export();
			}
		}

		private void toolStripMenuItem3_Click(object sender, EventArgs e)
		{
			// save
			saveCheck();
			
		}

		private void toolStripMenuItem2_Click(object sender, EventArgs e)
		{
			// Save as..
			GetFilename();
			Export();
		}

		private void GetFilename()
		{
			SaveFileDialog saveFileDialog1 = new SaveFileDialog();
			saveFileDialog1.Filter = "Galaxy Map|*.galmap";
			saveFileDialog1.Title = "Save As..";
			saveFileDialog1.OverwritePrompt = true;
			saveFileDialog1.ValidateNames = true;
			saveFileDialog1.ShowDialog();

			if (saveFileDialog1.FileName != "")
			{
				filename = saveFileDialog1.FileName;
			}
		}

		private void Export()
		{
			WriteToBinaryFile(filename, galaxy, false);
		}

		private void removeMoonToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (treeView1.SelectedNode.Tag is Moon tempMoon)
			{
				tempMoon.Destroy();
				refreshTree();
				statusPanel.Text = "Destroyed a Moon!";
			}
		}

		private void toolStripMenuItem4_Click(object sender, EventArgs e)
		{
			// load galmap
			
			OpenFileDialog openFileDialog1 = new OpenFileDialog();
			openFileDialog1.Filter = "Galaxy Maps|*.galmap";
			openFileDialog1.Title = "Select a Galaxy Map";

			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				galaxy = ReadFromBinaryFile<Sector[,]>(openFileDialog1.OpenFile());
			}
			treeView1.Refresh();
		}

		private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
		{
			textBox1.Text = String.Format("{0}", selectionX);
			textBox2.Text = String.Format("{0}", selectionY);

			xyPanel.Text = String.Format("X: {0} / Y: {1}", xcoord, ycoord);

			xcoord = e.X/GRIDSIZE;
			ycoord = e.Y/GRIDSIZE;
		}

		public static void WriteToBinaryFile<T>(string filePath, T objectToWrite, bool append = false)
		{
			using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
			{
				var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
				binaryFormatter.Serialize(stream, objectToWrite);
			}
		}

		public static T ReadFromBinaryFile<T>(string filePath)
		{
			using (Stream stream = File.Open(filePath, FileMode.Open))
			{
				var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
				return (T)binaryFormatter.Deserialize(stream);
			}
		}

		public static T ReadFromBinaryFile<T>(Stream strm)
		{
			var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
			return (T)binaryFormatter.Deserialize(strm);
		}
	}
}

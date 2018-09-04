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
		private const int GRID_MIN = 0;																								// just zero, but maybe someone wants to change that.
		private const int GRID_MAX = 181;                                                                                           // number of squares in the grid

		private const int SECTORSIZE = 100;																							// number of grid squares INSIDE a sector

		private const int GALAXY_SIZE = 181;                                                                                        // galaxy_size should equal grid_max (it doesn't have to, but it should)

		private const int STAR_MINMASS = 50000000;																					// set to 0 if you want but that makes no sense at all
		private const int STAR_MAXMASS = Int32.MaxValue - STAR_MINMASS;

		private const int PLANET_MAXDIST = 32000;                                                                                   // maximum distance a planet can be from its host star
		private const int PLANET_MINMASS = 10000;
		private const int PLANET_MAXMASS = 40000000;

		private const int MOON_MAXDIST = 24000;
		private const int MOON_MAXMASS = 4200000;
		private const int MOON_MINMASS = 1200;

		private const int COMET_MAXMASS = 1200;
		private const int COMET_MINMASS = 600;

		private const int ASTEROID_MAXDIST = 1600;
		private const int ASTEROID_MINMASS = 120;
		private const int ASTEROID_MAXMASS = 1200;																					// I totally made up each one of these numbers off the top of my head

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
		private Asteroid newAsteroid;
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

		private void SetupGUI()																										// no one wants to look at this.
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
			list3.Hide();
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
			int i = 5;																												// has to be 5 because 4 nodes exist in the list initially
			treeQueue = new List<TreeItem>()
			{
				new TreeItem("Sector", 1, 0),																						// sector is the root node
				new TreeItem("Stars", 2, 1),																						// stars is first child node
				new TreeItem("Comets", 3, 1),																						// comets is second child node
				new TreeItem("Asteroids", 4, 1)
			};
			
			
			foreach (Comet c in selected.comets)																				
			{
				treeQueue.Add(new TreeItem("Comet", i, 3) { tag = selected });														// add a comet to the queue and set its parent to the selected sector
				i++;																											
			}
			
			
			foreach (Asteroid a in selected.asteroids)
			{
				treeQueue.Add(new TreeItem("Asteroid", i, 4) { tag = selected });													// same idea as before, but with asteroids
				i++;
			}

			
			foreach (Star s in selected.stars)																						// this one we set the tags to the host stars
			{
				int starRoot = i;
				treeQueue.Add(new TreeItem("Star", i, 2) { tag = s });
				i++;
				int planetsRoot = i;
				treeQueue.Add(new TreeItem("Planets", i, starRoot) { tag = s });
				i++;
				foreach (Planet p in s.planets)
				{
					int planetRoot = i;
					treeQueue.Add(new TreeItem("Planet", i, planetsRoot) { tag = p });                                              // make a planet node
					i++;
					int moonsRoot = i;
					treeQueue.Add(new TreeItem("Moons", i, planetRoot) { tag = p });												// make a "Moons" subnode 
					i++;
					foreach (Moon m in p.moons)
					{
						int moonRoot = i;
						treeQueue.Add(new TreeItem("Moon", i, moonsRoot) { tag = m });
						i++;
						int moonAsteroidsRoot = i;
						treeQueue.Add(new TreeItem("Asteroids", i, moonRoot) { tag = m });
						i++;
						foreach (Asteroid a in m.asteroids)
						{
							treeQueue.Add(new TreeItem("Asteroid", i, moonAsteroidsRoot) { tag = a });
							i++;
						}
					}
					int planetAsteroidsRoot = i;
					treeQueue.Add(new TreeItem("Asteroids", i, planetRoot) { tag = p });
					i++;
					foreach (Asteroid ast in p.asteroids)
					{
						treeQueue.Add(new TreeItem("Asteroid", i, planetAsteroidsRoot));
						i++;
					}
				}
				int asteroidsRoot = i;
				treeQueue.Add(new TreeItem("Asteroids", i, starRoot) { tag = s });                                                  
				i++;                                                                                                                                                                                                           	
				foreach (Asteroid a in s.asteroids)                                                                             
				{
					treeQueue.Add(new TreeItem("Asteroid", i, asteroidsRoot) { tag = a });												
					i++;																										
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

		private void pictureBox1_Click(object sender, EventArgs e)																	// this is what happens when you click the mouse inside the picturebox
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
			pictureBox1.BackColor = Color.Black;																					// I have to set the variables this way because the designer wouldn't
			pictureBox1.Paint += new PaintEventHandler(this.pictureBox1_Paint);														// let me do it right, but it works.
			pictureBox1.MouseMove += new MouseEventHandler(this.pictureBox1_MouseMove);
			pictureBox1.Click += new EventHandler(this.pictureBox1_Click);
			
			Controls.Add(pictureBox1);
		}

		private void pictureBox1_Paint(object sender, PaintEventArgs e)																// this draws the grid (and anything else) in the picturebox
		{
			Graphics g = e.Graphics;
			Pen scribble = new Pen(Color.Gray);
			Pen selection = new Pen(Color.Wheat);

			g.DrawRectangle(selection, selectionX * GRIDSIZE, selectionY * GRIDSIZE, GRIDSIZE, GRIDSIZE);

			scribble.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

			for (int i = GRID_MIN; i < GRID_MAX; i++)
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
			if (treeView1.SelectedNode.Tag != null)
			{
				object select = treeView1.SelectedNode.Tag;

				List<string> onelist = new List<string>();
				List<string> twolist = new List<string>();
				List<string> threelist = new List<string>();

				if (select is Sector s)
				{
					nameBox.Show();
					nameBox.Text = s.name;
					typeBox.Hide();
					massBox.Hide();
					distBox.Hide();

					list1.Show();
					list2.Show();
					list3.Show();

					onelist.Clear();
					twolist.Clear();
					threelist.Clear();

					foreach (Star fire in s.stars)
					{
						onelist.Add(fire.name.ToString());
					}
					foreach (Comet comet in s.comets)
					{
						twolist.Add(comet.name.ToString());
					}
					foreach (Asteroid roids in s.asteroids)
					{
						threelist.Add(roids.name);
					}

					list1.DataSource = onelist;
					list2.DataSource = twolist;
					list3.DataSource = threelist;
				}
				if (select is Star st)
				{
					nameBox.Show();
					nameBox.Text = st.name;
					massBox.Show();
					massBox.Text = st.mass.ToString();
					typeBox.Show();
					typeBox.Text = st.type.ToString();
					distBox.Show();
					distBox.Text = st.x.ToString();
					angleBox.Show();
					angleBox.Text = st.y.ToString();
					list1.Show();
					list2.Show();
					twolist.Clear();
					onelist.Clear();
					foreach (Planet pl in st.planets)
					{
						onelist.Add(pl.getName());
					}
					list1.DataSource = onelist;
					foreach (Asteroid ast in st.asteroids)
					{
						twolist.Add(ast.name);
					}
					list2.DataSource = twolist;
					list3.Hide();
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
					descBox.Show();
					descBox.Text = c.desc;
					list2.Hide();
					list1.Hide();
					list3.Hide();
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
					descBox.Show();
					descBox.Text = p.getDescription();
					onelist.Clear();
					twolist.Clear();

					list1.Show();
					foreach (Moon mo in p.moons)
					{
						onelist.Add(mo.getName());
					}
					list1.DataSource = onelist;
					list2.Show();
					foreach (Asteroid ast in p.asteroids)
					{
						twolist.Add(ast.name);
					}
					list2.DataSource = twolist;
					list3.Hide();
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
					descBox.Show();
					descBox.Text = m.getDescription();
					onelist.Clear();
					list1.Show();
					foreach (Asteroid ast in m.asteroids)
					{
						onelist.Add(ast.name);
					}
					list1.DataSource = onelist;

					list2.Hide();
					list3.Hide();
				}
				if (select is Asteroid a)
				{
					nameBox.Show();
					nameBox.Text = a.name;
					massBox.Show();
					massBox.Text = a.mass.ToString();
					typeBox.Show();
					typeBox.Text = a.type.ToString();
					descBox.Show();
					descBox.Text = a.desc;
					distBox.Show();
					distBox.Text = a.x.ToString();                                                                                      // distance and angle are x/y in free asteroids
					angleBox.Show();
					angleBox.Text = a.y.ToString();
					list1.Hide();
					list2.Hide();
					list3.Hide();
				}
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
			if (treeView1.SelectedNode.Tag is Asteroid a)
			{
				a.Destroy();
				
				statusPanel.Text = "Destroyed an Asteroid!";
			}
			refreshTree();
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
				newPlanet = new Planet("Planet", "", 23, 42, 55, 12000, tempStar);
				tempStar.addPlanet(newPlanet);
				statusPanel.Text = "Created New Planet!";
			}
			refreshTree();
		}

		private void addAsteroidToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// this one is a little different, since there are 3 different lists that asteroids can be in.

			
			if (treeView1.SelectedNode.Tag == null)																						// null means its floating free in the sector
			{
				newAsteroid = new Asteroid((int)rnd.NextDouble() * SECTORSIZE, (int)rnd.NextDouble() * SECTORSIZE, 8, 450, "Asteroid", "", selected);
				selected.AddAsteroid(newAsteroid);
			}
			else if (treeView1.SelectedNode.Tag is Planet p)
			{
				newAsteroid = new Asteroid((int)rnd.NextDouble() * ASTEROID_MAXDIST, (int)rnd.NextDouble() * 360, 8, 450, "Asteroid", "", p);
				p.asteroids.Add(newAsteroid);
			}
			else if (treeView1.SelectedNode.Tag is Star s)
			{
				newAsteroid = new Asteroid((int)rnd.NextDouble() * ASTEROID_MAXDIST, (int)rnd.NextDouble() * 360, 8, 450, "Asteroid", "", s);
				s.asteroids.Add(newAsteroid);
			}
			else if (treeView1.SelectedNode.Tag is Moon m)
			{
				newAsteroid = new Asteroid((int)rnd.NextDouble() * ASTEROID_MAXDIST, (int)rnd.NextDouble() * 360, 8, 450, "Asteroid", "", m);
				m.asteroids.Add(newAsteroid);
			}
			refreshTree();
			
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
			newStar = new Star("Star", 27, 27, 32, 4000000, selected);
			selected.stars.Add(newStar);
			refreshTree();
			statusPanel.Text = "Created New Star!";
		}

		private void addCometToolStripMenuItem_Click(object sender, EventArgs e)
		{
			newComet = new Comet(0, 0, "Comet", "", 52, 120, selected);
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
						galaxy[x, y].stars.Add(new Star("Star", (int)(rnd.NextDouble() * SECTORSIZE), (int)(rnd.NextDouble() * SECTORSIZE), 32, (int)(rnd.NextDouble() * STAR_MAXMASS + STAR_MINMASS), galaxy[x, y])); 
					}
					foreach (Star s in galaxy[x, y].stars)
					{
						for (int i = 0; i < (int)(rnd.NextDouble() * 10); i++)
						{
							s.planets.Add(new Planet("Planet", "", (int)(rnd.NextDouble() * PLANET_MAXDIST), (int)(rnd.NextDouble() * 360), 32, (int)(rnd.NextDouble() * PLANET_MAXMASS + PLANET_MINMASS), s));
						}
						foreach (Planet p in s.planets)
						{
							for (int i = 0; i < (int)(rnd.NextDouble() * 10); i++)
							{
								p.moons.Add(new Moon("Moon", "", (int)(rnd.NextDouble() * MOON_MAXDIST), (int)(rnd.NextDouble() * 360), (int)(rnd.NextDouble() * MOON_MAXMASS + MOON_MINMASS), 24, p));
							}
							for (int i = 0; i < (int)(rnd.NextDouble() * 10); i++)
							{
								p.asteroids.Add(new Asteroid((int)rnd.NextDouble() * ASTEROID_MAXDIST, (int)rnd.NextDouble() * 360, 2, 200, "Asteroid", "", p));
							}
							foreach (Moon m in p.moons)
							{
								for (int i = 0; i < (int)(rnd.NextDouble() * 10); i++)
								{
									m.asteroids.Add(new Asteroid((int)rnd.NextDouble() * ASTEROID_MAXDIST, (int)rnd.NextDouble() * 360, 2, 200, "Asteroid", "", m));
								}

							}
						}
						for (int i = 0; i < (int)(rnd.NextDouble() * 10); i++)
						{
							s.asteroids.Add(new Asteroid((int)rnd.NextDouble() * ASTEROID_MAXDIST, (int)rnd.NextDouble() * 360, 2, 200, "Asteroid", "", s));
						}
					}
					for (int i = 0; i < (int)(rnd.NextDouble() * 10); i++)
					{
						galaxy[x, y].comets.Add(new Comet((int)rnd.NextDouble()*SECTORSIZE, (int)rnd.NextDouble()*SECTORSIZE, "Comet", "",  8, (int)(rnd.NextDouble() * COMET_MAXMASS + COMET_MINMASS), galaxy[x, y]));
					}
					for (int i = 0; i < (int)(rnd.NextDouble() * 10); i++)
					{
						galaxy[x, y].asteroids.Add(new Asteroid((int)rnd.NextDouble() * SECTORSIZE, (int)rnd.NextDouble() * SECTORSIZE, 2, 200, "Asteroid", "", galaxy[x, y]));
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

		private void nameBox_TextChanged(object sender, EventArgs e)
		{
			// can be anything but you gotta put a limit on length at some point
		}

		private void typeBox_TextChanged(object sender, EventArgs e)
		{
			// can only be integers less than integer.maxvalue
		}

		private void massBox_TextChanged(object sender, EventArgs e)
		{
			// can only be integers less than type.mass_max
		}

		private void distBox_TextChanged(object sender, EventArgs e)
		{
			// can only be integers less than dist.max
		}

		private void angleBox_TextChanged(object sender, EventArgs e)
		{
			// can only be integers less or equal to 360
		}

		private void descBox_TextChanged(object sender, EventArgs e)
		{
			// can be anything but limits etc
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

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

		private Comet newComet;																										// these are here for when a new object needs to transcend scope
		private Moon newMoon;

		private Random rnd;																											// as per technet instructions, only make a single random instance

		private List<TreeItem> treeQueue;                                                                                           // this caused so much headache to get to work (but it works!)

		private object select;

		public Form1()																												// the app's constructor
		{
			InitializeComponent();																									// setup the designer made form
			SetupGUI();																												// this just runs boilerplate bullshit
			selected = null;																										// start with no sector selected
			rnd = new Random();																										// roll the random
			SpoolNewGalaxy();																										// lets get this shit started!
		}

		private void SetupGUI()																										// no one wants to look at this.
		{
			bar = new StatusBar();
			statusPanel = new StatusBarPanel();
			xyPanel = new StatusBarPanel();

			massEditBtn.Hide();
			massLbl.Hide();
			massBox.Hide();

			nameEditBtn.Hide();
			nameLbl.Hide();
			nameBox.Hide();

			typeEditBtn.Hide();
			typeLbl.Hide();
			typeBox.Hide();

			distEditBtn.Hide();
			distLbl.Hide();
			distBox.Hide();

			angEditBtn.Hide();
			angLbl.Hide();
			angleBox.Hide();

			list1.Hide();
			list1Lbl.Hide();

			list2.Hide();
			list2Lbl.Hide();

			list3.Hide();
			list3Lbl.Hide();

			descEditBtn.Hide();
			descLbl.Hide();
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

		private void SpoolNewGalaxy()																								// spools an empty galaxy map
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

		private void refreshTree()																									// Refreshes the Treeview Window
		{
			string curSector = " [" + selected.x.ToString() + ", " + selected.y.ToString() + "]";
			string starsCount = " [" + selected.stars.Count + "]";
			string cometsCount = " [" + selected.comets.Count + "]";
			string asteroidsCount = " [" + selected.asteroids.Count + "]";

			Sector root = new Sector("test", 0, 0);


			treeView1.Nodes.Clear();																								// Clear any nodes that are in the treeview already
			treeView1.BeginUpdate();																								// Start the update for the treeview
			int i = 5;																												// has to be 5 because 4 nodes exist in the list initially
			treeQueue = new List<TreeItem>()																						// reset the list, or create it if it doesn't exist
			{
				new TreeItem("Sector" + curSector, 1, 0),																			// sector is the root node
				new TreeItem("Stars", 2, 1) {tag = root },																						 
				new TreeItem("Comets", 3, 1),																						// comets is the third child node
				new TreeItem("Asteroids", 4, 1)																						// etc
			};
			
			
			foreach (Comet c in selected.comets)																				
			{
				treeQueue.Add(new TreeItem("Comet", i, 3) { tag = c });																// add a comet to the queue, with a unique id (i), and a parent
				i++;																												// of 3, which will always be comets, so we can hardcode it.
			}
			
			
			foreach (Asteroid a in selected.asteroids)
			{
				treeQueue.Add(new TreeItem("Asteroid", i, 4) { tag = a });															// same idea as before, but with asteroids
				i++;
			}

			
			foreach (Star s in selected.stars)																						// this one is more complicated because stars have child branches
			{																														// its a lot less complicated after version 1.2, though..
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
					treeQueue.Add(new TreeItem("Asteroids", i, planetRoot) { tag = p });											// make an "Asteroids" subnode
					i++;
					foreach (Asteroid ast in p.asteroids)
					{
						treeQueue.Add(new TreeItem("Asteroid", i, planetAsteroidsRoot) { tag = ast });
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

		private void PopulateTreeView(int parentId, TreeNode parentNode)															// this populates the treeview window
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

		private void Form1_Load(object sender, EventArgs e)                                                                         // more boilerplate stuff
		{
			pictureBox1.BackColor = Color.Black;																					// I have to set these handlers this way because the designer wouldn't
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

		

		private void treeView1_MouseDoubleClick(object sender, MouseEventArgs e)                                                    // loads the doubleclicked object into the editor
		{
			
			if (treeView1.SelectedNode.Tag != null)
			{
				select = treeView1.SelectedNode.Tag;

				List<string> onelist = new List<string>();
				List<string> twolist = new List<string>();
				List<string> threelist = new List<string>();

				if (select is Sector s)
				{
					nameLbl.Show();
					nameBox.Show();
					nameBox.Text = s.name;
					nameEditBtn.Show();

					typeBox.Hide();
					typeLbl.Hide();
					typeEditBtn.Hide();

					massBox.Hide();
					massLbl.Hide();
					massEditBtn.Hide();

					distBox.Hide();
					distLbl.Hide();
					distEditBtn.Hide();

					list1.Show();
					list1Lbl.Show();
					list1Lbl.Text = "Stars:";

					list2.Show();
					list2Lbl.Show();
					list2Lbl.Text = "Comets:";

					list3.Show();
					list3Lbl.Show();
					list3Lbl.Text = "Asteroids:";

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
					nameLbl.Show();
					nameBox.Text = st.name;
					nameEditBtn.Show();

					massBox.Show();
					massLbl.Show();
					massBox.Text = st.mass.ToString();
					massEditBtn.Show();

					typeBox.Show();
					typeLbl.Show();
					typeBox.Text = st.type.ToString();
					typeEditBtn.Show();

					distBox.Show();
					distLbl.Show();
					distBox.Text = st.x.ToString();
					distEditBtn.Show();

					angleBox.Show();
					angLbl.Show();
					angleBox.Text = st.y.ToString();
					angEditBtn.Show();

					descBox.Show();
					descLbl.Show();
					descBox.Text = st.desc;
					descEditBtn.Show();

					list1.Show();
					list1Lbl.Show();
					list1Lbl.Text = "Planets:";

					list2.Show();
					list2Lbl.Show();
					list2Lbl.Text = "Asteroids:";

					twolist.Clear();
					onelist.Clear();
					foreach (Planet pl in st.planets)
					{
						onelist.Add(pl.name);
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
					nameLbl.Show();
					nameEditBtn.Show();

					massBox.Show();
					massBox.Text = c.mass.ToString();
					massLbl.Show();
					massEditBtn.Show();

					typeBox.Show();
					typeBox.Text = c.type.ToString();
					typeLbl.Show();
					typeEditBtn.Show();

					distBox.Show();
					distBox.Text = c.x.ToString();
					distLbl.Show();
					distEditBtn.Show();
					
					angleBox.Show();
					angleBox.Text = c.y.ToString();
					angLbl.Show();
					angEditBtn.Show();

					descBox.Show();
					descBox.Text = c.desc;
					descLbl.Show();
					descEditBtn.Show();

					list2.Hide();
					list2Lbl.Hide();

					list1.Hide();
					list1Lbl.Hide();

					list3.Hide();
					list3Lbl.Hide();
				}
				if (select is Planet p)
				{
					nameBox.Show();
					nameBox.Text = p.name;
					nameLbl.Show();
					nameEditBtn.Show();

					massBox.Show();
					massBox.Text = p.mass.ToString();
					massEditBtn.Show();
					massLbl.Show();

					typeBox.Show();
					typeLbl.Show();
					typeBox.Text = p.type.ToString();
					typeEditBtn.Show();

					distBox.Show();
					distBox.Text = p.x.ToString();
					distLbl.Show();
					distEditBtn.Show();

					angleBox.Show();
					angleBox.Text = p.y.ToString();
					angLbl.Show();
					angEditBtn.Show();

					descBox.Show();
					descBox.Text = p.desc;
					descLbl.Show();
					descEditBtn.Show();

					onelist.Clear();
					twolist.Clear();
					list1Lbl.Show();
					list1Lbl.Text = "Moons:";

					list1.Show();
					foreach (Moon mo in p.moons)
					{
						onelist.Add(mo.name);
					}
					list1.DataSource = onelist;

					list2Lbl.Show();
					list2Lbl.Text = "Asteroids:";
					list2.Show();
					foreach (Asteroid ast in p.asteroids)
					{
						twolist.Add(ast.name);
					}
					list2.DataSource = twolist;

					list3.Hide();
					list3Lbl.Hide();
				}
				if (select is Moon m)
				{
					nameBox.Show();
					nameBox.Text = m.name;
					nameLbl.Show();
					nameEditBtn.Show();

					massBox.Show();
					massBox.Text = m.mass.ToString();
					massLbl.Show();
					massEditBtn.Show();

					typeBox.Show();
					typeBox.Text = m.type.ToString();
					typeLbl.Show();
					typeEditBtn.Show();

					distBox.Show();
					distBox.Text = m.x.ToString();
					distLbl.Show();
					distEditBtn.Show();

					angleBox.Show();
					angleBox.Text = m.y.ToString();
					angLbl.Show();
					angEditBtn.Show();

					descBox.Show();
					descBox.Text = m.desc;
					descLbl.Show();
					descEditBtn.Show();

					onelist.Clear();
					list1.Show();
					list1Lbl.Show();
					list1Lbl.Text = "Asteroids";

					foreach (Asteroid ast in m.asteroids)
					{
						onelist.Add(ast.name);
					}
					list1.DataSource = onelist;

					list2.Hide();
					list2Lbl.Hide();
				
					list3.Hide();
					list3Lbl.Hide();
				}
				if (select is Asteroid a)
				{
					nameBox.Show();
					nameBox.Text = a.name;
					nameLbl.Show();
					nameEditBtn.Show();

					massBox.Show();
					massBox.Text = a.mass.ToString();
					massLbl.Show();
					massEditBtn.Show();

					typeBox.Show();
					typeBox.Text = a.type.ToString();
					typeLbl.Show();
					typeEditBtn.Show();

					descBox.Show();
					descBox.Text = a.desc;
					descLbl.Show();
					descEditBtn.Show();

					distBox.Show();
					distBox.Text = a.x.ToString();                                                                                      // distance and angle are x/y in free asteroids
					distLbl.Show();
					distEditBtn.Show();

					angleBox.Show();
					angleBox.Text = a.y.ToString();
					angLbl.Show();
					angEditBtn.Show();

					list1.Hide();
					list1Lbl.Hide();

					list2.Hide();
					list2Lbl.Hide();

					list3.Hide();
					list3Lbl.Hide();
				}
			}	
		}

		private void removeAsteroidToolStripMenuItem_Click(object sender, EventArgs e)												// Destroy Asteroid Menu
		{
			if (treeView1.SelectedNode.Tag is Asteroid a)
			{
				a.Destroy();
				
				statusPanel.Text = "Destroyed an Asteroid!";
			}
			refreshTree();
		}

		private void treeView1_MouseUp(object sender, MouseEventArgs e)                                                             // All right clicks in the treeview add/remove objects to the sector
		{
			if (e.Button == MouseButtons.Right)																						
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

		private void contextMenuStrip1_Click(object sender, EventArgs e)															// New Planet Menu
		{
			if (treeView1.SelectedNode.Tag is Star tempStar)
			{
				tempStar.addPlanet(new Planet("Planet", "", 23, 42, 55, 12000, tempStar));
				statusPanel.Text = "Created New Planet!";
			}
			refreshTree();
		}

		private void addAsteroidToolStripMenuItem_Click(object sender, EventArgs e)													// New Asteroid Menu
		{
			// this one is a little different, since there are 3 different lists that asteroids can be in.

			
			if (treeView1.SelectedNode.Tag == null)																					// null means its floating free in the sector
			{
				selected.AddAsteroid(new Asteroid((int)rnd.NextDouble() * SECTORSIZE, (int)rnd.NextDouble() * SECTORSIZE, 8, 450, "Asteroid", "", selected));
			}
			else if (treeView1.SelectedNode.Tag is Planet p)
			{
				p.asteroids.Add(new Asteroid((int)rnd.NextDouble() * ASTEROID_MAXDIST, (int)rnd.NextDouble() * 360, 8, 450, "Asteroid", "", p));
			}
			else if (treeView1.SelectedNode.Tag is Star s)
			{
				s.asteroids.Add(new Asteroid((int)rnd.NextDouble() * ASTEROID_MAXDIST, (int)rnd.NextDouble() * 360, 8, 450, "Asteroid", "", s));
			}
			else if (treeView1.SelectedNode.Tag is Moon m)
			{
				m.asteroids.Add(new Asteroid((int)rnd.NextDouble() * ASTEROID_MAXDIST, (int)rnd.NextDouble() * 360, 8, 450, "Asteroid", "", m));
			}
			refreshTree();
			
		}

		private void removePlanetToolStripMenuItem_Click(object sender, EventArgs e)												// Destroy Planet Menu
		{
			if (treeView1.SelectedNode.Tag is Planet tempPlanet)
			{
				tempPlanet.Destroy();
				statusPanel.Text = "Destroyed a Planet!";
				refreshTree();
			}
		}

		private void addStarToolStripMenuItem_Click(object sender, EventArgs e)														// New Star Menu
		{
			selected.stars.Add(new Star("Star", "", 27, 27, 32, 4000000, selected));
			refreshTree(); 
			statusPanel.Text = "Created New Star!";
		}

		private void addCometToolStripMenuItem_Click(object sender, EventArgs e)													// New Comet Menu
		{
			newComet = new Comet(0, 0, "Comet", "", 52, 120, selected);
			selected.comets.Add(newComet);
			refreshTree();
			statusPanel.Text = "Created New Comet!";
		}

		private void addMoonToolStripMenuItem_Click(object sender, EventArgs e)														// New Moon Menu
		{
			if (treeView1.SelectedNode.Tag is Planet tempPlanet)
			{
				newMoon = new Moon("Moon", "A Moon", 12, 31, 430000, 12, tempPlanet);
				tempPlanet.moons.Add(newMoon);
			}
			refreshTree();
			statusPanel.Text = "Created New Moon!";
		}

		private void removeStarToolStripMenuItem_Click(object sender, EventArgs e)													// Destroy Star Menu
		{
			if (treeView1.SelectedNode.Tag is Star tempStar)
			{
				tempStar.Destroy();
				refreshTree();
				statusPanel.Text = "Destroyed a Star!";
			}
		}

		private void removeCometToolStripMenuItem_Click(object sender, EventArgs e)													// Destroy Coment Menu
		{
			if (treeView1.SelectedNode.Tag is Comet tempComet)
			{
				tempComet.Destroy();
				refreshTree();
				statusPanel.Text = "Destroyed a Comet!";
			}
		}

		private void blankToolStripMenuItem_Click(object sender, EventArgs e)														// Create New Blank Galaxy Menu
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

		private void randomToolStripMenuItem_Click(object sender, EventArgs e)                                                      // Create New Random Galaxy Menu
		{
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


		private void SpoolNewRandomGalaxy()																							// This actually spools the random galaxy
		{
			galaxy = new Sector[GALAXY_SIZE, GALAXY_SIZE];

			for (int y = 0; y < GALAXY_SIZE; y++)
			{
				for (int x = 0; x < GALAXY_SIZE; x++)
				{
					galaxy[x, y] = new Sector("Sector", x, y);

					for (int i = 0; i < (int)(rnd.NextDouble() * 10); i++)
					{
						galaxy[x, y].stars.Add(new Star("Star", "", (int)(rnd.NextDouble() * SECTORSIZE), (int)(rnd.NextDouble() * SECTORSIZE), 32, (int)(rnd.NextDouble() * STAR_MAXMASS + STAR_MINMASS), galaxy[x, y])); 
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

		private void saveCheck()																									// makes sure the file has a name
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

		private void toolStripMenuItem3_Click(object sender, EventArgs e)															// save
		{
			// save
			saveCheck();
			
		}

		private void toolStripMenuItem2_Click(object sender, EventArgs e)															// save as
		{
			// Save as..
			GetFilename();
			Export();
		}

		private void GetFilename()																									// opens the save dialog
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

		private void Export()																										// doesn't really work well right now
		{
			WriteToBinaryFile(filename, galaxy, false);
		}

		private void removeMoonToolStripMenuItem_Click(object sender, EventArgs e)													// Destroy Moon Menu
		{
			if (treeView1.SelectedNode.Tag is Moon tempMoon)
			{
				tempMoon.Destroy();
				refreshTree();
				statusPanel.Text = "Destroyed a Moon!";
			}
		}

		private void toolStripMenuItem4_Click(object sender, EventArgs e)															// Load Map Menu
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

		private void pictureBox1_MouseMove(object sender, MouseEventArgs e)															// updates the xy coords when the mouse moves
		{
			//textBox1.Text = String.Format("{0}", selectionX);
			//textBox2.Text = String.Format("{0}", selectionY);

			xyPanel.Text = String.Format("X: {0} / Y: {1}", xcoord, ycoord);

			xcoord = e.X/GRIDSIZE;
			ycoord = e.Y/GRIDSIZE;
		}

		public static void WriteToBinaryFile<T>(string filePath, T objectToWrite, bool append = false)								// this doesn't work well but here it is for posterity
		{
			using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
			{
				var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
				binaryFormatter.Serialize(stream, objectToWrite);
			}
		}

		private void nameEditBtn_Click(object sender, EventArgs e)																	// Edit Name Menu
		{
			if (select is Asteroid a)
			{
				a.name = showTextDialogStr("Change Asteroid Name", a.name);															// its like this becuse inlining doesn't work in c# sometimes..
			}
			if (select is Sector s)
			{
				s.name = showTextDialogStr("Change Sector Name", s.name);
			}
			if (select is Comet c)
			{
				c.name = showTextDialogStr("Change Comet Name", c.name);
			}
			if (select is Planet p)
			{
				p.name = showTextDialogStr("Change Planet Name", p.name);
			}
			if (select is Moon m)
			{
				m.name = showTextDialogStr("Change Moon Name", m.name);
			}
			if (select is Star st)
			{
				st.name = showTextDialogStr("Change Star Name", st.name);
			}
		}

		private void typeEditBtn_Click(object sender, EventArgs e)																	// Edit Type Menu
		{
			if (select is Asteroid a)
			{
				a.type = showTextDialogInt("Change Asteroid Type", a.type);
			}

			if (select is Planet p)
			{
				p.type = showTextDialogInt("Change Planet Type", p.type);
			}

			if (select is Comet c)
			{
				c.type = showTextDialogInt("Change Comet Type", c.type);
			}

			if (select is Moon m)
			{
				m.type = showTextDialogInt("Change Moon Type", m.type);
			}

			if (select is Star st)
			{
				st.type = showTextDialogInt("Change Star Type", st.type);
			}
		}

		private void massEditBtn_Click(object sender, EventArgs e)																	// Edit Mass Menu
		{
			if (select is Asteroid a)
			{
				a.mass = showTextDialogInt("Change Asteroid Mass", a.mass);
			}
			if (select is Comet c)
			{
				c.mass = showTextDialogInt("Change Comet Mass", c.mass);
			}
			if (select is Planet p)
			{
				p.mass = showTextDialogInt("Change Planet Mass", p.mass);
			}
			if (select is Moon m)
			{
				m.mass = showTextDialogInt("Change Moon Mass", m.mass);
			}
			if (select is Star st)
			{
				st.mass = showTextDialogInt("Change Star Mass", st.mass);
			}
		}

		private void distEditBtn_Click(object sender, EventArgs e)																	// Edit Distance (or x) Menu
		{
			// x
			if (select is Asteroid a)
			{
				a.x = showTextDialogInt("Change Asteroid Distance (or x)", a.x);
			}

			if (select is Comet c)
			{
				c.x = showTextDialogInt("Change Comet Distance (or x)", c.x);
			}

			if (select is Planet p)
			{
				p.x = showTextDialogInt("Change Planet Distance (or x)", p.x);
			}

			if (select is Moon m)
			{
				m.x = showTextDialogInt("Change Moon Distance (or x)", m.x);
			}
			if (select is Star st)
			{
				st.x = showTextDialogInt("Change Star x-value", st.x);
			}
		}

		private void angEditBtn_Click(object sender, EventArgs e)																	// Edit Angle (or y) Menu
		{
			// y
			if (select is Asteroid a)
			{
				a.y = showTextDialogInt("Change Asteroid Angle (or y)", a.y);
			}

			if (select is Comet c)
			{
				c.y = showTextDialogInt("Change Comet Angle (or y)", c.y);
			}

			if (select is Planet p)
			{
				p.y = showTextDialogInt("Change Planet Angle (or y)", p.y);
			}

			if (select is Moon m)
			{
				m.y = showTextDialogInt("Change Moon Angle (or y)", m.y);
			}

			if (select is Star st)
			{
				st.y = showTextDialogInt("Change Star y-value", st.y);
			}
		}

		private void descEditBtn_Click(object sender, EventArgs e)																	// Edit Description Menu
		{
			if (select is Asteroid a)
			{
				a.desc = showTextDialogStr("Change Asteroid Description", a.desc);
			}

			if (select is Comet c)
			{
				c.desc = showTextDialogStr("Change Comet Description", c.desc);
			}

			if (select is Planet p)
			{
				p.desc = showTextDialogStr("Change Planet Description", p.desc);
			}

			if (select is Moon m)
			{
				m.desc = showTextDialogStr("Change Moon Description", m.desc);
			}

			if (select is Star st)
			{
				st.desc = showTextDialogStr("Change Star Description", st.desc);
			}
		}

		private string showTextDialogStr(string label, string recall)																// Helper method to parse text
		{
			TextDialog d = new TextDialog();
			d.labelText.Text = label;

			if (d.ShowDialog(this) == DialogResult.OK)
			{
				// its just text so we don't have to parse it

				return d.textBox1.Text;
			}
			else
			{
				return recall;
				// cancelled, so whatever the value was before input
			}
			
		}
		private int showTextDialogInt(string label, int recall)																		// Helper method to parse integers
		{
			TextDialog d = new TextDialog();
			d.labelText.Text = label;

			if (d.ShowDialog(this) == DialogResult.OK)
			{
				// parse input, redo if necessary
				int result;


				if (!Int32.TryParse(d.textBox1.Text, out result))
				{
					return recall;
				}
				return result;
			}
			else
			{
				return recall;
				// cancelled, so whatever the value was before input
			}
		}

		public static T ReadFromBinaryFile<T>(string filePath)																		// this has a memory leak somewhere so don't use it
		{
			using (Stream stream = File.Open(filePath, FileMode.Open))
			{
				var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
				return (T)binaryFormatter.Deserialize(stream);
			}
		}

		public static T ReadFromBinaryFile<T>(Stream strm)																			// same deal as above
		{
			var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
			return (T)binaryFormatter.Deserialize(strm);
		}
	}
}

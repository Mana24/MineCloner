using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Collections;
using System.Xml.Serialization;

// I might have gone overboard with how small my methods got.

// FUCK IT! I don't wanna work on this no more. Text only configs! No settings form or whatever.

namespace MineCloner
{
	public partial class MainWindow : Form
	{
		// Configured These two fields so that they control the number of columns and rows.
		// All columns and rows are of equal size
		private readonly string configFilePath = "MineClonerConfig.xml";
		private bool playerWon = false;
		private bool isFirstClick = true;
		private int tableColumnCount = 15;
		private int tableRowCount = 15;
		private int mineCount = 1;
		public static int flagCount; // Making this static is a bad hack to make this availabe to other classes (mainly MineButton)
		private TableLayoutPanel gameTable;
		private MineMap gameMap;
		private Button restartButton;
		private List<Label> spawnedLabels; // Used for rescaling the labels when the window is resized
		private List<MineButton> spawnedMineButtons;
		private List<MineButton> explosiveButtons;
		public static Label flagCountLabel; // Making this static is a bad hack to make this availabe to other classes (mainly MineButton)

		private Dictionary<int, Color> NumberColors =
			new Dictionary<int, Color>() // Hope to make this user-editable one day
			{
				{ 0, Color.Empty },
				{ 1, Color.Blue },
				{ 2, Color.Green },
				{ 3, Color.Red },
				{ 4, Color.Purple },
				{ 5, Color.Maroon },
				{ 6, Color.Turquoise },
				{ 7, Color.Black },
				{ 8, Color.DarkGray }
			};

		public MainWindow()
		{
			if (!File.Exists(configFilePath))
            {
                Config config = new Config(tableColumnCount, tableRowCount, mineCount);
                XmlSerializer xs = new XmlSerializer(typeof(Config));
                using (Stream s = File.Create(configFilePath))
                    xs.Serialize(s, config);
            }
			Initalize();
			ResizeFonts(this, new EventArgs());
			this.ResizeEnd += ResizeFonts;
		}

		private void Initalize()
		{
            // Initalize game variables
            Config config;
            XmlSerializer xs = new XmlSerializer(typeof(Config));
            using (Stream s = File.OpenRead(configFilePath))
                config = (Config)xs.Deserialize(s);

            tableColumnCount = config.TableColumnCount;
			tableRowCount = config.TableRowCount;
			mineCount = config.MineCount;
			playerWon = false;
			isFirstClick = true;

			//System.Diagnostics.Process.Start("Config.txt");
			InitializeComponent();
			gameTable = this.tableLayoutPanel1;
			InitalizeTable(gameTable, tableColumnCount, tableRowCount);

			gameMap = new MineMap(tableColumnCount, tableRowCount, mineCount);
			spawnedLabels = new List<Label>();
			spawnedMineButtons = new List<MineButton>();
			explosiveButtons = new List<MineButton>();

			MineButton.ClearFlagCount();

			// Make the first row a little thicker than the last row
			gameTable.RowStyles[0].Height = 100f / (float)tableRowCount * 2;
			gameTable.RowStyles[tableRowCount - 1].Height = 100f / (float)tableRowCount / 2;

			AddMineButtons(gameTable);
			int restartButtonColumnSpan = tableColumnCount % 2 == 0 ? 2 : 3; //If even columns then two else three
			restartButton = AddRestartButton(restartButtonColumnSpan, 0);
			flagCountLabel = AddFlagCountLabel(); flagCountLabel.ScaleFont();
		}

		private void InitalizeTable(TableLayoutPanel table, int columns, int rows)
		{
			table.ColumnCount = columns;
			float columnSize = 100f / (float)columns;
			table.ColumnStyles[0].Width = columnSize;
			for (int i = 1; i < columns; i++)
			{
				table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, columnSize));
			}

			table.RowCount = rows;
			float rowSize = 100f / (float)rows;
			table.RowStyles[0].Height = rowSize;
			for (int i = 1; i < rows; i++)
			{
				gameTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, rowSize));
			}
		}

		private void AddMineButtons(TableLayoutPanel table)
		{
			for (int column = 1; column < table.ColumnCount - 1; column++)
			{
				for (int row = 1; row < table.RowCount - 1; row++)
				{
					MineButton mineButton = CreateMineButton(column, row);
					table.Controls.Add(mineButton, mineButton.x, mineButton.y);

					spawnedMineButtons.Add(mineButton);
					//Making sure to remove these buttons from their lists when they're disposed
					mineButton.Disposed += GetDelegateToRemoveFromSequence(spawnedMineButtons);

					if (gameMap[mineButton.x, mineButton.y]) 
					{
						// Button has mine under it
						explosiveButtons.Add(mineButton);
						mineButton.Disposed += GetDelegateToRemoveFromSequence(explosiveButtons);
					}
				}
			}
		}

		private MineButton CreateMineButton(int column, int row)
		{
			MineButton button = new MineButton(column, row);

			button.Dock = System.Windows.Forms.DockStyle.Fill;
			//button.Location = new System.Drawing.Point(267, 149);
			button.Name = "button";
			//button.Size = new System.Drawing.Size(258, 140);
			button.TabIndex = 0;
			button.Text = gameMap[column, row] ? "" : "";
			button.Margin = new Padding(0, 0, 0, 0);
			button.UseVisualStyleBackColor = true;

			button.Click += DebugButtons;
			button.MouseUp += MineButton_Click;

			return button;
		}

		private Button AddRestartButton(int columnSpan, int row)
		{
			Button button = new Button();

			button.Dock = System.Windows.Forms.DockStyle.Fill;
			button.Name = "button";
			button.TabIndex = 0;
			button.Text = "😃";
			button.Margin = new Padding(0, 0, 0, 0);
			button.Padding = new Padding(0);

			gameTable.Controls.Add(button, tableColumnCount / 2 - columnSpan / 2, row);
			gameTable.SetColumnSpan(button, columnSpan);

			button.MouseUp += RestartButton_Click;
			// This is here becuase of a bug where the flags count label doesn't rescale on restart
			// This seemed to have fixed it, so Imma leave it here
			button.MouseUp += (sender, e) => { foreach (Label label in spawnedLabels) label.ScaleFont(); };

			new ToolTip().SetToolTip(button, "Restart Button");

			return button;
		}

		private Label AddFlagCountLabel()
		{
			Label label = new Label();

			label.AutoSize = true;
			label.Dock = System.Windows.Forms.DockStyle.Fill;
			label.Font = new System.Drawing.Font("Microsoft Sans Serif", 5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			label.Location = new System.Drawing.Point(0, 0);
			label.Margin = new System.Windows.Forms.Padding(0);
			label.Name = "label";
			label.Text = (mineCount - flagCount).ToString($"D{mineCount.ToString().Length}");
			label.Size = new System.Drawing.Size(528, 440);
			label.TabIndex = 0;
			label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			label.ForeColor = Color.Red;

			gameTable.Controls.Add(label, 
				Math.Max(0, gameTable.GetPositionFromControl(restartButton).Column - 2),
				gameTable.GetPositionFromControl(restartButton).Row);
			// Not gonna add this to the spawnedLabels list because I don't want it to be resized
			// Can you tell that I am starting to really hate this project
			// It's so fucking ugly on the inside. And all of that is because of me.
			//spawnedLabels.Add(label);
			
			return label;
		}

		private void UpdateFlagLabelDisplay()
		{
			// Update display
			flagCountLabel.Text =
				Math.Max(0, (mineCount - MineButton.FlagCount)) // dont wanna display negatives
				.ToString($"D{mineCount.ToString().Length}");
		}

		/// <summary>
		/// Creates Label + Adds it to the gameTable. I hope you don't forget to add color to it and scale it.
		/// </summary>
		private Label SpawnLabel(int column, int row, object text)
		{
			Label label = new Label();

			label.AutoSize = true;
			label.Dock = System.Windows.Forms.DockStyle.Fill;
			label.Font = new System.Drawing.Font("Microsoft Sans Serif", 2.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			label.Location = new System.Drawing.Point(0, 0);
			label.Margin = new System.Windows.Forms.Padding(0);
			label.Name = "label";
			label.Text = text?.ToString() ?? "";
			label.Size = new System.Drawing.Size(528, 440);
			label.TabIndex = 0;
			label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

			gameTable.Controls.Add(label, column, row);

			spawnedLabels.Add(label); // For later rescaling on resizing the window
			return label;
		}

		private List<TControl> GetControlsAroundPoint<TControl>(int PosX, int PosY) where TControl : Control
		{
			List<TControl> controls = new List<TControl>(); // Creating a list of buttons to click 
			foreach ((int x, int y) direction in Utility.EightDirectionsInNumbers.Values)
			{
				Control control = gameTable.GetControlFromPosition(PosX + direction.x,
					PosY + direction.y);

				if (control is TControl)
				{
					controls.Add((TControl)control);
				}
			}

			return controls;
		}

		private void MakeAllButtonsUnclickable()
		{
			foreach (MineButton button in spawnedMineButtons)
			{
				button.MouseUp -= MineButton_Click;
			}
		}

		private EventHandler GetDelegateToRemoveFromSequence<T>(List<T> sequence) where T : class
		{
			return (sender, e) => { T obj = sender as T; sequence.Remove(obj); };
		} 

		private void MineButton_Click(object sender, EventArgs e)
		{
			MineButton mineButton = (MineButton)sender;
			MouseEventArgs args = e as MouseEventArgs;

			if (mineButton.IsDisposed)
			{
				// Not sure if this will come into effect
				// It's just incase of weirdness with this method and its button's events
				// causing this being called twice on the same button
				return; 
			}
			
			int PosX = mineButton.x, PosY = mineButton.y;

			switch (args?.Button)
			{
				case null: // The case of the automatic click :/
				case MouseButtons.Left:
					if (mineButton.IsFlagged)
					{
						// Can't click flagged buttons
						return;
					}

					// Making it so your first click can't be a mine
					if (isFirstClick)
                    {
						isFirstClick = false;
						while(gameMap[PosX, PosY])
                        {
							gameMap.ReGenerateMines();
                        }
                    }

					int minesAroundCount = gameMap.MinesAroundPosition(PosX, PosY);

					if (gameMap[PosX, PosY] == true)
					{
						// Lose; Clicked on mine
						foreach (MineButton button in explosiveButtons)
						{
							button.BackColor = Color.Black;
							button.ForeColor = Color.White;
							button.Text = "💣";
						}
						mineButton.BackColor = Color.Red;
						mineButton.ForeColor = Color.Black;

						MakeAllButtonsUnclickable();
						restartButton.Text = "💀";
						restartButton.ForeColor = Color.Red;
					}
					else if (minesAroundCount == 0)
					{
						mineButton.Dispose();
						spawnedMineButtons.Remove(mineButton);
						// No mines around button, Going to click them automatically
						List<MineButton> mineButtons = GetControlsAroundPoint<MineButton>(PosX, PosY); // Creating a list of buttons to click 

						// Click Each of the mineButtons around me
						foreach (MineButton button in mineButtons)
						{
							// Unflagging these buttons because It's 100% true that these have no mines under them
							button.IsFlagged = false;

							// Doing the weird thing with adding and removing from the event
							// because PerformClick only works with the Click event
							// And I am using this because this method is usually in
							// the MouseUp event, and that is because the Click event
							// won't fucking trigger on right clicks! FFFUUUUUUUU
							button.Click += MineButton_Click;
							button.PerformClick(); // Clicks with EventArgs not MouseEventArgs
							button.Click -= MineButton_Click;
						}
					}
					else if (minesAroundCount != 0)
					{
						// It has some mines around it, so tell it to just put a label with the number of mines
						mineButton.Dispose();

						Label label = SpawnLabel(PosX, PosY, minesAroundCount);
						label.ScaleFont();
						label.ForeColor = NumberColors[minesAroundCount];
						label.MouseUp += Label_Click;
					}

					if (spawnedMineButtons.SequenceEqual(explosiveButtons))
					{
						// Player Won! Do winning stuff
						MakeAllButtonsUnclickable();

						restartButton.BackColor = Color.LightYellow;
						restartButton.ForeColor = Color.DarkOrange;
						restartButton.Text = "😎";

						if (!playerWon)
						{
							MessageBox.Show("A Winnar Is You!", "WIN!", MessageBoxButtons.OK);

							System.Diagnostics.Debug.WriteLine("Player Won!");
							playerWon = true;
						}
					}

					break;
				case MouseButtons.Right:
					// Do Flag Stuff
					mineButton.IsFlagged = !mineButton.IsFlagged;

					UpdateFlagLabelDisplay();

					break;
			}
		}

		private void RestartButton_Click(object sender, EventArgs e)
		{
			Size oldSize = this.Size; 
			this.Controls.Clear();
			Initalize();
			this.Size = oldSize;
			ResizeFonts(this, new EventArgs());
		}

		private void ResizeFonts(object sender, EventArgs e)
		{
			// When the window is rescaled, rescale the labels too
			if (spawnedLabels != null) foreach(Label label in spawnedLabels)
			{
				label?.ScaleFont();
			}
			if (spawnedMineButtons != null) foreach(Button button in spawnedMineButtons)
			{
				button?.ScaleFont();
			}
			restartButton?.ScaleFont();
		}

		private void Label_Click(object sender, MouseEventArgs e)
		{
			// This event is added in the button click events, not in the creation of labels
			// Spaghetti
			// This is because I might use the SpawnLabel method to do scoring and shit.

			if (e.Button != MouseButtons.Middle) { return; } // Can only middle click labels

			Label label = (Label)sender;
			int minesAroundCount = int.Parse(label.Text);

			TableLayoutPanelCellPosition position = gameTable.GetPositionFromControl(label);

			// Middle click stuff
			// Get Buttons around label
			List<MineButton> mineButtonsAround = GetControlsAroundPoint<MineButton>(position.Column, position.Row);
			
			// If there's the "correct" numbers of buttons around the label, flag them and return
			if (mineButtonsAround.Count == minesAroundCount)
			{
				foreach (MineButton mineButton in mineButtonsAround) { mineButton.IsFlagged = true; }

				UpdateFlagLabelDisplay();

				return;
			}

			List<MineButton> unflaggedButtonsAround = mineButtonsAround.Where((n) => (!n.IsFlagged)).ToList();
			List<MineButton> flaggedButtonsAround = mineButtonsAround.Where((n) => (n.IsFlagged)).ToList();

			// Return if the flag number is not the label number
			if (flaggedButtonsAround.Count != minesAroundCount) return;

			// This is the normal minesweeper middle click functionality.
			// if flags == labelnumber then click buttons around label that aren't flagged
			foreach (MineButton mineButton in unflaggedButtonsAround)
			{
				// To know why I am doing this weird thing with adding and removing the clicking method
				// Read my rant in that very method about why. You'll find it where it autoclicks other buttons
				mineButton.Click += MineButton_Click;
				mineButton.PerformClick();
				mineButton.Click -= MineButton_Click;

				// Don't want it to continue clicking buttons if the player lost
				if (explosiveButtons.Contains(mineButton))
				{
					break;
				}
			}

		}

		public void DebugButtons(object sender, EventArgs eventArgs)
		{
			MineButton b = (MineButton)sender;
			System.Diagnostics.Debug.WriteLine(b.x + " " + b.y);
		}
	}
}

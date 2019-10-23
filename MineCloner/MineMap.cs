using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MineCloner
{
	class MinesMoreThanSlotsException : Exception
	{
		public MinesMoreThanSlotsException(string message, Exception innerException)
			: base(message, innerException) { }

		public MinesMoreThanSlotsException(string message) : base(message) { }

		public MinesMoreThanSlotsException() : base() { }
	}

	class MineMap
	{
		protected BitArray[] mapArray { get; set; }
		protected int tableColumns { get; }
		protected int tableRows { get; }

		public MineMap(int tableColumns, int tableRows, int mineCount) : this(tableColumns, tableRows)
		{
			// Had to put this here otherwise it might get stuck in an infinite loop
			if (mineCount > ((tableColumns - 2) * (tableRows - 2)))
			{
				throw new MinesMoreThanSlotsException();
			}

			GenerateMines(mineCount);
		}

		protected MineMap(int tableColumns, int tableRows)
		{
			mapArray = new BitArray[tableColumns];

			for (int i = 0; i < mapArray.Length; i++)
			{
				mapArray[i] = new BitArray(tableRows, false);
			}

			this.tableColumns = tableColumns;
			this.tableRows = tableRows;
		}

		public bool this[int x, int y]
		{
			get
			{
				return mapArray[x][y];
			}

			protected set
			{
				mapArray[x][y] = value;
			}
		}

		protected virtual void GenerateMines(int mineCount)
		{
			Random random = new Random();

			int i = 0;
			while (i < mineCount)
			{
				int mineColumn = random.Next(1, this.tableColumns - 1);
				int mineRow = random.Next(1, this.tableRows - 1);

				if (this[mineColumn, mineRow] == true)
				{
					continue;
				}
				else
				{
					this[mineColumn, mineRow] = true;
				}

				i++;
			}
		}

		public void Debug()
		{
			for (int x = 0; x < tableColumns; x++)
			{
				for (int y = 0; y < tableRows; y++)
				{
					switch (this[x, y])
					{
						case true:
							System.Diagnostics.Debug.Write(1);
							break;
						case false:
							System.Diagnostics.Debug.Write(0);
							break;
					}
				}

				System.Diagnostics.Debug.Write('\n');
			}
		}

		/// <param name="x"> Shouldn't be less than 1 </param>
		/// <param name="y"> Shouldn't be less than 1 </param>
		public int MinesAroundPosition(int x, int y)
		{
			int mines = 0;

			foreach ((int deltaX, int deltaY) pos in Utility.EightDirectionsInNumbers.Values)
			{
				if (this[x + pos.deltaX, y + pos.deltaY] == true) mines++;
			}

			return mines;
		}
	}
}

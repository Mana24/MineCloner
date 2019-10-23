using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MineCloner
{
	class MineButton : Button
	{
		public readonly int x;
		public readonly int y;

		public static int FlagCount { get; private set; }

		private bool _isFlagged = false;

		public bool IsFlagged
		{
			get
			{
				return _isFlagged;
			}
			set
			{
				if (value == _isFlagged) return;

				if (_isFlagged == true)
				{
					_isFlagged = false;
					FlagCount--;
				}
				else
				{
					_isFlagged = true;
					FlagCount++;

					this.ScaleFont();
				}

				this.OnMouseEnter(new EventArgs()); // This causes the text to update for some goddamn reason.
				this.OnMouseLeave(new EventArgs()); // This just fixes some weirdness with autoflagging and adds some weirdness when manually flagging
			}
		}
		protected string FlagText { get; }

		public override string Text
		{
			get
			{
				if (IsFlagged)
					return FlagText;
				else
					return base.Text;
			}
			set
			{
				base.Text = value;
				this.ScaleFont();
			}
		}

		public static readonly string DefaultFlagText = "🚩";

		public MineButton(int x, int y, string flagText) : base()
		{
			this.x = x;
			this.y = y;
			this.FlagText = flagText;
		}

		public MineButton(int x, int y) : this(x, y, DefaultFlagText) { }

		public static void ClearFlagCount()
		{
			FlagCount = 0;
		}
	}
}

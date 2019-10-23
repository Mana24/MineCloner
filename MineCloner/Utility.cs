using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace MineCloner
{
	static class Utility
	{
		public static readonly Dictionary<EightDirections, (int x, int y)> EightDirectionsInNumbers =
			new Dictionary<EightDirections, (int x, int y)>()
			{
				{ EightDirections.North, (0, 1) }, // North
				{ EightDirections.East, (1, 0) }, // East
				{ EightDirections.West, (-1, 0) }, // West
				{ EightDirections.South, (0, -1) }, // South
				{ EightDirections.Northeast, (1, 1) }, // Northeast
				{ EightDirections.Northwest, (-1, 1) }, // Northwest
				{ EightDirections.Southeast, (1, -1) }, // Southeast
				{ EightDirections.Southwest, (-1, -1) }, // Southwest
			};

		public static void ScaleFont(this Control control)
		{
			if (control.Text == string.Empty || control.Text == "\0") return;

			SizeF extent = TextRenderer.MeasureText(control.Text, control.Font);

			float hRatio = control.Height / extent.Height;
			float wRatio = control.Width / extent.Width;
			float ratio = (hRatio < wRatio) ? hRatio : wRatio; // Choose the smallest ratio

			float newSize = control.Font.Size * ratio;

			control.Font = new Font(control.Font.FontFamily, newSize, control.Font.Style);
		}
	}
}

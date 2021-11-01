using Microsoft.Graphics.Canvas;
using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace VectorizerApp.Operators
{
	class RegionsViewerOperator : IViewerOperator
	{
		Viewer viewer;
		internal ushort[] board;
		CanvasBitmap bitmap;

		int width, height;

		public RegionsViewerOperator(Viewer viewer, ushort[] board, int width, int height)
		{
			this.viewer = viewer;
			this.board = board;

			this.width = width;
			this.height = height;
		}

		public void Initialize()
		{
			List<Color> colorvalues = new List<Color>()
			{
				Colors.AliceBlue, Colors.AntiqueWhite, Colors.Aqua, Colors.Aquamarine, Colors.Azure, Colors.Beige, Colors.Bisque, Colors.Black, Colors.BlanchedAlmond, Colors.Blue,
				Colors.BlueViolet, Colors.Brown, Colors.BurlyWood, Colors.CadetBlue, Colors.Chartreuse, Colors.Chocolate, Colors.Coral, Colors.CornflowerBlue, Colors.Cornsilk,
				Colors.Crimson, Colors.Cyan, Colors.DarkBlue, Colors.DarkCyan, Colors.DarkGoldenrod, Colors.DarkGray, Colors.DarkGreen, Colors.DarkKhaki, Colors.DarkMagenta,
				Colors.DarkOliveGreen, Colors.DarkOrange, Colors.DarkOrchid, Colors.DarkRed, Colors.DarkSalmon, Colors.DarkSeaGreen, Colors.DarkSlateBlue, Colors.DarkSlateGray,
				Colors.DarkTurquoise, Colors.DarkViolet, Colors.DeepPink, Colors.DeepSkyBlue, Colors.DimGray, Colors.DodgerBlue, Colors.Firebrick, Colors.FloralWhite, Colors.ForestGreen
			};

			Color[] colors = new Color[board.Length];
			for (int i = 0; i < board.Length; i++)
			{
				colors[i] = colorvalues[board[i] % colorvalues.Count];
			}

			bitmap = CanvasBitmap.CreateFromColors(viewer.Device, colors, width, height);

			viewer.SetSize(bitmap.Size);
		}

		public void Draw(DrawingArgs args)
		{
			args.Session.DrawImage(bitmap);
		}
	}
}

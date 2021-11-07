﻿using Microsoft.Graphics.Canvas;
using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorizerLib;
using Windows.UI;

namespace VectorizerApp.Operators
{
	class RegionsViewerOperator : IViewerOperator
	{
		Viewer viewer;
		public Context Context;
		internal ushort[] board;
		CanvasBitmap bitmap;

		int width, height;

		public RegionsViewerOperator(Viewer viewer, Context context)
		{
			this.viewer = viewer;
			Context = context;

			var bytes = context.OriginalBitmap.GetPixelBytes();

			RgbaByteSource source = new RgbaByteSource(bytes, (int)context.OriginalBitmap.SizeInPixels.Width);

			Vectorizer<RgbaByteRegionData> vectorizer = new Vectorizer<RgbaByteRegionData>();
			vectorizer.Source = source;
			vectorizer.Properties = context.Properties;
			vectorizer.Regionize();
			Context.Vectorizer = vectorizer;
			this.board = vectorizer.RegionizationResult.Board;

			this.width = source.Width;
			this.height = source.Height;
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

			Context.RegionsImage = bitmap;

			viewer.SetSize(bitmap.Size);
		}

		public void Draw(DrawingArgs args)
		{
			args.Session.DrawImage(bitmap, 0f, 0f, new Windows.Foundation.Rect(0, 0, bitmap.SizeInPixels.Width, bitmap.SizeInPixels.Height), 1f, CanvasImageInterpolation.NearestNeighbor);
		}
	}
}

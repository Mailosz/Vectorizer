﻿using Microsoft.Graphics.Canvas;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
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
		MainWindow mainWindow;
		private TextBlock ridTB;
		private TextBlock coordTB;
		private TextBlock rsplitvalueTB;
		private TextBlock maxdiffTB;

		bool matchingColors = true;
		private TextBlock rCountTB;
		private TextBlock areaTB;
		private TextBlock passesTB;

		public bool MatchingColors
		{
			get => matchingColors; 
			set
			{
				matchingColors = value;
				createBitmap();
				viewer.Invalidate();
			}
		}

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
			Context.RegionizationResult = vectorizer.RegionizationResult;
			Context.StepByStep = true;
			this.board = vectorizer.RegionizationResult.Board;

			this.width = source.Width;
			this.height = source.Height;
		}


		public void Initialize()
		{
			viewer.SetSize(Context.OriginalBitmap.Size);
			createBitmap();

			maxdiffTB.Text = "Maximum splitvalue: " + Context.RegionizationResult.PeakCov;
			rCountTB.Text = "Liczba regionów: " + Context.RegionizationResult.RegionCount;
			passesTB.Text = "Liczba kroków: " + Context.RegionizationResult.Steps;
		}

		private void createBitmap()
		{
			if (matchingColors)
			{
				Color[] colors = new Color[board.Length];
				for (int i = 0; i < board.Length; i++)
				{
					var c = Context.RegionizationResult.Regions[board[i]].Color;
					colors[i] = Color.FromArgb(c.A, c.R, c.G, c.B);
				}

				bitmap = CanvasBitmap.CreateFromColors(viewer.Device, colors, width, height);

				Context.RegionsImage = bitmap;
			}
			else
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
			}
		}

		public void Draw(DrawingArgs args)
		{
			args.Session.DrawImage(bitmap, 0f, 0f, new Windows.Foundation.Rect(0, 0, bitmap.SizeInPixels.Width, bitmap.SizeInPixels.Height), 1f, CanvasImageInterpolation.NearestNeighbor);
		}

		public void SetWindow(MainWindow mainWindow)
		{
			this.mainWindow = mainWindow;

			mainWindow.regionizeButton.IsEnabled = false;
			mainWindow.traceButton.IsEnabled = true;

			mainWindow.vectorizeButton.IsEnabled = false;

			StackPanel sp = new StackPanel();
			mainWindow.SetRightPanel(sp);

			ToggleSwitch ts1 = new ToggleSwitch();
			ts1.Header = "Kolory obrazka";
			ts1.Toggled += (s, e) => { MatchingColors = ts1.IsOn; };
			ts1.IsOn = matchingColors;
			sp.Children.Add(ts1);

			coordTB = new TextBlock();
			sp.Children.Add(coordTB);

			ridTB = new TextBlock();
			sp.Children.Add(ridTB);

			areaTB = new TextBlock();
			sp.Children.Add(areaTB);

			rsplitvalueTB = new TextBlock();
			sp.Children.Add(rsplitvalueTB);

			rCountTB = new TextBlock();
			sp.Children.Add(rCountTB);

			maxdiffTB = new TextBlock();
			sp.Children.Add(maxdiffTB);

			passesTB = new TextBlock();
			sp.Children.Add(passesTB);
		}
		public bool PointerPressed(PointerArgs args)
		{
			return false;
		}

		public bool PointerHover(PointerArgs args)
		{
			var point = args.Pointer.GetCurrentPoint(args.Sender).Position;

			int x = (int)point.X;
			int y = (int)point.Y;

			if (x >= 0 && y >= 0 && x < bitmap.SizeInPixels.Width && y < bitmap.SizeInPixels.Height)
			{
				ushort id = board[y * width + x];


				coordTB.Text = $"({x}, {y})";
				ridTB.Text = "Region: " + id.ToString();
				

				if (Context.RegionizationResult.Regions.TryGetValue(id, out IRegionData region))
				{
					rsplitvalueTB.Text = "SplitValue: " + region.SplitValue.ToString();
					areaTB.Text = "Powierzchnia: " + region.Area;
				}
				else
				{
					rsplitvalueTB.Text = "!!! No such region !!!";
					areaTB.Text = "";
				}
			}
			else
			{
				coordTB.Text = "()";
				ridTB.Text = "Region: brak";
				rsplitvalueTB.Text = "";
			}

			return true;
		}

		public bool PointerMoved(PointerArgs args)
		{
			return false;
		}

		public void PointerDoubleClick(PointerArgs args)
		{
			
		}
	}
}

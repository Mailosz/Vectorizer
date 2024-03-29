﻿using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;

namespace VectorizerApp.Operators
{
	internal class ComparingOperator : IViewerOperator
	{
		Viewer viewer;
		public CanvasBitmap bitmap1;
		public CanvasBitmap bitmap2;

		bool showHeatmap = true;
		bool showSideBySide;

		int chosenSize = 0;
		byte[] heatmap;
		CanvasBitmap heatmapBitmap;

		float meandiff;
		private MainWindow mainWindow;

		TextBlock diffTB;

		public ComparingOperator(Viewer viewer, CanvasBitmap bitmap1, CanvasBitmap bitmap2)
		{
			this.viewer = viewer;
			this.bitmap1 = bitmap1;
			this.bitmap2 = bitmap2;
		}

		public void Initialize()
		{
			viewer.SetSize(new Size(bitmap1.SizeInPixels.Width, bitmap1.SizeInPixels.Height));


			heatmap = compareImages(bitmap1, bitmap2, chosenSize);

			var colormap = (from p in heatmap select Color.FromArgb(255, 255, (byte)(255 - Math.Min(p * 4,255)), (byte)(255 - Math.Min(p * 4,255)))).ToArray();

			heatmapBitmap = CanvasBitmap.CreateFromColors(viewer.Device, colormap, (int)bitmap1.SizeInPixels.Width, (int)bitmap1.SizeInPixels.Height);
		}


		private byte[] compareImages(CanvasBitmap bmp1, CanvasBitmap bmp2, int size)
		{
			Color[] colors1 = bmp1.GetPixelColors();

			var colors2 = bmp2.GetPixelColors();

			var diffMap = new byte[colors1.Length];

			int width = (int)bmp1.SizeInPixels.Width;

			int start = 0;
			int sum = 0;
			do
			{
				for (int i = 0; i < width; i++)
				{
					int pos = start + i;
					var color1 = colors1[pos];
					byte mindiff = 255;

					for (int y = -size; y <= size; y++)
					{
						for (int x = -size; x <= size; x++)
						{
							int pos2 = pos + x + y * width;
							if (pos2 > 0 && pos2 < colors1.Length)
							{
								var color2 = colors2[pos2];
								var diff = (Math.Abs(color1.R - color2.R) + Math.Abs(color1.G - color2.G) + Math.Abs(color1.B - color2.B) + Math.Abs(color1.A - color2.A)) / 4;

								if (diff < mindiff)
								{
									mindiff = (byte)diff;
								}
							}
						}
					}

					diffMap[pos] = mindiff;
					sum += mindiff;
				}
				start += width;
			}
			while (start < colors1.Length);

			meandiff = (float)sum / colors1.Length;
			diffTB.Text = "Śr. Różnica: " + meandiff.ToString();

			return diffMap;
		}

		public void Draw(DrawingArgs args)
		{
			if (showHeatmap)
			{
				args.Session.DrawImage(heatmapBitmap, 0f, 0f, new Windows.Foundation.Rect(0, 0, bitmap1.SizeInPixels.Width, bitmap1.SizeInPixels.Height), 1f, CanvasImageInterpolation.NearestNeighbor);
			}
		}

		public void SetWindow(MainWindow mainWindow)
		{
			this.mainWindow = mainWindow;

			mainWindow.saveButton.IsEnabled = false;
			mainWindow.comparisonButton.IsEnabled = false;

			StackPanel sp = new StackPanel();
			mainWindow.SetRightPanel(sp);

			ComboBox cb = new ComboBox();
			cb.Items.Add(new ComboBoxItem() { Content = "1" });
			cb.Items.Add(new ComboBoxItem() { Content = "2" });
			cb.Items.Add(new ComboBoxItem() { Content = "3" });
			cb.Items.Add(new ComboBoxItem() { Content = "4" });
			cb.SelectedIndex = 0;
			cb.SelectionChanged += (s, e) =>
			{
				chosenSize = cb.SelectedIndex;
				Initialize();
				viewer.Invalidate();
			};
			sp.Children.Add(cb);

			diffTB = new TextBlock();
			sp.Children.Add(diffTB);
		}

		public bool PointerPressed(PointerArgs args)
		{
			return false;
		}

		public bool PointerMoved(PointerArgs args)
		{
			return false;
		}
		public bool PointerHover(PointerArgs args)
		{
			return false;
		}

		public void PointerDoubleClick(PointerArgs args)
		{

		}
	}
}

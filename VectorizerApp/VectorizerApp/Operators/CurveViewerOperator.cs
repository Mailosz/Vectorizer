using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VectorizerLib;
using Windows.UI;

namespace VectorizerApp.Operators
{
	class CurveViewerOperator : IViewerOperator
	{
		public Context Context;

		Viewer viewer;
		CanvasGeometry[] geometries;
		Color[] colorvalues;

		int width, height;
		CanvasStrokeStyle hairline = new CanvasStrokeStyle()
		{
			TransformBehavior = CanvasStrokeTransformBehavior.Hairline
		};
		private bool fillGeometries;
		private MainWindow mainWindow;
		private bool showBackground = true;

		public bool FillGeometries
		{
			get => fillGeometries;
			set
			{
				fillGeometries = value;
				viewer.Invalidate();
			}
		}

		public bool ShowBackground 
		{ 
			get => showBackground; 
			set
			{
				showBackground = value;
				viewer.Invalidate();
			}
		}

		TextBlock timeTB, countTB;

		public CurveViewerOperator(Viewer viewer, Context context)
		{
			this.viewer = viewer;

			Context = context;
		}

		public void Initialize()
		{
			if (Context.Vectorizer == null)
			{
				var bytes = Context.OriginalBitmap.GetPixelBytes();
				RgbaByteSource source = new RgbaByteSource(bytes, (int)Context.OriginalBitmap.SizeInPixels.Width);

				Vectorizer<RgbaByteRegionData> vectorizer = new Vectorizer<RgbaByteRegionData>();
				vectorizer.Source = source;
				vectorizer.Properties = Context.Properties;
				vectorizer.Regionize();
				Context.Vectorizer = vectorizer;

				Stopwatch timer = Stopwatch.StartNew();
				Context.Vectorizer.Vectorize();
				timer.Stop();
				timeTB.Text = "Czas: " + timer.ElapsedMilliseconds + " ms";

				Context.RegionizationResult = Context.Vectorizer.RegionizationResult;
				Context.TracingResult = Context.Vectorizer.TracingResult;
				Context.FittingResult = Context.Vectorizer.FittingResult;

			}
			else
			{
				Stopwatch timer = Stopwatch.StartNew();
				Context.Vectorizer.FitCurves();
				timer.Stop();
				timeTB.Text = "Czas: " + timer.ElapsedMilliseconds + " ms";
				Context.FittingResult = Context.Vectorizer.FittingResult;
			}



			geometries = new CanvasGeometry[Context.FittingResult.Regions.Count];
			colorvalues = new Color[Context.FittingResult.Regions.Count];
			int i = 0;
			int count = 0;
			foreach (var region in Context.FittingResult.Regions)
			{
				CanvasPathBuilder cpb = new CanvasPathBuilder(viewer.Device);

				cpb.BeginFigure(region.Start);
				foreach (var elem in region.Path)
				{
					switch (elem.ElementType)
					{
						case PathElementType.Line:
							cpb.AddLine(elem.Coords[0]);
							break;
						case PathElementType.Quadratic:
							break;
						case PathElementType.Cubic:
							cpb.AddCubicBezier(elem.Coords[0], elem.Coords[1], elem.Coords[2]);
							break;
					}
					count++;
				}
				cpb.EndFigure(region.IsClosed ? CanvasFigureLoop.Closed : CanvasFigureLoop.Open);

				geometries[i] = CanvasGeometry.CreatePath(cpb);

				countTB.Text = "Liczba krzywych: " + count.ToString();

				var mean = Context.RegionizationResult.Regions[region.Index].Mean;
				var color = Color.FromArgb((byte)mean[3], (byte)mean[2], (byte)mean[1], (byte)mean[0]);
				colorvalues[i] = color;
				i++;
			}

		}

		public CanvasBitmap GetFinalBitmap()
		{
			CanvasRenderTarget crt = new CanvasRenderTarget(viewer.Device, Context.OriginalBitmap.SizeInPixels.Width, Context.OriginalBitmap.SizeInPixels.Height, 96);

			using (var ds = crt.CreateDrawingSession())
			{
				int i = 0;
				foreach (var g in geometries)
				{
					ds.FillGeometry(g, colorvalues[i]);
					i++;
				}
			}

			return crt;
		}

		public void Draw(DrawingArgs args)
		{
			if (ShowBackground)
			{
				args.Session.DrawImage(Context.OriginalBitmap, 0f, 0f, new Windows.Foundation.Rect(0, 0, Context.OriginalBitmap.SizeInPixels.Width, Context.OriginalBitmap.SizeInPixels.Height), 1f, CanvasImageInterpolation.NearestNeighbor);
			}

			if (FillGeometries)
			{
				int i = 0;
				foreach (var g in geometries)
				{
					args.Session.FillGeometry(g, colorvalues[i]);
					i++;
				}
			}
			else
			{
				foreach (var g in geometries)
				{
					args.Session.DrawGeometry(g, Colors.Red, 1, hairline);
				}
			}
		}

		public void SetWindow(MainWindow mainWindow)
		{
			this.mainWindow = mainWindow;

			StackPanel sp = new StackPanel();
			mainWindow.SetRightPanel(sp);

			ToggleSwitch ts1 = new ToggleSwitch();
			ts1.Header = "Wypełnione";
			ts1.Toggled += (s, e) => { FillGeometries = ts1.IsOn; };
			ts1.IsOn = FillGeometries;
			sp.Children.Add(ts1);

			ToggleSwitch ts2 = new ToggleSwitch();
			ts2.Header = "Pokaż obrazek";
			ts2.Toggled += (s, e) => { ShowBackground = ts2.IsOn; };
			ts2.IsOn = ShowBackground;
			sp.Children.Add(ts2);

			timeTB = new TextBlock();
			sp.Children.Add(timeTB);
			
			countTB = new TextBlock();
			sp.Children.Add(countTB);
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

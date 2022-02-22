using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VectorizerLib;
using Windows.UI;

namespace VectorizerApp.Operators
{
	class TracingViewerOperator : IViewerOperator
	{
		public Context Context;

		Viewer viewer;
		List<CanvasGeometry> geometries;
		List<Color> colorvalues;

		int width, height;
		CanvasStrokeStyle hairline = new CanvasStrokeStyle()
		{
			TransformBehavior = CanvasStrokeTransformBehavior.Hairline
		};
		private MainWindow mainWindow;

		public TracingViewerOperator(Viewer viewer, Context context)
		{
			this.viewer = viewer;

			Context = context;
		}

		public void Initialize()
		{
			Context.Vectorizer.Trace();
			Context.TracingResult = Context.Vectorizer.TracingResult;

			colorvalues = new List<Color>()
			{
				Colors.AliceBlue, Colors.AntiqueWhite, Colors.Aqua, Colors.Aquamarine, Colors.Azure, Colors.Beige, Colors.Bisque, Colors.Black, Colors.BlanchedAlmond, Colors.Blue,
				Colors.BlueViolet, Colors.Brown, Colors.BurlyWood, Colors.CadetBlue, Colors.Chartreuse, Colors.Chocolate, Colors.Coral, Colors.CornflowerBlue, Colors.Cornsilk,
				Colors.Crimson, Colors.Cyan, Colors.DarkBlue, Colors.DarkCyan, Colors.DarkGoldenrod, Colors.DarkGray, Colors.DarkGreen, Colors.DarkKhaki, Colors.DarkMagenta,
				Colors.DarkOliveGreen, Colors.DarkOrange, Colors.DarkOrchid, Colors.DarkRed, Colors.DarkSalmon, Colors.DarkSeaGreen, Colors.DarkSlateBlue, Colors.DarkSlateGray,
				Colors.DarkTurquoise, Colors.DarkViolet, Colors.DeepPink, Colors.DeepSkyBlue, Colors.DimGray, Colors.DodgerBlue, Colors.Firebrick, Colors.FloralWhite, Colors.ForestGreen
			};

			geometries = new List<CanvasGeometry>(Context.TracingResult.Edges.Count);
			foreach (var edge in Context.TracingResult.Edges)
			{
				CanvasPathBuilder cpb = new CanvasPathBuilder(viewer.Device);

				cpb.BeginFigure(new Vector2(edge.Start.X, edge.Start.Y));
				for (int a = 0; a < edge.Points.Length; a++)
				{
					cpb.AddLine(edge.Points[a]);
				}
				cpb.AddLine(new Vector2(edge.End.X, edge.End.Y));
				cpb.EndFigure(CanvasFigureLoop.Open);

				CanvasGeometry g = CanvasGeometry.CreatePath(cpb);
				geometries.Add(g);
			}



			viewer.SetSize(Context.OriginalBitmap.Size);
		}

		public void Draw(DrawingArgs args)
		{
			args.Session.DrawImage(Context.OriginalBitmap, 0f, 0f, new Windows.Foundation.Rect(0, 0, Context.OriginalBitmap.SizeInPixels.Width, Context.OriginalBitmap.SizeInPixels.Height), 1f, CanvasImageInterpolation.NearestNeighbor);
			int i = 0;
			foreach (var g in geometries)
			{
				args.Session.DrawGeometry(g, colorvalues[i % colorvalues.Count], 1, hairline);
				i++;
			}
		}

		public void SetWindow(MainWindow mainWindow)
		{
			this.mainWindow = mainWindow;

			StackPanel sp = new StackPanel();
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

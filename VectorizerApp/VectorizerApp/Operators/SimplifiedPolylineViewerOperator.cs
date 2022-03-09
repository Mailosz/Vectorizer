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
	class SimplifiedPolylineViewerOperator : IViewerOperator
	{
		public Context Context;

		Viewer viewer;
		CanvasGeometry[] geometries;
		List<Color> colorvalues;

		int width, height;
		CanvasStrokeStyle hairline = new CanvasStrokeStyle()
		{
			TransformBehavior = CanvasStrokeTransformBehavior.Hairline
		};

		public SimplifiedPolylineViewerOperator(Viewer viewer, Context context)
		{
			this.viewer = viewer;

			Context = context;
		}

		public void Initialize()
		{
			geometries = new CanvasGeometry[Context.TracingResult.Edges.Count];
			int i = 0;


			foreach (var edge in Context.TracingResult.Edges)
			{
				var start = new Vector2(edge.Start.X, edge.Start.Y);
				var end = new Vector2(edge.End.X, edge.End.Y);
				var points = DouglasPeucker.Simplify(start, edge.Points, end, Context.Properties.FittingDistance).ToArray();
				edge.SimplifiedPoints = points;

				CanvasPathBuilder cpb = new CanvasPathBuilder(viewer.Device);

				cpb.BeginFigure(start);
				foreach (var point in points)
				{
					cpb.AddLine(point);
				}
				cpb.AddLine(end);
				cpb.EndFigure(CanvasFigureLoop.Open);

				geometries[i] = CanvasGeometry.CreatePath(cpb);
				i++;
			}

		}

		public void Draw(DrawingArgs args)
		{
			args.Session.DrawImage(Context.OriginalBitmap, 0f, 0f, new Windows.Foundation.Rect(0, 0, Context.OriginalBitmap.SizeInPixels.Width, Context.OriginalBitmap.SizeInPixels.Height), 1f, CanvasImageInterpolation.NearestNeighbor);
			int i = 0;
			foreach (var g in geometries)
			{
				args.Session.DrawGeometry(g, Colors.Red, 1, hairline);
				i++;
			}
		}

		public void SetWindow(MainWindow mainWindow)
		{
			StackPanel sp = new StackPanel();
			mainWindow.SetRightPanel(sp);

			mainWindow.regionizeButton.IsEnabled = false;
			mainWindow.traceButton.IsEnabled = false;
			mainWindow.simplifyButton.IsEnabled = false;
			mainWindow.curveButton.IsEnabled = true;
			mainWindow.saveButton.IsEnabled = false;
			mainWindow.comparisonButton.IsEnabled = false;
			mainWindow.saveButton.IsEnabled = false;
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

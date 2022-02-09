using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.UI;
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
	class CurveViewerOperator : IViewerOperator
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

		public CurveViewerOperator(Viewer viewer, Context context)
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
				var points = DouglasPeucker.Simplify(start, edge.Points, end, 1f).ToArray();

				geometries[i] = CanvasGeometry.CreatePolygon(viewer.Device, points.Prepend(start).Append(end).ToArray());
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

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
	class CurvePlaygroundOperator : IViewerOperator
	{
		List<Vector2> points = new List<Vector2>();
		int currentPoint = -1;
		Vector2 curveStart = new Vector2();
		List<Curve> curves = new List<Curve>();

		CanvasGeometry curveGeometry = null;

		Viewer viewer;
		CanvasStrokeStyle hairline = new CanvasStrokeStyle()
		{
			TransformBehavior = CanvasStrokeTransformBehavior.Hairline
		};

		public CurvePlaygroundOperator(Viewer viewer)
		{
			this.viewer = viewer;

		}

		public void Initialize()
		{

			viewer.SetSize(new Windows.Foundation.Size(1000, 1000));
		}

		public void Invalidate()
		{
			//

			curves.Clear();
			if (points.Count > 2)
			{
				curveStart = points[0];
				float totalLength = 0;
				for (int i = 1; i < points.Count; i++)
				{
					totalLength += Vector2.Distance(points[i - 1], points[i]);
				}

				if (totalLength == 0) return;

				float accLen = 0;
				float accW1 = 0;
				float accW2 = 0;
				Vector2 cp1 = new Vector2();
				Vector2 cp2 = new Vector2();
				for (int i = 1; i < points.Count - 1; i++)
				{
					accLen += Vector2.Distance(points[i - 1], points[i]);

					float t = accLen / totalLength;

					float w1 = 3 * (1 - t) * (1 - t) * t;
					float w2 = 3 * (1 - t) * t * t;				

					//float w1 = (1 - t);
					//float w2 = t;

					accW1 += w1;
					accW2 += w2;

					cp1 += points[i] * w1;
					cp2 += points[i] * w2;
				}

				cp1 /= accW1;
				cp2 /= accW2;

				Curve curve = new Curve()
				{
					CP1 = cp1,
					CP2 = cp2,
					End = points.Last()
				};
				curves.Add(curve);
			}

			//
			if (curves.Count > 0)
			{
				CanvasPathBuilder cpb = new CanvasPathBuilder(viewer.Device);

				cpb.BeginFigure(curveStart);
				foreach (var curve in curves)
				{
					cpb.AddCubicBezier(curve.CP1, curve.CP2, curve.End);
				}
				cpb.EndFigure(CanvasFigureLoop.Open);
				curveGeometry = CanvasGeometry.CreatePath(cpb);
			}

			viewer.Invalidate();
		}

		public void Draw(DrawingArgs args)
		{
			foreach (var point in points)
			{
				args.Session.FillCircle(point, 2, Colors.Violet);
			}

			if (currentPoint >= 0 && currentPoint < points.Count)
			{
				args.Session.DrawCircle(points[currentPoint], 3, Colors.Green);
			}

			if (curveGeometry != null)
			{
				args.Session.DrawGeometry(curveGeometry, Colors.Black);
			}

			if (curves.Count > 0)
			{
				Vector2 lastJoint = curveStart;
				foreach (var curve in curves)
				{
					args.Session.DrawLine(lastJoint, curve.CP1, Colors.Blue);
					args.Session.DrawLine(curve.CP1, curve.CP2, Colors.Blue);
					args.Session.DrawLine(curve.CP2, curve.End, Colors.Blue);
					lastJoint = curve.End;

					args.Session.FillCircle(curve.CP1, 2, Colors.Red);
					args.Session.FillCircle(curve.CP2, 2, Colors.Red);

				}
			}
		}

		public void SetWindow(MainWindow mainWindow)
		{
			
		}


		public bool PointerPressed(PointerArgs args)
		{
			float maxdis = 10;
			currentPoint = -1;
			for (int i = 0; i < points.Count; i++)
			{
				float dis = Vector2.Distance(args.Point.ToVector2(), points[i]);
				if (dis < maxdis)
				{
					currentPoint = i;
					maxdis = dis;
				}
			}
			viewer.Invalidate();

			return currentPoint != -1;
		}

		public bool PointerMoved(PointerArgs args)
		{
			if (currentPoint >= 0)
			{
				points[currentPoint] = args.Point.ToVector2();
				Invalidate();
				return true;
			}
			else return false;
		}

		public bool PointerHover(PointerArgs args)
		{
			return false;
		}

		public void PointerDoubleClick(PointerArgs args)
		{
			if (currentPoint >= 0)
			{
				points.RemoveAt(currentPoint);
				currentPoint = -1;
			}
			else
			{
				currentPoint = points.Count;
				points.Add(args.Point.ToVector2());
			}

			Invalidate();
		}
	}
}

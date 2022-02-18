﻿using Microsoft.Graphics.Canvas;
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
			SimpleCurveFitter scf = new SimpleCurveFitter();
			scf.Properties = Context.Properties;
			Context.FittingResult = scf.Fit(Context.TracingResult);


			geometries = new CanvasGeometry[Context.FittingResult.Regions.Count];
			int i = 0;
			foreach (var region in Context.FittingResult.Regions)
			{
				CanvasPathBuilder cpb = new CanvasPathBuilder(viewer.Device);

				bool isfigureclosed = false;
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
				}
				cpb.EndFigure(region.IsClosed ? CanvasFigureLoop.Closed : CanvasFigureLoop.Open);

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

﻿using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorizerApp.Operators
{
	class BitmapViewerOperator : IViewerOperator
	{
		Viewer viewer;
		public Context Context;

		public BitmapViewerOperator(Viewer viewer, Context context)
		{
			this.viewer = viewer;
			this.Context = context;
		}

		public void Initialize()
		{
			viewer.SetSize(Context.OriginalBitmap.Size);
		}

		public void Draw(DrawingArgs args)
		{
			args.Session.DrawImage(Context.OriginalBitmap, 0f, 0f, new Windows.Foundation.Rect(0,0, Context.OriginalBitmap.SizeInPixels.Width, Context.OriginalBitmap.SizeInPixels.Height), 1f, CanvasImageInterpolation.NearestNeighbor);
		}

		public void SetWindow(MainWindow mainWindow)
		{
			StackPanel sp = new StackPanel();
			mainWindow.SetRightPanel(sp);

			mainWindow.regionizeButton.IsEnabled = true;
			mainWindow.vectorizeButton.IsEnabled = true;

			mainWindow.traceButton.IsEnabled = false;
			mainWindow.simplifyButton.IsEnabled = false;
			mainWindow.curveButton.IsEnabled = false;
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

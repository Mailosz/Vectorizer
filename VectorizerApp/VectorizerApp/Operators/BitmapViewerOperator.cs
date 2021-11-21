using Microsoft.Graphics.Canvas;
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
			
		}
	}
}

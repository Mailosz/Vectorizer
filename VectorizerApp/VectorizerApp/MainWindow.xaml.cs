using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using VectorizerApp.Operators;
using VectorizerLib;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VectorizerApp
{
	/// <summary>
	/// An empty window that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainWindow : Window
	{
		List<IViewerOperator> history = new List<IViewerOperator>();

		IViewerOperator currentOperator;
		private VectorizerProperties properties;

		public VectorizerProperties Properties { get => properties; set => properties = value; }

		public float Angle
		{
			get => Properties.FittingAcuteAngle / MathF.PI * 180F;
			set
			{
				Properties.FittingAcuteAngle = value * MathF.PI / 180F;
			}
		}

		public MainWindow()
		{
			this.InitializeComponent();

			properties = new VectorizerProperties()
			{
				RegionizationTreshold = 0.1f,
				RegionizationMinimumSteps = 25,
				RegionizationMaximumSteps = 50,
			};
		}


		private void Viewer_Draw(Viewer sender, Microsoft.Graphics.Canvas.CanvasDrawingSession ds)
		{
			if (currentOperator != null)
			{
				DrawingArgs args = new DrawingArgs()
				{
					Session = ds,
				};

				currentOperator.Draw(args);
			}
		}

		private async void loadButton_Click(object sender, RoutedEventArgs e)
		{

			FileOpenPicker fop = new FileOpenPicker();
			fop.SetOwnerWindow(this);

			fop.FileTypeFilter.Add(".jpg");
			fop.FileTypeFilter.Add(".jpeg");
			fop.FileTypeFilter.Add(".png");
			fop.FileTypeFilter.Add(".bmp");
			fop.FileTypeFilter.Add(".gif");
			fop.FileTypeFilter.Add(".tiff");
			fop.FileTypeFilter.Add(".tif");

			var file = await fop.PickSingleFileAsync();

			if (file != null)
			{
				var bitmap = await CanvasBitmap.LoadAsync(viewer.Device, await file.OpenAsync(Windows.Storage.FileAccessMode.Read));

				openNewImage(bitmap);
			}
		}

		public void openNewImage(CanvasBitmap bitmap)
		{
			Context context = new Context();
			context.OriginalBitmap = bitmap;
			context.Properties = properties;
			BitmapViewerOperator op = new BitmapViewerOperator(viewer, context);
			openNewViewerOperator(op);
		}

		private void openNewViewerOperator(IViewerOperator newOperator)
		{
			if (currentOperator != null) history.Add(currentOperator);

			setViewerOperator(newOperator);
		}

		private void setViewerOperator(IViewerOperator newOperator)
		{
			currentOperator = newOperator;
			newOperator.SetWindow(this);
			newOperator.Initialize();

			viewer.Invalidate();
		}

		private void regionizeButton_Click(object sender, RoutedEventArgs e)
		{
			if (currentOperator is BitmapViewerOperator bvo)
			{
				var oper = new RegionsViewerOperator(viewer, bvo.Context);

				openNewViewerOperator(oper);
			}
		}

		private void traceButton_Click(object sender, RoutedEventArgs e)
		{
			if (currentOperator is RegionsViewerOperator rvo)
			{
				var oper = new TracingViewerOperator(viewer, rvo.Context);

				openNewViewerOperator(oper);
			}
		}
		private void simplifyButton_Click(object sender, RoutedEventArgs e)
		{
			if (currentOperator is TracingViewerOperator tvo)
			{
				var oper = new SimplifiedPolylineViewerOperator(viewer, tvo.Context);

				openNewViewerOperator(oper);
			}
		}

		private void curveButton_Click(object sender, RoutedEventArgs e)
		{
			if (currentOperator is SimplifiedPolylineViewerOperator svo)
			{
				var oper = new CurveViewerOperator(viewer, svo.Context);

				openNewViewerOperator(oper);
			}
		}

		public void SetRightPanel(object element)
		{
			rightPanel.Content = element;
		}

		private void undoButton_Click(object sender, RoutedEventArgs e)
		{
			
			if (history.Count > 0)
			{
				var opr = history.Last();
				history.Remove(opr);
				setViewerOperator((IViewerOperator)opr);
			}
		}

		private void playgroundButton_Click(object sender, RoutedEventArgs e)
		{
			var oper = new CurvePlaygroundOperator(viewer);

			openNewViewerOperator(oper);
		}

		bool ispressed = false;
		private void viewer_PointerMoved(UIElement sender, PointerRoutedEventArgs e)
		{
			var point = e.GetCurrentPoint(sender).Position;
			var args = new PointerArgs()
			{
				Sender = sender,
				Pointer = e,
				Zoom = viewer.Zoom,
				Point = point,
			};
			if (ispressed)
			{
				currentOperator?.PointerMoved(args);
			}
			else
			{
				currentOperator?.PointerHover(args);
			}
			
		}
		DateTime lastClick = DateTime.Now;
		private Point lastPoint;



		private void viewer_PointerPressed(UIElement sender, PointerRoutedEventArgs e)
		{
			var point = e.GetCurrentPoint(sender).Position;
			var args = new PointerArgs()
			{
				Sender = sender,
				Pointer = e,
				Zoom = viewer.Zoom,
				Point = point,
			};
			if (DateTime.Now.Subtract(lastClick).TotalMilliseconds < 500 && Vector2.Distance(lastPoint.ToVector2(), point.ToVector2()) < 10)
			{
				currentOperator?.PointerDoubleClick(args);
			} 
			else
			{
				currentOperator?.PointerPressed(args);
			}
			lastClick = DateTime.Now;
			lastPoint = point;
			ispressed = true;
		}

		private void viewer_PointerReleased(UIElement sender, PointerRoutedEventArgs e)
		{
			ispressed = false;
		}

		private void viewer_PointerCanceled(UIElement sender, PointerRoutedEventArgs e)
		{
			ispressed = false;
		}

		private void optionsButton_Click(object sender, RoutedEventArgs e)
		{
			optionsPopup.IsOpen = !optionsPopup.IsOpen;
		}

	}
}

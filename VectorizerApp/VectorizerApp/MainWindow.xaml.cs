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


		public MainWindow()
		{
			this.InitializeComponent();
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
			context.Properties = new VectorizerProperties()
			{
				RegionizationTreshold = 0.1f,
				RegionizationMinimumSteps = 10,
				RegionizationMaximumSteps = 100,
			};
			BitmapViewerOperator op = new BitmapViewerOperator(viewer, context);
			openNewViewerOperator(op);
		}

		private void openNewViewerOperator(IViewerOperator newOperator)
		{
			if (currentOperator != null) history.Add(currentOperator);

			currentOperator = newOperator;

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
	}
}

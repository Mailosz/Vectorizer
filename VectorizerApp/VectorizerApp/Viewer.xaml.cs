using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
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
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VectorizerApp
{
	public sealed partial class Viewer : UserControl
	{
		public delegate void DrawEventArgs(Viewer sender, CanvasDrawingSession ds);
		public event DrawEventArgs Draw;

		public delegate void PointerEventArgs(UIElement sender, PointerRoutedEventArgs e);
		public new event PointerEventArgs PointerMoved;
		public new event PointerEventArgs PointerPressed;
		public new event PointerEventArgs PointerReleased;
		public new event PointerEventArgs PointerCanceled;


		public float Zoom { get => scrollViewer.ZoomFactor; }

		public CanvasDevice Device { get => canvas.Device; }

		public Viewer()
		{
			this.InitializeComponent();
		}


		private void canvas_CreateResources(CanvasVirtualControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
		{

		}

		private void canvas_RegionsInvalidated(CanvasVirtualControl sender, CanvasRegionsInvalidatedEventArgs args)
		{
			var ds = sender.CreateDrawingSession(args.VisibleRegion);

			Draw?.Invoke(this, ds);
		}

		private void canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
		{
			Draw?.Invoke(this, args.DrawingSession);
		}

		public void Invalidate()
		{
			canvas.Invalidate();
		}

		internal void SetSize(Size size)
		{
			canvas.Width = size.Width;
			canvas.Height = size.Height;
		}

		private void scrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
		{
			canvas.Invalidate();
			canvas.DpiScale = scrollViewer.ZoomFactor;
		}

		private void canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
		{
			PointerPressed?.Invoke((UIElement)sender, e);
		}

		private void canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
		{
			PointerMoved?.Invoke((UIElement)sender, e);
		}

		private void canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
		{
			PointerReleased?.Invoke((UIElement)sender, e);
		}

		private void canvas_PointerCanceled(object sender, PointerRoutedEventArgs e)
		{
			PointerCanceled?.Invoke((UIElement)sender, e);
		}
	}
}

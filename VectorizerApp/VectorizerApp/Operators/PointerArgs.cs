using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.Foundation;

namespace VectorizerApp
{
	public class PointerArgs
	{
		public UIElement Sender { get; set; }
		public PointerRoutedEventArgs Pointer { get; set; }
		public float Zoom { get; set; }
		public Point Point { get; internal set; }
	}
}
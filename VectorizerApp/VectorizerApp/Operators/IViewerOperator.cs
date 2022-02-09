using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorizerApp
{
	interface IViewerOperator
	{
		void Draw(DrawingArgs args);
		void Initialize();
		void SetWindow(MainWindow mainWindow);

		bool PointerPressed(PointerArgs args);
		bool PointerMoved(PointerArgs args);
		bool PointerHover(PointerArgs args);
		void PointerDoubleClick(PointerArgs args);
	}
}

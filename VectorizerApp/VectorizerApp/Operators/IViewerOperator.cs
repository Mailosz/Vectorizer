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
	}
}

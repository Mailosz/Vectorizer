using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorizerLib
{
	public class CurveFitter
	{
		VectorizerProperties properties;
		public VectorizerProperties Properties { get => properties; set { properties = value; } }

		TracingResult traced;
		public TracingResult TracingResult { get => traced; set { traced = value; } }

		public FittingResult FitCurves()
		{
			return new FittingResult();
		}
	}
}

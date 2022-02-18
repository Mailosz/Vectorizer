using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorizerLib
{
	public class VectorizerProperties
	{
		public float RegionizationTreshold { get; set; }
		public uint RegionizationMaximumSteps { get; set; }
		public uint RegionizationMinimumSteps { get; set; }
		public long RegionMinimumArea { get; set; } = 25;
		public float FittingAcuteAngle { get; set; } = MathF.PI;
		public float FittingDistance { get; set; } = 1f;
	}
}

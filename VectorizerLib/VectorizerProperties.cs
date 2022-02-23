using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorizerLib
{
	public class VectorizerProperties
	{
		public float RegionizationTreshold { get; set; } = 1000;
		public uint RegionizationMaximumSteps { get; set; } = 100;
		public uint RegionizationMinimumSteps { get; set; } = 25;
		public long RegionMinimumArea { get; set; } = 25;
		public float FittingAcuteAngle { get; set; } = MathF.PI / 2;
		public float FittingDistance { get; set; } = 1.5f;
		public float JoiningTreshold { get; set; } = 20f;
	}
}

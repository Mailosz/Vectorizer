using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorizerLib;

namespace VectorizerApp
{
	class Context
	{
		public CanvasBitmap OriginalBitmap { get; set; }
		public CanvasBitmap FinalBitmap { get; set; }
		public VectorizerProperties Properties { get; set; }
		public RegionizationResult RegionizationResult { get; set; }
		public bool StepByStep { get; internal set; }
		public FittingResult FittingResult { get; internal set; }
		public CanvasBitmap RegionsImage { get; internal set; }
		public TracingResult TracingResult { get; set; }
		public Vectorizer<RgbaByteRegionData> Vectorizer { get; set; }
		public TracingResult SimplifiedTracingResult { get; internal set; }
	}
}

using System.Collections.Generic;

namespace VectorizerLib
{
	public class RegionizationResult
	{
		public int Width { get; set; }
		public int Height { get; set; }
		public ushort[] Board { get; set; }
		public int RegionCount { get; set; }
	}
}
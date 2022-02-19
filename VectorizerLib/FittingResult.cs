using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace VectorizerLib
{
	public class FittingResult
	{
		public List<FitRegion> Regions;
	}

	public class FitRegion
	{
		public ushort Index;
		public Vector2 Start;
		public bool IsClosed = false;
		public List<PathElement> Path;
	}

	public class PathElement
	{
		public PathElementType ElementType { get; set; }
		public Vector2[] Coords { get; set; }
	}

	public enum PathElementType : byte
	{
		Line, Quadratic, Cubic
	}
}
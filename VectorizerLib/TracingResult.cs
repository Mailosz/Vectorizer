using System.Collections.Generic;
using System.Numerics;

namespace VectorizerLib
{
	public class TracingResult
	{
		public Dictionary<ushort, TracedRegion> Regions { get; set; }
	}

	public class TracedRegion
	{
		public List<TracedEdge> Edges { get; set; }
	}

	public class TracedEdge
	{
		public TracedNode Start;
		public TracedNode End;
		public Vector2[] Points;
	}

	public class TracedNode
	{

	}

}
using System.Collections.Generic;
using System.Numerics;

namespace VectorizerLib
{
	public class TracingResult
	{
		public Dictionary<ushort, TracedRegion> Regions { get; set; }
		public List<TracedEdge> Edges { get; set; }
	}

	public class TracedRegion
	{
		public List<TracedEdge> Edges { get; set; } = new List<TracedEdge>();
		public List<TracedNode> Nodes { get; set; } = new List<TracedNode>();
	}

	public class TracedEdge
	{
		public TracedNode Start;
		public TracedNode End;
		public Vector2[] Points;
	}

	public class TracedNode
	{
		public List<TracedEdge> Edges { get; set; } = new List<TracedEdge>(4);
		public int X { get; set; }
		public int Y { get; set; }
	}

}
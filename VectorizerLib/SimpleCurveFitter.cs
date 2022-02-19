using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VectorizerLib
{
	public class SimpleCurveFitter
	{
		VectorizerProperties properties;
		public VectorizerProperties Properties { get => properties; set { properties = value; } }



		public FittingResult Fit(TracingResult tracingResult)
		{
			List<FitRegion> regions = new List<FitRegion>(tracingResult.Regions.Count);
			foreach (var (key, region) in tracingResult.Regions)
			{
				if (region.Edges.Count > 0)
				{
					var nr = fitRegion(key, region);
					regions.Add(nr);

				}
			}


			return new FittingResult()
			{
				Regions = regions,
			};
		}

		private FitRegion fitRegion(ushort key, TracedRegion region)
		{
			FitRegion result = new FitRegion()
			{
				Index = key,
				Path = new List<PathElement>(),
			};

			var edges = new List<TracedEdge>(region.Edges);
			var first = edges.First();
			edges.Remove(first);

			var curend = first.End;
			Vector2 prevPoint = first.Start.ToVector2();
			result.Start = prevPoint;
			Vector2 curPoint;

			IEnumerator<Vector2> pointsEnumerator = first.SimplifiedPoints.AsEnumerable().GetEnumerator();
			bool lastAcute = false;
			if (pointsEnumerator.MoveNext())
			{
				curPoint = pointsEnumerator.Current;
			}
			else 
			{
				curPoint = curend.ToVector2();
				if (!findNextEdge())
				{
					Console.WriteLine("TOOSHORT");
					return result;
				}
			}

			do
			{

				while (pointsEnumerator.MoveNext())
				{
					computePathElement(prevPoint, curPoint, pointsEnumerator.Current);

					prevPoint = curPoint;
					curPoint = pointsEnumerator.Current;
				}
				computePathElement(prevPoint, curPoint, curend.ToVector2());
				prevPoint = curPoint;
				curPoint = curend.ToVector2();

				// find next edge
				if (!findNextEdge()) break;

				Vector2 nextPoint;
				if (pointsEnumerator.MoveNext())
				{
					nextPoint = pointsEnumerator.Current;
				}
				else
				{
					nextPoint = curend.ToVector2();
					if (!findNextEdge()) break;
				}
				computePathElement(prevPoint, curPoint, nextPoint);
				prevPoint = curPoint;
				curPoint = nextPoint;
				continue;
			} 
			while (true);




			return result;

			Vector2 computePathElement(Vector2 point1, Vector2 point2, Vector2 point3)
			{
				var dir1 = VectorHelper.Direction(point1, point2);
				var dir2 = VectorHelper.Direction(point2, point3);
				var dis1 = Vector2.Distance(point1, point2);
				var dis2 = Vector2.Distance(point2, point3);

				float dif = dir2 - dir1;

				if (dif < properties.FittingAcuteAngle)
				{
					if (lastAcute)
					{
						addPathElement(PathElementType.Line, point1 + (point2 - point1) / 2);
					}

					var dir = VectorHelper.Direction(point1, point3);

					var lastPoint = point2 + (point3 - point2) / 2;
					addPathElement(PathElementType.Cubic,
							point2 - (new Vector2(MathF.Cos(dir1), MathF.Sin(dir1)) * dis1 / 4),
							point2 + (new Vector2(MathF.Cos(dir2), MathF.Sin(dir2)) * dis2 / 4),
							lastPoint);


					lastAcute = false;
					return lastPoint;
				}
				else
				{
					addPathElement(PathElementType.Line, point2);

					lastAcute = true;
					return point2;
				}
			}

			void addPathElement(PathElementType type, params Vector2[] coords)
			{
				result.Path.Add(new PathElement()
				{
					ElementType = type,
					Coords = coords
				});
			}

			bool findNextEdge()
			{
				if (curend == first.Start)
				{
					var nextPoint = first.Points.Length == 0 ? first.End.ToVector2() : first.Points[0];
					result.Start = computePathElement(prevPoint, curPoint, nextPoint);

					result.IsClosed = true;
					return false;
				}

				foreach (var edge in edges)
				{
					if (edge.Start == curend)
					{
						edges.Remove(edge);
						pointsEnumerator = edge.SimplifiedPoints.AsEnumerable().GetEnumerator();
						curend = edge.End;
						return true;
					}
					else if (edge.End == curend) // found, but backwards
					{
						edges.Remove(edge);
						pointsEnumerator = edge.SimplifiedPoints.Reverse().GetEnumerator();
						curend = edge.Start;
						return true;
					}
				}
				return false;
			}
		}

		private float findSmoothest(TracedNode node, out int edge1, out int edge2)
		{
			float mindif = float.PositiveInfinity;
			edge1 = 0;
			edge2 = 0;
			for (int a = 0; a < node.Edges.Count - 1; a++)
			{
				for (int b = a + 1; b < node.Edges.Count; b++)
				{
					float dif = checkSmoothness(node.Edges[a], node.Edges[b]);
					if (dif < mindif)
					{
						mindif = dif;
						edge1 = a;
						edge2 = b;
					}
				}
			}

			return mindif;



			float checkSmoothness(TracedEdge edge1, TracedEdge edge2)
			{
				Vector2 point1, point2;
				if (edge1.End == node)
				{
					if (edge1.Points.Length > 0)
					{
						point1 = edge1.Points.Last();
					}
					else point1 = edge1.Start.ToVector2();
				}
				else
				{
					if (edge1.Points.Length > 0)
					{
						point1 = edge1.Points.First();
					}
					else point1 = edge1.End.ToVector2();
				}

				if (edge2.Start == node)
				{
					if (edge2.Points.Length > 0)
					{
						point2 = edge2.Points.First();
					}
					else point2 = edge2.End.ToVector2();
				}
				else
				{
					if (edge2.Points.Length > 0)
					{
						point2 = edge2.Points.Last();
					}
					else point2 = edge2.Start.ToVector2();
				}

				float dir1 = VectorHelper.Direction(point1, node.ToVector2());
				float dir2 = VectorHelper.Direction(node.ToVector2(), point2);

				return dir2 - dir1;
			}
		}
	}

	public static class VectorHelper
	{
		public static float Direction(Vector2 v1, Vector2 v2)
		{
			return MathF.Atan2(v2.Y - v1.Y, v2.X - v1.X);
		}
	}
}

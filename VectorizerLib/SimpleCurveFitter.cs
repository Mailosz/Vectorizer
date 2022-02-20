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
			var curEdge = first;
			TracedEdge prevEdge = first;
			edges.Remove(first);

			var curend = first.End;
			Vector2 prevPoint = first.Start.ToVector2();
			result.Start = prevPoint;
			Vector2 curPoint;
			TracedNode curNode = first.Start;

			IEnumerator<Vector2> pointsEnumerator = first.SimplifiedPoints.AsEnumerable().GetEnumerator();
			bool lastAcute = false;
			//read secod point
			bool fitNode = false;
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

				//fitting edge
				while (pointsEnumerator.MoveNext())
				{
					if (fitNode)
					{
						curPoint = computeNodePathElement(prevPoint, curNode, pointsEnumerator.Current, prevEdge, curEdge);
					}
					else
					{
						computePathElement(prevPoint, curPoint, pointsEnumerator.Current);
					}

					prevPoint = curPoint;
					curPoint = pointsEnumerator.Current;

					fitNode = false;
				}
				var nextPoint = curend.ToVector2();
				if (fitNode)
				{
					curPoint = computeNodePathElement(prevPoint, curNode, nextPoint, prevEdge, curEdge);
				}
				else
				{
					computePathElement(prevPoint, curPoint, nextPoint);
				}

				// find next edge
				if (!findNextEdge()) break;

				prevPoint = curPoint;
				curPoint = nextPoint;
				continue;
			} 
			while (true);




			return result;

			Vector2 computeNodePathElement(Vector2 prevPoint, TracedNode node, Vector2 nextPoint, TracedEdge prevEdge, TracedEdge nextEdge)
			{
				if (node.IsBorder)
				{
					addPathElement(PathElementType.Line, node.ToVector2());
					lastAcute = true;
					return node.ToVector2();
				}
				else
				{
					float diff = findSmoothest(node, out int smooth1, out int smooth2);

					if (computeAcuteness(diff)) // acute
					{
						addPathElement(PathElementType.Line, node.ToVector2());
						lastAcute=true;
						return node.ToVector2();
					}
					else // smooth
					{
						var edge1 = node.Edges[smooth1];
						var edge2 = node.Edges[smooth2];
						if ((edge1 == prevEdge && edge2 == nextEdge)
							|| (edge2 == prevEdge && edge1 == nextEdge))
						{
							//this is the smoothest route
							return computePathElement(prevPoint, node.ToVector2(), nextPoint);
						}
						else
						{
							var (point1, point3) = getOutgoingPoints(edge1, node, edge2);
							var point2 = node.ToVector2();

							var t = 0.5f;

							var start = point1 + (point2 - point1) * t;
							var (cp1, cp2, lastPoint) = getCurveControlPoints(point1, point2, point3);

							var hp1 = start + (cp1 - start) * t;
							var hp2 = cp1 + (cp2 - cp1) * t;
							var hp3 = cp2 + (lastPoint - cp2) * t;

							var hp4 = hp1 + (hp2 - hp1) * t;
							var hp5 = hp2 + (hp3 - hp2) * t;

							var halfpoint = hp4 + (hp5 - hp4) * t;


							

							if (nextEdge == edge1)
							{
								addPathElement(PathElementType.Line, halfpoint);
								addPathElement(PathElementType.Cubic, hp4, hp1, start);

								lastAcute = false;
								return halfpoint;
							} 
							else if (nextEdge == edge2)
							{
								addPathElement(PathElementType.Line, halfpoint);
								addPathElement(PathElementType.Cubic, hp5, hp3, lastPoint);

								lastAcute = false;

								return halfpoint;
							}
							else if (prevEdge == edge1)
							{
								addPathElement(PathElementType.Cubic, hp1, hp4, halfpoint);
								lastAcute = true;
								return halfpoint;
							}
							else if (prevEdge == edge2)
							{
								addPathElement(PathElementType.Cubic, hp3, hp5, halfpoint);
								lastAcute = true;
								return halfpoint;
							}
							else
							{

								lastAcute = true;

								return halfpoint;
							}

							return halfpoint;
						}
					}
				}
			}

			Vector2 computePathElement(Vector2 point1, Vector2 point2, Vector2 point3)
			{
				var dir1 = Helper.Direction(point1, point2);
				var dir2 = Helper.Direction(point2, point3);
				var dis1 = Vector2.Distance(point1, point2);
				var dis2 = Vector2.Distance(point2, point3);

				float dif = Helper.Angle(dir2, dir1);

				bool acute = computeAcuteness(dif);
				if (acute)
				{
					addPathElement(PathElementType.Line, point2);
					lastAcute = acute;
					return point2;
				}
				else
				{
					if (lastAcute)
					{
						addPathElement(PathElementType.Line, point1 + (point2 - point1) / 2);
					}
					var (cp1, cp2, lastPoint) = getCurveControlPoints(point1, point2, point3);
					addPathElement(PathElementType.Cubic, cp1, cp2, lastPoint);

					lastAcute = acute;
					return lastPoint;
				}

			}

			PathElement createCubicPathElement(Vector2 point1, Vector2 point2, Vector2 point3, float dir1, float dir2, float dis1, float dis2)
			{

				var lastPoint = point2 + (point3 - point2) / 2;

				var element = new PathElement()
				{
					ElementType = PathElementType.Cubic,
					Coords = new[] {
						point2 - (new Vector2(MathF.Cos(dir1), MathF.Sin(dir1)) * dis1 / 4),
						point2 + (new Vector2(MathF.Cos(dir2), MathF.Sin(dir2)) * dis2 / 4),
						lastPoint}
				};
				return element;

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
				prevEdge = curEdge;
				fitNode = true;
				curNode = curend;
				if (curend == first.Start)
				{
					curEdge = first;
					var nextPoint = first.Points.Length == 0 ? first.End.ToVector2() : first.Points[0];
					result.Start = computeNodePathElement(prevPoint, curend, nextPoint, prevEdge, curEdge);

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
						curEdge = edge;
						return true;
					}
					else if (edge.End == curend) // found, but backwards
					{
						edges.Remove(edge);
						pointsEnumerator = edge.SimplifiedPoints.Reverse().GetEnumerator();
						curend = edge.Start;
						curEdge = edge;
						return true;
					}
				}
				return false;
			}
		}

		private bool computeAcuteness(float dif)
		{
			return dif > properties.FittingAcuteAngle;
		}

		private (Vector2, Vector2, Vector2) getCurveControlPoints(Vector2 point1, Vector2 point2, Vector2 point3)
		{
			return (point2 + (point1 - point2) / 4,
							point2 + (point3 - point2) / 4,
							point2 + (point3 - point2) / 2);
		}

		private (Vector2, Vector2) getOutgoingPoints(TracedEdge edge1, TracedNode node, TracedEdge edge2)
		{
			Vector2 point1, point2;
			if (edge1.End == node)
			{
				if (edge1.SimplifiedPoints.Length > 0)
				{
					point1 = edge1.SimplifiedPoints.Last();
				}
				else point1 = edge1.Start.ToVector2();
			}
			else
			{
				if (edge1.SimplifiedPoints.Length > 0)
				{
					point1 = edge1.SimplifiedPoints.First();
				}
				else point1 = edge1.End.ToVector2();
			}

			if (edge2.Start == node)
			{
				if (edge2.SimplifiedPoints.Length > 0)
				{
					point2 = edge2.SimplifiedPoints.First();
				}
				else point2 = edge2.End.ToVector2();
			}
			else
			{
				if (edge2.SimplifiedPoints.Length > 0)
				{
					point2 = edge2.SimplifiedPoints.Last();
				}
				else point2 = edge2.Start.ToVector2();
			}

			return (point1, point2);
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
				var (point1, point2) = getOutgoingPoints(edge1, node, edge2);

				float dir1 = Helper.Direction(point1, node.ToVector2());
				float dir2 = Helper.Direction(node.ToVector2(), point2);

				return Helper.Angle(dir2, dir1);
			}
		}
	}

	public static class Helper
	{
		public static float Direction(Vector2 v1, Vector2 v2)
		{
			return MathF.Atan2(v2.Y - v1.Y, v2.X - v1.X);
		}


		public static float Angle(float angle1, float angle2)
		{
			var angle = (angle2 - angle1) + MathF.PI;
			return Math.Abs(angle - MathF.Floor(angle / (MathF.PI*2)) * (MathF.PI*2) - MathF.PI);
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VectorizerLib
{
	public static class DouglasPeucker
	{
		public static IEnumerable<Vector2> Simplify(Vector2 startpoint, Span<Vector2> points, Vector2 endpoint, float maxdis)
		{
			if (points.Length == 0)
			{
				return new List<Vector2>() { };
			}

			//find furthest point
			//Vector2 startpoint = points[0];
			//Vector2 endpoint = points[points.Length - 1];
			float len = Vector2.Distance(startpoint, endpoint);
			float curdis = 0;
			int furthest = -1;
			if (len < float.Epsilon) // distance to point
			{
				for (int i = 0; i < points.Length; i++)
				{
					float dis = Vector2.Distance(startpoint, points[i]);
					if (dis > curdis)
					{
						furthest = i;
						curdis = dis;
					}
				}
			} 
			else // distance to line
			{
				for (int i = 0; i < points.Length; i++)
				{
					float xx = (endpoint.X - startpoint.X);
					float yy = (endpoint.Y - startpoint.Y);
					float dis = Math.Abs((xx * (startpoint.Y - points[i].Y) - (startpoint.X - points[i].X) * yy) / len);
					if (dis > curdis)
					{
						furthest = i;
						curdis = dis;
					}
				}
			}

			if (furthest != -1 && curdis > maxdis)
			{
				var splitpoint = points[furthest];
				IEnumerable<Vector2> left, right;
				if (furthest == 0)
				{
					left = Enumerable.Empty<Vector2>();
				}
				else
				{
					left = Simplify(startpoint, points.Slice(0, furthest), splitpoint, maxdis);
				}
				if (furthest == points.Length - 1)
				{
					right = Enumerable.Empty<Vector2>();
				}
				else
				{
					right = Simplify(splitpoint, points.Slice(furthest + 1), endpoint, maxdis);
				}


				return left.Concat(right.Prepend(splitpoint));
			}
			else // the whole line can be simplified
			{
				return new List<Vector2> { };
			}
		} 
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VectorizerLib
{
	public class CurveFitter
	{
		VectorizerProperties properties;
		public VectorizerProperties Properties { get => properties; set { properties = value; } }

		TracingResult traced;
		public TracingResult TracingResult { get => traced; set { traced = value; } }

		public FittingResult FitCurves()
		{
			return new FittingResult();
		}

		public void fitPath(List<Vector2> points)
		{
			float[] deltas = new float[points.Count - 2];

			float delta = 0f;
			float lastAngle = MathF.Atan2(points[1].Y - points[0].Y, points[1].X - points[0].X);
			float lastDelta = lastAngle / Vector2.Distance(points[0], points[1]);
			for (int i = 0; i < points.Count - 3; i++)
			{
				float angle = MathF.Atan2(points[i + 1].Y - points[i+2].Y, points[i+1].X - points[i+2].X);
				delta = lastAngle - angle;
				delta /= Vector2.Distance(points[i + 1], points[i + 2]);
				if (Math.Abs(delta) > Properties.FittingAcuteAngle)
				{
					//TODO: split
				}
				
				deltas[i] = (lastDelta - delta);
				lastDelta = delta;
			}


		}
	}

	public struct Curve
	{
		public Vector2 CP1 { get; set; }
		public Vector2 CP2 { get; set; }
		public Vector2 End { get; set; }
	}
}

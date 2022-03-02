using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace VectorizerLib
{
	public class Vectorizer<D> where D : class, IRegionData
	{
		VectorizerProperties properties;
		public VectorizerProperties Properties { get => properties; set { properties = value; } }

		IRasterSource<D> source;
		public IRasterSource<D> Source { get => source; set { source = value; } }

		RegionizationResult regions;
		public RegionizationResult RegionizationResult { get => regions; set { regions = value; } }
		
		TracingResult traced;
		public TracingResult TracingResult { get => traced; set { traced = value; } }
		
		FittingResult fitted;
		public FittingResult FittingResult { get => fitted; set { fitted = value; } }


		public Vectorizer()
		{

		}

		public void Vectorize()
		{
			Regionize();
			Trace();
			Simplify();
			FitCurves();
		}



		public void Regionize()
		{

			Regionizer<D> posterizer = new ()
			{
				Source = Source,
				Properties = Properties
			};

			regions = posterizer.Regionize();
		}

		public void Trace()
		{
			Tracer posterizer = new Tracer()
			{
				RegionizationResult = RegionizationResult,
				Properties = Properties
			};

			traced = posterizer.Trace();
		}

		private void Simplify()
		{
			foreach (var edge in traced.Edges)
			{
				var start = new Vector2(edge.Start.X, edge.Start.Y);
				var end = new Vector2(edge.End.X, edge.End.Y);
				var points = DouglasPeucker.Simplify(start, edge.Points, end, Properties.FittingDistance).ToArray();
				edge.SimplifiedPoints = points;
			}
		}

		public void FitCurves()
		{
			SimpleCurveFitter scf = new SimpleCurveFitter();
			scf.TracingResult = traced;
			scf.Properties = Properties;
			scf.TracingResult = traced;
			fitted = scf.Fit();
		}


		public void SaveSVG(System.IO.Stream stream)
		{
			if (fitted != null)
			{
				XmlWriter writer = XmlWriter.Create(stream);

				writer.WriteStartElement("svg", "http://www.w3.org/2000/svg");
				writer.WriteAttributeString("viewBox", "0 0 " + source.Width.ToString() + " " + source.Height.ToString());
				foreach (var region in fitted.Regions)
				{
					writer.WriteStartElement("path");
					var mean = regions.Regions[region.Index].Mean;
					writer.WriteAttributeString("fill", "rgba(" + (byte)mean[2] + "," + (byte)mean[1] + "," + (byte)mean[0] + "," + (byte)mean[3] + ")");

					StringBuilder sb = new StringBuilder();
					sb.Append("M");
					sb.Append(region.Start.X.ToString(CultureInfo.InvariantCulture));
					sb.Append(',');
					sb.Append(region.Start.Y.ToString(CultureInfo.InvariantCulture));
					foreach (var elem in region.Path)
					{
						sb.Append(' ');
						switch (elem.ElementType)
						{
							case PathElementType.Line:
								sb.Append("L");
								sb.Append(elem.Coords[0].X.ToString(CultureInfo.InvariantCulture));
								sb.Append(',');
								sb.Append(elem.Coords[0].Y.ToString(CultureInfo.InvariantCulture));
								break;
							case PathElementType.Quadratic:
								break;
							case PathElementType.Cubic:
								sb.Append("C");
								sb.Append(elem.Coords[0].X.ToString(CultureInfo.InvariantCulture));
								sb.Append(',');
								sb.Append(elem.Coords[0].Y.ToString(CultureInfo.InvariantCulture));
								sb.Append(' ');
								sb.Append(elem.Coords[1].X.ToString(CultureInfo.InvariantCulture));
								sb.Append(',');
								sb.Append(elem.Coords[1].Y.ToString(CultureInfo.InvariantCulture));
								sb.Append(' ');
								sb.Append(elem.Coords[2].X.ToString(CultureInfo.InvariantCulture));
								sb.Append(',');
								sb.Append(elem.Coords[2].Y.ToString(CultureInfo.InvariantCulture));
								break;
						}
					}
					writer.WriteAttributeString("d", sb.ToString());
					writer.WriteEndElement();
				}
				writer.WriteEndElement();
				writer.Flush();
			}
		}
	}
}

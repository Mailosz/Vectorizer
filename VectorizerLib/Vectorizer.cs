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
				PosterizationResult = RegionizationResult,
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
			scf.Properties = Properties;
			fitted = scf.Fit(traced);

			//CurveFitter fitter = new CurveFitter()
			//{
			//	TracingResult = TracingResult,
			//	Properties = Properties
			//};
			//fitted = fitter.FitCurves();
		}


		public void SaveSVG(System.IO.Stream stream)
		{
			if (fitted != null)
			{
				XmlWriter writer = XmlWriter.Create(stream);
				writer.WriteStartDocument();

				writer.WriteStartElement("svg");
				writer.WriteAttributeString("viewBox", "0 0 " + source.Width.ToString() + " " + source.Height.ToString());
				foreach (var region in fitted.Regions)
				{
					writer.WriteStartElement("path");
					var mean = regions.Regions[region.Index].Mean;
					string c = BitConverter.ToString((from m in mean select (byte)m).ToArray()).Remove('-');
					writer.WriteAttributeString("fill", "#" + c);

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
					}writer.WriteAttributeString("d", sb.ToString());
				}
				writer.WriteEndElementAsync();
			}
		}
	}
}

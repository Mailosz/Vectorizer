using System;

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

		private void Trace()
		{
			Tracer posterizer = new Tracer()
			{
				PosterizationResult = RegionizationResult,
				Properties = Properties
			};

			traced = posterizer.Trace();
		}

		private void FitCurves()
		{
			CurveFitter fitter = new CurveFitter()
			{
				TracingResult = TracingResult,
				Properties = Properties
			};

			fitted = fitter.FitCurves();
		}


	}
}

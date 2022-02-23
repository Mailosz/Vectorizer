using Accord.Math;
using Accord.Math.Decompositions;
using System;

namespace VectorizerLib
{
	internal static class MatrixHelper
	{

	}

	public class RgbaByteRegionData : RegionDataBase, IRegionData
	{
		public double[] mean = new double[4];
		public long[] cov2 = new long[4];
		public double[,] cov = new double[4, 4];

		public long[,] computedCov = new long[4, 4];
		public double testValue;
		public double[] eigenVector;
		public double eigenValue;

		public override void ComputeValues()
		{
			for (int i = 0; i < 4; i++)
			{
				Mean[i] = (float)(mean[i]);
			}

			Color = System.Drawing.Color.FromArgb((int)Mean[3], (int)Mean[0], (int)Mean[1], (int)Mean[2]);

			/*for (int a = 0; a < 4; a++)
			{
				for (int b = 0; b < 4; b++)
				{
					long covab = cov[a, b];
					long meana = mean[a];
					long meanb = mean[b];
					computedCov[a,b] = covab - (meana * meanb);
					//computedCov[a,b] = cov[a,b] - (mean[a] * mean[b]);
				}
			}*/


			double[,] matrix = new double[4, 4];

			for (int x = 0; x < 4; x++) 
			{
				for (int y = 0; y < 4; y++)
				{
					matrix[x, y] = cov[x, y] / Area;
				}
			}

			bool isSymmetric = Matrix.IsSymmetric(matrix);

			EigenvalueDecomposition decompositor = new EigenvalueDecomposition(matrix, true, true, true);
			eigenVector = decompositor.Eigenvectors.GetRow(0);
			eigenValue = decompositor.RealEigenvalues[0];

			testValue = 0;
			for (int i = 0; i < 4; i++)
			{
				testValue += mean[i] * eigenVector[i];
			}

			SplitValue = eigenValue * Math.Log(area, 1.2);
		}

		public override void CopyValuesFrom(IRegionData region)
		{
			this.Mean = region.Mean;
			this.SplitValue = region.SplitValue;
			this.X1 = region.X1;
			this.X2 = region.X2;
			this.Y1 = region.Y1;
			this.Y2 = region.Y2;
			this.Start = region.Start;
			this.Area = region.Area;
		}
	}
}
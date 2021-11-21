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
		public long[] mean = new long[4];
		public long[] cov2 = new long[4];
		public long[,] cov = new long[4, 4];

		public double[] computedMean = new double[4];
		public long[,] computedCov = new long[4, 4];
		public double testValue;
		public double[] eigenVector;
		public double eigenValue;

		public override void ComputeValues()
		{
			for (int i = 0; i < 4; i++)
			{
				computedMean[i] = (double)mean[i] / (double)area;
			}

			for (int a = 0; a < 4; a++)
			{
				for (int b = 0; b < 4; b++)
				{
					computedCov[a,b] = cov[a,b] - (mean[a] * mean[b]);
				}
			}


			double[,] matrix = new double[4, 4];

			for (int x = 0; x < 4; x++) 
			{
				for (int y = 0; y < 4; y++)
				{
					matrix[x, y] = computedCov[x, y];
				}
			}

			bool isSymmetric = Matrix.IsSymmetric(matrix);

			EigenvalueDecomposition decompositor = new EigenvalueDecomposition(matrix, true, true, true);
			eigenVector = decompositor.Eigenvectors.GetRow(0);
			eigenValue = decompositor.RealEigenvalues[0];

			testValue = 0;
			for (int i = 0; i < 4; i++)
			{
				testValue += computedMean[i] * eigenVector[i];
			}

			SplitValue = eigenValue;
		}
	}
}
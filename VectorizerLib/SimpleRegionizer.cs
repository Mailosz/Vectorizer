using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorizerLib
{


	class SimpleRegionizer<RegionData> where RegionData : class, IRegionData
	{
		VectorizerProperties properties;
		public VectorizerProperties Properties { get => properties; set { properties = value; } }

		IRasterSource<RegionData> source;
		public IRasterSource<RegionData> Source { get => source; set { source = value; } }


		ushort[] board;
		ushort lastId;
		RegionData[] regionsArray;
		List<RegionData> regionsList;
		int width;

		double maxDiff;
		ISourceRegionIterator<RegionData> iterator;
		int passesCount;

		internal RegionizationResult Regionize()
		{
			initialize();
			firstPass();
			bool keepOn = true;
			do { keepOn = singlePass(); } while (
				(passesCount < properties.RegionizationMinimumSteps ||
				maxDiff > properties.RegionizationTreshold ||
				passesCount < properties.RegionizationMinimumSteps)
				&& keepOn
				);
			finalPass();

			return new RegionizationResult()
			{
				Width = source.Width,
				Height = source.Height,
				Board = board,
				RegionCount = 4096
			};
		}

		private void initialize()
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (properties == null) throw new ArgumentNullException(nameof(properties));

			regionsArray = new RegionData[ushort.MaxValue];
			regionsList = new List<RegionData> (ushort.MaxValue);

			board = new ushort[source.Length];
			iterator = source.CreateRegionIterator();

			width = source.Width;

			passesCount = 0;
		}

		private void firstPass()
		{
			//initial region
			var region = createRegion();
			//full image
			region.X1 = 0; region.Y1 = 0; region.X2 = source.Width - 1; region.Y2 = source.Height - 1;


			iterator.ResetIterator(region);
			do
			{
				iterator.AppendCurrentPixelValuesToRegionData(region);
			}
			while (iterator.Next());

			region.ComputeValues();
		}


		private bool singlePass()
		{
			var region = getNextRegion();

			if (region != null)
			{
				divideRegion(region, iterator);
				passesCount++;
				return true;
			}
			else
			{
				return false;
			}
		}

		private void finalPass()
		{
			//TODO: anything
		}

		/// <summary>
		/// Returns region with highest eigenvalue
		/// </summary>
		private RegionData getNextRegion()
		{
			double maxValue = double.NegativeInfinity;
			RegionData region = null;
			foreach (var r in regionsList)
			{
				if (!r.IsFinal && r.SplitValue > maxValue)
				{
					maxValue = r.SplitValue;
					region = r;
				}
			}

			maxDiff = maxValue;

			return region;
		}

		/// <summary>
		/// Divides a region in two. Resets passed iterator to specified region before starting. Allows to choose iterator for concurrent runs.
		/// </summary>
		/// <param name="region"></param>
		/// <param name="iterator"></param>
		private void divideRegion(RegionData region, ISourceRegionIterator<RegionData> iterator)
		{
			var rAbove = createRegion();
			var rBelow = createRegion();

			//TODO: instead of creating two new regions only create one

			iterator.ResetIterator(region);
			do
			{
				if (board[iterator.GetPosition()] == region.Id)
				{
					switch (iterator.CheckCurrentPixel(region))
					{
						case PixelValue.Above:
							iterator.AppendCurrentPixelValuesToRegionData(rAbove);
							board[iterator.GetPosition()] = rAbove.Id;
							break;
						case PixelValue.Below:
							iterator.AppendCurrentPixelValuesToRegionData(rBelow);
							board[iterator.GetPosition()] = rBelow.Id;
							break;
						default: throw new Exception("Illegal state");
					};
				}
			}
			while (iterator.Next());

			if (rAbove.Area == 0)
			{
				rBelow.IsFinal = true;
				rBelow.ComputeValues();
				removeRegion(rAbove);
			}
			else if (rBelow.Area == 0)
			{
				rAbove.IsFinal = true;
				rAbove.ComputeValues();
				removeRegion(rBelow);
			}
			else
			{
				rAbove.ComputeValues();
				finishRegion(rAbove);
				rBelow.ComputeValues();
				finishRegion(rBelow);
			}


			removeRegion(region);

			void finishRegion(RegionData rdata)
			{
				if (rdata.X2 < rdata.X1) rdata.X2 = rdata.X1;
				if (rdata.Y2 < rdata.Y1) rdata.Y2 = rdata.Y1;

				rdata.X1 += region.X1;
				rdata.X2 += region.X1;
				rdata.Y1 += region.Y1;
				rdata.Y2 += region.Y1;
			}
		}


		private RegionData createRegion()
		{
			if (regionsList.Count == regionsList.Capacity) throw new IndexOutOfRangeException("Too many regions");

			while (regionsArray[lastId] != null)
			{
				lastId++;
			}

			RegionData region = source.CreateRegionData();
			region.Id = lastId;
			// TODO: CAUTION, TESTS
			region.X1 = int.MaxValue; region.Y1 = int.MaxValue; region.X2 = 0; region.Y2 = 0;
			//
			regionsArray[lastId] = region;
			regionsList.Add(region);
			return region;
		}

		private void removeRegion(RegionData region)
		{
			regionsArray[region.Id] = null;
			regionsList.Remove(region);
		}


	}


}

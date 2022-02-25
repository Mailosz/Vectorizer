using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorizerLib
{

	class Regionizer<RegionData> where RegionData : class, IRegionData
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
			int cleanCount = 10;
			int cleanStart = 10;
			do 
			{ 
				keepOn = singlePass();
				if (--cleanCount == 0)
				{
					cleanStart *= 2;
					cleanCount = cleanStart;
					joinPass();
				}
			} 
			while (
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
				Regions = regionsList.ToDictionary((rd) => rd.Id, (rd) => (IRegionData)rd),
				RegionCount = regionsCount,
				PeakCov = maxDiff,
				Steps = passesCount,
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
			var region = createRegion(0);
			//full image
			region.X1 = 0; region.Y1 = 0; region.X2 = source.Width - 1; region.Y2 = source.Height - 1;


			iterator.ResetIterator(region);
			do
			{
				iterator.AppendPixelLocation(region);
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
				if ((region.SplitValue < Properties.RegionizationTreshold && passesCount > Properties.RegionizationMinimumSteps) || passesCount > Properties.RegionizationMaximumSteps)
				{
					return false;
				}

				divideRegion(region, iterator);
				passesCount++;
				return true;
			}
			else
			{
				return false;
			}
		}

		private void joinPass()
		{
			for (int i = 0; i < regionsList.Count; i++)
			{
				var region = regionsList[i];
				if (!tryFindClosestNeighbor(region, out ushort selectedNeighbor, out float minDiff, out ushort[] neighbors)) continue;
				var neighbor = regionsArray[selectedNeighbor];


				if (minDiff < Properties.JoiningTreshold)
				{
					if (region.Area < neighbor.Area)
					{
						joinRegions(neighbor, region, neighbors);
						i--;
					}
					else
					{
						joinRegions(region, neighbor, neighbor.GetNeighbors().ToArray());
						if (selectedNeighbor < i) { i--; }
					}
				}

			}
		}

		private void finalPass()
		{
			for (int i = 0; i < regionsList.Count;i++ )
			{
				if (regionsList[i].Area < Properties.RegionMinimumArea)
				{
					if (eraseRegion(regionsList[i]))
						i--;
					continue;
				}
			}

			
			//TODO: anything
		}

		/// <summary>
		/// Adds region to nearest neighbor
		/// </summary>
		/// <param name="rr"></param>
		private bool eraseRegion(RegionData rr)
		{
			//find region to append this region to

			
			
			
			
			if (!tryFindClosestNeighbor(rr, out ushort selectedNeighbor, out _, out var neighbors)) return false;
			var neighbor = regionsArray[selectedNeighbor]; 

			joinRegions(neighbor, rr, neighbors);

			return true;
		}

		private bool tryFindClosestNeighbor(RegionData region, out ushort neighbor, out float minDiff, out ushort[] neighbors)
		{
			neighbors = region.GetNeighbors().ToArray();
			neighbor = 0;
			minDiff = float.PositiveInfinity;
			if (neighbors.Length == 0) return false;

			int selectedNeighbor = 0;
			for (int n = 0; n < neighbors.Length; n++)
			{
				float diff = 0f;
				for (int i = 0; i < region.Mean.Length; i++)
				{
					diff += MathF.Abs(region.Mean[i] - regionsArray[neighbors[n]].Mean[i]);
				}

				if (diff < minDiff)
				{
					minDiff = diff;
					selectedNeighbor = n;
				}
			}
			neighbor = neighbors[selectedNeighbor];
			return true;
		}

		/// <summary>
		/// Joins two regions and leaves only the first
		/// </summary>
		/// <param name="region"></param>
		/// <param name="joined"></param>
		/// <exception cref="NotImplementedException"></exception>
		private void joinRegions(RegionData region, RegionData joined, ushort[] neighbors)
		{
			foreach (var n in neighbors)
			{
				regionsArray[n].RemoveNeighbor(joined.Id);
				if (n != region.Id)
				{
					regionsArray[n].AddNeighbor(region.Id);

					region.AddNeighbor(n);
				}
			}

			iterator.ResetIterator(joined);
			if (joined.Area == 1)
			{
				board[iterator.GetPosition()] = region.Id;
				iterator.AppendPixelLocation(region);
				iterator.AppendCurrentPixelValuesToRegionData(region);
			}
			else
			{

				do
				{
					if (board[iterator.GetPosition()] == joined.Id)
					{
						board[iterator.GetPosition()] = region.Id;
						iterator.AppendPixelLocation(region);
						iterator.AppendCurrentPixelValuesToRegionData(region);
					}
				}
				while (iterator.Next());
			}

			region.ComputeValues();
			finishRegion(region);
			removeRegion(joined);
		}

		void finishRegion(RegionData rdata)
		{
			if (rdata.X2 < rdata.X1) rdata.X2 = rdata.X1;
			if (rdata.Y2 < rdata.Y1) rdata.Y2 = rdata.Y1;
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
			var rAbove = createRegion(region.Times);
			var rBelow = createRegion(region.Times);

			bool anybelow = false, anyabove = false;

			//TODO: instead of creating two new regions only create one -nah

			iterator.ResetIterator(region);
			do
			{
				if (board[iterator.GetPosition()] == region.Id)
				{
					switch (iterator.CheckCurrentPixel(region))
					{
						case PixelValue.Above:
							//iterator.AppendPixelLocation(rAbove);
							anyabove = true;
							board[iterator.GetPosition()] = rAbove.Id;
							break;
						case PixelValue.Below:
							//iterator.AppendPixelLocation(rBelow);
							anybelow = true;
							board[iterator.GetPosition()] = rBelow.Id;
							break;
						default: throw new Exception("Illegal state");
					};
				}
			}
			while (iterator.Next());

			if (!anyabove)
			{
				rBelow.IsFinal = true;
				rBelow.CopyValuesFrom(region);

				foreach (var n in region.GetNeighbors())
				{
					var neighbor = regionsArray[n];
					neighbor.AddNeighbor(rBelow.Id);
					rBelow.AddNeighbor(n);
				}
				removeRegion(rAbove);
			}
			else if (!anybelow)
			{
				rAbove.IsFinal = true;
				rAbove.CopyValuesFrom(region);

				foreach (var n in region.GetNeighbors())
				{
					var neighbor = regionsArray[n];
					neighbor.AddNeighbor(rAbove.Id);
					rAbove.AddNeighbor(n);
				}
				removeRegion(rBelow);
			}
			else
			{
				finishRegion(rAbove);
				finishRegion(rBelow);
				var regions = floodFill(region, rBelow, rAbove);

				removeRegion(rAbove);
				removeRegion(rBelow);

				foreach (var frd in regions)
				{
					frd.Region.ComputeValues();

					finishRegion(frd.Region);
				}


			}


			removeRegion(region);

		}


		int regionsCount = 0;
		private RegionData createRegion(int times)
		{
			if (regionsList.Count == ushort.MaxValue-1) throw new IndexOutOfRangeException("Too many regions");

			while (regionsArray[lastId] != null)
			{
				lastId++;
				if (lastId == ushort.MaxValue)
				{
					lastId = 0;
				}
			}

			RegionData region = source.CreateRegionData();
			region.Id = lastId;
			region.Times = times + 1;
			if (lastId == 15548)
			{
				Console.WriteLine("FOUND");
			}
			// TODO: CAUTION, TESTS
			region.X1 = int.MaxValue; region.Y1 = int.MaxValue; region.X2 = 0; region.Y2 = 0;
			//
			regionsArray[lastId] = region;
			regionsList.Add(region);
			regionsCount++;
			return region;
		}

		/// <summary>
		/// Removes region and references to it from its neighbors
		/// </summary>
		/// <param name="region"></param>
		private void removeRegion(RegionData region)
		{
			regionsArray[region.Id] = null;
			regionsList.Remove(region);
			regionsCount--;

			foreach (var n in region.GetNeighbors())
			{
				var neighbor = regionsArray[n];
				neighbor.RemoveNeighbor(region.Id);
			}
		}


		//
		// flood fill
		//

		struct Seed
		{
			public int Start { get; set; }
			public int End { get; set; }
		}

		class FloodRegionData
		{
			public RegionData Region { get; set; }
		}


		private List<FloodRegionData> floodFill(RegionData oldregion, RegionData region1, RegionData region2)
		{
			int line = oldregion.Y1;
			int end = oldregion.Y2 * width + oldregion.X2;
			int start = oldregion.Y1 * width + oldregion.X1;
			int rw = oldregion.X2 - oldregion.X1 + 1;

			RegionData currentRegion = null;

			int lineEnd = start + rw;
			int searchpos = start;

			List<FloodRegionData> nRegions = new List<FloodRegionData>(64);
			Queue<Seed> seeds = new Queue<Seed>(oldregion.Y2 - oldregion.Y1 + 5);

			while (findBeginning())
			{
				int seedstart = searchpos;
				//creating new region
				FloodRegionData frd = new FloodRegionData()
				{
					Region = createRegion(oldregion.Times),
				};
				frd.Region.Start = searchpos;
				nRegions.Add(frd);
				paintPixel(searchpos, frd);
				if (searchpos != start) checkForNeighborsHorizontally(searchpos);
				checkForNeighborsVertically(searchpos);

				//selecting first line
				for (searchpos++; searchpos < lineEnd; searchpos++)
				{
					if (board[searchpos] == currentRegion.Id)
					{
						paintPixel(searchpos, frd);
						checkForNeighborsVertically(searchpos);
					}
					else
					{
						addNeighbors(frd.Region.Id, board[searchpos]);
						break;
					}

				}
				//creating first seed
				addSeed(seedstart, searchpos, 1);

				while (seeds.TryDequeue(out Seed seed))
				{
					processSeed(seed, frd);
				}
			}

			return nRegions;

			//
			// inline methods
			//
			void paintPixel(int pos, FloodRegionData frd)
			{
				board[pos] = frd.Region.Id;
				int y = Math.DivRem(pos, width, out int x);
				frd.Region.AppendPixelLocation(x, y);
				iterator.AppendPixelValuesToRegionData(frd.Region, pos);
			}

			bool findBeginning()
			{
				while (true)
				{
					if (searchpos >= lineEnd)
					{
						//new line
						start += width;
						if (start > end)
						{
							return false;
						}
						searchpos = start;
						lineEnd = start + rw;
						line++;
						continue;
					}

					if (board[searchpos] == region1.Id)
					{
						currentRegion = region1;
						return true;

					}
					else if (board[searchpos] == region2.Id)
					{
						currentRegion = region2;
						return true;
					}

					//checkForNeighborsAtCurrentSearchpos();
					searchpos++;
				}
			}

			void checkForNeighborsHorizontally(int pos)
			{
				if (pos > 0)
				{
					ushort r1 = board[pos - 1];
					ushort r2 = board[pos];
					if (r1 != r2 && regionsArray[r1] != null)
					{
						addNeighbors(r1, r2);
					}
				}
			}

			void checkForNeighborsVertically(int pos)
			{
				if (pos >= width)
				{
					ushort r1 = board[pos - width];
					ushort r2 = board[pos];
					if (r1 != r2 && regionsArray[r1] != null)
					{
						addNeighbors(r1, r2);
					}
				}
			}

			///

			void addNeighbors(ushort r1, ushort r2)
			{
				//only checking if r2is not null, r1 should alway be not null
				if (r1 == r2)
				{
					Console.WriteLine("EQ");
				}

				if (regionsArray[r2] != null)
				{
					regionsArray[r1].AddNeighbor(r2);
					regionsArray[r2].AddNeighbor(r1);
				}
			}


			void addSeed(int seedstart, int seedend, int direction){
				Seed seed;
				if (direction == 1)
				{
					seedstart += width;
					seedend += width;
					if (seedend > end) // out of bounds
					{
						return;
					}
					seed = new Seed()
					{
						Start = seedstart,
						End = seedend,
					};
				} 
				else
				{
					seedstart -= width;
					seedend -= width;
					if (seedstart < start) // out of bounds
					{
						return;
					}
					seed = new Seed()
					{
						Start = seedstart,
						End = seedend * -1,
					};
				}
				seeds.Enqueue(seed);
			}

			void processSeed(Seed seed, FloodRegionData frd)
			{
				int direction = Math.Sign(seed.End);
				int seedend = Math.Abs(seed.End);
				int minimum = seed.Start - seed.Start % width;

				int pos, seedstart;
				if (board[seed.Start] == currentRegion.Id)
				{
					paintPixel(seed.Start, frd);
					//left side
					pos = seed.Start - 1;
					
					while (pos >= minimum)
					{
						if (board[pos] == currentRegion.Id)
						{
							paintPixel(pos, frd);
						}
						else
						{
							addNeighbors(frd.Region.Id, board[pos]);
							break;
						}
						pos--;
					}
					seedstart = pos + 1;
					//
					if (seedstart < seed.Start - 1)
					{
						addSeed(seedstart, seed.Start - 1, direction * -1);
					}
					pos = seed.Start + 1;
				}
				else //searching for the beginning of matching pixels to fill
				{
					pos = seed.Start + 1;
					while (true)
					{
						if (pos >= seedend)
						{
							// no pixels to fill - dead end
							return;
						}
						if (board[pos] == currentRegion.Id)
						{
							paintPixel(pos, frd);
							seedstart = pos;
							pos++;
							break;
						}
						else if (frd.Region.Id != board[pos]) // different region, make them neighbors
						{
							addNeighbors(frd.Region.Id, board[pos]);
						}
						pos++;
					}
				}
				
				//inside
				while (pos < seedend)
				{
					if (board[pos] == currentRegion.Id)
					{
						paintPixel(pos, frd);
					}
					else
					{
						addNeighbors(frd.Region.Id, board[pos]);
						addSeed(seedstart, pos - 1, direction);
						while (true)
						{
							pos++;
							if (pos >= seedend)
							{
								//that's all in this line
								return;
							}
							if (board[pos] == currentRegion.Id)
							{
								seedstart = pos;
								paintPixel(pos, frd);
								break;
							}
							else if (frd.Region.Id != board[pos]) // different region, make them neighbors
							{
								addNeighbors(frd.Region.Id, board[pos]);
							}
						}
					}
					pos++;
				}

				//right side
				int maximum = minimum + width;
				while (pos < maximum)
				{
					if (board[pos] == currentRegion.Id)
					{
						paintPixel(pos, frd);
					}
					else
					{
						addNeighbors(frd.Region.Id, board[pos]);
						break;
					}
					pos++;
				}

				addSeed(seedstart, pos - 1, direction);
				if (pos > seedend + 1)
				{
					addSeed(seedend + 1, pos, direction * -1);
				}
			}
		}




	}


}

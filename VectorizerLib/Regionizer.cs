﻿using System;
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
							//iterator.AppendCurrentPixelValuesToRegionData(rAbove);
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


		//
		// flood fill
		//

		class FloodFiller
		{
			public RegionData Region { get; set; }
			public Queue<Seed> Seeds { get; set; }
			public int SearchPos { get; set; }
		}

		struct Seed
		{
			public int Start { get; set; }
			public int End { get; set; }
		}

		class FloodRegionData
		{
			public RegionData Region { get; set; }
		}


		private void regionFill(RegionData region)
		{
			FloodFiller ff = new FloodFiller()
			{
				Region = region,
				Seeds = new Queue<Seed>(region.Y2 - region.Y1 + 5)
			};

			int end = region.Y2 * width + region.X2;
			int start = region.Y1 * width + region.X1;
			int rw = region.X2 - region.X1 + 1;

			int lineEnd = start + rw;
			int searchpos = start;

			List<FloodRegionData> nRegions = new List<FloodRegionData>(16);
			Queue<Seed> seeds = new Queue<Seed>(region.Y2 - region.Y1 + 5);

			while (findBeginning())
			{
				int seedstart = searchpos;
				//creating new region
				FloodRegionData frd = new FloodRegionData()
				{
					Region = createRegion(),
				};
				nRegions.Add(frd);
				board[searchpos] = frd.Region.Id;

				//selecting first line
				for (searchpos++; searchpos < lineEnd; searchpos++)
				{
					if (board[searchpos] == region.Id)
					{
						paintPixel(searchpos, frd);
					}
					else
					{
						
						break;
					}
				}
				//creating first seed
				addSeed(seedstart, searchpos, 1);

				while (seeds.TryDequeue(out Seed seed))
				{
					processSeed(seed, frd);
				}

				//next pixel
				searchpos++;
			}

			//
			// inline methods
			//
			void paintPixel(int pos, FloodRegionData frd)
			{
				board[pos] = frd.Region.Id;
				frd.Region.Area++;
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
						continue;
					}

					if (board[searchpos] == region.Id)
					{
						return true;
					}

					searchpos++;
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
				int minimum = seed.Start % width;

				int pos, seedstart;
				if (board[seed.Start] == region.Id)
				{
					paintPixel(seed.Start, frd);
					//left side
					pos = seed.Start - 1;
					while (pos >= minimum)
					{
						if (board[pos] == region.Id)
						{
							paintPixel(pos, frd);
						}
						else
						{
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
				else
				{
					pos = seed.Start + 1;
					while (true)
					{
						if (pos >= seedend)
						{
							// no pixels to fill - dead end
							return;
						}
						if (board[pos] == region.Id)
						{
							paintPixel(pos, frd);
							seedstart = pos;
							pos++;
							break;
						}
						pos++;
					}
				}
				
				//inside
				while (pos < seedend)
				{
					if (board[pos] == region.Id)
					{
						paintPixel(pos, frd);
					}
					else
					{
						addSeed(seedstart, pos - 1, direction);
						while (true)
						{
							pos++;
							if (pos >= seedend)
							{
								//that's all in this line
								return;
							}
							if (board[pos] == region.Id)
							{
								seedstart = pos;
								paintPixel(pos, frd);
								break;
							}
						}
					}
					pos++;
				}

				//right side
				int maximum = minimum + width;
				while (pos < maximum)
				{
					if (board[pos] == region.Id)
					{
						paintPixel(pos, frd);
					}
					else
					{
						addSeed(seedstart, pos - 1, direction);
						if (pos > seedend + 1)
						{
							addSeed(seedend + 1, pos, direction * -1);
						}
					}
				}


			}
		}




	}


}

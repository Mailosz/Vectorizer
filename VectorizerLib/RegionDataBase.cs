﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace VectorizerLib
{
	public interface IRegionData
	{
		ushort Id { get; set; }
		int X1 { get; set; }
		int X2 { get; set; }
		int Y1 { get; set; }
		int Y2 { get; set; }
		int Start { get; set; }
		int Times { get; set; }
		long Area { get; set; }

		Color Color { get; set; }
		double SplitValue { get; set; }
		bool IsFinal { get; set; }

		float[] Mean { get; set; }

		void AddNeighbor(ushort id);
		void RemoveNeighbor(ushort id);
		bool isNeighbor(ushort id);
		IEnumerable<ushort> GetNeighbors();
		void ComputeValues();
		void AppendPixelLocation(int x, int y);
		void CopyValuesFrom(IRegionData region);
	}

	public abstract class RegionDataBase : IRegionData
	{
		ushort id;
		public ushort Id { get => id; set { id = value; } } 
		protected int x1, y1, x2, y2, start;
		public int X1 { get => x1; set { x1 = value; } }
		public int X2 { get => x2; set { x2 = value; } }
		public int Y1 { get => y1; set { y1 = value; } }
		public int Y2 { get => y2; set { y2 = value; } }
		public int Start { get => start; set { start = value; } }
		public int Times { get; set; }


		internal long area;
		public long Area { get => area; set { area = value; } }
		public double SplitValue { get; set; }

		//public BitArray neighbors = new BitArray(ushort.MaxValue);
		HashSet<ushort> neighbors = new HashSet<ushort>(32);

		public Color Color { get; set; }

		public float[] Mean { get; set; } = new float[4];

		public float[] Cov { get; set; } = new float[4];

		public bool IsFinal { get; set; }

		public abstract void ComputeValues();

		public void AppendPixelLocation(int x, int y)
		{
			this.area++;
			if (this.X1 > x) this.X1 = x;
			else if (this.X2 < x) this.X2 = x;
			if (this.Y1 > y) this.Y1 = y;
			else if (this.Y2 < y) this.Y2 = y;

			if (x == 0 && this.Id == 15548)
			{
				Console.WriteLine("FOUND");
			}
		}

		public override string ToString()
		{
			return "{Region " + Id.ToString() + "}";
		}

		public void AddNeighbor(ushort id)
		{
			//neighbors[id] = true;
			neighbors.Add(id);
			if (this.Id ==15548 && id == 21398)
			{
				Console.WriteLine("FOUND");
			}
		}

		public void RemoveNeighbor(ushort id)
		{
			//neighbors[id] = false;
			neighbors.Remove(id);
		}

		public bool isNeighbor(ushort id)
		{
			//return neighbors[id] == true;
			return neighbors.Contains(id);
		}

		public IEnumerable<ushort> GetNeighbors()
		{
			return neighbors;
			/*for (ushort i = 0; i < ushort.MaxValue; i++)
			{
				if (neighbors[i] == true)
				{
					yield return i;
				}
			}*/
		}

		public abstract void CopyValuesFrom(IRegionData region);
	}


}

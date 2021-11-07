using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace VectorizerLib
{
	class Edge
	{
		Joint Start;
		Joint End;
		IRegionData Left;
		IRegionData Right;
	}

	class Joint
	{
		int Position;

	}

	class Arrow
	{
		public int X { get; set; }
		public int Y { get; set; }
		public ushort Left { get; set; }
		public ushort Right { get; set; }
		public ArrowParams Params { get; set; }
		public TracedNode Node { get; set; }
	}

	[Flags]
	enum ArrowParams : byte
	{
		Left = 0b1000,
		Right = 0b1100,
		Up = 0b0000,
		Down = 0b0100,
		IsHorizontal = 0b1000,
		/// <summary>
		/// Forward means right or down, as opposed to left or up
		/// </summary>
		IsForward = 0b0100,
		HasBoth = 0b0010,
		HasOnlyRight = 0b0001,
	}

	public class Tracer
	{
		VectorizerProperties properties;
		public VectorizerProperties Properties { get => properties; set { properties = value; } }

		RegionizationResult poster;
		public RegionizationResult PosterizationResult { get => poster; set { poster = value; } }

		Queue<Arrow> arrows;
		List<Vector2> pointsList;
		Dictionary<ushort, TracedRegion> regions;
		List<TracedEdge> allEdges;

		internal TracingResult Trace()
		{
			if (poster == null) throw new ArgumentNullException(nameof(poster));
			if (properties == null) throw new ArgumentNullException(nameof(properties));

			initialize();

			

			while (arrows.TryDequeue(out Arrow arrow))
			{
				traceOneEdge(arrow);
			}

			return new TracingResult()
			{
				Regions = regions,
				Edges = allEdges,
			};
		}

		private TracedRegion getOrCreateTracedRegion(ushort id)
		{
			if (regions.TryGetValue(id, out TracedRegion region))
			{
				return region;
			}

			TracedRegion ntr = new TracedRegion();
			regions.Add(id, ntr);
			return ntr;
		}

		private void initialize()
		{
			arrows = new Queue<Arrow>(1024);
			pointsList = new List<Vector2>(4096);
			regions = new Dictionary<ushort, TracedRegion>(poster.RegionCount);
			allEdges = new List<TracedEdge>(1024);

			ushort cornerregion = poster.Board[0];

			TracedNode cornernode = new TracedNode();

			arrows.Enqueue(new Arrow()
			{
				X = 0,
				Y = 0,
				Left = 0,
				Right = cornerregion,
				Params = ArrowParams.Right | ArrowParams.HasOnlyRight,
				Node = cornernode,
			});

			arrows.Enqueue(new Arrow()
			{
				X = 0,
				Y = 0,
				Left = cornerregion,
				Right = 0,
				Params = ArrowParams.Down,
				Node = cornernode,
			});
		}

		private TracedEdge traceOneEdge(Arrow arrow)
		{
			TracedEdge edge = new TracedEdge()
			{
				Start = arrow.Node,
			};

			while (moveArrow(arrow))
			{
				//add point to pointlist
				pointsList.Add(new Vector2(arrow.X, arrow.Y));
			}

			edge.Points = pointsList.ToArray();
			edge.End = arrow.Node;

			return edge;

			//
			// inline functions
			//

			/// <summary>
			/// Checks whether the node already exists
			/// </summary>
			/// <param name="arrow"></param>
			/// <returns>True if node created (false means this point has already been reached, don't create new arrows)</returns>
			bool finishEdgeWithNode(Arrow arrow)
			{
				allEdges.Add(edge);

				//TODO: we are retrieving region twice - here, and at the end of the 
				TracedRegion leftregion, rightregion;
				TracedNode node;
				bool nnodecreated = false;
				if (arrow.Params.HasFlag(ArrowParams.HasBoth))
				{
					leftregion = getOrCreateTracedRegion(arrow.Left);
					rightregion = getOrCreateTracedRegion(arrow.Right);

					leftregion.Edges.Add(edge);
					rightregion.Edges.Add(edge);

					
					if (leftregion.Nodes.Count < rightregion.Nodes.Count)
					{
						nnodecreated = findOrCreateNode(arrow.X, arrow.Y, leftregion.Nodes, out node);
					}
					else
					{
						nnodecreated = findOrCreateNode(arrow.X, arrow.Y, rightregion.Nodes, out node);
					}

					if (nnodecreated)
					{
						leftregion.Nodes.Add(node);
						rightregion.Nodes.Add(node);
					}
				}
				else if (arrow.Params.HasFlag(ArrowParams.HasOnlyRight))
				{
					rightregion = getOrCreateTracedRegion(arrow.Right);

					rightregion.Edges.Add(edge);

					nnodecreated = findOrCreateNode(arrow.X, arrow.Y, rightregion.Nodes, out node);

					if (nnodecreated)
					{
						rightregion.Nodes.Add(node);
					}
				}
				else
				{
					leftregion = getOrCreateTracedRegion(arrow.Left);

					leftregion.Edges.Add(edge);

					nnodecreated = findOrCreateNode(arrow.X, arrow.Y, leftregion.Nodes, out node);

					if (nnodecreated)
					{
						leftregion.Nodes.Add(node);
					}
				}

				node.Edges.Add(edge);


				return nnodecreated;

				//
				// end of body
				//

				bool findOrCreateNode(int x, int y, List<TracedNode> list, out TracedNode node)
				{
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i].X == x && list[i].Y == y)
						{
							node = list[i];
							return false;
						}
					}
					node = new TracedNode()
					{
						X = x,
						Y = y,
					};
					return true;
				}
			}

			/// returns whether to continue or stop
			bool moveArrow(Arrow arrow)
			{
				if (arrow.Params.HasFlag(ArrowParams.IsHorizontal))
				{
					if (arrow.Params.HasFlag(ArrowParams.IsForward)) // going right
					{
						arrow.X++;// moving

						//wall check
						if (arrow.X == poster.Width)
						{
							if (arrow.Params.HasFlag(ArrowParams.HasBoth))//split at the edge of the board
							{
								if (finishEdgeWithNode(arrow))
								{
									//new arrow leftward
									arrows.Enqueue(new Arrow()
									{
										X = arrow.X,
										Y = arrow.Y,
										Left = arrow.Left,
										Params = createArrowParams(false, false, false, false),
										Node = arrow.Node,
									});

									//new arrow rightward
									arrows.Enqueue(new Arrow()
									{
										X = arrow.X,
										Y = arrow.Y,
										Right = arrow.Right,
										Params = createArrowParams(false, true, false, true),
										Node = arrow.Node,
									});
								}
							}
							else //corner
							{
								if (arrow.Params.HasFlag(ArrowParams.HasOnlyRight))
								{
									//turn right
									arrow.Params = createArrowParams(false, true, false, true);
								}
								else
								{
									//turn left
									arrow.Params = createArrowParams(false, false, false, false);
								}
							}
							return false;
						}
						else
						{
							return arrowRegionsCheck(true, true, arrow);
						}
					}
					else // going left
					{
						arrow.X--;// moving

						//wall check
						if (arrow.X == 0)
						{
							if (arrow.Params.HasFlag(ArrowParams.HasBoth))//split at the edge of the board
							{
								if (finishEdgeWithNode(arrow))
								{
									//new arrow leftward
									arrows.Enqueue(new Arrow()
									{
										X = arrow.X,
										Y = arrow.Y,
										Left = arrow.Left,
										Params = createArrowParams(false, true, false, false),
										Node = arrow.Node,
									});

									//new arrow rightward
									arrows.Enqueue(new Arrow()
									{
										X = arrow.X,
										Y = arrow.Y,
										Right = arrow.Right,
										Params = createArrowParams(false, false, false, true),
										Node = arrow.Node,
									});
								}
							}
							else //corner
							{
								if (arrow.Params.HasFlag(ArrowParams.HasOnlyRight))
								{
									//turn right
									arrow.Params = createArrowParams(false, false, false, true);
								}
								else
								{
									//turn left
									arrow.Params = createArrowParams(false, true, false, false);
								}
							}
							return false;
						}
						else
						{
							return arrowRegionsCheck(true, false, arrow);
						}
					}
				}
				else
				{
					if (arrow.Params.HasFlag(ArrowParams.IsForward)) // going down
					{
						arrow.Y++;// moving

						//wall check
						if (arrow.Y == poster.Height)
						{
							if (arrow.Params.HasFlag(ArrowParams.HasBoth))//split at the edge of the board
							{
								if (finishEdgeWithNode(arrow))
								{
									//new arrow leftward
									arrows.Enqueue(new Arrow()
									{
										X = arrow.X,
										Y = arrow.Y,
										Left = arrow.Left,
										Params = createArrowParams(true, true, false, false),
										Node = arrow.Node,
									});

									//new arrow rightward
									arrows.Enqueue(new Arrow()
									{
										X = arrow.X,
										Y = arrow.Y,
										Right = arrow.Right,
										Params = createArrowParams(true, false, false, true),
										Node = arrow.Node,
									});
								}
							}
							else //corner
							{
								if (arrow.Params.HasFlag(ArrowParams.HasOnlyRight))
								{
									//turn right
									arrow.Params = createArrowParams(true, false, false, true);
								}
								else
								{
									//turn left
									arrow.Params = createArrowParams(true, true, false, false);
								}
							}
							return false;
						}
						else
						{
							return arrowRegionsCheck(false, true, arrow);
						}
					}
					else // going up
					{
						arrow.Y--;// moving

						//wall check
						if (arrow.Y == 0)
						{
							if (arrow.Params.HasFlag(ArrowParams.HasBoth))//split at the edge of the board
							{
								if (finishEdgeWithNode(arrow))
								{
									//new arrow leftward
									arrows.Enqueue(new Arrow()
									{
										X = arrow.X,
										Y = arrow.Y,
										Left = arrow.Left,
										Params = createArrowParams(true, false, false, false),
										Node = arrow.Node,
									});

									//new arrow rightward
									arrows.Enqueue(new Arrow()
									{
										X = arrow.X,
										Y = arrow.Y,
										Right = arrow.Right,
										Params = createArrowParams(true, true, false, true),
										Node = arrow.Node,
									});
								}
							}
							else //corner
							{
								if (arrow.Params.HasFlag(ArrowParams.HasOnlyRight))
								{
									//turn right
									arrow.Params = createArrowParams(true, true, false, true);
								}
								else
								{
									//turn left
									arrow.Params = createArrowParams(true, false, false, false);
								}
							}
							return false;
						}
						else
						{
							return arrowRegionsCheck(false, false, arrow);
						}
					}
				}

				//
				// inline functions
				//

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				bool arrowRegionsCheck(bool horizontal, bool forward, Arrow arrow)
				{
					ushort left, right;
					void addLeftwardArrow()
					{
						arrows.Enqueue(new Arrow()
						{
							X = arrow.X,
							Y = arrow.Y,
							Left = arrow.Left,
							Right = left,
							Params = createArrowParams(!horizontal, horizontal != forward, true, false),
							Node = arrow.Node,
						});
					}
					void addForwardArrow()
					{
						arrows.Enqueue(new Arrow()
						{
							X = arrow.X,
							Y = arrow.Y,
							Left = left,
							Right = right,
							Params = createArrowParams(horizontal, forward, true, false),
							Node = arrow.Node,
						});
					}
					void addRightwardArrow()
					{
						arrows.Enqueue(new Arrow()
						{
							X = arrow.X,
							Y = arrow.Y,
							Left = right,
							Right = arrow.Right,
							Params = createArrowParams(!horizontal, horizontal == forward, true, false),
							Node = arrow.Node,
						});
					}
					int position = arrow.X + arrow.Y * poster.Width;
					if (arrow.Params.HasFlag(ArrowParams.HasBoth))
					{
						left = getNextLeftIndex(horizontal, forward, position);
						right = getNextRightIndex(horizontal, forward, position);

						if (right == arrow.Right)
						{
							if (left == arrow.Left) // continue
							{
								return true;
							}
							else
							{
								if (left == arrow.Right) // corner
								{
									//turn left
									arrow.Params = createArrowParams(!horizontal, horizontal != forward, true, false);
									return true;
								}
								else // change on the left
								{
									addLeftwardArrow();
									addForwardArrow();
									return false;
								}
							}
						}
						else
						{
							if (left == arrow.Left)
							{
								if (right == arrow.Left) // corner
								{
									//turn right
									arrow.Params = createArrowParams(!horizontal, horizontal == forward, true, false);
									return true;
								}
								else // change on the right
								{
									if (finishEdgeWithNode(arrow))
									{
										addForwardArrow();
										addRightwardArrow();
									}
									return false;
								}
							}
							else
							{
								if (left == right) // single region ahead
								{
									if (finishEdgeWithNode(arrow))
									{
										addLeftwardArrow();
										addRightwardArrow();
									}
									return false;
								}
								else // split into two regions ahead (4-way connection point)
								{
									if (finishEdgeWithNode(arrow))
									{
										addLeftwardArrow();
										addForwardArrow();
										addRightwardArrow();
									}
									return false;
								}
							}
						}
					}
					else if (arrow.Params.HasFlag(ArrowParams.HasOnlyRight)) // has only right side
					{
						right = getNextRightIndex(horizontal, forward, position);
						if (right == arrow.Right) //continue
						{
							return true;
						}
						else // new region ahead
						{
							if (finishEdgeWithNode(arrow))
							{
								//new arrow forward
								arrows.Enqueue(new Arrow()
								{
									X = arrow.X,
									Y = arrow.Y,
									Right = right,
									Params = createArrowParams(horizontal, forward, false, true),
									Node = arrow.Node,
								});

								//new arrow rightward
								arrows.Enqueue(new Arrow()
								{
									X = arrow.X,
									Y = arrow.Y,
									Left = right,
									Right = arrow.Right,
									Params = createArrowParams(!horizontal, horizontal == forward, true, false),
									Node = arrow.Node,
								});
							}

							return false;
						}
					}
					else // has only left side
					{
						left = getNextLeftIndex(horizontal, forward, position);
						if (left == arrow.Left) //continue
						{
							return true;
						}
						else // new region ahead
						{
							if (finishEdgeWithNode(arrow))
							{
								//new arrow leftward
								arrows.Enqueue(new Arrow()
								{
									X = arrow.X,
									Y = arrow.Y,
									Left = arrow.Left,
									Right = left,
									Params = createArrowParams(!horizontal, horizontal != forward, true, false),
									Node = arrow.Node,
								});

								//new arrow forward
								arrows.Enqueue(new Arrow()
								{
									X = arrow.X,
									Y = arrow.Y,
									Left = left,
									Params = createArrowParams(horizontal, forward, false, false),
									Node = arrow.Node,
								});
							}
							return false;
						}
					}
				}

			}
		}

		private void readArrow(Arrow arrow)
		{
			//TODO: find yet not found node
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ushort getNextLeftIndex(bool horizontal, bool forward, int position)
		{
			if (horizontal)
			{
				if (forward) //right
				{
					return poster.Board[position - poster.Width];
				}
				else // left
				{
					return poster.Board[position - 1];
				}
			}
			else
			{
				if (forward) // down
				{
					return poster.Board[position];
				}
				else // up
				{
					return poster.Board[position - poster.Width - 1];
				}
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ushort getNextRightIndex(bool horizontal, bool forward, int position)
		{
			if (horizontal)
			{
				if (forward) //right
				{
					return poster.Board[position];
				}
				else // left
				{
					return poster.Board[position - poster.Width - 1];
				}
			}
			else
			{
				if (forward) // down
				{
					return poster.Board[position - 1];
				}
				else // up
				{
					return poster.Board[position - poster.Width];
				}
			}
		}


		private ArrowParams createArrowParams(bool horizontal, bool forward, bool hasBothSides, bool hasRight)
		{
			return (ArrowParams)((horizontal ? 0b1000 : 0b0000) | (forward ? 0b0100 : 0b0000) | (hasBothSides ? 0b0010 : 0b0000) | (hasRight ? 0b0001 : 0b0000));
		}

		

	}
}

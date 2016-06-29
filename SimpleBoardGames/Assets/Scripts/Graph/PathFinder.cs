using System;
using System.Collections.Generic;
using UnityEngine;


namespace Graph
{
	/// <summary>
	/// Wraps A*/Djikstra. Can be used to find a path from point to point,
	///     or to just build out a search tree from a given point.
	/// There are two ways to specify an end for the search:
	///   1) a function that takes a node and returns whether it is an acceptable destination ("IsEndNodeFunc")
	///   2) a specific single destination ("End")
	/// If both "IsEndNodeFunc" and "End" are null, there is no specific destination (i.e. no A* heuristic).
	/// If both "IsEndNodeFunc" and "End" are not null, both are used for end points. "End" is used for the heuristic.
	/// </summary>
	/// <typeparam name="N">The type representing a node in the graph.</typeparam>
	public class PathFinder<N> where N : Node
	{
		public bool HasSpecificEnd { get { return End != null; } }
		public bool HasSeveralEnds { get { return IsEndNodeFunc != null; } }
		public bool HasAnEnd { get { return HasSpecificEnd || HasSeveralEnds; } }


		/// <summary>
		/// The most recently-calculated path between the start and end nodes.
		/// </summary>
		public List<N> CurrentPath = new List<N>();
		/// <summary>
		/// The current path tree -- every key Node indexes to the next Node to travel to
		/// in order to get back to the starting node as quick as possible.
		/// </summary>
		public Dictionary<N, N> PathTree = new Dictionary<N, N>();
		/// <summary>
		/// Nodes that have been added to the search space but not yet been checked.
		/// </summary>
		public IndexedPriorityQueue<Edge<N>> NodesToSearch = new IndexedPriorityQueue<Edge<N>>(true);
		/// <summary>
		/// Indexes every node in the path tree or search frontier by the cost to reach it from Start.
		/// </summary>
		public Dictionary<N, float> CostToMoveToNode = new Dictionary<N, float>();

		/// <summary>
		/// The graph to search.
		/// </summary>
		public Graph<N> Graph;

		public N Start;

		//At most one of these may be set to something other than null.
		public N End = null;
		public Func<N, bool> IsEndNodeFunc = null;

		/// <summary>
		/// Returns an Edge connecting the two given nodes.
		/// Used to construct certain edge-case edges during the pathing algorithm
		/// that may not represent a real connection.
		/// </summary>
		public Func<N, N, Edge<N>> MakeEdge;
	

		//Data for A*.
		private N finalDestination = null;


		/// <summary>
		/// Creates a new PathFinder.
		/// </summary>
		/// <param name="graph">The graph to search.</param>
		/// <param name="makeEdge">Constructs an Edge from the given start and end nodes.</param>
		public PathFinder(Graph<N> graph, Func<N, N, Edge<N>> makeEdge)
		{
			Graph = graph;

			Start = null;
			End = null;

			MakeEdge = makeEdge;
		}


		/// <summary>
		/// Builds a search tree moving outward from Start. Returns whether a valid end node was actually found.
		/// </summary>
		/// <param name="maxSearchCost">The maximum Edge search cost this pathfinder can search from start.
		/// This can be used to limit the graph search space.
		/// This is different from the heuristic cost!</param>
		/// <param name="onlySearchToDestination">If this is true, and the end Node is not null,
		/// this function will stop building the path tree when the destination is found.
		/// Otherwise, it will search the entire graph space within the max search cost.</param>
		/// <param name="setEnd">If true, sets this PathFinder's "End" field to the last node traversed by this algorithm.
		/// This can be used to see what the pather stopped at in case it never reached its goal.</param>
		public bool CalculatePathTree(float maxSearchCost, bool onlySearchToDestination, bool setEnd = false)
		{
			CurrentPath.Clear();
			PathTree.Clear();


			//Set up A*/Djikstra.

			finalDestination = null;

			List<N> considered = new List<N>();

			List<Edge<N>> connections = new List<Edge<N>>();

			CostToMoveToNode.Clear();
			Dictionary<N, float> getToNodeSearchCost = new Dictionary<N, float>();

			NodesToSearch.Clear();


			KeyValuePair<float, Edge<N>> closest;
			N closestN = null;
			float closestCost, tempCost,
					closestSearchCost, tempSearchCost;


			//Start searching from the source node.
			NodesToSearch.Push(MakeEdge(null, Start), 0.0f);
			PathTree.Add(Start, null);
			CostToMoveToNode.Add(Start, 0.0f);
			getToNodeSearchCost.Add(Start, 0.0f);
			considered.Add(Start);

			bool foundEnd = false;


			//While the search frontier is not empty, keep grabbing the nearest Node to search.
			while (!NodesToSearch.IsEmpty)
			{
				//Get the closest Node.
				closest = NodesToSearch.Pop();
				closestN = closest.Value.End;
				closestCost = closest.Key;
				closestSearchCost = getToNodeSearchCost[closestN];


				//Put it into the path.

				//If it was already in the path, then a shorter route to it has already been found.
				if (!PathTree.ContainsKey(closestN))
					PathTree.Add(closestN, closest.Value.Start);

				//If the target has been found, exit.
				if (HasSpecificEnd && closestN.IsEqualTo(End))
				{
					foundEnd = true;
					if (onlySearchToDestination)
					{
						break;
					}
				}
				if (HasSeveralEnds && IsEndNodeFunc(closestN))
				{
					finalDestination = closestN;
					if (setEnd)
					{
						End = closestN;
					}

					foundEnd = true;
					if (onlySearchToDestination)
					{
						break;
					}
				}
				if (getToNodeSearchCost[closestN] >= maxSearchCost)
				{
					continue;
				}


				//Now process all the connected nodes.


				//Grab connected nodes.
				connections.Clear();
				Graph.GetConnections(closestN, connections);

				//Go through each of them and add them to the search frontier.
				for (int i = 0; i < connections.Count; ++i)
				{
					//If a node is already on the search frontier, see if the calculated cost is more than this connections' cost.
					int contains = considered.IndexOf(connections[i].End);
					tempCost = closestCost + connections[i].GetTraversalCost(this);
					if (contains == -1)
					{
						tempSearchCost = closestSearchCost + connections[i].GetSearchCost(this);

						//If the search cost is small enough, add the edge to the search space.
						if (tempSearchCost <= maxSearchCost)
						{
							NodesToSearch.Push(connections[i], tempCost);
							CostToMoveToNode.Add(connections[i].End, tempCost);
							getToNodeSearchCost.Add(connections[i].End, tempSearchCost);
							considered.Add(connections[i].End);
						}
					}
					else if (tempCost < CostToMoveToNode[considered[contains]])
					{
						tempSearchCost = closestSearchCost + connections[i].GetSearchCost(this);

						//If the search cost is small enough, update the path tree.
						if (tempSearchCost <= maxSearchCost)
						{
							CostToMoveToNode[connections[i].End] = tempCost;
							getToNodeSearchCost[connections[i].End] = tempSearchCost;
							PathTree[connections[i].End] = connections[i].Start;
						}
					}
				}
			}

			return foundEnd;
		}

		/// <summary>
		/// Gets the End node, or the closest Node to End in the search space if End wasn't found.
		/// Assumes that End exists.
		/// </summary>
		private N GetDest()
		{
			if (End != null && PathTree.ContainsKey(End))
				return End;
			if (finalDestination != null) return finalDestination;


			//End was too far away from Start for the search tree to find, so get an available node closest to End.

			if (PathTree.Count == 0) return null;

			N bestEnd = null;
			float bestDist = Single.PositiveInfinity;
			float tempDist;

			foreach (N n in PathTree.Values)
			{
				tempDist = MakeEdge(n, End).GetTraversalCost(this);
				if (tempDist < bestDist)
				{
					bestDist = tempDist;
					bestEnd = n;
				}
			}

			return bestEnd;
		}
		/// <summary>
		/// Gets the path from Start to End, assuming the path tree has been computed.
		/// </summary>
		/// <returns>A list of the Nodes from Start to End, with the first element being Start
		/// and the last element being the closest to End the search tree could get.</returns>
		public void CalculatePath()
		{
			if (End == null)
			{
				End = GetDest();
			}

			CurrentPath.Clear();

			//Build backwards from the end to start, since that's how the path was stored.
			N counter = GetDest();
			if (counter == null)
			{
				CurrentPath.Add(Start);
				return;
			}
			while (counter.IsNotEqualTo(Start))
			{
				CurrentPath.Add(counter);
				counter = PathTree[counter];
			}

			//Add in the start node.
			CurrentPath.Add(Start);

			//Reverse the list to put it in the right order.
			CurrentPath.Reverse();
		}

		/// <summary>
		/// Recalculates the path from this PathFinder's start to its end.
		/// Assumes this pather has both a start and an end.
		/// Returns whether an end node was successfully found.
		/// </summary>
		/// <param name="maxSearchCost">Any nodes with a higher cost than this value will not be part of the search space.</param>
		public bool FindPath(float maxSearchCost = Single.PositiveInfinity)
		{
			bool foundEnd = CalculatePathTree(maxSearchCost, true, End == null);

			CalculatePath();

			return foundEnd;
		}
	}
}
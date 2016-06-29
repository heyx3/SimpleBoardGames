using System;
using System.Collections.Generic;

namespace Graph
{
	/// <summary>
	/// An element in a graph.
	/// </summary>
	public abstract class Node
	{
		public abstract bool IsEqualTo(Node other);
		public abstract bool IsNotEqualTo(Node other);

		public abstract override int GetHashCode();

		public sealed override bool Equals(object obj)
		{
			Node n = obj as Node;
			return n != null && n.IsEqualTo(this);
		}
	}


	/// <summary>
	/// A connection between two nodes, as well as the "cost" of the connection in a path.
	/// Edges provide two different search costs:
	///   1) The cost of actually traversing this edge; used to decide which path is the shortest.
	///   2) The cost of searching this edge; used to optionally limit the reach of the A* algorithm to a certain search cost.
	/// For example, the traversal cost of an edge in A* might be [edge length] + [edge's distance to destination],
	///     but the search cost would just be [edge length].
	/// </summary>
	public abstract class Edge<N> where N : Node
	{
		public N Start { get; protected set; }
		public N End { get; protected set; }

		public Edge(N start, N end)
		{
			Start = start;
			End = end;
		}

		/// <summary>
		/// Takes in the pather instance that is calling this function.
		/// </summary>
		public abstract float GetTraversalCost(PathFinder<N> pather);
		/// <summary>
		/// Takes in the pather instance that is calling this function.
		/// </summary>
		public abstract float GetSearchCost(PathFinder<N> pather);
	}


	/// <summary>
	/// A collection of nodes connected via edges.
	/// </summary>
	public interface Graph<N> where N : Node
	{
		/// <summary>
		/// Get all edges that start at the given Node, and put them into the given empty list.
		/// </summary>
		void GetConnections(N starting, List<Edge<N>> outEdgeList);
	}
}
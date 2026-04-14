using System;
using System.Collections.Generic;
using UnityEngine;

public static class SECTR_Graph
{
	public class Node : IComparable<Node>
	{
		public SECTR_Portal Portal;

		public SECTR_Sector Sector;

		public float CostPlusEstimate;

		public float Cost;

		public int Depth;

		public bool ForwardTraversal;

		public Node Parent;

		public int CompareTo(Node other)
		{
			if (CostPlusEstimate > other.CostPlusEstimate)
			{
				return 1;
			}
			if (CostPlusEstimate < other.CostPlusEstimate)
			{
				return -1;
			}
			return 0;
		}

		public static void ReconstructPath(List<Node> path, Node currentNode)
		{
			if (currentNode != null)
			{
				path.Insert(0, currentNode);
				ReconstructPath(path, currentNode.Parent);
			}
		}
	}

	private static List<SECTR_Sector> initialSectors = new List<SECTR_Sector>(4);

	private static List<SECTR_Sector> goalSectors = new List<SECTR_Sector>(4);

	private static SECTR_PriorityQueue<Node> openSet = new SECTR_PriorityQueue<Node>(64);

	private static Dictionary<SECTR_Portal, Node> closedSet = new Dictionary<SECTR_Portal, Node>(64);

	public static void DepthWalk(ref List<Node> nodes, SECTR_Sector root, SECTR_Portal.PortalFlags stopFlags, int maxDepth)
	{
		nodes.Clear();
		if (root == null)
		{
			return;
		}
		if (maxDepth == 0)
		{
			Node node = new Node();
			node.Sector = root;
			nodes.Add(node);
			return;
		}
		int count = SECTR_Sector.All.Count;
		for (int i = 0; i < count; i++)
		{
			SECTR_Sector.All[i].Visited = false;
		}
		Stack<Node> stack = new Stack<Node>(count);
		Node node2 = new Node();
		node2.Sector = root;
		node2.Depth = 1;
		stack.Push(node2);
		root.Visited = true;
		int num = 0;
		while (stack.Count > 0)
		{
			Node node3 = stack.Pop();
			nodes.Add(node3);
			num++;
			if (maxDepth >= 0 && node3.Depth > maxDepth)
			{
				continue;
			}
			int count2 = node3.Sector.Portals.Count;
			for (int j = 0; j < count2; j++)
			{
				SECTR_Portal sECTR_Portal = node3.Sector.Portals[j];
				if ((bool)sECTR_Portal && (sECTR_Portal.Flags & stopFlags) == 0)
				{
					SECTR_Sector sECTR_Sector = ((!(sECTR_Portal.FrontSector == node3.Sector)) ? sECTR_Portal.FrontSector : sECTR_Portal.BackSector);
					if ((bool)sECTR_Sector && !sECTR_Sector.Visited)
					{
						Node node4 = new Node();
						node4.Parent = node3;
						node4.Sector = sECTR_Sector;
						node4.Portal = sECTR_Portal;
						node4.Depth = node3.Depth + 1;
						stack.Push(node4);
						sECTR_Sector.Visited = true;
					}
				}
			}
		}
	}

	public static void BreadthWalk(ref List<Node> nodes, SECTR_Sector root, SECTR_Portal.PortalFlags stopFlags, int maxDepth)
	{
		nodes.Clear();
		if (root == null)
		{
			return;
		}
		if (maxDepth == 0)
		{
			Node node = new Node();
			node.Sector = root;
			nodes.Add(node);
			return;
		}
		int count = SECTR_Sector.All.Count;
		for (int i = 0; i < count; i++)
		{
			SECTR_Sector.All[i].Visited = false;
		}
		Queue<Node> queue = new Queue<Node>(count);
		Node node2 = new Node();
		node2.Sector = root;
		node2.Depth = 0;
		queue.Enqueue(node2);
		root.Visited = true;
		int num = 0;
		while (queue.Count > 0)
		{
			Node node3 = queue.Dequeue();
			nodes.Add(node3);
			num++;
			if (maxDepth >= 0 && node3.Depth >= maxDepth)
			{
				continue;
			}
			int count2 = node3.Sector.Portals.Count;
			for (int j = 0; j < count2; j++)
			{
				SECTR_Portal sECTR_Portal = node3.Sector.Portals[j];
				if ((bool)sECTR_Portal && (sECTR_Portal.Flags & stopFlags) == 0)
				{
					SECTR_Sector sECTR_Sector = ((!(sECTR_Portal.FrontSector == node3.Sector)) ? sECTR_Portal.FrontSector : sECTR_Portal.BackSector);
					if ((bool)sECTR_Sector && !sECTR_Sector.Visited)
					{
						Node node4 = new Node();
						node4.Parent = node3;
						node4.Sector = sECTR_Sector;
						node4.Portal = sECTR_Portal;
						node4.Depth = node3.Depth + 1;
						queue.Enqueue(node4);
						node3.Sector.Visited = true;
					}
				}
			}
		}
	}

	public static void FindShortestPath(ref List<Node> path, Vector3 start, Vector3 goal, SECTR_Portal.PortalFlags stopFlags)
	{
		path.Clear();
		openSet.Clear();
		closedSet.Clear();
		SECTR_Sector.GetContaining(ref initialSectors, start);
		SECTR_Sector.GetContaining(ref goalSectors, goal);
		int count = initialSectors.Count;
		for (int i = 0; i < count; i++)
		{
			SECTR_Sector sECTR_Sector = initialSectors[i];
			if (goalSectors.Contains(sECTR_Sector))
			{
				Node node = new Node();
				node.Sector = sECTR_Sector;
				path.Add(node);
				return;
			}
			int count2 = sECTR_Sector.Portals.Count;
			for (int j = 0; j < count2; j++)
			{
				SECTR_Portal sECTR_Portal = sECTR_Sector.Portals[j];
				if ((sECTR_Portal.Flags & stopFlags) == 0)
				{
					Node node2 = new Node();
					node2.Portal = sECTR_Portal;
					node2.Sector = sECTR_Sector;
					node2.ForwardTraversal = sECTR_Sector == sECTR_Portal.FrontSector;
					node2.Cost = Vector3.Magnitude(start - sECTR_Portal.transform.position);
					float num = Vector3.Magnitude(goal - sECTR_Portal.transform.position);
					node2.CostPlusEstimate = node2.Cost + num;
					openSet.Enqueue(node2);
				}
			}
		}
		while (openSet.Count > 0)
		{
			Node node3 = openSet.Dequeue();
			SECTR_Sector sECTR_Sector2 = ((!node3.ForwardTraversal) ? node3.Portal.FrontSector : node3.Portal.BackSector);
			if (!sECTR_Sector2)
			{
				continue;
			}
			if (goalSectors.Contains(sECTR_Sector2))
			{
				Node.ReconstructPath(path, node3);
				break;
			}
			int count3 = sECTR_Sector2.Portals.Count;
			for (int k = 0; k < count3; k++)
			{
				SECTR_Portal sECTR_Portal2 = sECTR_Sector2.Portals[k];
				if (!(sECTR_Portal2 != node3.Portal) || (sECTR_Portal2.Flags & stopFlags) != 0)
				{
					continue;
				}
				Node node4 = new Node();
				node4.Parent = node3;
				node4.Portal = sECTR_Portal2;
				node4.Sector = sECTR_Sector2;
				node4.ForwardTraversal = sECTR_Sector2 == sECTR_Portal2.FrontSector;
				node4.Cost = node3.Cost + Vector3.Magnitude(node4.Portal.transform.position - node3.Portal.transform.position);
				float num2 = Vector3.Magnitude(goal - node4.Portal.transform.position);
				node4.CostPlusEstimate = node4.Cost + num2;
				Node value = null;
				closedSet.TryGetValue(node4.Portal, out value);
				if (value != null && value.CostPlusEstimate < node4.CostPlusEstimate)
				{
					continue;
				}
				Node node5 = null;
				for (int l = 0; l < openSet.Count; l++)
				{
					if (openSet[l].Portal == node4.Portal)
					{
						node5 = openSet[l];
						break;
					}
				}
				if (node5 == null || !(node5.CostPlusEstimate < node4.CostPlusEstimate))
				{
					openSet.Enqueue(node4);
				}
			}
			if (!closedSet.ContainsKey(node3.Portal))
			{
				closedSet.Add(node3.Portal, node3);
			}
		}
	}

	public static string GetGraphAsDot(string graphName)
	{
		string text = "graph " + graphName;
		text += " {\n";
		text += "\tlayout=neato\n";
		foreach (SECTR_Portal item in SECTR_Portal.All)
		{
			text += "\t";
			text += item.GetInstanceID();
			text += " [";
			text = text + "label=" + item.name;
			text += ",shape=hexagon";
			text += "];\n";
		}
		foreach (SECTR_Sector item2 in SECTR_Sector.All)
		{
			text += "\t";
			text += item2.GetInstanceID();
			text += " [";
			text = text + "label=" + item2.name;
			text += ",shape=box";
			text += "];\n";
		}
		foreach (SECTR_Portal item3 in SECTR_Portal.All)
		{
			if ((bool)item3.FrontSector)
			{
				text += "\t";
				string text2 = text;
				text = text2 + item3.GetInstanceID() + " -- " + item3.FrontSector.GetInstanceID();
				text += ";\n";
			}
			if ((bool)item3.BackSector)
			{
				text += "\t";
				string text2 = text;
				text = text2 + item3.GetInstanceID() + " -- " + item3.BackSector.GetInstanceID();
				text += ";\n";
			}
		}
		return text + "\n}";
	}
}

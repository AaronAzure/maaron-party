using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
	public List<Node> nextNodes;
	private Vector3 offset = new Vector3(0,0.25f);
	public ushort nodeId;

	enum NodeSpace { blue, red, green, star }
	[SerializeField] private NodeSpace nodeSpace;

	private void OnDrawGizmosSelected() 
	{
		Gizmos.color = Color.magenta;
		foreach (Node node in nextNodes)
		{
			if (node != null)
			{
				Gizmos.DrawLine(transform.position + offset, node.transform.position + offset);
				Gizmos.DrawSphere(node.transform.position + offset, 0.2f);
			}
		}
	}

	/// <summary>
	/// Returns true if no event, else false
	/// </summary>
	/// <returns></returns>
	public bool GetNodeTraverseEffect()
	{
		switch (nodeSpace)
		{
			case NodeSpace.star: return true;
			default: return false;
		}
	}

	/// <summary>
	/// Returns true if no event, else false
	/// </summary>
	/// <returns></returns>
	public bool GetNodeLandEffect(PlayerControls p)
	{
		switch (nodeSpace)
		{
			case NodeSpace.blue: 
				p.NodeEffect(3);
				return true;
			case NodeSpace.red: 
				p.NodeEffect(-3);
				return true;
			case NodeSpace.green: 

				return false;
		}
		return true;
	}
}

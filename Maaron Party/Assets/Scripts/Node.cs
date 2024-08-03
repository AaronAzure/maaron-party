using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
	public List<Node> nextNodes;
	private Vector3 offset = new Vector3(0,0.25f);

	private void OnDrawGizmosSelected() 
	{
		Gizmos.color = Color.magenta;
		foreach (Node node in nextNodes)
			Gizmos.DrawLine(transform.position + offset, node.transform.position + offset);
	}
}

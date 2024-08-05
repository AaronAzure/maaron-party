using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour
{
	public static NodeManager Instance;
	[SerializeField] private Node[] nodes;


	private void Awake() 
	{
		Instance = this;
	}

	// Start is called before the first frame update
	void Start()
	{
		nodes = GetComponentsInChildren<Node>();
		for (ushort i=0 ; i<nodes.Length ; i++)
		{
			nodes[i].nodeId = i;
		}
	}

	public Node GetNode(ushort ind)
	{
		return ind >= 0 && ind < nodes.Length ? nodes[ind] : nodes[0];
	}
}

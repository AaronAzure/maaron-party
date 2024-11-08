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
			nodes[i].nodeId = i;
		foreach (int trapId in GameManager.Instance.traps.Keys)
			nodes[trapId].ToggleThorn(true, GameManager.Instance.traps[trapId][0], GameManager.Instance.traps[trapId][1]);
	}

	public Node GetNode(int ind)
	{
		return ind >= 0 && ind < nodes.Length ? nodes[ind] : nodes[0];
	}
	public void SetDistanceAway(ushort ind, int num, int movesLeft)
	{
		if (ind >= 0 && ind < nodes.Length) nodes[ind].SetDistanceAway(num, movesLeft);
	}
	public void ClearDistanceAway(ushort ind)
	{
		if (ind >= 0 && ind < nodes.Length) nodes[ind].ClearDistanceAway();
	}
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Node : MonoBehaviour
{
	public List<Node> nextNodes;
	private Vector3 offset = new Vector3(0,0.25f);
	public ushort nodeId;

	enum NodeSpace { blue, red, green, star, shop }
	[SerializeField] private NodeSpace nodeSpace;
	[SerializeField] private GameObject targetObj;
	[SerializeField] private TextMeshPro txt;
	private bool canSpellTarget=true;
	[HideInInspector] public int n=999;

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
	public bool GetNodeTraverseEffect(PlayerControls p)
	{
		switch (nodeSpace)
		{
			case NodeSpace.star: 
				p.OnStarNode();
				return true;
			case NodeSpace.shop: 
				p.OnShopNode();
				return true;
			default: 
				return false;
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

	/// <summary>
	/// Returns true if movement decreases when reached, else false (e.g. shop, star)
	/// </summary>
	/// <returns></returns>
	public bool DoesConsumeMovement()
	{
		return nodeSpace switch
		{
			NodeSpace.star => false,
			NodeSpace.shop => false,
			_ => true,
		};
	}

	public void ClearDistanceAway()
	{
		if (txt != null && n != 999)
		{
			n = 999;
			txt.text = "";
			txt.color = Color.white;
			foreach (Node node in nextNodes)
				if (node != null)
					node.ClearDistanceAway();
		}
	}
	public void SetDistanceAway(int x, int movesLeft)
	{
		if (txt != null && (txt.text == "" || x < n))
		{
			n = x;
			if (DoesConsumeMovement())
			{
				if (movesLeft == x)
					txt.color = Color.green;
				txt.text = $"{n}";
				foreach (Node node in nextNodes)
					if (node != null)
						node.SetDistanceAway(x+1, movesLeft);
			}
			else
				foreach (Node node in nextNodes)
					if (node != null)
						node.SetDistanceAway(x, movesLeft);
		}
	}
	public void SetCanSpellTarget(bool canSpellTarget) => this.canSpellTarget = canSpellTarget;
	private void OnTriggerEnter(Collider other) 
	{
		if (other.CompareTag("Range") && canSpellTarget)
			targetObj.SetActive(true);
	}

	private void OnTriggerExit(Collider other) 
	{
		if (other.CompareTag("Range"))		
			targetObj.SetActive(false);
	}
}

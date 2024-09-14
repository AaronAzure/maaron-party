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
	[SerializeField] private GameObject thornObj;
	[SerializeField] private Animator targetAnim;
	[SerializeField] private TextMeshPro txt;
	private bool canSpellTarget=true;
	[HideInInspector] public int n=999;
	private PlayerControls p { get { return PlayerControls.Instance; } }

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
		if (txt != null && (txt.text == "" || x < n) && n != 0)
		{
			n = x;
			if (DoesConsumeMovement())
			{
				if (n > 0)
				{
					if (movesLeft == x)
						txt.color = Color.green;
					txt.text = $"{n}";
				}
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

	public void ToggleThorn(bool active) 
	{
		thornObj.SetActive(active);
	}

	private void OnTriggerEnter(Collider other) 
	{
		if (other.CompareTag("Range") && canSpellTarget && DoesConsumeMovement())
			targetObj.SetActive(true);
	}
	private void OnTriggerExit(Collider other) 
	{
		if (other.CompareTag("Range"))		
			targetObj.SetActive(false);
	}


	private void OnMouseOver() 
	{
		if (targetObj.activeSelf)
		{
			//Debug.Log("<color=white>Mouse over</color>");
			if (Input.GetMouseButtonDown(0))
			{
				//Debug.Log("<color=#EFA01D>MOUSE CLICK</color>");
				switch (p._spellInd)
				{
					case 0: 
						ToggleThorn(true);
						PlayerControls.Instance.UseThornSpell(this);
						break;
					case 3:
						PlayerControls.Instance.UseFireSpell(transform);
						break;
					case 4:
						PlayerControls.Instance.UseFireSpell(transform);
						break;
					case 5:
						PlayerControls.Instance.UseFireSpell(transform);
						break;
				}
			}
		}
	}

	private void OnMouseEnter() 
	{
		if (targetObj.activeSelf)
			targetAnim.enabled = true;
	}

	private void OnMouseExit() 
	{
		if (targetObj.activeSelf)
		{
			targetAnim.enabled = false;
			targetObj.SetActive(false);
			targetObj.SetActive(true);
			//targetAnim.enabled = false;
		}
	}

}

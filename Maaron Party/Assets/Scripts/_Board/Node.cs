using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using UnityEngine;

public class Node : MonoBehaviour
{
	public List<Node> nextNodes;
	private Vector3 offset = new Vector3(0,0.25f);
	[HideInInspector] public ushort nodeId;


	enum NodeSpace { blue, red, green, star, shop }
	[Space] [SerializeField] private NodeSpace nodeSpace;
	public bool hasStar {get; private set;}

	
	[Space] [SerializeField] ParticleSystem blueGlowPs;
	[SerializeField] ParticleSystem redGlowPs;

	[Space] [SerializeField] private GameObject targetObj;
	[SerializeField] private Animator targetAnim;
	[SerializeField] private ParticleSystem starPs;
	public Transform maaronPos;
	
	[Space] [SerializeField] private GameObject thornObj;
	[SerializeField] private GameObject thornExplosionObj;
	
	[Space] [SerializeField] private TextMeshPro txt;
	private bool canSpellTarget=true;
	[HideInInspector] public int n=999;
	[HideInInspector] public int m=999;
	private PlayerControls p { get { return PlayerControls.Instance; } }
	List<PlayerControls> players;


	private void OnDrawGizmosSelected() 
	{
		if (nodeSpace == NodeSpace.red)
			Gizmos.color = new Color(1,0.6f,0);
		else if (nodeSpace == NodeSpace.blue)
			Gizmos.color = Color.magenta;
		else if (nodeSpace == NodeSpace.shop)
			Gizmos.color = Color.gray;
		else if (nodeSpace == NodeSpace.star)
			Gizmos.color = Color.yellow;

		foreach (Node node in nextNodes)
		{
			if (node != null)
			{
				Gizmos.DrawLine(transform.position + offset, node.transform.position + offset);
				Gizmos.DrawSphere(node.transform.position + offset, 0.2f);
			}
		}
	}


	public void PlayGlowVfx()
	{
		switch (nodeSpace)
		{
			case NodeSpace.blue:
				blueGlowPs.Play();
				break;
			case NodeSpace.red:
				redGlowPs.Play();
				break;
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
				if (hasStar)
					p.OnStarNode();
				return hasStar;
			case NodeSpace.shop: 
				p.OnShopNode();
				return true;
			default: 
				return false;
		}
	}

	public void AddPlayer(PlayerControls p)
	{
		if (players == null)
			players = new();
		players.Add(p);
	}
	public void RemovePlayer(PlayerControls p)
	{
		if (players != null && players.Contains(p))
		{
			players.Remove(p);
		}
	}
	//public void RagdollPlayers()
	//{
	//	foreach (PlayerControls player in players)
	//	{
	//		if (player != null)
	//		{
	//			player.CmdPlayerToggle(false);
	//			player.CmdRagdollToggle(true);
	//		}
	//	}
	//}
	public void HitPlayers(int penalty)
	{
		if (players != null && players.Contains(p) && !p.isShield)
		{
			p.CmdPlayerToggle(false);
			p.CmdRagdollToggle(true);
			p.NodeEffect(penalty);
		}
	}

	/// <summary>
	/// Returns true if no event, else false
	/// </summary>
	/// <returns></returns>
	public bool GetNodeLandEffect(PlayerControls p)
	{
		if (thornObj.activeSelf)
		{
			TriggerTrap();
			return false;
		}
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
	public void TriggerTrap() => StartCoroutine(TriggerTrapCo());
	IEnumerator TriggerTrapCo()
	{
		yield return new WaitForSeconds(0.75f);
		thornExplosionObj.SetActive(false);
		thornExplosionObj.SetActive(true);
		if (!p.isShield)
		{
			p.CmdPlayerToggle(false);
			p.CmdRagdollToggle(true);
			p.NodeEffect(-10);
		}
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
			m = 999;
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
	public int GetDistanceAway(int x)
	{
		Dictionary<Node, int> visited = new();
		List<Node> queue = new();
		queue.Add(this);
		visited.Add(this, x);

		for (int i=0 ; i<queue.Count ; i++)
		{
			if (queue[i].hasStar)
			{
				Debug.Log($"<color=yellow>NODE FOUND |{visited[queue[i]]}|</color>", queue[i].gameObject);
				return visited[queue[i]];
			}
			else
			{
				foreach (Node nextNode in queue[i].nextNodes)
				{
					// new node
					if (!visited.ContainsKey(nextNode))
					{
						queue.Add(nextNode);
						visited.Add(nextNode, queue[i].DoesConsumeMovement() ? 
							visited[queue[i]] + 1 : visited[queue[i]]
						);
					}
				}
			}
		}

		return -1;
	}

	
	Coroutine canTargetCo;
	public void SetCanSpellTarget(bool canSpellTarget) => this.canSpellTarget = canSpellTarget;
	public void SetCanSpellTargetDelay(bool canSpellTarget)
	{
		if (canTargetCo != null)
			StopCoroutine(canTargetCo);
		canTargetCo = StartCoroutine( SetCanSpellTargetCo(canSpellTarget) );
	} 
	IEnumerator SetCanSpellTargetCo(bool canSpellTarget)
	{
		yield return new WaitForSeconds(0.75f);
		this.canSpellTarget = canSpellTarget;
		canTargetCo = null;
	} 

	

	public void ToggleStarNode(bool active)
	{
		hasStar = active;
		if (active)
			starPs.Play();
		else
			starPs.Stop();
	} 
	public void ToggleThorn(bool active) => thornObj.SetActive(active);

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
					case 1: 
						ToggleThorn(true);
						PlayerControls.Instance.UseThornSpell(this);
						break;
					case 2: 
						ToggleThorn(true);
						PlayerControls.Instance.UseThornSpell(this);
						break;
					case 3:
						PlayerControls.Instance.UseFireSpell(this);
						break;
					case 4:
						PlayerControls.Instance.UseFireSpell(this);
						break;
					case 5:
						PlayerControls.Instance.UseFireSpell(this);
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

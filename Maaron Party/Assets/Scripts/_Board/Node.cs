using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using UnityEngine;

public enum NodeSpace { blue, red, green_rotate, green_speed, star, shop, door }

public class Node : MonoBehaviour
{
	public NodeSpace nodeSpace;
	private Vector3 offset = new Vector3(0,0.25f);
	[HideInInspector] public ushort nodeId;



	[Space] public List<Node> nextNodes;
	public bool hasStar {get; private set;}


	[Space] [Header("Door")]
	public Node altNode;
	[SerializeField] private int doorInd;
	[SerializeField] private Animator doorAnim;
	[SerializeField] private TextMeshPro[] tollTxts;


	[Space] [Header("Node Type")]
	[Space] [SerializeField] MeshRenderer mesh;
	[SerializeField] Material blueMat;
	[SerializeField] Material redMat;
	[SerializeField] Material greenRotateMat;
	[SerializeField] Material greenSpeedMat;
	[SerializeField] Material shopMat;
	[SerializeField] Material starMat;
	[SerializeField] Material starFadeMat;
	[SerializeField] Material emptyMat;

	
	[Space] [SerializeField] GameObject blueLandVfx;
	[SerializeField] private GameObject redLandVfx;
	[SerializeField] private GameObject greenLandVfx;

	[Space] [SerializeField] ParticleSystem blueGlowPs;
	[SerializeField] ParticleSystem redGlowPs;
	[SerializeField] private GameObject[] arrows;

	[Space] [SerializeField] private GameObject targetObj;
	public Transform target;
	[SerializeField] private Animator targetAnim;
	[SerializeField] private ParticleSystem starPs;
	public Transform maaronPos;
	
	[Space] [Header("Traps")]
	[SerializeField] private GameObject thornObj;
	//[SerializeField] private GameObject thornObj2;
	//[SerializeField] private GameObject thornObj3;
	[SerializeField] private ParticleSystem[] thornPs;
	[SerializeField] private Material thornMat1;
	[SerializeField] private Material thornMat2;
	[SerializeField] private Material thornMat3;

	[Space] [SerializeField] private GameObject thornExplosionObj;
	[SerializeField] private int thornId;
	[SerializeField] private int trapId;
	
	[Space] [SerializeField] private TextMeshPro txt;
	private bool canSpellTarget=true;
	[HideInInspector] public int n=999;
	[HideInInspector] public int m=999;
	private PlayerControls p { get { return PlayerControls.Instance; } }
	List<PlayerControls> players;

	

	#region Methods

	private void OnDrawGizmosSelected() 
	{
		if (nodeSpace == NodeSpace.red)
			Gizmos.color = new Color(1,0.6f,0);
		else if (nodeSpace == NodeSpace.blue)
			Gizmos.color = Color.cyan;
		else if (nodeSpace == NodeSpace.shop)
			Gizmos.color = Color.gray;
		else if (nodeSpace == NodeSpace.star)
			Gizmos.color = Color.yellow;
		else if (nodeSpace == NodeSpace.green_rotate)
			Gizmos.color = Color.green;
		else if (nodeSpace == NodeSpace.green_speed)
			Gizmos.color = Color.green;

		foreach (Node node in nextNodes)
		{
			if (node != null)
			{
				Gizmos.DrawLine(transform.position + offset, node.transform.position + offset);
				Gizmos.DrawSphere(node.transform.position + offset, 0.2f);
			}
		}
	}

	public void ChangeNodeSpace()
	{
		switch (nodeSpace)
		{
			case NodeSpace.blue: 
				mesh.material = blueMat;
				break;
			case NodeSpace.red: 
				mesh.material = redMat;
				break;
			case NodeSpace.green_speed: 
				mesh.material = greenSpeedMat;
				break;
			case NodeSpace.green_rotate: 
				mesh.material = greenRotateMat;
				break;
			case NodeSpace.shop: 
				mesh.material = shopMat;
				break;
			case NodeSpace.star: 
				mesh.material = starFadeMat;
				break;
			case NodeSpace.door: 
				mesh.material = emptyMat;
				break;
		}
	}

	private void Start() 
	{
		if (nextNodes != null && arrows != null)
		{
			for (int i=0 ; i<nextNodes.Count && i<arrows.Length ; i++)
			{
				if (nextNodes[i] != null && arrows[i] != null)
				{
					arrows[i].SetActive(true);
					arrows[i].transform.SetPositionAndRotation(
						Vector3.Lerp(transform.position, nextNodes[i].transform.position, 0.5f), 
						Quaternion.LookRotation((transform.position - nextNodes[i].transform.position).normalized)
					);
				}
			}
		}
	}
	#endregion


	#region Door

	public bool IsDoor() => nodeSpace == NodeSpace.door;
	public int GetDoorInd() => doorInd;
	public void PlayDoorAnim() 
	{
		if (doorAnim != null)
			doorAnim.SetTrigger("open");
	}
	public void SetNewToll(int newToll)
	{
		//Debug.Log($"<color=yellow>{name} - SETTING NEW TOLL {newToll}</color>");
		if (tollTxts != null) 
			foreach (TextMeshPro tollTxt in tollTxts)
				tollTxt.text = $"{newToll}";
	}

	#endregion


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

	public void HitPlayers(int penalty)
	{
		if (players != null && players.Contains(p) && !p.isShield)
		{
			p.CmdPlayerToggle(false);
			p.CmdRagdollToggle(true);
			p.NodeEffect(penalty);
		}
	}
	public void HitPlayersStars()
	{
		if (players != null && players.Contains(p) && !p.isShield)
		{
			p.CmdPlayerToggle(false);
			p.CmdRagdollToggle(true);
			p.NodeEffect(-1, true);
		}
	}

	/// <summary>
	/// Returns true if no event, else false
	/// </summary>
	/// <returns></returns>
	public float GetNodeLandEffect(PlayerControls p)
	{
		if (IsTrapped())
		{
			// own trap
			if (p.id == thornId)
				p.NodeEffect(5);
			else
				TriggerTrap(thornId);
			return p.id == thornId ? 0.5f : 5f;
		}
		switch (nodeSpace)
		{
			case NodeSpace.blue: 
				p.NodeEffect(3);
				return 0.5f;
			case NodeSpace.red: 
				p.NodeEffect(-3);
				return 0.5f;
			case NodeSpace.green_rotate: 
				BoardManager.Instance.CmdTurretRotateCo();
				return 4.5f;
			case NodeSpace.green_speed: 
				BoardManager.Instance.CmdTurretTurnCo();
				return GameManager.Instance.turretReady == 4 ? 10.5f : 3.5f;
		}
		return 0.5f;
	}
	public void TriggerNodeLandVfx()
	{
		//Debug.Log("<color=#FF9900>NODE LAND</color>");
		switch (nodeSpace)
		{
			case NodeSpace.blue: 
				blueLandVfx.SetActive(true);
				break;
			case NodeSpace.red: 
				redLandVfx.SetActive(true);
				break;
			case NodeSpace.green_rotate: 
				greenLandVfx.SetActive(true);
				break;
			case NodeSpace.green_speed: 
				greenLandVfx.SetActive(true);
				break;
		}
	}
	
	
	#region Trap
	public void ToggleThorn(bool active, int pId, int cId, int trapId) 
	{
		thornObj.SetActive(false);
		thornObj.SetActive(active);
		foreach (ParticleSystem thornP in thornPs)
		{
			thornP.GetComponent<ParticleSystemRenderer>().material = 
				trapId == 3 ? thornMat3 : trapId == 2 ? thornMat2 : thornMat1;

			var main = thornP.main;
			main.startColor = cId == 0 ? new Color(0.7f,0.13f,0.13f) : cId == 1 ? new Color(0.4f,0.7f,0.3f) 
				: cId == 2 ? new Color(0.85f,0.85f,0.5f) : new Color(0.7f,0.5f,0.8f);
		}
		thornId = pId;
		this.trapId = trapId;
	}
	public void TriggerTrap(int atkId) => StartCoroutine(TriggerTrapCo(atkId));
	IEnumerator TriggerTrapCo(int atkId)
	{
		yield return new WaitForSeconds(0.75f);
		thornExplosionObj.SetActive(false);
		thornExplosionObj.SetActive(true);
		if (!p.isShield)
		{
			p.CmdPlayerToggle(false);
			p.CmdRagdollToggle(true);
			if (trapId == 3) // 1 star
			{
				int stolenStars = p.GetStars() > 0 ? 1 : 0;
				BoardManager.Instance.CmdTrapReward(atkId, stolenStars, true);
				p.NodeEffect(-stolenStars, true);
			}
			if (trapId == 2) // 20 coins
			{
				int stolenCoins = p.GetCoins() < 20 ? p.GetCoins() : 20;
				BoardManager.Instance.CmdTrapReward(atkId, stolenCoins, false);
				p.NodeEffect(-stolenCoins);
			}
			if (trapId == 1) // 10 coins
			{
				int stolenCoins = p.GetCoins() < 10 ? p.GetCoins() : 10;
				BoardManager.Instance.CmdTrapReward(atkId, stolenCoins, false);
				p.NodeEffect(-stolenCoins);
			}
		}
	}
	#endregion


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
			NodeSpace.door => false,
			_ => true,
		};
	}
	public bool CanBeTrap()
	{
		return nodeSpace switch
		{
			NodeSpace.blue => true,
			NodeSpace.red => true,
			_ => false,
		};
	}


	#region Distance
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
		//Debug.Log("<color=cyan>Getting Distance Away</color>");
		Dictionary<Node, int> visited = new();
		List<Node> queue = new();
		queue.Add(this);
		visited.Add(this, x);

		for (int i=0 ; i<queue.Count ; i++)
		{
			if (queue[i].hasStar)
			{
				//Debug.Log($"<color=magenta>{queue[i].name} FOUND |{visited[queue[i]]}|</color>", queue[i].gameObject);
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
	#endregion


	public Transform GetTargetTransform() => maaronPos != null ? maaronPos.transform : null;
	
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
		Debug.Log($"<color=yellow>STAR == {name} |{active}| = {mesh != null}</color>");
		hasStar = active;
		if (mesh != null) mesh.material = active ? starMat : starFadeMat;
		if (active)
			starPs.Play();
		else
			starPs.Stop();
	} 
	public bool IsTrapped() => thornObj.activeSelf;

	private void OnTriggerEnter(Collider other) 
	{
		// blue + red spaces
		if (other.CompareTag("Range") && canSpellTarget && CanBeTrap())
			targetObj.SetActive(true);
		// all spaces except star/shop
		if (other.CompareTag("Range2") && canSpellTarget && DoesConsumeMovement())
			targetObj.SetActive(true);
		if (other.CompareTag("Death"))
		{
			if (players != null && players.Contains(p))
			//if (players != null && players.Contains(p) && !p.isShield)
			{
				Debug.Log($"<color=red>{p.name} HIT!!</color>");
				p.CmdPlayerToggle(false);
				p.CmdRagdollToggle(true);
				p.LoseAllCoins();
			}
		}

	}
	private void OnTriggerExit(Collider other) 
	{
		if (other.CompareTag("Range") || other.CompareTag("Range2"))		
			targetObj.SetActive(false);
	}


	private void OnMouseOver() 
	{
		if (targetObj.activeSelf)
		{
			// trap spell cost
			if (p._spellInd >= 0 && p._spellInd <= 2)
				p.SetSpellCost(p._spellInd + 1, thornId == p.id ? 0 : trapId);
			else if (p._spellInd >= 3 && p._spellInd <= 5)
				p.SetSpellCost(p._spellInd - 1, 0);
			//Debug.Log("<color=white>Mouse over</color>");
			if (Input.GetMouseButtonDown(0))
			{
				//Debug.Log("<color=#EFA01D>MOUSE CLICK</color>");
				switch (p._spellInd)
				{
					case 0: 
						if (p.GetMana() < 1 + (thornId == p.id ? 0 : trapId)) 
						{
							p.NoManaAlert();
							break; // not enough mana to overwrite
						}
						p.UseThornSpell(this, 1 + (thornId == p.id ? 0 : trapId), 1);
						BoardManager.Instance.CmdThornNode(nodeId, p.id, p.characterInd, 1);
						break;
					case 1: 
						if (p.GetMana() < 2 + (thornId == p.id ? 0 : trapId)) 
						{
							p.NoManaAlert();
							break; // not enough mana to overwrite
						}
						p.UseThornSpell(this, 2 + (thornId == p.id ? 0 : trapId), 2);
						BoardManager.Instance.CmdThornNode(nodeId, p.id, p.characterInd, 2);
						break;
					case 2: 
						if (p.GetMana() < 3 + (thornId == p.id ? 0 : trapId)) 
						{
							p.NoManaAlert();
							break; // not enough mana to overwrite
						}
						p.UseThornSpell(this, 3 + (thornId == p.id ? 0 : trapId), 3);
						BoardManager.Instance.CmdThornNode(nodeId, p.id, p.characterInd, 3);
						break;
					case 3:
						p.UseFireSpell(this, 2, 1);
						break;
					case 4:
						p.UseFireSpell(this, 3, 2);
						break;
					case 5:
						p.UseFireSpell(this, 4, 3);
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
		// trap spell cost
		if (p._spellInd >= 0 && p._spellInd <= 2)
			p.SetSpellCost(p._spellInd + 1, 0);
	}

}

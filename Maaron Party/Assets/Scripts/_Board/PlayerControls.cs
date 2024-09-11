using System.Collections;
using System.Collections.Generic;
using TMPro;
using Rewired;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerControls : NetworkBehaviour
{
	public static PlayerControls Instance;
	private Player player;
	[SerializeField] private Node currNode;
	[SerializeField] private Node nextNode;
	[SerializeField] private float moveSpeed=2.5f;
	[SerializeField] private float rotateSpeed=5f;
	private Vector3 startPos;
	private float time;
	[SyncVar] public int characterInd=-1;
	[SyncVar] public int id=-1;

	
	//[Space] [Header("Network")]
	//public NetworkVariable<ulong> id = new NetworkVariable<ulong>(
	//	0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	//[SerializeField] private NetworkObject nwObj;
	
	[Space] [Header("Model")]
	[SerializeField] private Transform model;
	[SerializeField] private GameObject[] models;
	[SerializeField] private Animator anim;
	[SerializeField] private Transform vCam;
	[SerializeField] private GameObject starCam;
	[SerializeField] private GameObject rangeObj;
	[SerializeField] private Animator rangeAnim;

	[Space] [SerializeField] private GameObject bonusObj;
	[SerializeField] private TextMeshPro bonusTxt;
	
	[Space] [SerializeField] private GameObject penaltyObj;
	[SerializeField] private TextMeshPro penaltyTxt;

	[Space] [SerializeField] private Direction up;
	[SerializeField] private Direction down;
	[SerializeField] private Direction left;
	[SerializeField] private Direction right;
	[SerializeField] private Direction upLeft;
	[SerializeField] private Direction upRight;
	[SerializeField] private Direction downLeft;
	[SerializeField] private Direction downRight;


	[Space] [SerializeField] private int movesLeft;
	[SerializeField] private TextMeshPro movesLeftTxt;

	
	
	[Space] [Header("Stats")]
	[SyncVar] private int coins=10;
	private int coinsT;
	[SyncVar] private int stars;
	private int starsT;
	[SerializeField] private float currencyT;
	[SerializeField] private float currencySpeedT=1;


	[Space] [Header("UI")]
	[SerializeField] private GameObject canvas;
	[SerializeField] private GameObject starUi;
	[SerializeField] private GameObject shopUi;
	
	[Space] [SerializeField] private Transform dataUi;
	[SerializeField] private Image dataImg;
	[SerializeField] private TextMeshProUGUI coinTxt;
	[SerializeField] private TextMeshProUGUI starTxt;
	private BoardManager bm
	{
		get 
		{
			return BoardManager.Instance;
		}
	}
	private GameNetworkManager nm
	{
		get 
		{
			return GameNetworkManager.Instance;
		}
	}
	private GameManager gm
	{
		get 
		{
			return GameManager.Instance;
		}
	}


	#region States

	[Space] [Header("States")]
	[SerializeField] private bool isAtFork;
	[SerializeField] private bool isAtStar;
	[SerializeField] private bool isBuyingStar;
	[SerializeField] private bool isAtShop;
	[SerializeField] private bool isStop;
	[SerializeField] private bool isUsingSpell;
	private bool isCurrencyAsync;

	#endregion

	[Space] [Header("Shop")]
	[SerializeField] private Button[] shopItems;
	[SyncVar] [SerializeField] List<int> items = new();
	[SerializeField] Image[] itemImgs;
	[SerializeField] private Sprite emptySpr;


	[Space] [Header("Spell")]
	[SerializeField] private GameObject spellCam;
	[SerializeField] private float spellCamSpeed=0.5f;


	[Space] [Header("Ragdoll")]
	[SerializeField] private GameObject shoveObj;
	[SerializeField] private GameObject ragdollObj;
	[SerializeField] private Rigidbody[] ragdollRb;
	[SerializeField] private float ragdollKb=15;


	[Space] [Header("HACKS")]
	[SerializeField] private int controlledRoll=-1;
	[SerializeField] private bool freeShop;


	#region Methods

	private void Awake() 
	{
		DontDestroyOnLoad(this);
	}
	public override void OnStartClient()
	{
		base.OnStartClient();
		if (isOwned)
			Instance = this;	
		nm.AddBoardConnection(this);
	}
	public override void OnStopClient()
	{
		base.OnStopClient();
		if (isOwned)
			nm.RemoveBoardConnection(this);
	}


	public void MediateRemoteStart()
	{
		TargetRemoteStart(netIdentity.connectionToClient);
	}
	[Command(requiresAuthority = false)] private void CmdPing() => Debug.Log($"<color=yellow>TargetRemoteStart {name}</color>");
	[TargetRpc] private void TargetRemoteStart(NetworkConnectionToClient target) 
	{
		//Debug.Log($"<color=yellow>TargetRemoteStart</color>");
		player = ReInput.players.GetPlayer(0);
		if (vCam != null)
			vCam.parent = null;
		
		CmdSetModel(characterInd);
		if (gm.nTurn == 1)
		{
			transform.position = BoardManager.Instance.GetSpawnPos().position + new Vector3(-4 + 2*id,0,0);
			startPos = this.transform.position;
		}
		
		if (!isOwned) {
			enabled = false;
			return;
		}
		
		// after first turn
		if (gm.nTurn > 1)
		{
			LoadData();
		}
		// 第一名 
		else
		{
			coinsT = coins;
			starsT = stars;
		}
		
		CmdSetCoinText(coins);
		CmdSetStarText(stars);
		CmdReplaceItems(items);
		CmdShowItems();
	}

	[Command(requiresAuthority = false)] public void CmdSetModel(int ind) => RpcSetModel(ind);
	[ClientRpc] public void RpcSetModel(int ind)
	{
		name = $"__ PLAYER {id} __";
		if (isOwned)
			CmdPing();
		transform.parent = bm.transform;
		for (int i=0 ; i<models.Length ; i++)
			models[i].SetActive(false);
		if (models != null && ind >= 0 && ind < models.Length)
			models[ind].SetActive(true);

		if (isOwned)
			anim = models[ind].GetComponent<Animator>();

		if (bm != null)
			bm.SetUiLayout(dataUi);
		dataUi.gameObject.SetActive(true);
		dataImg.color = ind == 0 ? new Color(0,1,0) : ind == 1 ? new Color(1,0.6f,0) 
			: ind == 2 ? new Color(1,0.5f,0.8f) : Color.blue;
	}

	public void SetStartNode(Node startNode)
	{
		nextNode = startNode;
	}

	[Command(requiresAuthority=false)] private void CmdSetCoinText(int n) => RpcSetCoinText(n);
	[ClientRpc] private void RpcSetCoinText(int n) => coinTxt.text = $"{n}";
	[Command(requiresAuthority=false)] private void CmdSetStarText(int n) => RpcSetStarText(n);
	[ClientRpc] private void RpcSetStarText(int n) => starTxt.text = $"{n}";

	#endregion


	#region FixedUpdate
	void FixedUpdate()
	{
		if (!isOwned) return;
		if (coins != coinsT)
		{
			if (currencyT < 0.1f)
			{
				currencyT += Time.fixedDeltaTime * currencySpeedT;
			} 
			else
			{
				coinsT = coinsT < coins ? coinsT + 1 : coinsT - 1;
				CmdSetCoinText(coinsT);
				currencyT = 0;
			}
			if (coins == coinsT)
			{
				isCurrencyAsync = false;
				currencySpeedT = 1;
			}
		}
		else if (stars != starsT)
		{
			if (currencyT < 0.1f)
			{
				currencyT += Time.fixedDeltaTime * currencySpeedT;
			} 
			else
			{
				starsT = starsT < stars ? starsT + 1 : starsT - 1;
				CmdSetStarText(starsT);
				currencyT = 0;
			}
			if (stars == starsT)
			{
				isBuyingStar = isCurrencyAsync = false;
				anim.SetBool("hasStar", false);
				starCam.SetActive(false);
				currencySpeedT = 1;
			}
		}

		if (spellCam.activeSelf)
		{
			float moveX = player.GetAxis("Move Horizontal");
			float moveZ = player.GetAxis("Move Vertical");
			
			spellCam.transform.position += new Vector3(moveX, 0, moveZ) * spellCamSpeed;
		}

		if (isStop) {}
		else if (isAtFork) {}
		else if (isAtStar) {}
		else if (isAtShop) {}
		else if (isBuyingStar) {}
		else if (movesLeft > 0)
		{
			if (transform.position != nextNode.transform.position)
			{
				var lookPos = nextNode.transform.position + - transform.position;
				if (anim != null) anim.SetFloat("moveSpeed", moveSpeed);
				lookPos.y = 0;
				var rotation = Quaternion.LookRotation(lookPos);
				model.rotation = Quaternion.Slerp(model.rotation, rotation, Time.fixedDeltaTime * rotateSpeed);

				transform.position = Vector3.Lerp(startPos, nextNode.transform.position, Mathf.SmoothStep(0,1,time));
				if (time < 1)
					time += Time.fixedDeltaTime * moveSpeed;
			}
			else
			{
				time = 0;
				if (nextNode.GetNodeTraverseEffect(this))
				{
					isStop = true;
					if (anim != null) anim.SetFloat("moveSpeed", 0);
					return;
				}

				movesLeft--;
				CmdUpdateMovesLeft(movesLeft);
				if (movesLeft <= 0)
				{
					currNode = nextNode; 
					movesLeftTxt.text = "";
					if (anim != null) anim.SetFloat("moveSpeed", 0);
					StartCoroutine( NodeEffectCo() );
				}
				else
				{
					startPos = transform.position;
					// more than one path
					if (nextNode.nextNodes.Count > 1)
					{
						StuckAtFork();
					}
					// single path
					else
					{
						nextNode = nextNode.nextNodes[0];
					}
				}
			}
		}
	}

	#endregion

	public void YourTurn()
	{
		CmdCamToggle(true);
		//CmdPlayerToggle(true);
		TargetYourTurn(netIdentity.connectionToClient);
	}
	[Command(requiresAuthority=false)] private void CmdCamToggle(bool activate) => RpcCamToggle(activate);
	[ClientRpc] private void RpcCamToggle(bool activate) => vCam.gameObject.SetActive(activate);
	[Command(requiresAuthority=false)] private void CmdShoveToggle(bool activate) => RpcShoveToggle(activate);
	[ClientRpc(includeOwner=false)] private void RpcShoveToggle(bool activate) => shoveObj.SetActive(activate);
	[TargetRpc] public void TargetYourTurn(NetworkConnectionToClient target)
	{
		if (canvas != null)
			canvas.SetActive(true);
		this.enabled = true;
		CmdShoveToggle(true);
	}
	public void EndTurn()
	{
		CmdCamToggle(false);
		//vCam.gameObject.SetActive(false);
		if (canvas != null)
			canvas.SetActive(false);
		CmdShoveToggle(false);
		this.enabled = false;
		SaveData();
		//!Debug.Log($"<color=yellow>TURN ENDED</color>");
	}

	#region Saving data

	private void SaveData()
	{
		gm.SaveCurrNode(currNode.nodeId, id);
		gm.CmdSaveCoins(coins, id);
		gm.CmdSaveStars(stars, id);
		gm.CmdSaveItems(items, id);
	}
	private void LoadData()
	{
		currNode = NodeManager.Instance.GetNode( gm.GetCurrNode(id) );
		startPos = transform.position = currNode.transform.position;
		coinsT = coins = gm.GetCoins(id);
		starsT = stars = gm.GetStars(id);
		items = gm.GetItems(id);
	}

	#endregion


	#region BUTTONS
	public void _PURCHASE_STAR(bool purchase)
	{
		if (purchase && coins >= 20)
		{
			coins -= 20;
			currencySpeedT = 2;
			stars++;
			isBuyingStar = true;
			anim.SetBool("hasStar", true);
			Vector3 dir = Camera.main.transform.position - transform.position;
			model.rotation = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z).normalized);
		}
		else	
			starCam.SetActive(false);

		startPos = transform.position;
		// more than one path
		if (nextNode.nextNodes.Count > 1)
		{
			StuckAtFork();
		}
		// single path
		else
		{
			nextNode = nextNode.nextNodes[0];
		}

		if (starUi != null)
			starUi.SetActive(false);
		isStop = isAtStar = false;
	}
	public void _CLOSE_SHOP()
	{
		startPos = transform.position;
		// more than one path
		if (nextNode.nextNodes.Count > 1)
		{
			StuckAtFork();
		}
		// single path
		else
		{
			nextNode = nextNode.nextNodes[0];
		}

		if (shopUi != null)
			shopUi.SetActive(false);
		isStop = isAtShop = false;
	}
	public void _ROLL_DICE()
	{
		if (isUsingSpell) return;

		if (currNode != null)
		{
			if (currNode.nextNodes.Count > 1)
			{
				StuckAtFork();
				nextNode = currNode;
			}
			else
			{
				isAtFork = false;
				nextNode = currNode.nextNodes[0];
			}
		}

		int rng = controlledRoll != -1 ? controlledRoll : Random.Range(1, 11);
		Debug.Log($"<color=magenta>ROLLED {rng}</color>");
		movesLeft = rng;
		CmdUpdateMovesLeft( movesLeft );

		if (canvas != null)
			canvas.SetActive(false);
	}

	public void _TOGGLE_MAP()
	{
		if (spellCam.activeSelf)
			CmdShowNodeDistance(false, nextNode == null ? (ushort) 0 : nextNode.nodeId, 1, -1);
		else
		{
			spellCam.transform.localPosition = new Vector3(0,25,-10);
			CmdShowNodeDistance(true, nextNode == null ? (ushort) 0 : nextNode.nodeId, 1, -1);
		}
		
		CmdToggleMapCam(!spellCam.activeSelf);
	}
	#endregion


	#region Nodes

	private void StuckAtFork()
	{
		isAtFork = true;
		if (anim != null) anim.SetFloat("moveSpeed", 0);
		HidePaths();
		CmdShowNodeDistance(true, nextNode.nodeId, 0, movesLeft);
		spellCam.transform.localPosition = new Vector3(0,25,-10);
		CmdToggleMapCam(true);
		for (int i=0 ; i<nextNode.nextNodes.Count ; i++)
			RevealPaths(nextNode.nextNodes[i].transform.position, i);
	}
	[Command(requiresAuthority=false)] void CmdToggleMapCam(bool active) => RpcToggleMapCam(active);
	[ClientRpc] void RpcToggleMapCam(bool active) => spellCam.SetActive(active);
	[Command(requiresAuthority=false)] void CmdShowNodeDistance(bool active, ushort nodeId, int num, int movesLeft) 
		=> RpcShowNodeDistance(active, nodeId, num, movesLeft);
	[ClientRpc] void RpcShowNodeDistance(bool active, ushort nodeId, int num, int movesLeft) 
	{
		if (active)
			NodeManager.Instance.SetDistanceAway(nodeId, num, movesLeft);
		else
			NodeManager.Instance.ClearDistanceAway(nodeId);
	}

	public void OnShopNode()
	{
		isAtShop = true;
		if (anim != null) anim.SetFloat("moveSpeed", 0);
		shopUi.SetActive(true);
		isStop = false;
	}
	public void OnStarNode()
	{
		isAtStar = true;
		if (anim != null) anim.SetFloat("moveSpeed", 0);
		starUi.SetActive(true);
		starCam.SetActive(true);
		isStop = false;
	}
	public void NodeEffect(int bonus)
	{
		coins = Mathf.Clamp(coins + bonus, 0, 999);
		isCurrencyAsync = true;
		CmdShowBonusTxt(bonus);
	}
	[Command] private void CmdShowBonusTxt(int n) => RpcShowBonusTxt(n);
	[ClientRpc] private void RpcShowBonusTxt(int n)
	{
		bonusObj.SetActive(false);
		bonusObj.SetActive(true);
		bonusTxt.text = n > 0 ? $"+{n}" : $"{n}";
	}

	private IEnumerator NodeEffectCo()
	{
		// no event
		if (currNode.GetNodeLandEffect(this))
			yield return new WaitForSeconds(0.5f);
		else 
			yield return new WaitForSeconds(0.5f);

		while (isCurrencyAsync)
			yield return new WaitForSeconds(0.1f);

		EndTurn();
		bm.NextPlayerTurn();
	}

	void HidePaths()
	{
		if (up.gameObject.activeSelf)
			up.gameObject.SetActive(false);
		if (down.gameObject.activeSelf)
			down.gameObject.SetActive(false);
		if (left.gameObject.activeSelf)
			left.gameObject.SetActive(false);
		if (right.gameObject.activeSelf)
			right.gameObject.SetActive(false);
		if (upLeft.gameObject.activeSelf)
			upLeft.gameObject.SetActive(false);
		if (upRight.gameObject.activeSelf)
			upRight.gameObject.SetActive(false);
		if (downLeft.gameObject.activeSelf)
			downLeft.gameObject.SetActive(false);
		if (downRight.gameObject.activeSelf)
			downRight.gameObject.SetActive(false);
	}
	void RevealPaths(Vector3 nextPos, int ind)
	{
		Vector3 toPos = (nextPos - startPos).normalized;
		bool goingUp = toPos.z >= 0.33f;
		bool goingDown = toPos.z <= -0.33f;
		bool goingRight = toPos.x >= 0.33f;
		bool goingLeft = toPos.x <= -0.33f;

		if (goingUp && !goingLeft && !goingRight)
		{
			up.gameObject.SetActive(true);
			up.index = ind;
		}
		else if (goingDown && !goingLeft && !goingRight)
		{
			down.gameObject.SetActive(true);
			down.index = ind;
		}
		else if (goingLeft && !goingUp && !goingDown)
		{
			left.gameObject.SetActive(true);
			left.index = ind;
		}
		else if (goingRight && !goingUp && !goingDown)
		{
			right.gameObject.SetActive(true);
			right.index = ind;
		}
		else if (goingUp && goingLeft)
		{
			upLeft.gameObject.SetActive(true);
			upLeft.index = ind;
		}
		else if (goingUp && goingRight)
		{
			upRight.gameObject.SetActive(true);
			upRight.index = ind;
		}
		else if (goingDown && goingLeft)
		{
			downLeft.gameObject.SetActive(true);
			downLeft.index = ind;
		}
		else if (goingDown && goingRight)
		{
			downRight.gameObject.SetActive(true);
			downRight.index = ind;
		}
	}

	public void ChoosePath(int ind)
	{
		CmdShowNodeDistance(false, nextNode.nodeId, 0, movesLeft);
		nextNode = nextNode.nextNodes[ind];
		HidePaths();
		CmdToggleMapCam(false);
		isAtFork = false;
	}

	#endregion


	#region Items/Spells
	public void _BUY_ITEM(int itemId)
	{
		Debug.Log($"<color=cyan>BOUGHT ITEM {itemId}</color>");
		if (items.Count < 3)
			items.Add(itemId);
		CmdReplaceItems(items);
		CmdShowItems();
	}
	[Command(requiresAuthority=false)] private void CmdShowItems() => RpcShowItems();
	[ClientRpc] private void RpcShowItems()
	{
		for (int i = 0; i < itemImgs.Length; i++)
		{
			if (items.Count > i)
				itemImgs[i].sprite = Item.instance.GetSprite( items[i] );
			else
				itemImgs[i].sprite = emptySpr;
		}
	}
	[Command(requiresAuthority=false)] private void CmdReplaceItems(List<int> ints) => RpcReplaceItems(ints);
	[ClientRpc(includeOwner=false)] private void RpcReplaceItems(List<int> ints)
	{
		items = ints;
	}
	public void _USE_SPELL() => CmdUseSpell(!rangeObj.activeSelf);
	[Command(requiresAuthority=false)] private void CmdUseSpell(bool active) => RpcUseSpell(active);
	[ClientRpc] private void RpcUseSpell(bool active)
	{
		if (currNode != null) currNode.SetCanSpellTarget(!active);

		if (active)
			spellCam.transform.localPosition = new Vector3(0,25,-10);
		spellCam.SetActive(active);
		isUsingSpell = active;
	 	rangeAnim.SetTrigger(active ? "on" : "off");
	}

	#endregion

	
	[Command(requiresAuthority=false)] void CmdUpdateMovesLeft(int x) => RpcUpdateMovesLeft(x);
	[ClientRpc] void RpcUpdateMovesLeft(int x) => movesLeftTxt.text = $"{(x == 0 ? "" : x)}";

	//private void OnTriggerEnter(Collider other) 
	//{
	//	if (isOwned && !this.enabled && model.gameObject.activeSelf && other.gameObject.CompareTag("Enemy"))
	//	{
	//		CmdPlayerToggle(false);
	//		Debug.Log("<color=red>KNOCKBACK!!</color>");
	//		CmdRagdollToggle(true);
	//		ragdollObj.transform.parent = null;
	//		Vector3 dir = (transform.position - other.transform.position).normalized;
	//		foreach (Rigidbody ragdoll in ragdollRb)
	//		{
	//			ragdoll.AddForce(dir * ragdollKb, ForceMode.Impulse);
	//			//ragdoll.AddExplosionForce(ragdollKb, other.transform.position, 50f, 70f, ForceMode.Impulse);
	//		}
	//	}
	//}
	[Command(requiresAuthority=false)] private void CmdRagdollToggle(bool active) => RpcRagdollToggle(active);
	[ClientRpc] private void RpcRagdollToggle(bool active) => model.gameObject.SetActive(active);

	[Command(requiresAuthority=false)] private void CmdPlayerToggle(bool active) => RpcPlayerToggle(active);
	[ClientRpc] private void RpcPlayerToggle(bool active) => ragdollObj.SetActive(active);
	//IEnumerator MoveCo()
	//{
	//	yield return new WaitForSeconds(2);
	//	UpdateMovesLeft( Random.Range(1, 11) );
	//}
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using Rewired;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Cinemachine;

public class PlayerControls : NetworkBehaviour
{
	[Header("HACKS")]
	[SerializeField] [Range(-1,30)] private int controlledRoll=-1;
	[SerializeField] private bool freeShop;

	
	[HideInInspector] public static PlayerControls Instance;
	private Player player;
	
	[Space] [Header("Static")]
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
	[SerializeField] private CinemachineVirtualCamera nodeCam;
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


	[Space] [Header("Ragdoll")]
	[SerializeField] private GameObject shoveObj;
	[SerializeField] private GameObject[] ragdollObj;
	[SerializeField] private Rigidbody[] ragdollRb;
	[SerializeField] private float ragdollKb=15;
	
	
	[Space] [Header("Stats")]
	[SyncVar] private int coins=10;
	private int coinsT;
	[SyncVar] private int stars;
	private int starsT;
	[SerializeField] private float currencyT;
	[SerializeField] private float currencySpeedT=1;


	#region UI

	[Space] [Header("UI")]
	[SerializeField] private GameObject canvas;
	[SerializeField] private GameObject starUi;
	[SerializeField] private GameObject shopUi;
	[SerializeField] private GameObject spellUi;
	[SerializeField] private GameObject backToBaseUi;
	[SerializeField] private GameObject baseUi;

	#endregion
	
	[Space] [SerializeField] private Transform dataUi;
	[SerializeField] private Image dataImg;
	[SerializeField] private TextMeshProUGUI distanceTxt;
	[SerializeField] private TextMeshProUGUI coinTxt;
	[SerializeField] private TextMeshProUGUI starTxt;
	private BoardManager bm { get { return BoardManager.Instance; } }
	private GameNetworkManager nm { get { return GameNetworkManager.Instance; } }
	private GameManager gm { get { return GameManager.Instance; } }


	#region States

	[Space] [Header("States")]
	[SerializeField] private bool isAtFork;
	[SerializeField] private bool isAtStar;
	[SerializeField] private bool isBuyingStar;
	[SerializeField] private bool isAtShop;
	[SerializeField] private bool isStop;
	[SerializeField] private bool isUsingSpell;
	[SerializeField] private bool inMap;
	[SerializeField] private bool isDashing;
	[SerializeField] private bool inSpellAnimation;
	public bool isShield {get; private set;}
	bool usingFireSpell1;
	private bool isCurrencyAsync;

	#endregion



	[Space] [Header("Shop")]
	[SerializeField] private Button[] shopItems;
	[SyncVar] [SerializeField] List<int> itemInds = new();
	[SerializeField] Image[] itemImgs;
	[SerializeField] private Sprite emptySpr;


	#region Spell Vars

	[Space] [Header("Spell")]
	[SerializeField] private Item[] items;
	[SerializeField] private GameObject spellCam;
	[SerializeField] private float spellCamSpeed=0.5f;
	[SerializeField] private GameObject fireSpell1;
	[SerializeField] private ParticleSystem dashSpellPs1;
	[SerializeField] private GameObject shieldSpell1;
	public int _spellInd {get; private set;} 

	#endregion


	#region __Methods__

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
	//[Command(requiresAuthority = false)] private void CmdPing() => Debug.Log($"<color=yellow>TargetRemoteStart {name}</color>");
	[TargetRpc] private void TargetRemoteStart(NetworkConnectionToClient target) 
	{
		//Debug.Log($"<color=yellow>TargetRemoteStart</color>");
		player = ReInput.players.GetPlayer(0);
		if (vCam != null)
			vCam.parent = null;
		
		CmdSetModel(characterInd);
		model.rotation = Quaternion.LookRotation(Vector3.back);
		if (gm.nTurn == 1)
		{
			transform.position = BoardManager.Instance.GetSpawnPos().position + new Vector3(-4 + 2*id,0,-2);
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
		CmdReplaceItems(itemInds);
		CmdShowItems();
	}

	[Command(requiresAuthority = false)] public void CmdSetModel(int ind) => RpcSetModel(ind);
	[ClientRpc] public void RpcSetModel(int ind)
	{
		name = $"__ PLAYER {id} __";
		//if (isOwned)
		//	CmdPing();
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

	public void SetStartNode(Node startNode) => nextNode = startNode;

	[Command(requiresAuthority=false)] private void CmdSetCoinText(int n) => RpcSetCoinText(n);
	[ClientRpc] private void RpcSetCoinText(int n) => coinTxt.text = $"{n}";
	[Command(requiresAuthority=false)] private void CmdSetStarText(int n) => RpcSetStarText(n);
	[ClientRpc] private void RpcSetStarText(int n) => starTxt.text = $"{n}";

	private void RotateDirection(Vector3 dir)
	{
		model.rotation = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
	}

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
				isCurrencyAsync = false;
				currencySpeedT = 1;
			}
		}

		if (spellCam.activeSelf)
		{
			float moveX = player.GetAxis("Move Horizontal");
			float moveZ = player.GetAxis("Move Vertical");
			
			spellCam.transform.position += new Vector3(moveX, 0, moveZ) * spellCamSpeed;
		}

		//if (usingFireSpell1) FireSpell();

		if (isStop) {}
		else if (inMap) {}
		else if (isAtFork) {}
		else if (isAtStar) {}
		else if (isAtShop) {}
		else if (isBuyingStar) {}
		else if (movesLeft > 0)
		{
			// moving to next node
			if (transform.position != nextNode.transform.position)
			{
				var lookPos = nextNode.transform.position - transform.position;
				if (anim != null) anim.SetFloat("moveSpeed", moveSpeed);
				lookPos.y = 0;
				var rotation = Quaternion.LookRotation(lookPos);
				model.rotation = Quaternion.Slerp(model.rotation, rotation, Time.fixedDeltaTime * rotateSpeed);

				transform.position = Vector3.Lerp(startPos, nextNode.transform.position, time);
				//transform.position = Vector3.Lerp(startPos, nextNode.transform.position, Mathf.SmoothStep(0,1,time));
				if (time < 1)
					time += Time.fixedDeltaTime * moveSpeed * (isDashing ? 2 : 1);
			}
			// landed on next node
			else
			{
				time = 0;
				// if shop or star
				if (nextNode.GetNodeTraverseEffect(this))
				{
					isStop = true;
					if (anim != null) anim.SetFloat("moveSpeed", 0);
					return;
				}

				if (nextNode.DoesConsumeMovement())
					movesLeft--;
				CmdPlayNodeTraverseVfx(nextNode.nodeId);
				CmdUpdateMovesLeft(movesLeft);

				// end at space
				if (movesLeft <= 0)
				{
					currNode = nextNode; 
					if (currNode != null)
						currNode.AddPlayer(this);
					movesLeftTxt.text = "";
					if (anim != null) anim.SetFloat("moveSpeed", 0);
					if (!isDashing)
						StartCoroutine( NodeEffectCo() );
					// used dash spell
					else
					{
						startPos = transform.position;
						if (canvas != null)
							canvas.SetActive(true);
						CmdToggleDashVfx(false);
						isDashing = false;
						if (nextNode.nextNodes.Count == 1)
							nextNode = nextNode.nextNodes[0];
					}
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
		//Debug.Log("<color=magenta>YOUR TURN!!</color>");
		CmdCamToggle(true);
		//CmdPlayerToggle(true);
		if (currNode != null)
			ShowDistanceAway(currNode.GetDistanceAway(0));
		else if (nextNode != null)
			ShowDistanceAway(nextNode.GetDistanceAway(1));
		TargetYourTurn(netIdentity.connectionToClient);
	}
	private void ShowDistanceAway(int n)
	{
		distanceTxt.text = n == -1 ? "? spaces away" : n == 1 ? "1 space away" : $"{n} spaces away";
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
		//CmdShoveToggle(true);
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
		gm.CmdSaveItems(itemInds, id);
	}
	private void LoadData()
	{
		currNode = NodeManager.Instance.GetNode( gm.GetCurrNode(id) );
		currNode.AddPlayer(this);
		startPos = transform.position = currNode.transform.position;
		transform.position = new Vector3(transform.position.x, 0, transform.position.z);
		coinsT = coins = gm.GetCoins(id);
		starsT = stars = gm.GetStars(id);
		itemInds = gm.GetItems(id);
	}

	#endregion


	#region BUTTONS
	public void _BACK_TO_BASE_UI()
	{
		// show base ui
		CmdUseSpell(false, currNode != null ? currNode.nodeId : -1);
		backToBaseUi.SetActive(false);
		spellUi.SetActive(false);
		baseUi.SetActive(true);
	}
	public void _TOGGLE_SPELLS_UI()
	{
		// show base ui
		if (spellUi.activeSelf)
		{
			spellUi.SetActive(false);
			baseUi.SetActive(true);
		}
		// show spell ui
		else
		{
			ToggleSpellUi(true);
			spellUi.SetActive(true);
			baseUi.SetActive(false);
		}
	}
	public void _PURCHASE_STAR(bool purchase)
	{
		if (purchase && (freeShop || coins >= 20))
		{
			if (!freeShop)
				coins -= 20;
			currencySpeedT = 2;
			stars++;
			StartCoroutine(PurchaseStarCo());
			Vector3 dir = Camera.main.transform.position - transform.position;
			model.rotation = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z).normalized);
		}
		else	
			CmdToggleStarCam(false);

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
	IEnumerator PurchaseStarCo()
	{
		isBuyingStar = true;
		anim.SetBool("hasStar", true);

		yield return new WaitForSeconds(2);
		anim.SetBool("hasStar", false);
		CmdToggleStarCam(false);
		bm.CmdChooseStar();
		//StartCoroutine(WaitForNewStarCo());
		yield return new WaitForSeconds(6.5f);
		isBuyingStar = false;
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
		CmdToggleStarCam(false);
		isStop = isAtShop = false;
	}
	public void _ROLL_DICE()
	{
		if (isUsingSpell || inMap) return;

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
		if (currNode != null)
			currNode.RemovePlayer(this);
		CmdUpdateMovesLeft( movesLeft );

		if (canvas != null)
			canvas.SetActive(false);
	}

	public void _TOGGLE_MAP()
	{
		// deactive map
		if (spellCam.activeSelf)
		{
			CmdShowNodeDistance(false, nextNode != null ? nextNode.nodeId : currNode.nodeId, nextNode != null ? 1 : 0, -1);
			inMap = false;
		}
		else
		{
			spellCam.transform.localPosition = new Vector3(0,25,-10);
			CmdShowNodeDistance(true, nextNode != null ? nextNode.nodeId : currNode.nodeId, nextNode != null ? 1 : 0, -1);
			inMap = true;
		}
		
		CmdToggleMapCam(!spellCam.activeSelf);
	}
	
	#endregion


	#region Nodes

	[Command(requiresAuthority=false)] void CmdPlayNodeTraverseVfx(int nodeId) => RpcPlayNodeTraverseVfx(nodeId);
	[ClientRpc] void RpcPlayNodeTraverseVfx(int nodeId) => NodeManager.Instance.GetNode(nodeId).PlayGlowVfx();
	private void StuckAtFork()
	{
		isAtFork = true;
		if (anim != null) anim.SetFloat("moveSpeed", 0);
		HidePaths();
		CmdShowNodeDistance(true, nextNode != null ? nextNode.nodeId : currNode.nodeId, 0, movesLeft);
		spellCam.transform.localPosition = new Vector3(0,25,-10);
		inMap = true;
		CmdToggleMapCam(true);
		if (nextNode != null)
			for (int i=0 ; i<nextNode.nextNodes.Count ; i++)
				RevealPaths(nextNode.nextNodes[i].transform.position, i);
		else
			for (int i=0 ; i<currNode.nextNodes.Count ; i++)
				RevealPaths(currNode.nextNodes[i].transform.position, i);
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
		if (nextNode != null && nextNode.target != null)
		{
			Vector3 dir = (nextNode.target.position - transform.position).normalized;
			RotateDirection(dir);
		}
		CmdToggleStarCam(true);
		shopUi.SetActive(true);
		isStop = false;
	}
	public void OnStarNode()
	{
		isAtStar = true;
		if (anim != null) anim.SetFloat("moveSpeed", 0);
		starUi.SetActive(true);
		if (nextNode != null)
		{
			Vector3 dir = (nextNode.GetTargetTransform().position - transform.position).normalized;
			RotateDirection(dir);
		}
		CmdToggleStarCam(true);
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

	[Command(requiresAuthority=false)] private void CmdToggleStarCam(bool active) => RpcToggleStarCam(active);
	[ClientRpc] private void RpcToggleStarCam(bool active) => starCam.SetActive(active);

	private IEnumerator NodeEffectCo()
	{
		// no event
		if (currNode.GetNodeLandEffect(this))
			yield return new WaitForSeconds(0.5f);
		else 
			yield return new WaitForSeconds(3.5f);

		while (isCurrencyAsync)
			yield return new WaitForSeconds(0.1f);

		EndTurn();
		bm.NextPlayerTurn();
	}

	//private IEnumerator WaitForNewStarCo()
	//{
	//	bm.ChooseStar();
	//	yield return new WaitForSeconds(4.5f);
	//	isBuyingStar = false;
	//}

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
		CmdShowNodeDistance(false, nextNode != null ? nextNode.nodeId : currNode.nodeId, 0, movesLeft);
		nextNode = nextNode.nextNodes[ind];
		HidePaths();
		CmdToggleMapCam(false);
		inMap = isAtFork = false;
	}

	#endregion


	#region Items/Spells
	public void _BUY_ITEM(int itemId)
	{
		Debug.Log($"<color=cyan>BOUGHT ITEM {itemId}</color>");
		if (itemInds.Count < 3)
			itemInds.Add(itemId);
		CmdReplaceItems(itemInds);
		CmdShowItems();
	}
	[Command(requiresAuthority=false)] private void CmdShowItems() => RpcShowItems();
	[ClientRpc] private void RpcShowItems()
	{
		for (int i = 0; i < itemImgs.Length; i++)
		{
			if (itemInds.Count > i)
				itemImgs[i].sprite = Item.instance.GetSprite( itemInds[i] );
			else
				itemImgs[i].sprite = emptySpr;
		}
		for (int i = 0; i < items.Length; i++)
		{
			if (itemInds.Count > i)
			{
				items[i].ind = itemInds[i];
				items[i].SetImage();
			}
			else
			{
				items[i].ind = -1;
				items[i].SetImage();
			}
		}
	}
	[Command(requiresAuthority=false)] private void CmdReplaceItems(List<int> ints) => RpcReplaceItems(ints);
	[ClientRpc(includeOwner=false)] private void RpcReplaceItems(List<int> ints)
	{
		itemInds = ints;
	}
	public void _USE_SPELL(int ind) 
	{
		_spellInd = ind;
		CmdUseSpell(!rangeObj.activeSelf, currNode != null ? currNode.nodeId : -1);
	}
	[Command(requiresAuthority=false)] private void CmdUseSpell(bool active, int nodeId) => RpcUseSpell(active, nodeId);
	[ClientRpc] private void RpcUseSpell(bool active, int nodeId)
	{
		if (!active && nodeId != -1)
			NodeManager.Instance.GetNode(nodeId).SetCanSpellTargetDelay(true);
		else if (active && nodeId != -1) 
			NodeManager.Instance.GetNode(nodeId).SetCanSpellTarget(false);

		if (active)
			spellCam.transform.localPosition = new Vector3(0,25,-10);
		spellCam.SetActive(active);
	 	rangeAnim.SetTrigger(active ? "on" : "off");
		if (isOwned)
		{
			isUsingSpell = active;
			backToBaseUi.SetActive(active);
			ToggleSpellUi(false);
		}
	}
	void ToggleSpellUi(bool active)
	{
		spellUi.SetActive(active);
	}

	//float spellTime;
	//Vector3 spellPos;
	//Vector3 spellEndPos;
	Coroutine spellCo;
	public void UseDashSpell(int dashMove)
	{
		isDashing = true;
		ToggleSpellUi(false);
		CmdToggleDashVfx(true);

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

		movesLeft = dashMove;
		if (currNode != null)
			currNode.RemovePlayer(this);
		CmdUpdateMovesLeft( movesLeft );

		if (canvas != null)
			canvas.SetActive(false);
	}
	[Command(requiresAuthority=false)] private void CmdToggleDashVfx(bool active) => RpcToggleDashVfx(active);
	[ClientRpc] private void RpcToggleDashVfx(bool active)
	{
		if (active) 
			dashSpellPs1.Play(true);
		else
			dashSpellPs1.Stop(true, ParticleSystemStopBehavior.StopEmitting);
	} 


	public void UseShieldSpell()
	{
		ToggleSpellUi(false);
		CmdShieldSpell(true);
		//if (canvas != null)
		//	canvas.SetActive(false);
	}
	[Command(requiresAuthority=false)] private void CmdShieldSpell(bool active) => RpcShieldSpell(active);
	[ClientRpc] private void RpcShieldSpell(bool active)
	{
		shieldSpell1.SetActive(active);
		isShield = active;
	} 
	
	public void UseThornSpell(Node target)
	{
		ToggleSpellUi(false);
		usingFireSpell1 = true;
		if (spellCo == null)
			spellCo = StartCoroutine( ThornCo(target) );
	}
	IEnumerator ThornCo(Node target)
	{
		nodeCam.m_Follow = target.transform;
		CmdSaveTrap(target.nodeId);
		CmdToggleNodeCam(true);

		//yield return new WaitForSeconds(1f);
		//CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera.VirtualCameraGameObject.name;
		Debug.Log($"{CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera.VirtualCameraGameObject.name}");
		
		yield return new WaitForSeconds(1.5f);
		CmdToggleNodeCam(false);
		spellCo = null;
	}
	[Command(requiresAuthority=false)] private void CmdSaveTrap(int nodeId) => gm.SaveTrap(nodeId, id);
	public void UseFireSpell(Node target)
	{
		ToggleSpellUi(false);
		usingFireSpell1 = true;
		if (spellCo == null)
			spellCo = StartCoroutine( SpellCo(target) );
	}
	IEnumerator SpellCo(Node target)
	{
		//yield return new WaitForSeconds(0.5f);
		nodeCam.m_Follow = target.transform;
		CmdToggleNodeCam(true);
		//nodeCam.gameObject.SetActive(true);

		yield return new WaitForSeconds(1f);
		CmdFireSpell1(target.transform.position);
		//fireSpell1.transform.position = target.transform.position;
		//fireSpell1.SetActive(false);
		//fireSpell1.SetActive(true);

		yield return new WaitForSeconds(0.5f);
		gm.CmdHitPlayersAtNode(target.nodeId);
		Debug.Log($"{CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera.VirtualCameraGameObject.name}");
		
		yield return new WaitForSeconds(1.5f);
		CmdToggleNodeCam(false);
		spellCo = null;
	}
	[Command(requiresAuthority=false)] private void CmdToggleNodeCam(bool active) => RpcToggleNodeCam(active);
	[ClientRpc] private void RpcToggleNodeCam(bool active) => nodeCam.gameObject.SetActive(active);
	[Command(requiresAuthority=false)] private void CmdFireSpell1(Vector3 target) => RpcFireSpell1(target);
	[ClientRpc] private void RpcFireSpell1(Vector3 target)
	{
		fireSpell1.transform.position = target;
		fireSpell1.SetActive(false);
		fireSpell1.SetActive(true);
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
	[Command(requiresAuthority=false)] public void CmdPlayerToggle(bool active) => RpcPlayerToggle(active);
	[ClientRpc] private void RpcPlayerToggle(bool active) => model.gameObject.SetActive(active);

	[Command(requiresAuthority=false)] public void CmdRagdollToggle(bool active) => RpcRagdollToggle(active);
	[ClientRpc] private void RpcRagdollToggle(bool active) 
	{
		if (active)
		{
			Rigidbody[] bones = ragdollObj[characterInd].GetComponentsInChildren<Rigidbody>();
			foreach (Rigidbody bone in bones) {
				bone.velocity = Vector3.up * ragdollKb;
				bone.angularVelocity = Vector3.forward * ragdollKb;
			}
		}
		ragdollObj[characterInd].transform.rotation = model.rotation;
		ragdollObj[characterInd].SetActive(active);
	}
	//IEnumerator MoveCo()
	//{
	//	yield return new WaitForSeconds(2);
	//	UpdateMovesLeft( Random.Range(1, 11) );
	//}
}

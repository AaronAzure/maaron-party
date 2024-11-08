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
	[SerializeField] private int controlledRoll=-1;
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
	[SyncVar] public int boardOrder;

	
	
	[Space] [Header("Model")]
	[SerializeField] private Transform model;
	[SerializeField] private GameObject[] models;
	[SerializeField] private Animator anim;
	[SerializeField] private Transform vCam;
	[SerializeField] private GameObject starCam;
	[SerializeField] private GameObject doorCam;
	[SerializeField] private CinemachineVirtualCamera nodeCam;
	[SerializeField] private GameObject rangeObj; // trap
	[SerializeField] private GameObject rangeObj2; // fireball
	[SerializeField] private Animator rangeAnim; // trap
	[SerializeField] private Animator rangeAnim2; // fireball

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

	[Space] [SerializeField] private GameObject melon;
	[SerializeField] private Transform upPos;
	[SerializeField] private Transform downPos;
	[SerializeField] private Transform leftPos;
	[SerializeField] private Transform rightPos;
	[SerializeField] private Transform upLeftPos;
	[SerializeField] private Transform upRightPos;
	[SerializeField] private Transform downLeftPos;
	[SerializeField] private Transform downRightPos;


	[Space] [SerializeField] private int movesLeft;
	//[SerializeField] private TextMeshPro movesLeftTxt;
	[SerializeField] private TextMeshProUGUI movesLeftTxt;


	[Space] [Header("Vfx")]
	[SerializeField] private GameObject maaronFireVfx;
	[SerializeField] private GameObject maaronSpotlightVfx;


	[Space] [Header("Ragdoll")]
	[SerializeField] private GameObject shoveObj;
	[SerializeField] private GameObject[] ragdollObj;
	//[SerializeField] private Rigidbody[] ragdollRb;
	[SerializeField] private float ragdollKb=15;
	
	
	#region Stats

	[Space] [Header("Stats")]
	[SyncVar] private int coins=10;
	private int coinsT;
	[SyncVar] private int stars;
	private int starsT;
	[SyncVar] [SerializeField] private int mana=5;
	//private int manaT;
	[SerializeField] private float currencyT;
	private float currencySpeedT=0.5f;

	#endregion


	#region UI

	[Space] [Header("UI")]
	[SerializeField] private GameObject canvas;
	[SerializeField] private GameObject starUi;
	[SerializeField] private GameObject shopUi;
	[SerializeField] private GameObject spellUi;
	[SerializeField] private GameObject fullUi;
	[SerializeField] private GameObject doorUi;
	[SerializeField] private TextMeshProUGUI doorTollTxt;
	[SerializeField] private GameObject tollUi;
	[SerializeField] private Slider tollSldr;
	[SerializeField] private TextMeshProUGUI tollTxt;
	[SerializeField] private GameObject brokeUi;
	[SerializeField] private GameObject backToBaseUi;
	[SerializeField] private GameObject baseUi;

	[Space] [SerializeField] private Animator introAnim;
	[SerializeField] private Button introBtn;
	[SerializeField] private Image introImg;
	[SerializeField] private GameObject clickAnywhereUi;
	[SerializeField] private TextMeshProUGUI introTxt;
	[SerializeField] private GameObject[] profilePics;

	[Space] [SerializeField] private Slider manaSld;
	[SerializeField] private TextMeshProUGUI manaTxt;

	[Space] [SerializeField] private GameObject manalessUi;
	[SerializeField] private GameObject coinlessUi;
	[SerializeField] private TextMeshProUGUI thinkingTxt;

	#endregion
	
	[Space] [SerializeField] private RectTransform dataUi;
	[SerializeField] private GameObject[] profileUis;
	[SerializeField] private Image dataImg;
	[SerializeField] private TextMeshProUGUI distanceTxt;
	[SerializeField] private TextMeshProUGUI coinTxt;
	[SerializeField] private TextMeshProUGUI starTxt;

	[Space] [SerializeField] private RectTransform finalUi;
	[SerializeField] private TextMeshProUGUI finalCoinsTxt;
	[SerializeField] private TextMeshProUGUI finalStarsTxt;

	private BoardManager bm { get { return BoardManager.Instance; } }
	private GameNetworkManager nm { get { return GameNetworkManager.Instance; } }
	private GameManager gm { get { return GameManager.Instance; } }


	#region States

	[Space] [Header("States")]
	[SerializeField] private bool canMove;
	[SerializeField] private bool isAtFork;
	[SerializeField] private bool isAtStar;
	[SerializeField] private bool isBuyingStar;
	[SerializeField] private bool isAtShop;
	[SerializeField] private bool isAtDoor;
	[SerializeField] private bool isStop;
	[SerializeField] private bool isUsingSpell;
	[SerializeField] private bool inMap;
	[SerializeField] private bool isDashing;
	[SerializeField] private bool inSpellAnimation;
	[SyncVar] bool yourTurn;
	public bool isShield {get; private set;}
	bool usingFireSpell1;
	private bool isCurrencyAsync;

	#endregion



	[Space] [Header("Shop")]
	[SerializeField] private Button[] shopItems;
	[SyncVar] [SerializeField] List<int> itemInds = new();
	[SerializeField] Image[] itemImgs;
	[SerializeField] private Sprite emptySpr;
	[SerializeField] private GameObject buyBtnObj;
	[SerializeField] private TextMeshProUGUI titleTxt;
	[SerializeField] private TextMeshProUGUI descTxt;
	[SerializeField] private TextMeshProUGUI manaCostTxt;


	#region Spell Vars

	[Space] [Header("Spell")]
	[SerializeField] private Item[] items;
	[SerializeField] private GameObject spellCam;
	[SerializeField] private float spellCamSpeed=0.5f;
	[SerializeField] private GameObject fireSpell1;
	[SerializeField] private GameObject fireSpell2;
	[SerializeField] private GameObject fireSpell3;
	[SerializeField] private ParticleSystem dashSpellPs1;
	[SerializeField] private GameObject shieldSpell1;
	[SerializeField] private int newSpellId;
	[SerializeField] private ItemShopButton[] newSpellBtns;
	public int _spellInd {get; private set;} 
	public int _spellSlot {get; private set;} 

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
		//Debug.Log($"<color=#FF9900>PLAYER DISCONNECT ({isOwned}) | {isServer} | {yourTurn}</color>");
		base.OnStopClient();
		if (isOwned)
		{
			nm.RemoveBoardConnection(this);
		}
		if (isServer && yourTurn)
			bm.NextPlayerTurn();
	}


	public void MediateRemoteStart()
	{
		TargetRemoteStart(netIdentity.connectionToClient);
	}
	
	// Start()
	[TargetRpc] private void TargetRemoteStart(NetworkConnectionToClient target) 
	{
		//Debug.Log($"<color=yellow>TargetRemoteStart</color>");
		player = ReInput.players.GetPlayer(0);
		if (vCam != null)
			vCam.parent = null;
		
		CmdSetModel(characterInd);
		model.rotation = Quaternion.LookRotation(Vector3.back);

		// first turn
		if (gm.nTurn == 1)
		{
			transform.position = BoardManager.Instance.GetSpawnPos().position + new Vector3(-4 + 2*id,0,-2);
			startPos = this.transform.position;
			if (nm.skipIntro)
				CmdSetDataUi(id);
		}
		// game over
		else if (gm.nTurn > gm.maxTurns)
		{
			transform.position = BoardManager.Instance.GetSpawnPos().position + new Vector3(-4 + 2*id,0,-2);
			coinsT = coins = gm.GetCoins(id);
			starsT = stars = gm.GetStars(id);
			return;
		}
		
		if (!isOwned) {
			enabled = false;
			return;
		}
		
		// after first turn
		if (gm.nTurn > 1)
		{
			LoadData();
			CmdSetDataUi(nm.skipIntro ? id : gm.GetPlacements(id));
		}
		// 第一名 
		else
		{
			coinsT = coins;
			starsT = stars;
		}
		
		CmdSetCoinText(coins);
		CmdSetStarText(stars);
		CmdShowMana();
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

		//if (bm != null)
		//	bm.SetUiLayout(dataUi);
		//dataUi.gameObject.SetActive(true);
		dataImg.color = ind == 0 ? new Color(0.7f,0.13f,0.13f) : ind == 1 ? new Color(0.4f,0.7f,0.3f) 
				: ind == 2 ? new Color(0.85f,0.85f,0.5f) : new Color(0.7f,0.5f,0.8f);
		for (int i=0 ; i<profileUis.Length ; i++)
			profileUis[i].SetActive(i == ind);
		for (int i=0 ; i<profilePics.Length ; i++)
			profilePics[i].SetActive(i == ind);
		introTxt.color = ind == 0 ? new Color(0.7f,0.13f,0.13f) : ind == 1 ? new Color(0.4f,0.7f,0.3f) 
				: ind == 2 ? new Color(0.85f,0.85f,0.5f) : new Color(0.7f,0.5f,0.8f);
		switch (ind)
		{
			case 0: introTxt.text = "Red's Turn!"; break;
			case 1: introTxt.text = "Green's Turn!"; break;
			case 2: introTxt.text = "Yellow's Turn!"; break;
			case 3: introTxt.text = "Periwinkle's Turn!"; break;
			default: introTxt.text = "Someone's Turn!"; break;
		}
		introImg.color = ind == 0 ? new Color(0.7f,0.13f,0.13f) : ind == 1 ? new Color(0.4f,0.7f,0.3f) 
				: ind == 2 ? new Color(0.85f,0.85f,0.5f) : new Color(0.7f,0.5f,0.8f);
	}

	[Command(requiresAuthority=false)] public void CmdSetDataUi(int n) => RpcSetDataUi(n);
	[ClientRpc] private void RpcSetDataUi(int n)
	{
		dataUi.anchoredPosition = new Vector3(125, -137.5f - 175 * n);
		dataUi.gameObject.SetActive(true);
	}

	[Command(requiresAuthority=false)] public void CmdSetFinalUi() => RpcSetFinalUi(id, coins, stars);
	[ClientRpc] private void RpcSetFinalUi(int n, int coins, int stars)
	{
		finalUi.anchoredPosition = new Vector3(400 + 250 * n, -650);
		finalUi.gameObject.SetActive(true);
		finalCoinsTxt.text = $"{coins}";
		finalStarsTxt.text = $"{stars}";
	}

	[Command(requiresAuthority=false)] public void CmdSetOrder(int order) => boardOrder = order;

	public void SetStartNode(Node startNode) => nextNode = startNode;

	[Command(requiresAuthority=false)] private void CmdSetCoinText(int n) => RpcSetCoinText(n);
	[ClientRpc] private void RpcSetCoinText(int n) => coinTxt.text = $"{n}";
	[Command(requiresAuthority=false)] private void CmdSetStarText(int n) => RpcSetStarText(n);
	[ClientRpc] private void RpcSetStarText(int n) => starTxt.text = $"{n}";


	[Command(requiresAuthority=false)] void CmdPlayerThinking(bool active, int ind) => RpcPlayerThinking(active, ind); 
	[ClientRpc(includeOwner=false)] void RpcPlayerThinking(bool active, int ind)
	{
		if (!isOwned)
		{
			thinkingTxt.gameObject.SetActive(active);
			//introTxt.color = ind == 0 ? new Color(0.7f,0.13f,0.13f) : ind == 1 ? new Color(0.4f,0.7f,0.3f) 
			//	: ind == 2 ? new Color(0.85f,0.85f,0.5f) : new Color(0.7f,0.5f,0.8f);
			switch (ind)
			{
				case 0: thinkingTxt.text = "Red is Thinking..."; break;
				case 1: thinkingTxt.text = "Green is Thinking..."; break;
				case 2: thinkingTxt.text = "Yellow is Thinking..."; break;
				case 3: thinkingTxt.text = "Periwinkle is Thinking..."; break;
				default: thinkingTxt.text = "Someone is Thinking..."; break;
			}
		}
	}

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
			if (currencyT < currencySpeedT)
			{
				currencyT += Time.fixedDeltaTime;
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
				if (!yourTurn)
				{
					SaveData();
					this.enabled = false;
				}
			}
		}
		else if (stars != starsT)
		{
			if (currencyT < currencySpeedT)
			{
				currencyT += Time.fixedDeltaTime;
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
		else if (isAtDoor) {}
		else if (isAtShop) {}
		else if (isBuyingStar) {}
		else if (!canMove) {}
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
						canMove = isDashing = false;
						if (nextNode.nextNodes.Count == 1)
							nextNode = nextNode.nextNodes[0];
					}
				}
				// still moving
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
						IsStuckAtDoor();
					}
				}
			}
		}
	}

	#endregion

	public void YourTurn()
	{
		//Debug.Log("<color=cyan>YOUR TURN!!\n currNode != null => {currNode != null} nextNode != null => {nextNode != null}</color>");
		CmdCamToggle(true);
		TargetYourTurn(netIdentity.connectionToClient);
	}
	private void ShowDistanceAway(int n)
	{
		distanceTxt.text = n == -1 ? "? spaces away" : n == 1 ? "1 space away" : $"{n} spaces away";
	}
	[Command(requiresAuthority=false)] public void CmdCamToggle(bool activate) => RpcCamToggle(activate);
	[ClientRpc] private void RpcCamToggle(bool activate) => vCam.gameObject.SetActive(activate);
	[Command(requiresAuthority=false)] private void CmdShoveToggle(bool activate) => RpcShoveToggle(activate);
	[ClientRpc(includeOwner=false)] private void RpcShoveToggle(bool activate) => shoveObj.SetActive(activate);
	[TargetRpc] public void TargetYourTurn(NetworkConnectionToClient target)
	{
		//if (canvas != null)
		//	canvas.SetActive(true);
		if (currNode != null)
			ShowDistanceAway(currNode.GetDistanceAway(0));
		else if (nextNode != null)
			ShowDistanceAway(nextNode.GetDistanceAway(1));
		this.enabled = true;
		CmdToggleYourTurn(true);
		CmdToggleIntroUi(true, characterInd);
		if (ragdollObj[characterInd].activeSelf)
		{
			CmdPlayerToggle(true);
			CmdRagdollToggle(false);
		}
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
		CmdToggleYourTurn(false);
		SaveData();
	}
	[Command(requiresAuthority=false)] private void CmdToggleYourTurn(bool active) => yourTurn = active;
	[Command(requiresAuthority=false)] void CmdToggleIntroUi(bool active, int id) => RpcToggleIntroUi(active, id);
	[ClientRpc] void RpcToggleIntroUi(bool active, int id)
	{
		introBtn.interactable = isOwned;
		clickAnywhereUi.SetActive(isOwned);

		if (active)
			introAnim.gameObject.SetActive(active);
		else
			introAnim.SetTrigger("close");
	}


	#region Saving data
	[Command(requiresAuthority=false)] public void CmdSaveData() => TargetSaveData(netIdentity.connectionToClient);
	[TargetRpc] public void TargetSaveData(NetworkConnectionToClient target) 
	{
		gm.SaveCurrNode(currNode.nodeId, id);
		gm.CmdSaveCoins(coins, id);
		gm.CmdSaveStars(stars, id);
		gm.CmdSaveMana(mana, id);
		gm.CmdSaveItems(itemInds, id);
		CmdDataSaved();
	}
	[Command(requiresAuthority=false)] void CmdDataSaved() => nm.IncreasePlayerDataSaved();
	
	private void SaveData()
	{
		gm.SaveCurrNode(currNode.nodeId, id);
		gm.CmdSaveCoins(coins, id);
		gm.CmdSaveStars(stars, id);
		gm.CmdSaveMana(mana, id);
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
		mana = gm.GetMana(id);
		itemInds = gm.GetItems(id);
	}

	#endregion


	#region BUTTONS
	public void _START_PLAYER()
	{
		CmdToggleIntroUi(false, characterInd);
		if (canvas != null)
			canvas.SetActive(true);
	}
	public void _BACK_TO_BASE_UI()
	{
		// show base ui
		if (rangeObj.activeSelf)
			CmdUseSpell(false, currNode != null ? currNode.nodeId : -1);
		if (rangeObj2.activeSelf)
			CmdUseSpell2(false, currNode != null ? currNode.nodeId : -1);
		backToBaseUi.SetActive(false);
		spellUi.SetActive(false);
		baseUi.SetActive(true);
	}
	public void StopUsingSpells()
	{
		// show base ui
		isUsingSpell = false;
		ToggleSpellUi(false);
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
			//ToggleSpellUi(true);
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
			stars++;
			currencySpeedT = 0.025f;
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
			IsStuckAtDoor();
		}

		if (starUi != null)
			starUi.SetActive(false);
		if (brokeUi != null)
			brokeUi.SetActive(false);
		isStop = isAtStar = false;
	}
	IEnumerator PurchaseStarCo()
	{
		isBuyingStar = true;
		anim.SetBool("hasStar", true);
		bm.CmdMaaronClap(true);

		yield return new WaitForSeconds(2);
		bm.CmdMaaronClap(false);
		anim.SetBool("hasStar", false);
		CmdToggleStarCam(false);
		bm.CmdChooseStar();
		//StartCoroutine(WaitForNewStarCo());
		yield return new WaitForSeconds(9.5f);
		isBuyingStar = false;
	}

	public bool HasFreeShop() => freeShop;
	public void _BUY_ITEM(int itemId, int itemCost)
	{
		if (!freeShop)
			NodeEffect(-itemCost);

		if (itemInds.Count < 3)
		{
			itemInds.Add(itemId);
			CmdReplaceItems(itemInds);
			CmdShowItems();
			if (!freeShop)
				StartCoroutine( CloseShopCo() );
		}
		// inventory full!
		else
		{
			newSpellId = itemId;
			for (int i=0 ; i<newSpellBtns.Length ; i++)
				newSpellBtns[i].ind = i >= 0 && i < itemInds.Count ? itemInds[i] : itemId;
			fullUi.SetActive(true);
			shopUi.SetActive(false);
		}
	}
	public void _REPLACE_ITEM(int ind)
	{
		// replaced item
		if (ind >= 0 && ind < itemInds.Count)
		{
			itemInds[ind] = newSpellId;
			CmdReplaceItems(itemInds);
			CmdShowItems();
			fullUi.SetActive(false);
			//if (isAtShop)
			//	shopUi.SetActive(true);
			StartCoroutine( CloseShopCo() );
		}
		else
		{
			fullUi.SetActive(false);
			if (isAtShop)
				shopUi.SetActive(true);
		}
	}
	public void _OPEN_TOLL()
	{
		doorUi.SetActive(false);
		tollUi.SetActive(true);
		tollTxt.text = $"Spend: <b>{tollSldr.value}</b> (Next person pays: <b>{tollSldr.value+1}</b>)";
		//tollTxt.text = $"Set New Toll: <b>{gm.GetDoorToll(_doorInd)}<b>";
		tollSldr.minValue = gm.GetDoorToll(_doorInd);
		tollSldr.maxValue = coins;
	}
	public void _SET_NEW_TOLL_TEXT()
	{
		tollTxt.text = $"Spend: <b>{tollSldr.value}</b> (Next person pays: <b>{tollSldr.value+1}</b>)";
		//gm.SetDoorToll(_doorInd, (int)tollSldr.value);
	}
	public void _PAY_DOOR_TOLL()
	{
		NodeEffect(-(int)tollSldr.value);
		bm.CmdPlayDoorAnim(nextNode.nodeId);
		isAtDoor = false;
		doorUi.SetActive(false);
		tollUi.SetActive(false);
		brokeUi.SetActive(false);
		nextNode = nextNode.nextNodes[0];
		CmdToggleDoorCam(false);
		gm.CmdSetDoorToll(_doorInd, (int)tollSldr.value+1);
	}
	public void _CANCEL_DOOR_TOLL()
	{
		if (isAtDoor)
			StartCoroutine(CancelDoorTollCo());
		else if (isAtStar)
			_PURCHASE_STAR(false);
	}
	IEnumerator CancelDoorTollCo()
	{
		doorUi.SetActive(false);
		tollUi.SetActive(false);
		brokeUi.SetActive(false);
		CmdToggleDoorCam(false);
		yield return new WaitForSeconds(1);
		nextNode = nextNode.altNode;
		isAtDoor = false;
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
			IsStuckAtDoor();
		}

		if (shopUi != null)
			shopUi.SetActive(false);
		CmdToggleStarCam(false);
		CmdPlayerThinking(false, characterInd);

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
				IsStuckAtDoor();
			}
		}

		int rng = controlledRoll != -1 ? controlledRoll : Random.Range(1, 11);
		//Debug.Log($"<color=magenta>ROLLED {rng}</color>");
		movesLeft = rng;
		if (currNode != null)
			currNode.RemovePlayer(this);
		CmdUpdateMovesLeft( movesLeft );
		CmdResetMovesLeft(true);

		if (canvas != null)
			canvas.SetActive(false);
		StartCoroutine(MoveCo());
	}
	IEnumerator MoveCo()
	{
		yield return new WaitForSeconds(0.5f);
		canMove = true;
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
		CmdPlayerThinking(true, characterInd);
		if (nextNode != null)
		{
			int shortestPath = 9999;
			int shortestInd = 0;
			for (int i=0 ; i<nextNode.nextNodes.Count ; i++)
			{
				RevealPaths(nextNode.nextNodes[i].transform.position, i);
				int distance = nextNode.nextNodes[i].GetDistanceAway(1);
				if (shortestPath > distance)
				{
					shortestPath = distance;
					shortestInd = i;
				}
			}
			if (shortestInd < nextNode.nextNodes.Count && nextNode.nextNodes[shortestInd] != null)
				ShowShortestPath(nextNode.nextNodes[shortestInd].transform.position);
		}
		else
		{
			int shortestPath = 9999;
			int shortestInd = 0;
			for (int i=0 ; i<currNode.nextNodes.Count ; i++)
			{
				RevealPaths(currNode.nextNodes[i].transform.position, i);
				int distance = currNode.nextNodes[i].GetDistanceAway(1);
				if (shortestPath > distance)
				{
					shortestPath = distance;
					shortestInd = i;
				}
			}
			if (shortestInd < currNode.nextNodes.Count && currNode.nextNodes[shortestInd] != null)
				ShowShortestPath(currNode.nextNodes[shortestInd].transform.position);
		}
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

	private void IsStuckAtDoor()
	{
		if (nextNode.IsDoor()) OnDoorNode(nextNode.GetDoorInd());
	}
	int _doorInd;
	public void OnDoorNode(int doorInd)
	{
		isAtDoor = true;
		_doorInd = doorInd;
		var lookPos = nextNode.transform.position - transform.position;
		lookPos.y = 0;
		model.rotation = Quaternion.LookRotation(lookPos);

		if (doorTollTxt != null)
			doorTollTxt.text = $"Pay Toll: <color=yellow>{gm.GetDoorToll(_doorInd)}</color> Coins to Pass";

		if (anim != null) anim.SetFloat("moveSpeed", 0);
		if (nextNode != null && nextNode.target != null)
		{
			Vector3 dir = (nextNode.target.position - transform.position).normalized;
			RotateDirection(dir);
		}
		CmdToggleDoorCam(true);
		if (coins >= gm.GetDoorToll(_doorInd))
			doorUi.SetActive(true);
		else
			brokeUi.SetActive(true);
		isStop = false;
	}

	public void OnShopNode()
	{
		isAtShop = true;
		CmdPlayerThinking(true, characterInd);

		if (buyBtnObj != null) buyBtnObj.SetActive(false);
		if (titleTxt != null) titleTxt.text = "Click on an Item!";
		if (descTxt != null) descTxt.text = "I'm waiting...";
		if (manaCostTxt != null) manaCostTxt.text = ":D";

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

		if (coins >= 20)
			starUi.SetActive(true);
		else
			brokeUi.SetActive(true);
		if (nextNode != null)
		{
			Vector3 dir = (nextNode.GetTargetTransform().position - transform.position).normalized;
			RotateDirection(dir);
		}
		CmdToggleStarCam(true);
		isStop = false;
	}
	public int GetMana() => mana;
	public int GetCoins() => coins;
	public int GetStars() => stars;
	[Command(requiresAuthority=false)] public void CmdNodeEffect(int bonus, bool isStar) 
		=> TargetNodeEffect(netIdentity.connectionToClient, bonus, isStar);
	[TargetRpc] public void TargetNodeEffect(NetworkConnectionToClient target, int bonus, bool isStar) 
		=> NodeEffect(bonus, isStar);
	public void NodeEffect(int bonus, bool isStar=false)
	{
		// star related
		if (isStar)
		{
			currencySpeedT = 0.05f;
			stars = Mathf.Max(stars+bonus, 0);
			if (stars != 0)
				this.enabled = isCurrencyAsync = true;
		}
		// coin related
		else
		{
			if (bonus != 0) currencySpeedT = 0.5f / Mathf.Abs(bonus);
			coins = Mathf.Clamp(coins + bonus, 0, 999);
			if (coins != 0)
				this.enabled = isCurrencyAsync = true;
		}
		CmdShowBonusTxt(bonus, isStar);
	}
	public void LoseAllCoins()
	{
		//Debug.Log($"<color=white>{name} LOST ALL COINS</color>");
		if (coins != 0) currencySpeedT = 0.5f / coins;
		int temp = coins;
		coins = 0;
		this.enabled = isCurrencyAsync = true;
		CmdShowBonusTxt(-temp, false);
	}
	[Command] private void CmdShowBonusTxt(int n, bool isStar) => RpcShowBonusTxt(n, isStar);
	[ClientRpc] private void RpcShowBonusTxt(int n, bool isStar)
	{
		bonusObj.SetActive(false);
		bonusObj.SetActive(true);
		bonusTxt.text = isStar ? "<sprite name=\"star\">" : $"<sprite name=\"coin\">";
		bonusTxt.text += n >= 0 ? $"+{n}" : $"{n}";
	}

	[Command(requiresAuthority=false)] private void CmdToggleStarCam(bool active) => RpcToggleStarCam(active);
	[ClientRpc] private void RpcToggleStarCam(bool active) => starCam.SetActive(active);
	[Command(requiresAuthority=false)] private void CmdToggleDoorCam(bool active) => RpcToggleDoorCam(active);
	[ClientRpc] private void RpcToggleDoorCam(bool active) => doorCam.SetActive(active);

	private IEnumerator NodeEffectCo()
	{
		CmdTriggerNodeVfx(currNode.nodeId); // vfx on node
		yield return new WaitForSeconds(currNode.GetNodeLandEffect(this)); // event duration
		// no event
		//if (currNode.GetNodeLandEffect(this))
		//else 
		//	yield return new WaitForSeconds(3.5f);

		while (isCurrencyAsync)
			yield return new WaitForSeconds(0.1f);

		EndTurn();
		bm.NextPlayerTurn();
	}
	[Command(requiresAuthority=false)] void CmdTriggerNodeVfx(int nodeId) => RpcTriggerNodeVfx(nodeId);
	[ClientRpc] void RpcTriggerNodeVfx(int nodeId) => NodeManager.Instance.GetNode(nodeId).TriggerNodeLandVfx();

	//private IEnumerator WaitForNewStarCo()
	//{
	//	bm.ChooseStar();
	//	yield return new WaitForSeconds(4.5f);
	//	isBuyingStar = false;
	//}

	void HidePaths()
	{
		melon.SetActive(false);
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
	void ShowShortestPath(Vector3 nextPos)
	{
		Vector3 toPos = (nextPos - startPos).normalized;
		bool goingUp = toPos.z >= 0.33f;
		bool goingDown = toPos.z <= -0.33f;
		bool goingRight = toPos.x >= 0.33f;
		bool goingLeft = toPos.x <= -0.33f;

		melon.SetActive(true);
		if (goingUp && !goingLeft && !goingRight)
			melon.transform.position = upPos.position;
		else if (goingDown && !goingLeft && !goingRight)
			melon.transform.position = downPos.position;
		else if (goingLeft && !goingUp && !goingDown)
			melon.transform.position = leftPos.position;
		else if (goingRight && !goingUp && !goingDown)
			melon.transform.position = rightPos.position;
		else if (goingUp && goingLeft)
			melon.transform.position = upLeftPos.position;
		else if (goingUp && goingRight)
			melon.transform.position = upRightPos.position;
		else if (goingDown && goingLeft)
			melon.transform.position = downLeftPos.position;
		else if (goingDown && goingRight)
			melon.transform.position = downRightPos.position;
	}

	public void ChoosePath(int ind)
	{
		CmdShowNodeDistance(false, nextNode != null ? nextNode.nodeId : currNode.nodeId, 0, movesLeft);
		nextNode = nextNode.nextNodes[ind];
		CmdPlayerThinking(false, characterInd);
		IsStuckAtDoor();
		HidePaths();
		CmdToggleMapCam(false);
		inMap = isAtFork = false;
	}

	#endregion


	#region Items/Spells
	public void NoManaAlert() 
	{
		if (manalessUi != null)
		{
			manalessUi.SetActive(false);
			manalessUi.SetActive(true);
		}
	}
	public void NoCoinAlert() 
	{
		if (coinlessUi != null)
		{
			coinlessUi.SetActive(false);
			coinlessUi.SetActive(true);
		}
	}
	public void ConsumeMana(int cost)
	{
		mana = Mathf.Max(mana - cost, 0);
		CmdShowMana();
	}
	[Command(requiresAuthority=false)] private void CmdShowMana() => RpcShowMana();
	[ClientRpc] private void RpcShowMana() 
	{
		manaSld.value = mana;
		manaTxt.text = $"{mana}/{manaSld.maxValue}";
	}

	private void RemoveSpell(int ind)
	{
		if (itemInds != null && ind >= 0 && ind < itemInds.Count)
			itemInds.RemoveAt(ind);
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
	
	public void _USE_SPELL(int slot, int ind) 
	{
		_spellSlot = slot;
		_spellInd = ind;
		CmdUseSpell(!rangeObj.activeSelf, currNode != null ? currNode.nodeId : -1);
	}
	public void _USE_SPELL_2(int slot, int ind) 
	{
		_spellSlot = slot;
		_spellInd = ind;
		CmdUseSpell2(!rangeObj.activeSelf, currNode != null ? currNode.nodeId : -1);
	}
	
	[Command(requiresAuthority=false)] private void CmdToggleRange(
		bool active, int nodeId) => RpcToggleRange(active, nodeId);
	[ClientRpc] private void RpcToggleRange(bool active, int nodeId)
	{
		if (!active && nodeId != -1)
			NodeManager.Instance.GetNode(nodeId).SetCanSpellTargetDelay(true);
		else if (active && nodeId != -1) 
			NodeManager.Instance.GetNode(nodeId).SetCanSpellTarget(false);

		if (active)
			spellCam.transform.localPosition = new Vector3(0,25,-10);
		spellCam.SetActive(active);
		rangeAnim.SetTrigger(active ? "on" : "off");
	}
	[Command(requiresAuthority=false)] private void CmdToggleRange2(
		bool active, int nodeId) => RpcToggleRange2(active, nodeId);
	[ClientRpc] private void RpcToggleRange2(bool active, int nodeId)
	{
		if (!active && nodeId != -1)
			NodeManager.Instance.GetNode(nodeId).SetCanSpellTargetDelay(true);
		else if (active && nodeId != -1) 
			NodeManager.Instance.GetNode(nodeId).SetCanSpellTarget(false);

		if (active)
			spellCam.transform.localPosition = new Vector3(0,25,-10);
		spellCam.SetActive(active);
		rangeAnim2.SetTrigger(active ? "on" : "off");
	}
	
	[Command(requiresAuthority=false)] private void CmdUseSpell(bool active, int nodeId) 
		=> RpcUseSpell(active, nodeId);
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
	[Command(requiresAuthority=false)] private void CmdUseSpell2(bool active, int nodeId) 
		=> RpcUseSpell2(active, nodeId);
	[ClientRpc] private void RpcUseSpell2(bool active, int nodeId)
	{
		if (!active && nodeId != -1)
			NodeManager.Instance.GetNode(nodeId).SetCanSpellTargetDelay(true);
		else if (active && nodeId != -1) 
			NodeManager.Instance.GetNode(nodeId).SetCanSpellTarget(false);

		if (active)
			spellCam.transform.localPosition = new Vector3(0,25,-10);
		spellCam.SetActive(active);
	 	rangeAnim2.SetTrigger(active ? "on" : "off");
		if (isOwned)
		{
			isUsingSpell = active;
			backToBaseUi.SetActive(active);
			ToggleSpellUi(false);
		}
	}
	void ToggleSpellUi(bool active) => spellUi.SetActive(active);

	IEnumerator CloseShopCo()
	{
		if (shopUi != null)
			shopUi.SetActive(false);
		CmdUpdateMovesLeft(0);

		yield return new WaitForSeconds(0.5f);
		CmdUpdateMovesLeft(movesLeft);
		startPos = transform.position;
		// more than one path
		if (nextNode.nextNodes.Count > 1)
			StuckAtFork();
		// single path
		else
			nextNode = nextNode.nextNodes[0];

		CmdToggleStarCam(false);
		CmdPlayerThinking(false, characterInd);
		isStop = isAtShop = false;
	}

	//float spellTime;
	//Vector3 spellPos;
	//Vector3 spellEndPos;
	Coroutine spellCo;
	public void UseDashSpell(int dashMove, int manaCost)
	{
		isDashing = true;
		ToggleSpellUi(false);
		CmdToggleDashVfx(true);
		RemoveSpell(_spellSlot);
		ConsumeMana(manaCost);

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
				IsStuckAtDoor();
			}
		}

		movesLeft = dashMove;
		if (currNode != null)
			currNode.RemovePlayer(this);
		CmdUpdateMovesLeft( movesLeft );

		if (canvas != null)
			canvas.SetActive(false);
		canMove = true;
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
	
	public void UseThornSpell(Node target, int manaCost, int trapId)
	{
		ToggleSpellUi(false);
		backToBaseUi.SetActive(false);

		if (spellCo == null)
			spellCo = StartCoroutine( ThornCo(target, manaCost, trapId) );
	}
	IEnumerator ThornCo(Node target, int manaCost, int trapId)
	{
		nodeCam.m_Follow = target.transform;
		CmdSaveTrap(target.nodeId, trapId);
		CmdToggleNodeCam(true);
		CmdToggleRange(false, currNode != null ? currNode.nodeId : -1);
		RemoveSpell(_spellSlot);
		ConsumeMana(manaCost);


		//yield return new WaitForSeconds(1f);
		//CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera.VirtualCameraGameObject.name;
		//Debug.Log($"{CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera.VirtualCameraGameObject.name}");
		
		yield return new WaitForSeconds(1.5f);
		CmdToggleNodeCam(false);
		spellCo = null;
		StopUsingSpells();
	}
	[Command(requiresAuthority=false)] private void CmdSaveTrap(int nodeId, int trapId) => gm.SaveTrap(nodeId, id, trapId);
	public void UseFireSpell(Node target, int manaCost, int fireSpellInd)
	{
		ToggleSpellUi(false);
		backToBaseUi.SetActive(false);

		usingFireSpell1 = true;
		if (spellCo == null)
			spellCo = StartCoroutine( SpellCo(target, manaCost, fireSpellInd) );
	}
	IEnumerator SpellCo(Node target, int manaCost, int fireSpellInd)
	{
		nodeCam.m_Follow = target.transform;
		CmdToggleNodeCam(true);
		CmdToggleRange2(false, currNode != null ? currNode.nodeId : -1);
		RemoveSpell(_spellSlot);
		ConsumeMana(manaCost);

		yield return new WaitForSeconds(1f);
		if (fireSpellInd == 1) CmdFireSpell1(target.transform.position);
		if (fireSpellInd == 2) CmdFireSpell2(target.transform.position);
		if (fireSpellInd == 3) CmdFireSpell3(target.transform.position);

		yield return new WaitForSeconds(0.5f);
		if (fireSpellInd == 1) gm.CmdHitPlayersAtNode(target.nodeId, -15);
		if (fireSpellInd == 2) gm.CmdHitPlayersAtNode(target.nodeId, -25);
		if (fireSpellInd == 3) gm.CmdHitPlayersStarsAtNode(target.nodeId);
		
		yield return new WaitForSeconds(1.5f);
		CmdToggleNodeCam(false);
		spellCo = null;
		StopUsingSpells();
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
	[Command(requiresAuthority=false)] private void CmdFireSpell2(Vector3 target) => RpcFireSpell2(target);
	[ClientRpc] private void RpcFireSpell2(Vector3 target)
	{
		fireSpell2.transform.position = target;
		fireSpell2.SetActive(false);
		fireSpell2.SetActive(true);
	}
	[Command(requiresAuthority=false)] private void CmdFireSpell3(Vector3 target) => RpcFireSpell3(target);
	[ClientRpc] private void RpcFireSpell3(Vector3 target)
	{
		fireSpell3.transform.position = target;
		fireSpell3.SetActive(false);
		fireSpell3.SetActive(true);
	}

	#endregion

	
	[Command(requiresAuthority=false)] void CmdResetMovesLeft(bool active) => RpcResetMovesLeft(active);
	[ClientRpc] void RpcResetMovesLeft(bool active) 
	{
		movesLeftTxt.gameObject.SetActive(false);
		if (active)
			movesLeftTxt.gameObject.SetActive(true);
	}
	
	[Command(requiresAuthority=false)] void CmdUpdateMovesLeft(int x) => RpcUpdateMovesLeft(x);
	[ClientRpc] void RpcUpdateMovesLeft(int x) => movesLeftTxt.text = $"{(x == 0 ? "" : x)}";


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

	[Command(requiresAuthority=false)] public void CmdLose() => RpcLose();
	[ClientRpc] private void RpcLose() => StartCoroutine(LoseCo());

	IEnumerator LoseCo()
	{
		maaronFireVfx.SetActive(true);

		yield return new WaitForSeconds(0.5f);
		model.gameObject.SetActive(false);

		Rigidbody[] bones = ragdollObj[characterInd].GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody bone in bones) {
			bone.velocity = Vector3.up * ragdollKb * 2;
			//bone.angularVelocity = Vector3.forward * ragdollKb * 3;
		}
		ragdollObj[characterInd].transform.rotation = model.rotation;
		ragdollObj[characterInd].SetActive(true);
	}

	[Command(requiresAuthority=false)] public void CmdWin() => RpcWin();
	[ClientRpc] private void RpcWin() => maaronSpotlightVfx.SetActive(true);
}

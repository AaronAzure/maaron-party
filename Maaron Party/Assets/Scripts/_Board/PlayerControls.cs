using System.Collections;
using System.Collections.Generic;
using TMPro;
using Rewired;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
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
	public NetworkVariable<int> characterInd = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	public NetworkVariable<int> id = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	public NetworkVariable<int> boardOrder = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

	
	
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
	[SerializeField] private NetworkVariable<int> coins = new NetworkVariable<int>(10, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	private int coinsT;
	[SerializeField] private NetworkVariable<int> stars = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	private int starsT;
	[SerializeField] private NetworkVariable<int> mana = new NetworkVariable<int>(5, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
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
	bool yourTurn;
	public bool isShield {get; private set;}
	bool usingFireSpell1;
	private bool isCurrencyAsync;

	#endregion



	[Space] [Header("Shop")]
	[SerializeField] private Button[] shopItems;
	[SerializeField] private int nItems;
	[SerializeField] NetworkVariable<int> itemInd0 = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	[SerializeField] NetworkVariable<int> itemInd1 = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	[SerializeField] NetworkVariable<int> itemInd2 = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
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
	[SerializeField] private TextMeshProUGUI spellCostTxt;
	public int _spellInd {get; private set;} 
	public int _spellSlot {get; private set;} 

	#endregion


	#region __Methods__

	private void Awake() 
	{
		DontDestroyOnLoad(this);
	}
	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		if (IsOwner)
			Instance = this;	
		nm.AddBoardConnection(this);
	}
	public override void OnNetworkDespawn()
	{
		//Debug.Log($"<color=#FF9900>PLAYER DISCONNECT ({IsOwner}) | {IsServer} | {yourTurn}</color>");
		base.OnNetworkDespawn();
		if (IsOwner)
		{
			nm.RemoveBoardConnection(this);
		}
		if (IsServer && yourTurn)
			bm.NextPlayerTurn();
	}


	//public void MediateRemoteStart()
	//{
	//	RemoteStartClientRpc(new ClientRpcParams{Send={TargetClientIds=new[]{OwnerClientId}}});
	//}
	
	// Start()
	public void Setup()
	//[ClientRpc] private void RemoteStartClientRpc(ClientRpcParams rpc) 
	{
		//Debug.Log($"<color=yellow>TargetRemoteStart</color>");
		player = ReInput.players.GetPlayer(0);
		if (vCam != null)
			vCam.parent = null;
		
		SetModelServerRpc(characterInd.Value);
		model.rotation = Quaternion.LookRotation(Vector3.back);

		// first turn
		if (gm.nTurn.Value == 1)
		{
			transform.position = BoardManager.Instance.GetSpawnPos().position + new Vector3(-4 + 2*id.Value,0,-2);
			startPos = this.transform.position;
			if (nm.skipIntro)
				SetDataUiServerRpc(id.Value);
		}
		// game over
		else if (gm.nTurn.Value > gm.maxTurns.Value)
		{
			transform.position = BoardManager.Instance.GetSpawnPos().position + new Vector3(-4 + 2*id.Value,0,-2);
			coinsT = coins.Value = gm.GetCoins(id.Value);
			starsT = stars.Value = gm.GetStars(id.Value);
			return;
		}
		
		if (!IsOwner) {
			enabled = false;
			return;
		}
		
		// after first turn
		if (gm.nTurn.Value > 1)
		{
			LoadData();
			SetDataUiServerRpc(nm.skipIntro ? id.Value : gm.GetPlacements(id.Value));
		}
		// 第一名 
		else
		{
			coinsT = coins.Value;
			starsT = stars.Value;
		}
		
		SetCoinTextServerRpc(coins.Value);
		SetStarTextServerRpc(stars.Value);
		ShowManaServerRpc(mana.Value);
		ShowItemsServerRpc();
	}

	[ServerRpc] public void SetModelServerRpc(int ind) => SetModelClientRpc(ind);
	[ClientRpc] public void SetModelClientRpc(int ind)
	{
		name = $"__ PLAYER {id.Value} __";
		//if (IsOwner)
		//	PingServerRpc();
		transform.parent = bm.transform;
		for (int i=0 ; i<models.Length ; i++)
			models[i].SetActive(false);
		if (models != null && ind >= 0 && ind < models.Length)
			models[ind].SetActive(true);

		if (IsOwner)
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

	[ServerRpc] public void SetDataUiServerRpc(int n) => SetDataUiClientRpc(n);
	[ClientRpc] private void SetDataUiClientRpc(int n)
	{
		dataUi.anchoredPosition = new Vector3(125, -137.5f - 175 * n);
		dataUi.gameObject.SetActive(true);
	}

	[ServerRpc] public void SetFinalUiServerRpc() => SetFinalUiClientRpc(id.Value, coins.Value, stars.Value);
	[ClientRpc] private void SetFinalUiClientRpc(int n, int coins, int stars)
	{
		finalUi.anchoredPosition = new Vector3(400 + 250 * n, -650);
		finalUi.gameObject.SetActive(true);
		finalCoinsTxt.text = $"{coins}";
		finalStarsTxt.text = $"{stars}";
	}

	[ServerRpc] public void SetOrderServerRpc(int order) => boardOrder.Value = order;

	public void SetStartNode(Node startNode) => nextNode = startNode;

	[ServerRpc] private void SetCoinTextServerRpc(int n) => SetCoinTextClientRpc(n);
	[ClientRpc] private void SetCoinTextClientRpc(int n) => coinTxt.text = $"{n}";
	[ServerRpc] private void SetStarTextServerRpc(int n) => SetStarTextClientRpc(n);
	[ClientRpc] private void SetStarTextClientRpc(int n) => starTxt.text = $"{n}";


	[ServerRpc] void PlayerThinkingServerRpc(bool active, int ind) => PlayerThinkingClientRpc(active, ind); 
	[ClientRpc] void PlayerThinkingClientRpc(bool active, int ind)
	{
		if (!IsOwner)
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
		if (!IsOwner) return;
		if (coins.Value != coinsT)
		{
			if (currencyT < currencySpeedT)
			{
				currencyT += Time.fixedDeltaTime;
			} 
			else
			{
				coinsT = coinsT < coins.Value ? coinsT + 1 : coinsT - 1;
				SetCoinTextServerRpc(coinsT);
				currencyT = 0;
			}
			if (coins.Value == coinsT)
			{
				isCurrencyAsync = false;
				if (!yourTurn)
				{
					//SaveData();
					this.enabled = false;
				}
			}
		}
		else if (stars.Value != starsT)
		{
			if (currencyT < currencySpeedT)
			{
				currencyT += Time.fixedDeltaTime;
			} 
			else
			{
				starsT = starsT < stars.Value ? starsT + 1 : starsT - 1;
				SetStarTextServerRpc(starsT);
				currencyT = 0;
			}
			if (stars.Value == starsT)
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
				PlayNodeTraverseVfxServerRpc(nextNode.nodeId);
				UpdateMovesLeftServerRpc(movesLeft);

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
						ToggleDashVfxServerRpc(false);
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

	[ClientRpc] public void YourTurnClientRpc(ClientRpcParams crp)
	{
		//Debug.Log("<color=cyan>YOUR TURN!!\n currNode != null => {currNode != null} nextNode != null => {nextNode != null}</color>");
		CamToggleServerRpc(true);
		if (currNode != null)
			ShowDistanceAway(currNode.GetDistanceAway(0));
		else if (nextNode != null)
			ShowDistanceAway(nextNode.GetDistanceAway(1));
		this.enabled = true;
		ToggleYourTurnServerRpc(true);
		ToggleIntroUiServerRpc(true);
		if (ragdollObj[characterInd.Value].activeSelf)
		{
			PlayerToggleServerRpc(true);
			RagdollToggleServerRpc(false);
		}
	}
	private void ShowDistanceAway(int n)
	{
		distanceTxt.text = n == -1 ? "? spaces away" : n == 1 ? "1 space away" : $"{n} spaces away";
	}
	[ServerRpc] public void CamToggleServerRpc(bool activate) => CamToggleClientRpc(activate);
	[ClientRpc] private void CamToggleClientRpc(bool activate) => vCam.gameObject.SetActive(activate);
	[ServerRpc] private void ShoveToggleServerRpc(bool activate) => ShoveToggleClientRpc(activate);
	[ClientRpc] private void ShoveToggleClientRpc(bool activate) => shoveObj.SetActive(activate);
	public void EndTurn()
	{
		CamToggleServerRpc(false);
		//vCam.gameObject.SetActive(false);
		if (canvas != null)
			canvas.SetActive(false);
		ShoveToggleServerRpc(false);
		this.enabled = false;
		ToggleYourTurnServerRpc(false);
		SaveData();
	}
	[ServerRpc] private void ToggleYourTurnServerRpc(bool active) => yourTurn = active;
	[ServerRpc] void ToggleIntroUiServerRpc(bool active) => ToggleIntroUiClientRpc(active);
	[ClientRpc] void ToggleIntroUiClientRpc(bool active)
	{
		introBtn.interactable = IsOwner;
		clickAnywhereUi.SetActive(IsOwner);

		if (active)
			introAnim.gameObject.SetActive(active);
		else
			introAnim.SetTrigger("close");
	}


	#region Saving data
	[ServerRpc] public void SaveDataServerRpc() => SaveDataClientRpc(new ClientRpcParams{Send={TargetClientIds=new[]{OwnerClientId}}});
	[ClientRpc] public void SaveDataClientRpc(ClientRpcParams rpc) 
	{
		gm.SaveCurrNode(currNode.nodeId, id.Value);
		gm.SaveCoinsServerRpc(coins.Value, id.Value);
		gm.SaveStarsServerRpc(stars.Value, id.Value);
		gm.SaveManaServerRpc(mana.Value, id.Value);
		gm.SaveNumItemsServerRpc(nItems, id.Value);
		gm.SaveItemsServerRpc(new int[]{itemInd0.Value, itemInd1.Value, itemInd2.Value}, id.Value);
		DataSavedServerRpc();
	}
	[ServerRpc] void DataSavedServerRpc() => nm.IncreasePlayerDataSaved();
	
	private void SaveData()
	{
		gm.SaveCurrNode(currNode.nodeId, id.Value);
		gm.SaveCoinsServerRpc(coins.Value, id.Value);
		gm.SaveStarsServerRpc(stars.Value, id.Value);
		gm.SaveManaServerRpc(mana.Value, id.Value);
		gm.SaveNumItemsServerRpc(nItems, id.Value);
		gm.SaveItemsServerRpc(new int[]{itemInd0.Value, itemInd1.Value, itemInd2.Value}, id.Value);
	}
	private void LoadData()
	{
		currNode = NodeManager.Instance.GetNode( gm.GetCurrNode(id.Value) );
		currNode.AddPlayer(this);
		startPos = transform.position = currNode.transform.position;
		transform.position = new Vector3(transform.position.x, 0, transform.position.z);
		coinsT = coins.Value = gm.GetCoins(id.Value);
		starsT = stars.Value = gm.GetStars(id.Value);
		mana.Value = gm.GetMana(id.Value);
		nItems = gm.GetNumItems(id.Value);
		int[] itemInds = gm.GetItems(id.Value);
		itemInd0 = new NetworkVariable<int>(itemInds[0], NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
		itemInd1 = new NetworkVariable<int>(itemInds[1], NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
		itemInd2 = new NetworkVariable<int>(itemInds[2], NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	}

	#endregion


	#region BUTTONS
	public void _START_PLAYER()
	{
		ToggleIntroUiServerRpc(false);
		if (gm.nTurn.Value % 2 == 1 && gm.nTurn.Value > 1)
		{
			mana.Value = Mathf.Min(mana.Value + 1, (int)manaSld.maxValue);
			ShowManaBonusTxtServerRpc(1);
			ShowManaServerRpc(mana.Value);
		}
		if (canvas != null)
			canvas.SetActive(true);
	}
	public void _BACK_TO_BASE_UI()
	{
		// show base ui
		if (rangeObj.activeSelf)
			UseSpellServerRpc(false, currNode != null ? currNode.nodeId : -1);
		if (rangeObj2.activeSelf)
			UseSpell2ServerRpc(false, currNode != null ? currNode.nodeId : -1);

		spellCostTxt.gameObject.SetActive(false);
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
		if (purchase && (freeShop || coins.Value >= 20))
		{
			if (!freeShop)
				coins.Value -= 20;
			stars.Value++;
			currencySpeedT = 0.025f;
			StartCoroutine(PurchaseStarCo());
			Vector3 dir = Camera.main.transform.position - transform.position;
			model.rotation = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z).normalized);
		}
		else	
			ToggleStarCamServerRpc(false);

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
		bm.MaaronClapServerRpc(true);

		yield return new WaitForSeconds(2);
		bm.MaaronClapServerRpc(false);
		anim.SetBool("hasStar", false);
		ToggleStarCamServerRpc(false);
		bm.ChooseStarServerRpc();
		//StartCoroutine(WaitForNewStarCo());
		yield return new WaitForSeconds(9.5f);
		isBuyingStar = false;
	}

	public bool HasFreeShop() => freeShop;
	public void _BUY_ITEM(int itemId, int itemCost)
	{
		if (!freeShop)
			NodeEffect(-itemCost);

		if (nItems < 3)
		{
			if (nItems == 0) itemInd0.Value = itemId;
			if (nItems == 1) itemInd1.Value = itemId;
			if (nItems == 2) itemInd2.Value = itemId;
			ShowItemsServerRpc();
			if (!freeShop)
				StartCoroutine( CloseShopCo() );
		}
		// inventory full!
		else
		{
			newSpellId = itemId;
			for (int i=0 ; i<newSpellBtns.Length ; i++)
				newSpellBtns[i].ind = i >= 0 && i < nItems ? GetItemInd(i) : itemId;
			fullUi.SetActive(true);
			shopUi.SetActive(false);
		}
	}
	public void _REPLACE_ITEM(int ind)
	{
		// replaced item
		if (ind >= 0 && ind < nItems)
		{
			if (nItems == 0) itemInd0.Value = newSpellId;
			if (nItems == 1) itemInd1.Value = newSpellId;
			if (nItems == 2) itemInd2.Value = newSpellId;
			//itemInds.Value[ind] = newSpellId;
			ShowItemsServerRpc();
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
		tollSldr.maxValue = coins.Value;
	}
	public void _SET_NEW_TOLL_TEXT()
	{
		tollTxt.text = $"Spend: <b>{tollSldr.value}</b> (Next person pays: <b>{tollSldr.value+1}</b>)";
		//gm.SetDoorToll(_doorInd, (int)tollSldr.value);
	}
	public void _PAY_DOOR_TOLL()
	{
		NodeEffect(-(int)tollSldr.value);
		bm.PlayDoorAnimServerRpc(nextNode.nodeId);
		isAtDoor = false;
		doorUi.SetActive(false);
		tollUi.SetActive(false);
		brokeUi.SetActive(false);
		nextNode = nextNode.nextNodes[0];
		ToggleDoorCamServerRpc(false);
		gm.SetDoorTollServerRpc(_doorInd, (int)tollSldr.value+1);
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
		ToggleDoorCamServerRpc(false);
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
		ToggleStarCamServerRpc(false);
		PlayerThinkingServerRpc(false, characterInd.Value);

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
		UpdateMovesLeftServerRpc( movesLeft );
		ResetMovesLeftServerRpc(true);

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
			ShowNodeDistanceServerRpc(false, nextNode != null ? nextNode.nodeId : currNode.nodeId, nextNode != null ? 1 : 0, -1);
			inMap = false;
		}
		else
		{
			spellCam.transform.localPosition = new Vector3(0,25,-10);
			ShowNodeDistanceServerRpc(true, nextNode != null ? nextNode.nodeId : currNode.nodeId, nextNode != null ? 1 : 0, -1);
			inMap = true;
		}
		
		ToggleMapCamServerRpc(!spellCam.activeSelf);
	}
	
	#endregion


	#region Nodes

	[ServerRpc] void PlayNodeTraverseVfxServerRpc(int nodeId) => PlayNodeTraverseVfxClientRpc(nodeId);
	[ClientRpc] void PlayNodeTraverseVfxClientRpc(int nodeId) => NodeManager.Instance.GetNode(nodeId).PlayGlowVfx();
	private void StuckAtFork()
	{
		isAtFork = true;
		if (anim != null) anim.SetFloat("moveSpeed", 0);
		HidePaths();
		ShowNodeDistanceServerRpc(true, nextNode != null ? nextNode.nodeId : currNode.nodeId, 0, movesLeft);
		spellCam.transform.localPosition = new Vector3(0,25,-10);
		inMap = true;
		ToggleMapCamServerRpc(true);
		PlayerThinkingServerRpc(true, characterInd.Value);
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
	[ServerRpc] void ToggleMapCamServerRpc(bool active) => ToggleMapCamClientRpc(active);
	[ClientRpc] void ToggleMapCamClientRpc(bool active) => spellCam.SetActive(active);
	[ServerRpc] void ShowNodeDistanceServerRpc(bool active, ushort nodeId, int num, int movesLeft) 
		=> ShowNodeDistanceClientRpc(active, nodeId, num, movesLeft);
	[ClientRpc] void ShowNodeDistanceClientRpc(bool active, ushort nodeId, int num, int movesLeft) 
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
		ToggleDoorCamServerRpc(true);
		if (coins.Value >= gm.GetDoorToll(_doorInd))
			doorUi.SetActive(true);
		else
			brokeUi.SetActive(true);
		isStop = false;
	}

	public void OnShopNode()
	{
		isAtShop = true;
		PlayerThinkingServerRpc(true, characterInd.Value);

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
		ToggleStarCamServerRpc(true);
		shopUi.SetActive(true);
		isStop = false;
	}
	public void OnStarNode()
	{
		isAtStar = true;
		if (anim != null) anim.SetFloat("moveSpeed", 0);

		if (coins.Value >= 20)
			starUi.SetActive(true);
		else
			brokeUi.SetActive(true);
		if (nextNode != null)
		{
			Vector3 dir = (nextNode.GetTargetTransform().position - transform.position).normalized;
			RotateDirection(dir);
		}
		ToggleStarCamServerRpc(true);
		isStop = false;
	}
	public int GetMana() => mana.Value;
	public int GetCoins() => coins.Value;
	public int GetStars() => stars.Value;
	[ServerRpc] public void NodeEffectServerRpc(int bonus, bool isStar) 
		=> NodeEffectClientRpc(bonus, isStar, new ClientRpcParams{Send={TargetClientIds=new ulong[]{OwnerClientId}}});
	[ClientRpc] public void NodeEffectClientRpc(int bonus, bool isStar, ClientRpcParams rpc) 
		=> NodeEffect(bonus, isStar);
	public void NodeEffect(int bonus, bool isStar=false)
	{
		// star related
		if (isStar)
		{
			currencySpeedT = 0.05f;
			stars.Value = Mathf.Max(stars.Value+bonus, 0);
			if (stars.Value != 0)
				this.enabled = isCurrencyAsync = true;
		}
		// coin related
		else
		{
			if (bonus != 0) currencySpeedT = 0.5f / Mathf.Abs(bonus);
			coins.Value = Mathf.Clamp(coins.Value + bonus, 0, 999);
			if (coins.Value != 0)
				this.enabled = isCurrencyAsync = true;
		}
		ShowBonusTxtServerRpc(bonus, isStar);
	}
	public void LoseAllCoins()
	{
		//Debug.Log($"<color=white>{name} LOST ALL COINS</color>");
		if (coins.Value != 0) currencySpeedT = 0.5f / coins.Value;
		int temp = coins.Value;
		coins.Value = 0;
		this.enabled = isCurrencyAsync = true;
		ShowBonusTxtServerRpc(-temp, false);
	}
	[ServerRpc] private void ShowBonusTxtServerRpc(int n, bool isStar) => ShowBonusTxtClientRpc(n, isStar);
	[ClientRpc] private void ShowBonusTxtClientRpc(int n, bool isStar)
	{
		bonusObj.SetActive(false);
		bonusObj.SetActive(true);
		bonusTxt.text = isStar ? "<sprite name=\"star\">" : $"<sprite name=\"coin\">";
		bonusTxt.text += n >= 0 ? $"+{n}" : $"{n}";
	}
	[ServerRpc] private void ShowManaBonusTxtServerRpc(int n) => ShowManBonusTxtClientRpc(n);
	[ClientRpc] private void ShowManBonusTxtClientRpc(int n)
	{
		bonusObj.SetActive(false);
		bonusObj.SetActive(true);
		bonusTxt.text = "<sprite name=mana.Value>";
		bonusTxt.text += n >= 0 ? $"+{n}" : $"{n}";
	}

	[ServerRpc] private void ToggleStarCamServerRpc(bool active) => ToggleStarCamClientRpc(active);
	[ClientRpc] private void ToggleStarCamClientRpc(bool active) => starCam.SetActive(active);
	[ServerRpc] private void ToggleDoorCamServerRpc(bool active) => ToggleDoorCamClientRpc(active);
	[ClientRpc] private void ToggleDoorCamClientRpc(bool active) => doorCam.SetActive(active);

	private IEnumerator NodeEffectCo()
	{
		TriggerNodeVfxServerRpc(currNode.nodeId); // vfx on node
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
	[ServerRpc] void TriggerNodeVfxServerRpc(int nodeId) => TriggerNodeVfxClientRpc(nodeId);
	[ClientRpc] void TriggerNodeVfxClientRpc(int nodeId) => NodeManager.Instance.GetNode(nodeId).TriggerNodeLandVfx();

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
		ShowNodeDistanceServerRpc(false, nextNode != null ? nextNode.nodeId : currNode.nodeId, 0, movesLeft);
		nextNode = nextNode.nextNodes[ind];
		PlayerThinkingServerRpc(false, characterInd.Value);
		IsStuckAtDoor();
		HidePaths();
		ToggleMapCamServerRpc(false);
		inMap = isAtFork = false;
	}

	#endregion


	#region Items/Spells
	private int GetItemInd(int ind)
	{
		if (nItems == 0) return itemInd0.Value;
		if (nItems == 1) return itemInd1.Value;
		if (nItems == 2) return itemInd2.Value;
		return -1;
	}
	private void SetItemInd(int ind, int itemId)
	{
		if (nItems == 0) itemInd0.Value = itemId;
		if (nItems == 1) itemInd1.Value = itemId;
		if (nItems == 2) itemInd2.Value = itemId;
	}

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
		mana.Value = Mathf.Max(mana.Value - cost, 0);
		ShowManaServerRpc(mana.Value);
	}
	[ServerRpc] private void ShowManaServerRpc(int mana) => ShowManaClientRpc(mana);
	[ClientRpc] private void ShowManaClientRpc(int mana) 
	{
		manaSld.value = mana;
		manaTxt.text = $"{mana}/{manaSld.maxValue}";
	}

	private void RemoveSpell(int ind)
	{
		if (ind >= 0 && ind < nItems)
			SetItemInd(ind, -1);
		ShowItemsServerRpc();
	}
	[ServerRpc] private void ShowItemsServerRpc() => ShowItemsClientRpc();
	[ClientRpc] private void ShowItemsClientRpc()
	{
		for (int i = 0; i < itemImgs.Length; i++)
		{
			if (nItems > i)
				itemImgs[i].sprite = Item.instance.GetSprite( GetItemInd(i) );
			else
				itemImgs[i].sprite = emptySpr;
		}
		for (int i = 0; i < items.Length; i++)
		{
			if (nItems > i)
			{
				items[i].ind = GetItemInd(i);
				items[i].SetImage();
			}
			else
			{
				items[i].ind = -1;
				items[i].SetImage();
			}
		}
	}
	
	//![ServerRpc] private void ReplaceItemsServerRpc(List<int> ints) => ReplaceItemsClientRpc(ints);
	//![ClientRpc] private void ReplaceItemsClientRpc(List<int> ints)
	//!{
	//!	itemInds = ints;
	//!}
	
	public void SetSpellCost(int n, int extra)
	{
		if (extra > 0)
			spellCostTxt.text = $"<sprite name=mana.Value><color=red>-{n+extra}</color>";
		else
			spellCostTxt.text = $"<sprite name=mana.Value>-{n}";
	}

	public void _USE_SPELL(int slot, int ind) 
	{
		_spellSlot = slot;
		_spellInd = ind;
		UseSpellServerRpc(!rangeObj.activeSelf, currNode != null ? currNode.nodeId : -1);
		switch (_spellInd)
		{
			case 0: SetSpellCost(1, 0); break;
			case 1: SetSpellCost(2, 0); break;
			case 2: SetSpellCost(3, 0); break;

			case 3: SetSpellCost(2, 0); break;
			case 4: SetSpellCost(3, 0); break;
			case 5: SetSpellCost(4, 0); break;
		}
	}
	public void _USE_SPELL_2(int slot, int ind) 
	{
		_spellSlot = slot;
		_spellInd = ind;
		UseSpell2ServerRpc(!rangeObj.activeSelf, currNode != null ? currNode.nodeId : -1);
		switch (_spellInd)
		{
			case 0: SetSpellCost(1, 0); break;
			case 1: SetSpellCost(2, 0); break;
			case 2: SetSpellCost(3, 0); break;

			case 3: SetSpellCost(2, 0); break;
			case 4: SetSpellCost(3, 0); break;
			case 5: SetSpellCost(4, 0); break;
		}
	}
	
	[ServerRpc] private void ToggleRangeServerRpc(
		bool active, int nodeId) => ToggleRangeClientRpc(active, nodeId);
	[ClientRpc] private void ToggleRangeClientRpc(bool active, int nodeId)
	{
		if (!active && nodeId != -1)
			NodeManager.Instance.GetNode(nodeId).SetCanSpellTargetDelay(true);
		else if (active && nodeId != -1) 
			NodeManager.Instance.GetNode(nodeId).SetCanSpellTarget(false);

		if (active)
			spellCam.transform.localPosition = new Vector3(0,25,-10);
		spellCam.SetActive(active);
		rangeAnim.SetTrigger(active ? "on" : "off");
		spellCostTxt.gameObject.SetActive(active);
	}
	[ServerRpc] private void ToggleRange2ServerRpc(
		bool active, int nodeId) => ToggleRange2ClientRpc(active, nodeId);
	[ClientRpc] private void ToggleRange2ClientRpc(bool active, int nodeId)
	{
		if (!active && nodeId != -1)
			NodeManager.Instance.GetNode(nodeId).SetCanSpellTargetDelay(true);
		else if (active && nodeId != -1) 
			NodeManager.Instance.GetNode(nodeId).SetCanSpellTarget(false);

		if (active)
			spellCam.transform.localPosition = new Vector3(0,25,-10);
		spellCam.SetActive(active);
		rangeAnim2.SetTrigger(active ? "on" : "off");
		spellCostTxt.gameObject.SetActive(active);
	}
	
	[ServerRpc] private void UseSpellServerRpc(bool active, int nodeId) 
		=> UseSpellClientRpc(active, nodeId);
	[ClientRpc] private void UseSpellClientRpc(bool active, int nodeId)
	{
		if (!active && nodeId != -1)
			NodeManager.Instance.GetNode(nodeId).SetCanSpellTargetDelay(true);
		else if (active && nodeId != -1) 
			NodeManager.Instance.GetNode(nodeId).SetCanSpellTarget(false);

		if (active)
			spellCam.transform.localPosition = new Vector3(0,25,-10);
		spellCam.SetActive(active);
		rangeAnim.SetTrigger(active ? "on" : "off");
		spellCostTxt.gameObject.SetActive(active);
		if (IsOwner)
		{
			isUsingSpell = active;
			backToBaseUi.SetActive(active);
			ToggleSpellUi(false);
		}
	}
	[ServerRpc] private void UseSpell2ServerRpc(bool active, int nodeId) 
		=> UseSpell2ClientRpc(active, nodeId);
	[ClientRpc] private void UseSpell2ClientRpc(bool active, int nodeId)
	{
		if (!active && nodeId != -1)
			NodeManager.Instance.GetNode(nodeId).SetCanSpellTargetDelay(true);
		else if (active && nodeId != -1) 
			NodeManager.Instance.GetNode(nodeId).SetCanSpellTarget(false);

		if (active)
			spellCam.transform.localPosition = new Vector3(0,25,-10);
		spellCam.SetActive(active);
	 	rangeAnim2.SetTrigger(active ? "on" : "off");
		spellCostTxt.gameObject.SetActive(active);
		if (IsOwner)
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
		UpdateMovesLeftServerRpc(0);

		yield return new WaitForSeconds(0.5f);
		UpdateMovesLeftServerRpc(movesLeft);
		startPos = transform.position;
		// more than one path
		if (nextNode.nextNodes.Count > 1)
			StuckAtFork();
		// single path
		else
			nextNode = nextNode.nextNodes[0];

		ToggleStarCamServerRpc(false);
		PlayerThinkingServerRpc(false, characterInd.Value);
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
		ToggleDashVfxServerRpc(true);
		RemoveSpell(_spellSlot);
		ConsumeMana(manaCost);
		ShowManaBonusTxtServerRpc(-manaCost);

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
		UpdateMovesLeftServerRpc( movesLeft );

		if (canvas != null)
			canvas.SetActive(false);
		canMove = true;
	}
	[ServerRpc] private void ToggleDashVfxServerRpc(bool active) => ToggleDashVfxClientRpc(active);
	[ClientRpc] private void ToggleDashVfxClientRpc(bool active)
	{
		if (active) 
			dashSpellPs1.Play(true);
		else
			dashSpellPs1.Stop(true, ParticleSystemStopBehavior.StopEmitting);
	} 


	public void UseShieldSpell()
	{
		ToggleSpellUi(false);
		ShieldSpellServerRpc(true);
		//if (canvas != null)
		//	canvas.SetActive(false);
	}
	[ServerRpc] private void ShieldSpellServerRpc(bool active) => ShieldSpellClientRpc(active);
	[ClientRpc] private void ShieldSpellClientRpc(bool active)
	{
		shieldSpell1.SetActive(active);
		isShield = active;
	} 
	
	public void UseThornSpell(Node target, int manaCost, int trapId)
	{
		Debug.Log($"<color=magenta>manaCost = {manaCost}</color>");
		ToggleSpellUi(false);
		backToBaseUi.SetActive(false);
		ConsumeMana(manaCost);
		ShowManaBonusTxtServerRpc(-manaCost);

		if (spellCo == null)
			spellCo = StartCoroutine( ThornCo(target, trapId) );
	}
	IEnumerator ThornCo(Node target, int trapId)
	{
		nodeCam.m_Follow = target.transform;
		SaveTrapServerRpc(target.nodeId, trapId);
		ToggleNodeCamServerRpc(true);
		ToggleRangeServerRpc(false, currNode != null ? currNode.nodeId : -1);
		RemoveSpell(_spellSlot);

		//yield return new WaitForSeconds(1f);
		//CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera.VirtualCameraGameObject.name;
		//Debug.Log($"{CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera.VirtualCameraGameObject.name}");
		
		yield return new WaitForSeconds(1.5f);
		ToggleNodeCamServerRpc(false);
		spellCo = null;
		StopUsingSpells();
	}
	[ServerRpc] private void SaveTrapServerRpc(int nodeId, int trapId) 
		=> gm.SaveTrap(nodeId, id.Value, characterInd.Value, trapId);
	public void UseFireSpell(Node target, int manaCost, int fireSpellInd)
	{
		ToggleSpellUi(false);
		backToBaseUi.SetActive(false);
		ConsumeMana(manaCost);
		ShowManaBonusTxtServerRpc(-manaCost);

		usingFireSpell1 = true;
		if (spellCo == null)
			spellCo = StartCoroutine( SpellCo(target, fireSpellInd) );
	}
	IEnumerator SpellCo(Node target, int fireSpellInd)
	{
		nodeCam.m_Follow = target.transform;
		ToggleNodeCamServerRpc(true);
		ToggleRange2ServerRpc(false, currNode != null ? currNode.nodeId : -1);
		RemoveSpell(_spellSlot);

		yield return new WaitForSeconds(1f);
		if (fireSpellInd == 1) FireSpell1ServerRpc(target.transform.position);
		if (fireSpellInd == 2) FireSpell2ServerRpc(target.transform.position);
		if (fireSpellInd == 3) FireSpell3ServerRpc(target.transform.position);

		yield return new WaitForSeconds(0.5f);
		if (fireSpellInd == 1) gm.HitPlayersAtNodeServerRpc(target.nodeId, -15);
		if (fireSpellInd == 2) gm.HitPlayersAtNodeServerRpc(target.nodeId, -25);
		if (fireSpellInd == 3) gm.HitPlayersStarsAtNodeServerRpc(target.nodeId);
		
		yield return new WaitForSeconds(1.5f);
		ToggleNodeCamServerRpc(false);
		spellCo = null;
		StopUsingSpells();
	}
	[ServerRpc] private void ToggleNodeCamServerRpc(bool active) => ToggleNodeCamClientRpc(active);
	[ClientRpc] private void ToggleNodeCamClientRpc(bool active) => nodeCam.gameObject.SetActive(active);
	[ServerRpc] private void FireSpell1ServerRpc(Vector3 target) => FireSpell1ClientRpc(target);
	[ClientRpc] private void FireSpell1ClientRpc(Vector3 target)
	{
		fireSpell1.transform.position = target;
		fireSpell1.SetActive(false);
		fireSpell1.SetActive(true);
	}
	[ServerRpc] private void FireSpell2ServerRpc(Vector3 target) => FireSpell2ClientRpc(target);
	[ClientRpc] private void FireSpell2ClientRpc(Vector3 target)
	{
		fireSpell2.transform.position = target;
		fireSpell2.SetActive(false);
		fireSpell2.SetActive(true);
	}
	[ServerRpc] private void FireSpell3ServerRpc(Vector3 target) => FireSpell3ClientRpc(target);
	[ClientRpc] private void FireSpell3ClientRpc(Vector3 target)
	{
		fireSpell3.transform.position = target;
		fireSpell3.SetActive(false);
		fireSpell3.SetActive(true);
	}

	#endregion

	
	[ServerRpc] void ResetMovesLeftServerRpc(bool active) => ResetMovesLeftClientRpc(active);
	[ClientRpc] void ResetMovesLeftClientRpc(bool active) 
	{
		movesLeftTxt.gameObject.SetActive(false);
		if (active)
			movesLeftTxt.gameObject.SetActive(true);
	}
	
	[ServerRpc] void UpdateMovesLeftServerRpc(int x) => UpdateMovesLeftClientRpc(x);
	[ClientRpc] void UpdateMovesLeftClientRpc(int x) => movesLeftTxt.text = $"{(x == 0 ? "" : x)}";


	[ServerRpc] public void PlayerToggleServerRpc(bool active) => PlayerToggleClientRpc(active);
	[ClientRpc] private void PlayerToggleClientRpc(bool active) => model.gameObject.SetActive(active);

	[ServerRpc] public void RagdollToggleServerRpc(bool active) => RagdollToggleClientRpc(active);
	[ClientRpc] private void RagdollToggleClientRpc(bool active) 
	{
		if (active)
		{
			Rigidbody[] bones = ragdollObj[characterInd.Value].GetComponentsInChildren<Rigidbody>();
			foreach (Rigidbody bone in bones) {
				bone.velocity = Vector3.up * ragdollKb;
				bone.angularVelocity = Vector3.forward * ragdollKb;
			}
		}
		ragdollObj[characterInd.Value].transform.rotation = model.rotation;
		ragdollObj[characterInd.Value].SetActive(active);
	}

	[ServerRpc] public void LoseServerRpc() => LoseClientRpc();
	[ClientRpc] private void LoseClientRpc() => StartCoroutine(LoseCo());

	IEnumerator LoseCo()
	{
		maaronFireVfx.SetActive(true);

		yield return new WaitForSeconds(0.5f);
		model.gameObject.SetActive(false);

		Rigidbody[] bones = ragdollObj[characterInd.Value].GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody bone in bones) {
			bone.velocity = Vector3.up * ragdollKb * 2;
			//bone.angularVelocity = Vector3.forward * ragdollKb * 3;
		}
		ragdollObj[characterInd.Value].transform.rotation = model.rotation;
		ragdollObj[characterInd.Value].SetActive(true);
	}

	[ServerRpc] public void WinServerRpc() => WinClientRpc();
	[ClientRpc] private void WinClientRpc() => maaronSpotlightVfx.SetActive(true);
}

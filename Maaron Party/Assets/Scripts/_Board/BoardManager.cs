using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using Cinemachine;
using Unity.Collections;

public class BoardManager : NetworkBehaviour
{
	public static BoardManager Instance;
	//[SerializeField] private PlayerControls playerPrefab;
	[SerializeField] private GameObject startCam;
	[SerializeField] private Transform spawnPos;
	[SerializeField] private Transform maaronSpawnPos;
	private PlayerControls _player;
	GameManager gm {get{return GameManager.Instance;}}
	GameNetworkManager nm {get{return GameNetworkManager.Instance;}}
	UiDialogue dialogue {get{return UiDialogue.Instance;}}


	[Space] [Header("Text")]
	[TextArea(2, 5)] [SerializeField] FixedString128Bytes[] introSents;
	[TextArea(2, 5)] [SerializeField] FixedString128Bytes[] lastSents;
	[TextArea(2, 5)] [SerializeField] FixedString128Bytes[] gameOverSents;


	[Space] [Header("Universal")]
	[SerializeField] private GameObject mainUi;
	[SerializeField] private Transform dataUi;
	[SerializeField] private TextMeshProUGUI turnTxt;
	[SerializeField] private GameObject newStockUi;
	[SerializeField] private GameObject finalFiveUi;
	[SerializeField] private CinemachineVirtualCamera starCam;
	[SerializeField] private Animator maaronAnim;
	[SerializeField] private ParticleSystem maaronSpotlightPs;
	[SerializeField] private CanvasGroup placementCg;
	[SerializeField] private GameObject placementUi;
	[SerializeField] private CinemachineVirtualCamera boardCam;
	[SerializeField] private PlacementButton[] placementBtns;
	NetworkVariable<bool[]> placementChosen = new();

	[Space] [SerializeField] private TreasureChest[] chests;
	public int nBmReady; 
	//bool isChoosingStar;


	[Space] [Header("MUST REFERENCE PER BOARD")]
	[SerializeField] private Node startNode;
	[SerializeField] private starNodes[] starNodes;
	[SerializeField] private BoardTurret turret;
	[SerializeField] private Animator turretIntro;
	[SerializeField] private Node[] doors;
	private bool turretTurnDone;


	[Space] [Header("HACKS")]
	[SerializeField] private bool fireTurret;


	[Space] [Header("States")]
	[SerializeField] private bool isIntro;
	[SerializeField] private bool isLast5;
	[SerializeField] private bool gameOver;


	private void Awake() 
	{
		Instance = this;		
	}

	public void SetUiLayout(Transform ui)
	{
		if (ui != null)
		{
			ui.parent = dataUi;
			ui.localScale = Vector3.one;
		}
	}

	private void Start() 
	{

		turnTxt.text = $"Turn: {gm.nTurn.Value}/{gm.maxTurns.Value}";

		if (!gm.gameStarted)
		{
			ToggleMainUi(false);
			ToggleMainUiServerRpc(false);
			if (IsServer)
			{
				placementChosen = new NetworkVariable<bool[]>(new bool[placementBtns.Length], NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
				//placementChosen = new bool[placementBtns.Length];
				gm.SetupDoorTollsServerRpc(doors == null ? 0 : doors.Length);
			}
		}
	}


	[ServerRpc] public void ReadyUpServerRpc()
	{
		++nBmReady;
		//Debug.Log($"<color=white>{nBmReady} >= {nm.numPlayers}</color>");
		if (nBmReady >= NetworkManager.Singleton.ConnectedClients.Count)
		{
			SetUpPlayerClientRpc();
			nm.UnparentBoardControls();
		}
	} 
	[ClientRpc] private void SetUpPlayerClientRpc()
	{
		_player = PlayerControls.Instance;
		if (gm.nTurn.Value == 1)
			_player.SetStartNode(startNode);
		if (IsServer)
			StartCoroutine( StartGameCo() );
	}
	public Transform GetSpawnPos() => spawnPos;
	
	#region Start game
	IEnumerator StartGameCo()
	{
		//yield return new WaitForSeconds(0.5f);
		//gm.TriggerTransitionServerRpc(false);
		if (turret != null)
			TurretStartServerRpc();

		yield return new WaitForSeconds(1);
		// turn 1
		if (!gm.gameStarted)
		{
			gm.gameStarted = true;
			if (nm.skipBoard)
			{
				yield return new WaitForSeconds(1f);
				NextPlayerTurn();
			}
			else if (nm.skipIntro)
			{
				yield return ChooseStarCo(0);
				ToggleMainUi(true);
				ToggleMainUiServerRpc(true);
			}
			else
			{
				isIntro = true;
				ToggleStartCamServerRpc(true);
				
				yield return new WaitForSeconds(1.5f);
				MaaronIntroServerRpc();

				yield return new WaitForSeconds(2);
				//ToggleDialogue(true, introSents);
				//ToggleDialogueServerRpc(true, introSents);
				ToggleNextButtonServerRpc(true);

				yield break;
			}
			//yield return ChooseStarCo(0);
		}
		// Game Over
		else if (gm.nTurn.Value > gm.maxTurns.Value)
		{
			//isIntro = gm.gameStarted = true;
			gameOver = true;
			ToggleStartCamServerRpc(true);

			//yield return new WaitForSeconds(1f);
			//FinalFiveServerRpc();
			
			yield return new WaitForSeconds(0.5f);
			MaaronIntroServerRpc();

			yield return new WaitForSeconds(2);
			//ToggleDialogue(true, gameOverSents);
			//ToggleDialogueServerRpc(true, gameOverSents);
			ToggleNextButtonServerRpc(true);

			yield break;
		}
		// last 5 turns
		else if (gm.nTurn.Value == gm.maxTurns.Value - 4)
		{
			//isIntro = gm.gameStarted = true;
			isLast5 = true;
			ToggleStartCamServerRpc(true);

			yield return new WaitForSeconds(1f);
			FinalFiveServerRpc();
			
			yield return new WaitForSeconds(0.5f);
			MaaronIntroServerRpc();

			yield return new WaitForSeconds(2);
			//ToggleDialogue(true, lastSents);
			//ToggleDialogueServerRpc(true, lastSents);
			ToggleNextButtonServerRpc(true);

			yield break;
		}
		// turn 2+
		else
		{
			ToggleMainUi(true);
			ToggleMainUiServerRpc(true);
			StartCoroutine( SetupStarNode(gm.prevStarInd) );
		}

		yield return new WaitForSeconds(0.5f);
		nm.NextBoardPlayerTurn();

		if (gm.nTurn.Value == 5)
		{
			yield return new WaitForSeconds(1);
			NewStockServerRpc();
		}
		if (gm.nTurn.Value == gm.maxTurns.Value - 4)
		{
			yield return new WaitForSeconds(1);
			NewStockServerRpc();
		}
	}
	#endregion

	#region Dialogue
	
	// ui = player data, turn
	void ToggleMainUi(bool active) => mainUi.gameObject.SetActive(active);
	[ServerRpc] public void ToggleMainUiServerRpc(bool active) => ToggleMainUiClientRpc(active);
	[ClientRpc] void ToggleMainUiClientRpc(bool active) => mainUi.gameObject.SetActive(active);
	
	// Dialogue
	//public void ToggleDialogue(bool active, FixedString128Bytes[] sents) => dialogue.SetSentence(active, "Maaron", sents);
	//[ServerRpc] public void ToggleDialogueServerRpc(bool active, FixedString128Bytes[] sents) => ToggleDialogueClientRpc(active, sents);
	//[ClientRpc] void ToggleDialogueClientRpc(bool active, FixedString128Bytes[] sents) => dialogue.SetSentence(active, "Maaron", sents);

	[ServerRpc] public void ToggleNextButtonServerRpc(bool targeted) 
	{
		if (targeted)
			ToggleNextButtonClientRpc(new ClientRpcParams{ Send = { TargetClientIds = new[] { _player.OwnerClientId } }});
		else
			DisableNextButtonClientRpc();
	}
	[ClientRpc] void ToggleNextButtonClientRpc(ClientRpcParams rpc) => dialogue.ToggleButton(true);
	[ClientRpc] void DisableNextButtonClientRpc() => dialogue.ToggleButton(false);
	
	public void NextDialogue() => dialogue.NextSentence();
	[ServerRpc] public void NextDialogueServerRpc() => NextDialogueClientRpc();
	[ClientRpc] void NextDialogueClientRpc() => dialogue.NextSentence();

	Coroutine teleportCo;
	//~ --------------------------------------------------------
	[ServerRpc] public void EndDialogueServerRpc() 
	{
		EndDialogueClientRpc();
		if (isIntro && IsServer && teleportCo == null)
		{
			StartCoroutine( PlacementCardsCo() );
		}
		if (isLast5 && IsServer)
		{
			SpawnChestsServerRpc();
		}
		if (gameOver && IsServer)
		{
			StartCoroutine( WinnerCo() );
		}
	}
	public void EndDialogue() => dialogue.CloseDialogue();
	[ClientRpc] void EndDialogueClientRpc() => dialogue.CloseDialogue();

	#endregion


	#region Placement
	IEnumerator PlacementCardsCo()
	{
		List<int> temp = new();
		tempPlayerOrder = new();
		for (int i=0 ; i<nm.GetNumPlayers() ; i++)
		{
			temp.Add(i);
			tempPlayerOrder.Add(i);
		}

		yield return new WaitForSeconds(1);
		TogglePlacementUiClientRpc(true);
		// create random order
		for (int i=0 ; i<nm.GetNumPlayers() ; i++)
		{
			int rng = temp[Random.Range(0, temp.Count)];
			SetPlacementCardClientRpc(i, rng);
			temp.Remove(rng);
		}
		TogglePlacementCardClientRpc(true);
	}
	[ClientRpc] void SetPlacementCardClientRpc(int i, int rng)
	{
		if (i >= 0 && i < placementBtns.Length && placementBtns[i] != null)
			placementBtns[i].SetPlacement(rng, i);
	}
	[ClientRpc] void TogglePlacementCardClientRpc(bool active)
	{
		for (int i=0 ; i<placementBtns.Length && i<nm.GetNumPlayers() ; i++)
			if (placementBtns[i] != null)
				placementBtns[i].gameObject.SetActive(active);
	}
	[ClientRpc] void TogglePlacementUiClientRpc(bool active) => placementUi.SetActive(active);


	
	List<int> tempPlayerOrder;
	public void DoneChoosingPlacement(int placement)
	{
		placementCg.interactable = false;
		gm.SavePlacementsServerRpc(placement, _player.id.Value);
		_player.SetDataUiServerRpc(placement);
	}
	[ServerRpc] public void RevealPlacementCardServerRpc(
		int ind, int playerId, int characterInd, int placement, ulong targetId
	) 
	{
		// already chosen
		if (!placementBtns[ind].enabled) return;

		if (ind >= 0 && ind < placementBtns.Length && placementBtns[ind] != null &&
			ind < placementChosen.Value.Length && !placementChosen.Value[ind])
		{
			placementBtns[ind].enabled = false;
			placementChosen.Value[ind] = true;
			tempPlayerOrder[placement] = playerId;
			RevealPlacementCardClientRpc(ind, new ClientRpcParams{Send={TargetClientIds=new ulong[]{targetId}}});
			RevealPlacementCardClientRpc(ind, characterInd);
		}
		bool allReady = true;
		for (int i=0 ; i<tempPlayerOrder.Count ; i++)
			if (!placementChosen.Value[i])
				allReady = false;
		if (allReady && endPlacementCo == null)
			endPlacementCo = StartCoroutine( EndPlacementCo() );
	}
	[ClientRpc] void RevealPlacementCardClientRpc(int ind, ClientRpcParams rpc)
	{
		if (ind >= 0 && ind < placementBtns.Length && placementBtns[ind] != null)
		{
			placementBtns[ind].ChooseCard();
		}
	}
	[ClientRpc] void RevealPlacementCardClientRpc(int ind, int characterInd)
	{
		if (ind >= 0 && ind < placementBtns.Length && placementBtns[ind] != null)
		{
			placementBtns[ind].enabled = false;
			placementBtns[ind].RevealCard(characterInd);
		}
	}

	Coroutine endPlacementCo;
	IEnumerator EndPlacementCo()
	{
		yield return new WaitForSeconds(1);
		TogglePlacementCardClientRpc(false);
		TogglePlacementUiClientRpc(false);
		nm.SetPlayerOrder(tempPlayerOrder);

		yield return new WaitForSeconds(0.5f);
		teleportCo = StartCoroutine( TeleportToStarCo(0) );
	}
	
	#endregion


	#region Introduction

	// start location camera
	[ServerRpc] public void ToggleStartCamServerRpc(bool active) => ToggleStartCamClientRpc(active);
	[ClientRpc] void ToggleStartCamClientRpc(bool active) => startCam.SetActive(active);
	
	// spawn maaron at start location
	[ServerRpc] public void MaaronIntroServerRpc() => MaaronIntroClientRpc();
	[ClientRpc] void MaaronIntroClientRpc() 
	{ 
		maaronAnim.gameObject.SetActive(false);
		maaronAnim.gameObject.SetActive(true);
		maaronAnim.gameObject.transform.position = maaronSpawnPos.position;
		maaronAnim.transform.rotation = Quaternion.Euler(0,180,0);
	}
	
	#endregion


	#region Trap

	[ServerRpc] public void TrapRewardServerRpc(int atkId, int stolen, bool isStar) 
		=> StartCoroutine(TrapCo(atkId, stolen, isStar));
	IEnumerator TrapCo(int atkId, int stolen, bool isStar)
	{
		yield return new WaitForSeconds(1);
		nm.ToggleBoardControlCam(atkId, true); 

		yield return new WaitForSeconds(1);
		nm.RewardBoardControl(atkId, stolen, isStar);

		yield return new WaitForSeconds(1);
		nm.ToggleBoardControlCam(atkId, false);
	}

	[ServerRpc] public void ThornNodeServerRpc(int nodeId, int playerId, int characterInd, int trapId) 
		=> ThornNodeClientRpc(nodeId, playerId, characterInd, trapId);
	[ClientRpc] private void ThornNodeClientRpc(int nodeId, int playerId, int characterInd, int trapId) 
		=> NodeManager.Instance.GetNode(nodeId).ToggleThorn(true, playerId, characterInd, trapId);

	[ServerRpc] public void PlayDoorAnimServerRpc(int nodeId) => PlayDoorAnimClientRpc(nodeId);
	[ClientRpc] private void PlayDoorAnimClientRpc(int nodeId) => NodeManager.Instance.GetNode(nodeId).PlayDoorAnim();

	[ServerRpc] public void SetNewTollServerRpc(int doorInd, int newToll) => SetNewTollClientRpc(doorInd, newToll);
	[ClientRpc] private void SetNewTollClientRpc(int doorInd, int newToll) => NodeManager.Instance.GetNode(doors[doorInd].nodeId).SetNewToll(newToll);

	#endregion

	#region Chest

	// spawn chests
	[ServerRpc] public void SpawnChestsServerRpc()
	{
		SpawnChestsClientRpc();
		ChooseChestClientRpc(new ClientRpcParams{Send={TargetClientIds=new ulong[]{nm.GetLosingPlayer()}}});
	} 
	[ClientRpc] void SpawnChestsClientRpc() 
	{ 
		maaronAnim.SetTrigger("magic");
		//foreach (TreasureChest chest in chests)
		for (int i=0 ; i<chests.Length ; i++)
		{
			chests[i].gameObject.SetActive(true);
			chests[i].ind = i;
		}
	}
	[ClientRpc] void ChooseChestClientRpc(ClientRpcParams rpc) 
	{ 
		foreach (TreasureChest chest in chests)
			chest.ToggleChooseable(true);
	}

	[ServerRpc] public void SelectChestServerRpc(int n) 
	{
		SelectChestClientRpc(n);
		StartCoroutine( OpenChestCo() );
	}
	[ClientRpc] private void SelectChestClientRpc(int n)
	{
		chests[n].OpenChest();
	}
	IEnumerator OpenChestCo()
	{
		yield return new WaitForSeconds(2);
		MaaronTeleportServerRpc();

		yield return new WaitForSeconds(2);
		//SetMaaronServerRpc(gm.prevStarInd, true);
		SetMaaronServerRpc(gm.prevStarInd, true);
		SetStarNodeServerRpc(starNodes[gm.prevStarInd].node.nodeId, true);
		
		yield return new WaitForSeconds(1);
		ToggleStartCamServerRpc(false);
		nm.NextBoardPlayerTurn();
		ToggleMainUi(true);
		ToggleMainUiServerRpc(true);

		yield return new WaitForSeconds(1);
		ToggleSpotlightServerRpc(true);
	}

	#endregion



	#region Star
	//public void TeleportToStar(int fixedInd=-1) => StartCoroutine( TeleportToStarCo(fixedInd) );
	IEnumerator TeleportToStarCo(int fixedInd=-1, bool changeLoc=false)
	{
		if (changeLoc)
			SetStarNodeServerRpc(gm.prevStarInd, false);
		int rng = fixedInd == -1 ? Random.Range(0, starNodes.Length) : fixedInd;
		while (rng == gm.prevStarInd || 
			(gm.prevStarInd >= 0 && gm.prevStarInd < starNodes.Length &&
			starNodes[gm.prevStarInd].invalid != null && starNodes[gm.prevStarInd].invalid != starNodes[rng].node))
		{
			rng = Random.Range(0, starNodes.Length);
		}
		gm.prevStarInd = rng;
		starCam.m_Follow = starNodes[rng].node.transform;
		MaaronTeleportServerRpc();

		yield return new WaitForSeconds(2f);
		ToggleStartCamServerRpc(false);
		ToggleStarCamServerRpc(true);

		yield return new WaitForSeconds(1f);
		StartCoroutine( SetupStarNode(rng) );
		
		yield return new WaitForSeconds(4);
		ToggleStarCamServerRpc(false);
		
		//yield return new WaitForSeconds(1);
		isIntro = false;
		nm.NextBoardPlayerTurn();
		ToggleMainUi(true);
		ToggleMainUiServerRpc(true);
		teleportCo = null;
	}
	[ServerRpc] public void ChooseStarServerRpc() => StartCoroutine( ChooseStarCo(-1, true) );
	IEnumerator ChooseStarCo(int fixedInd=-1, bool changeLoc=false)
	{
		if (changeLoc)
		{
			ToggleSpotlightServerRpc(false);

			yield return new WaitForSeconds(1f);
			MaaronTeleportClientRpc();

			yield return new WaitForSeconds(1f);
			SetStarNodeServerRpc(starNodes[gm.prevStarInd].node.nodeId, false);

			yield return new WaitForSeconds(0.5f);
		}
		int rng = fixedInd == -1 ? Random.Range(0, starNodes.Length) : fixedInd;
		while (rng == gm.prevStarInd || 
			(gm.prevStarInd >= 0 && gm.prevStarInd < starNodes.Length &&
			starNodes[gm.prevStarInd].invalid != null && starNodes[gm.prevStarInd].invalid == starNodes[rng].node))
		{
			rng = Random.Range(0, starNodes.Length);
		}
		gm.prevStarInd = rng;
		starCam.m_Follow = starNodes[rng].node.transform;

		yield return new WaitForSeconds(1f);
		ToggleStarCamServerRpc(true);

		yield return new WaitForSeconds(1f);
		StartCoroutine( SetupStarNode(rng) );
		
		yield return new WaitForSeconds(4);
		ToggleStarCamServerRpc(false);
	}
	[ServerRpc] void ToggleStarCamServerRpc(bool active) => ToggleStarCamClientRpc(active);
	[ClientRpc] void ToggleStarCamClientRpc(bool active) => starCam.gameObject.SetActive(active);
	private IEnumerator SetupStarNode(int ind)
	{
		SetMaaronServerRpc(ind, true);
		SetStarNodeServerRpc(starNodes[ind].node.nodeId, true);
		
		yield return new WaitForSeconds(2);
		ToggleSpotlightServerRpc(true);
	}

	// maaron teleports
	[ServerRpc] void MaaronTeleportServerRpc() => MaaronTeleportClientRpc();
	[ClientRpc] void MaaronTeleportClientRpc() => maaronAnim.SetTrigger("teleport");

	[ServerRpc] public void MaaronClapServerRpc(bool active) => MaaronClapClientRpc(active);
	[ClientRpc] void MaaronClapClientRpc(bool active) => maaronAnim.SetBool("isClapping", active);

	// set maaron location
	[ServerRpc] void SetMaaronServerRpc(int nodeId, bool isStarSpace) => SetMaaronClientRpc(nodeId, isStarSpace);
	[ClientRpc] void SetMaaronClientRpc(int nodeId, bool isStarSpace) 
	{ 
		maaronAnim.gameObject.SetActive(false);
		maaronAnim.gameObject.SetActive(isStarSpace);
		maaronAnim.gameObject.transform.position = starNodes[nodeId].node.maaronPos.position;
		maaronAnim.transform.LookAt(starNodes[nodeId].node.transform.position, Vector3.up);
	}

	// toggle spotlight for maaron
	[ServerRpc] void ToggleSpotlightServerRpc(bool active) => ToggleSpotlightClientRpc(active);
	[ClientRpc] void ToggleSpotlightClientRpc(bool active) 
	{
		if (active)
			maaronSpotlightPs.Play();
		else
			maaronSpotlightPs.Stop();
	}

	// set node space (star)
	[ServerRpc] void SetStarNodeServerRpc(int nodeId, bool isStarSpace) => SetStarNodeClientRpc(nodeId, isStarSpace);
	[ClientRpc] void SetStarNodeClientRpc(int nodeId, bool isStarSpace) => NodeManager.Instance.GetNode(nodeId).ToggleStarNode(isStarSpace);

	// new stock text
	[ServerRpc] void NewStockServerRpc() => NewStockClientRpc();
	[ClientRpc] void NewStockClientRpc() => newStockUi.SetActive(true);

	// new stock text
	[ServerRpc] void FinalFiveServerRpc() => FinalFiveClientRpc();
	[ClientRpc] void FinalFiveClientRpc() => finalFiveUi.SetActive(true);
	
	#endregion



	#region Winner
	IEnumerator WinnerCo()
	{
		MaaronMagicServerRpc();
		yield return new WaitForSeconds(2);
		nm.PunishNonWinners();

		yield return new WaitForSeconds(1);
		nm.ShowWinner();
		ShowFinalDataServerRpc();
	}
	[ServerRpc] void MaaronMagicServerRpc() => MaaronMagicClientRpc();
	[ClientRpc] void MaaronMagicClientRpc() => maaronAnim.SetTrigger("magicOnly");

	[ServerRpc] void ShowFinalDataServerRpc() => ShowFinalDataClientRpc();
	[ClientRpc] void ShowFinalDataClientRpc() => _player.SetFinalUiServerRpc();
	#endregion

	public void NextPlayerTurn()
	{
		NextPlayerTurnServerRpc(); // calls to server
	}
	[ServerRpc] private void NextPlayerTurnServerRpc()
	{
		//Debug.Log($"<color=cyan>NextPlayerTurnServerRpc()</color>");
		if (turret != null)
		{
			if (nm.StillHavePlayerTurns())
				nm.NextBoardPlayerTurn();
			else
				StartCoroutine(TurretCo());
			//else
			//	nm.NextBoardPlayerTurn();
		}
		else
			nm.NextBoardPlayerTurn();
		//if (!firstTurn)
		//	nPlayerOrder = ++nPlayerOrder;
		//	//nPlayerOrder = ++nPlayerOrder % nPlayers;
		//	players[nPlayerOrder].YourTurn();
		//if (nPlayerOrder.Value >= 0 && nPlayerOrder.Value < players.Length)
		//	gm.NextPlayerTurnServerRpc((ulong) ++nPlayerOrder.Value);
		//else if (nPlayerOrder.Value >= nPlayers)
		//{
		//	//DisablePlayerServerRpc();
		//	gm.LoadPreviewMinigameServerRpc("TestMinigame");
		//}
			//gm.LoadMinigameServerRpc();
			//LoadMinigame("TestMinigame");
	}

	#region Turret
	IEnumerator TurretCo(bool goToNextPlayer=true)
	{
		yield return new WaitForSeconds(goToNextPlayer ? 0 : 0.5f);
		ToggleTurretCamServerRpc(true);

		// ui showing Freakin' giant turret's turn!
		if (goToNextPlayer)
		{
			TurretIntroServerRpc(true);
			
			yield return new WaitForSeconds(1f);
			TurretIntroServerRpc(false);
		}

		yield return new WaitForSeconds(1f);
		TurretTurnServerRpc(++gm.turretReady.Value);
		if (gm.turretReady.Value == 5 || fireTurret)
		{
			int temp = boardCam.m_Priority;
			SetBoardCamPriorityServerRpc(10000);
			ToggleTurretCamServerRpc(false);
			yield return new WaitForSeconds(7);
			SetBoardCamPriorityServerRpc(temp);
		}
		gm.turretReady.Value = gm.turretReady.Value == 5 ? 0 : gm.turretReady.Value;

		yield return new WaitForSeconds(1);
		ToggleTurretCamServerRpc(false);
		if (goToNextPlayer)
			nm.NextBoardPlayerTurn();
	}
	IEnumerator TurretRotateCo()
	{
		yield return new WaitForSeconds(0.5f);
		ToggleTurretCamServerRpc(true);

		yield return new WaitForSeconds(1f);
		TurretRotateClientRpc(gm.turretRot.Value, gm.turretRot.Value+1);
		++gm.turretRot.Value;

		yield return new WaitForSeconds(2);
		ToggleTurretCamServerRpc(false);
	}

	[ServerRpc] public void TurretStartServerRpc() => TurretStartClientRpc(gm.turretReady.Value, gm.turretRot.Value);
	[ClientRpc] void TurretStartClientRpc(int x, int y) => turret.RemoteStart(x, y);

	[ServerRpc] public void SetBoardCamPriorityServerRpc(int val) => SetBoardCamPriorityClientRpc(val);
	[ClientRpc] void SetBoardCamPriorityClientRpc(int val) => boardCam.m_Priority = val;

	[ServerRpc] public void TurretIntroServerRpc(bool active) => TurretIntroClientRpc(active);
	[ClientRpc] void TurretIntroClientRpc(bool active)
	{
		if (active) turretIntro.gameObject.SetActive(true);
		else turretIntro.SetTrigger("close");
	}

	[ServerRpc] public void TurretTurnCoServerRpc() => StartCoroutine( TurretCo(false) );
	[ServerRpc] public void TurretTurnServerRpc(int x) => TurretTurnClientRpc(x);
	[ClientRpc] void TurretTurnClientRpc(int x)
	{
		if (fireTurret) turret.JustFire();
		else turret.IncreaseReady(x);
	} 

	[ServerRpc] public void TurretRotateCoServerRpc() => StartCoroutine( TurretRotateCo() );
	[ClientRpc] void TurretRotateClientRpc(int n, int m) => turret.RotateTurret(n, m);

	[ServerRpc] public void ToggleTurretCamServerRpc(bool active) => ToggleTurretCamClientRpc(active);
	[ClientRpc] void ToggleTurretCamClientRpc(bool active) => turret.ToggleCam(active);

	[ServerRpc] public void ShakeCamServerRpc(float intensity, float duration) => ShakeCamClientRpc(intensity, duration);
	[ClientRpc] void ShakeCamClientRpc(float intensity, float duration) => CinemachineShake.Instance.ShakeCam(intensity, duration);

	#endregion
}

[System.Serializable]
public class starNodes
{
	public Node node;
	public Node invalid;
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using Cinemachine;

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
	//[SerializeField] UiDialogue dialogue;
	UiDialogue dialogue {get{return UiDialogue.Instance;}}


	[Space] [Header("Text")]
	[TextArea(2, 5)] [SerializeField] string[] introSents;
	[TextArea(2, 5)] [SerializeField] string[] lastSents;
	[TextArea(2, 5)] [SerializeField] string[] gameOverSents;


	[Space] [Header("Universal")]
	[SerializeField] private GameObject mainUi;
	[SerializeField] private Transform dataUi;
	[SerializeField] private TextMeshProUGUI turnTxt;
	[SerializeField] private GameObject newStockUi;
	[SerializeField] private GameObject finalFiveUi;
	[SerializeField] private CinemachineVirtualCamera starCam;
	[SerializeField] private Animator maaronAnim;
	[SerializeField] private ParticleSystem maaronSpotlightPs;

	[Space] [SerializeField] private TreasureChest[] chests;
	[SyncVar] public int nBmReady; 
	[SyncVar] public int n;
	bool isChoosingStar;


	[Space] [Header("MUST REFERENCE PER BOARD")]
	[SerializeField] private Node startNode;
	[SerializeField] private starNodes[] starNodes;
	[SerializeField] private BoardTurret turret;
	[SerializeField] private Animator turretIntro;
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
		CmdReadyUp();
		turnTxt.text = $"Turn: {gm.nTurn}/{gm.maxTurns}";

		if (!gm.gameStarted)
			CmdToggleMainUi(false);
		//string s = $"<color=#FF8D07>";
		//s += $"NetworkServer.connections.Count = {NetworkServer.connections.Count} | ";
		//s += $"GameNetworkManager.Instance.numPlayers = {GameNetworkManager.Instance.numPlayers} | ";
		//s += "</color>";
		//Debug.Log(s);
	}

	[Command(requiresAuthority=false)] public void CmdReadyUp()
	{
		++nBmReady;
		//Debug.Log($"<color=white>{nBmReady} >= {nm.numPlayers}</color>");
		if (nBmReady >= nm.numPlayers)
		{
			RpcSetUpPlayer();
			nm.UnparentBoardControls();
		}
	} 
	[ClientRpc] private void RpcSetUpPlayer()
	{
		Debug.Log($"<color=white>Setting Up</color>");
		_player = PlayerControls.Instance;
		if (gm.nTurn == 1)
			_player.SetStartNode(startNode);
		//_player.RemoteStart(spawnPos);
		if (isServer)
			StartCoroutine( StartGameCo() );
	}
	public Transform GetSpawnPos() => spawnPos;
	public int GetNth()
	{
		return n++;
	}
	
	#region Start game
	IEnumerator StartGameCo()
	{
		//yield return new WaitForSeconds(0.5f);
		//gm.CmdTriggerTransition(false);
		if (turret != null)
			CmdTurretStart();

		yield return new WaitForSeconds(1);
		// turn 1
		if (!gm.gameStarted)
		{
			isIntro = gm.gameStarted = true;
			CmdToggleStartCam(true);
			
			yield return new WaitForSeconds(1.5f);
			CmdMaaronIntro();

			yield return new WaitForSeconds(2);
			CmdToggleDialogue(true, introSents);
			CmdToggleNextButton(true);

			yield break;
			//yield return ChooseStarCo(0);
		}
		// Game Over
		else if (gm.nTurn > gm.maxTurns)
		{
			//isIntro = gm.gameStarted = true;
			gameOver = true;
			CmdToggleStartCam(true);

			//yield return new WaitForSeconds(1f);
			//CmdFinalFive();
			
			yield return new WaitForSeconds(0.5f);
			CmdMaaronIntro();

			yield return new WaitForSeconds(2);
			CmdToggleDialogue(true, gameOverSents);
			CmdToggleNextButton(true);

			yield break;
		}
		// last 5 turns
		else if (gm.nTurn == gm.maxTurns - 4)
		{
			//isIntro = gm.gameStarted = true;
			isLast5 = true;
			CmdToggleStartCam(true);

			yield return new WaitForSeconds(1f);
			CmdFinalFive();
			
			yield return new WaitForSeconds(0.5f);
			CmdMaaronIntro();

			yield return new WaitForSeconds(2);
			CmdToggleDialogue(true, lastSents);
			CmdToggleNextButton(true);

			yield break;
		}
		// turn 2+
		else
		{
			CmdToggleMainUi(true);
			StartCoroutine( SetupStarNode(gm.prevStarInd) );
		}

		yield return new WaitForSeconds(0.5f);
		nm.NextBoardPlayerTurn();

		if (gm.nTurn == 5)
		{
			yield return new WaitForSeconds(1);
			CmdNewStock();
		}
		if (gm.nTurn == gm.maxTurns - 4)
		{
			yield return new WaitForSeconds(1);
			CmdNewStock();
		}
	}
	#endregion

	#region Dialogue
	
	// ui = player data, turn
	[Command(requiresAuthority=false)] public void CmdToggleMainUi(bool active) => RpcToggleMainUi(active);
	[ClientRpc] void RpcToggleMainUi(bool active) => mainUi.gameObject.SetActive(active);
	
	// Dialogue
	[Command(requiresAuthority=false)] public void CmdToggleDialogue(bool active, string[] sents) => RpcToggleDialogue(active, sents);
	[ClientRpc] void RpcToggleDialogue(bool active, string[] sents) => dialogue.SetSentence(active, "Maaron", sents);

	[Command(requiresAuthority=false)] public void CmdToggleNextButton(bool targeted) 
	{
		if (targeted)
			TargetToggleNextButton(_player.netIdentity.connectionToClient);
		else
			RpcDisableNextButton();
	}
	[TargetRpc] void TargetToggleNextButton(NetworkConnectionToClient target) => dialogue.ToggleButton(true);
	[ClientRpc] void RpcDisableNextButton() => dialogue.ToggleButton(false);
	
	[Command(requiresAuthority=false)] public void CmdNextDialogue() => RpcNextDialogue();
	[ClientRpc] void RpcNextDialogue() => dialogue.NextSentence();

	Coroutine teleportCo;
	//~ --------------------------------------------------------
	[Command(requiresAuthority=false)] public void CmdEndDialogue() 
	{
		RpcEndDialogue();
		if (isIntro && isServer && teleportCo == null)
		{
			teleportCo = StartCoroutine( TeleportToStarCo(0) );
		}
		if (isLast5 && isServer)
		{
			CmdSpawnChests();
			//teleportCo = StartCoroutine( TeleportToStarCo(0) );
		}
		if (gameOver && isServer)
		{
			StartCoroutine( WinnerCo() );
		}
	}
	[ClientRpc] void RpcEndDialogue() => dialogue.CloseDialogue();

	#endregion



	#region Introduction

	// start location camera
	[Command(requiresAuthority=false)] public void CmdToggleStartCam(bool active) => RpcToggleStartCam(active);
	[ClientRpc] void RpcToggleStartCam(bool active) => startCam.SetActive(active);
	
	// spawn maaron at start location
	[Command(requiresAuthority=false)] public void CmdMaaronIntro() => RpcMaaronIntro();
	[ClientRpc] void RpcMaaronIntro() 
	{ 
		maaronAnim.gameObject.SetActive(false);
		maaronAnim.gameObject.SetActive(true);
		maaronAnim.gameObject.transform.position = maaronSpawnPos.position;
		maaronAnim.transform.rotation = Quaternion.Euler(0,180,0);
	}
	
	#endregion


	#region Trap

	[Command(requiresAuthority=false)] public void CmdTrapReward(int atkId, int stolen) => StartCoroutine(TrapCo(atkId, stolen));
	IEnumerator TrapCo(int atkId, int stolen)
	{
		yield return new WaitForSeconds(1);
		nm.ToggleBoardControlCam(atkId, true);

		yield return new WaitForSeconds(1);
		nm.RewardBoardControl(atkId, stolen);

		yield return new WaitForSeconds(1);
		nm.ToggleBoardControlCam(atkId, false);
	}

	[Command(requiresAuthority=false)] public void CmdThornNode(int nodeId, int playerId) => RpcThornNode(nodeId, playerId);
	[ClientRpc] private void RpcThornNode(int nodeId, int playerId) => NodeManager.Instance.GetNode(nodeId).ToggleThorn(true, playerId);
	#endregion

	#region Chest

	// spawn chests
	[Command(requiresAuthority=false)] public void CmdSpawnChests()
	{
		RpcSpawnChests();
		TargetChooseChest(nm.GetLosingPlayer());
	} 
	[ClientRpc] void RpcSpawnChests() 
	{ 
		maaronAnim.SetTrigger("magic");
		//foreach (TreasureChest chest in chests)
		for (int i=0 ; i<chests.Length ; i++)
		{
			chests[i].gameObject.SetActive(true);
			chests[i].ind = i;
		}
	}
	[TargetRpc] void TargetChooseChest(NetworkConnectionToClient target) 
	{ 
		foreach (TreasureChest chest in chests)
			chest.ToggleChooseable(true);
	}

	[Command(requiresAuthority=false)] public void CmdSelectChest(int n) 
	{
		RpcSelectChest(n);
		StartCoroutine( OpenChestCo() );
	}
	[ClientRpc] private void RpcSelectChest(int n)
	{
		chests[n].OpenChest();
	}
	IEnumerator OpenChestCo()
	{
		yield return new WaitForSeconds(2);
		CmdMaaronTeleport();

		yield return new WaitForSeconds(2);
		//CmdSetMaaron(gm.prevStarInd, true);
		CmdSetMaaron(gm.prevStarInd, true);
		CmdSetStarNode(starNodes[gm.prevStarInd].node.nodeId, true);
		
		yield return new WaitForSeconds(1);
		CmdToggleStartCam(false);
		nm.NextBoardPlayerTurn();
		CmdToggleMainUi(true);

		yield return new WaitForSeconds(1);
		CmdToggleSpotlight(true);
	}

	#endregion



	#region Star
	//public void TeleportToStar(int fixedInd=-1) => StartCoroutine( TeleportToStarCo(fixedInd) );
	IEnumerator TeleportToStarCo(int fixedInd=-1, bool changeLoc=false)
	{
		if (changeLoc)
			CmdSetStarNode(gm.prevStarInd, false);
		int rng = fixedInd == -1 ? Random.Range(0, starNodes.Length) : fixedInd;
		while (rng == gm.prevStarInd || 
			(gm.prevStarInd >= 0 && gm.prevStarInd < starNodes.Length &&
			starNodes[gm.prevStarInd].invalid != null && starNodes[gm.prevStarInd].invalid != starNodes[rng].node))
		{
			rng = Random.Range(0, starNodes.Length);
		}
		gm.prevStarInd = rng;
		starCam.m_Follow = starNodes[rng].node.transform;
		CmdMaaronTeleport();

		yield return new WaitForSeconds(2f);
		CmdToggleStartCam(false);
		CmdToggleStarCam(true);

		yield return new WaitForSeconds(1f);
		StartCoroutine( SetupStarNode(rng) );
		
		yield return new WaitForSeconds(4);
		CmdToggleStarCam(false);
		
		//yield return new WaitForSeconds(1);
		isIntro = false;
		nm.NextBoardPlayerTurn();
		CmdToggleMainUi(true);
		teleportCo = null;
	}
	[Command(requiresAuthority=false)] public void CmdChooseStar() => StartCoroutine( ChooseStarCo() );
	IEnumerator ChooseStarCo(int fixedInd=-1, bool changeLoc=false)
	{
		if (changeLoc)
			CmdSetStarNode(gm.prevStarInd, false);
		int rng = fixedInd == -1 ? Random.Range(0, starNodes.Length) : fixedInd;
		while (rng == gm.prevStarInd || 
			(gm.prevStarInd >= 0 && gm.prevStarInd < starNodes.Length &&
			starNodes[gm.prevStarInd].invalid != null && starNodes[gm.prevStarInd].invalid != starNodes[rng].node))
		{
			rng = Random.Range(0, starNodes.Length);
		}
		gm.prevStarInd = rng;
		starCam.m_Follow = starNodes[rng].node.transform;

		yield return new WaitForSeconds(1f);
		CmdToggleStarCam(true);

		yield return new WaitForSeconds(1f);
		StartCoroutine( SetupStarNode(rng) );
		
		yield return new WaitForSeconds(4);
		CmdToggleStarCam(false);
	}
	[Command(requiresAuthority=false)] void CmdToggleStarCam(bool active) => RpcToggleStarCam(active);
	[ClientRpc] void RpcToggleStarCam(bool active) => starCam.gameObject.SetActive(active);
	private IEnumerator SetupStarNode(int ind)
	{
		CmdSetMaaron(ind, true);
		CmdSetStarNode(starNodes[ind].node.nodeId, true);
		
		yield return new WaitForSeconds(2);
		CmdToggleSpotlight(true);
	}

	// maaron teleports
	[Command(requiresAuthority=false)] void CmdMaaronTeleport() => RpcMaaronTeleport();
	[ClientRpc] void RpcMaaronTeleport() => maaronAnim.SetTrigger("teleport");

	// set maaron location
	[Command(requiresAuthority=false)] void CmdSetMaaron(int nodeId, bool isStarSpace) => RpcSetMaaron(nodeId, isStarSpace);
	[ClientRpc] void RpcSetMaaron(int nodeId, bool isStarSpace) 
	{ 
		maaronAnim.gameObject.SetActive(false);
		maaronAnim.gameObject.SetActive(isStarSpace);
		maaronAnim.gameObject.transform.position = starNodes[nodeId].node.maaronPos.position;
		maaronAnim.transform.LookAt(starNodes[nodeId].node.transform.position, Vector3.up);
	}

	// toggle spotlight for maaron
	[Command(requiresAuthority=false)] void CmdToggleSpotlight(bool active) => RpcToggleSpotlight(active);
	[ClientRpc] void RpcToggleSpotlight(bool active) 
	{
		if (active)
			maaronSpotlightPs.Play();
		else
			maaronSpotlightPs.Stop();
	}

	// set node space (star)
	[Command(requiresAuthority=false)] void CmdSetStarNode(int nodeId, bool isStarSpace) => RpcSetStarNode(nodeId, isStarSpace);
	[ClientRpc] void RpcSetStarNode(int nodeId, bool isStarSpace) => NodeManager.Instance.GetNode(nodeId).ToggleStarNode(isStarSpace);

	// new stock text
	[Command(requiresAuthority=false)] void CmdNewStock() => RpcNewStock();
	[ClientRpc] void RpcNewStock() => newStockUi.SetActive(true);

	// new stock text
	[Command(requiresAuthority=false)] void CmdFinalFive() => RpcFinalFive();
	[ClientRpc] void RpcFinalFive() => finalFiveUi.SetActive(true);
	
	#endregion



	#region Winner
	IEnumerator WinnerCo()
	{
		yield return new WaitForSeconds(2);
		int winnerId = nm.GetWinningPlayer();
	}
	#endregion

	public void NextPlayerTurn()
	{
		CmdNextPlayerTurn(); // calls to server
	}
	[Command(requiresAuthority=false)] private void CmdNextPlayerTurn()
	{
		//Debug.Log($"<color=cyan>CmdNextPlayerTurn()</color>");
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
		//	gm.CmdNextPlayerTurn((ulong) ++nPlayerOrder.Value);
		//else if (nPlayerOrder.Value >= nPlayers)
		//{
		//	//CmdDisablePlayer();
		//	gm.CmdLoadPreviewMinigame("TestMinigame");
		//}
			//gm.CmdLoadMinigame();
			//LoadMinigame("TestMinigame");
	}

	#region Turret
	IEnumerator TurretCo(bool goToNextPlayer=true)
	{
		yield return new WaitForSeconds(goToNextPlayer ? 1 : 0.5f);
		CmdToggleTurretCam(true);
		if (goToNextPlayer)
		{
			CmdTurretIntro(true);
			
			yield return new WaitForSeconds(1f);
			CmdTurretIntro(false);
		}

		yield return new WaitForSeconds(1f);
		CmdTurretTurn(++gm.turretReady);
		if (gm.turretReady == 5 || fireTurret)
		{
			CmdToggleTurretCam(false);
			yield return new WaitForSeconds(7);
		}
		gm.turretReady = gm.turretReady == 5 ? 0 : gm.turretReady;

		yield return new WaitForSeconds(1);
		CmdToggleTurretCam(false);
		if (goToNextPlayer)
			nm.NextBoardPlayerTurn();
	}
	IEnumerator TurretRotateCo()
	{
		yield return new WaitForSeconds(0.5f);
		CmdToggleTurretCam(true);

		yield return new WaitForSeconds(1f);
		RpcTurretRotate(gm.turretRot, gm.turretRot+1);
		++gm.turretRot;

		yield return new WaitForSeconds(2);
		CmdToggleTurretCam(false);
	}

	[Command(requiresAuthority=false)] public void CmdTurretStart() => RpcTurretStart(gm.turretReady, gm.turretRot);
	[ClientRpc] void RpcTurretStart(int x, int y) => turret.RemoteStart(x, y);

	[Command(requiresAuthority=false)] public void CmdTurretIntro(bool active) => RpcTurretIntro(active);
	[ClientRpc] void RpcTurretIntro(bool active)
	{
		if (active) turretIntro.gameObject.SetActive(true);
		else turretIntro.SetTrigger("close");
	}

	[Command(requiresAuthority=false)] public void CmdTurretTurnCo() => StartCoroutine( TurretCo(false) );
	[Command(requiresAuthority=false)] public void CmdTurretTurn(int x) => RpcTurretTurn(x);
	[ClientRpc] void RpcTurretTurn(int x)
	{
		if (fireTurret) turret.JustFire();
		else turret.IncreaseReady(x);
	} 

	[Command(requiresAuthority=false)] public void CmdTurretRotateCo() => StartCoroutine( TurretRotateCo() );
	//[Command(requiresAuthority=false)] public void CmdTurretRotate() { gm.turretRot++; RpcTurretRotate(gm.turretRot); }
	[ClientRpc] void RpcTurretRotate(int n, int m) => turret.RotateTurret(n, m);

	[Command(requiresAuthority=false)] public void CmdToggleTurretCam(bool active) => RpcToggleTurretCam(active);
	[ClientRpc] void RpcToggleTurretCam(bool active) => turret.ToggleCam(active);

	[Command(requiresAuthority=false)] public void CmdShakeCam(float intensity, float duration) => RpcShakeCam(intensity, duration);
	[ClientRpc] void RpcShakeCam(float intensity, float duration) => CinemachineShake.Instance.ShakeCam(intensity, duration);

	#endregion
}

[System.Serializable]
public class starNodes
{
	public Node node;
	public Node invalid;
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class BoardManager : NetworkBehaviour
{
	public static BoardManager Instance;
	[SerializeField] private PlayerControls playerPrefab;
	[SerializeField] private Transform spawnPos;
	[SerializeField] private PlayerControls[] players;
	private PlayerControls _player;
	GameManager gm {get{return GameManager.Instance;}}
	GameNetworkManager nm {get{return GameNetworkManager.Instance;}}


	[Space] [Header("Universal")]
	[SerializeField] private Transform dataUi;
	[SerializeField] private TextMeshProUGUI turnTxt;
	[SyncVar] public int nBmReady; 
	[SyncVar] public int n;


	[Space] [Header("MUST REFERENCE PER BOARD")]
	[SerializeField] private Node startNode;


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
		//string s = $"<color=#FF8D07>";
		//s += $"NetworkServer.connections.Count = {NetworkServer.connections.Count} | ";
		//s += $"GameNetworkManager.Instance.numPlayers = {GameNetworkManager.Instance.numPlayers} | ";
		//s += "</color>";
		//Debug.Log(s);
	}

	[Command(requiresAuthority=false)] public void CmdReadyUp()
	{
		++nBmReady;
		Debug.Log($"<color=white>{nBmReady} >= {nm.numPlayers}</color>");
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
	public Transform GetSpawnPos()
	{
		return spawnPos;
	}
	public int GetNth()
	{
		return n++;
	}
	
	IEnumerator StartGameCo()
	{
		//yield return new WaitForSeconds(0.5f);
		//gm.CmdTriggerTransition(false);
		
		yield return new WaitForSeconds(1);
		nm.NextBoardPlayerTurn();
	}

	public void NextPlayerTurn()
	{
		CmdNextPlayerTurn();
	}
	[Command(requiresAuthority=false)] private void CmdNextPlayerTurn()
	{
		Debug.Log($"<color=cyan>CmdNextPlayerTurn()</color>");
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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Experimental.AI;

public class BoardManager : NetworkBehaviour
{
	public static BoardManager Instance;
	//[SerializeField] private NetworkObject playerToSpawn;
	[SerializeField] private Transform spawnPos;
	[SerializeField] private PlayerControls[] players;
	private PlayerControls _player;
	int nTurn;
	//public NetworkVariable<int> nPlayerOrder = new NetworkVariable<int>(
	//	0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	int nPlayers;
	GameManager gm;


	[Space] [Header("Universal")]
	[SerializeField] private Transform dataUi;


	[Space] [Header("MUST REFERENCE PER BOARD")]
	[SerializeField] private Node startNode;


	private void Awake() 
	{
		Instance = this;		
	}

	public Transform GetUiLayout()
	{
		return dataUi;
	}

	private void Start() 
	{
		gm = GameManager.Instance;

		//Debug.Log($"<color=magenta>===> {NetworkManager.Singleton.LocalClientId}</color>");
		//CmdSpawnPlayer((int) NetworkManager.Singleton.LocalClientId);

		/* only host can start game */
		//if (!IsHost) return;
		//nPlayers = gm.nPlayers.Value;
		players = new PlayerControls[nPlayers];
		StartCoroutine( StartGameCo() );
	}

	[Command(requiresAuthority=false)] public void CmdCmdSpawnPlayer(int clientId)
	{
		//var networkObject = NetworkManager.SpawnManager.InstantiateAndSpawn(
		//	playerToSpawn, (ulong) clientId, position:spawnPos.position + new Vector3(-2 + 2*clientId,0,0), destroyWithScene:true);
		//var p = networkObject.GetComponent<PlayerControls>();
		if (!gm.hasStarted)
			RpcSpawnPlayer(clientId);
			//p.SetStartNode(startNode);
		//Debug.Log($"{p.name} Joined");
	}
	[ClientRpc] private void RpcSpawnPlayer(int clientId)
	{ 
		_player = PlayerControls.Instance;
		_player.SetStartNode(startNode);
	}

	IEnumerator StartGameCo()
	{
		yield return new WaitForSeconds(0.5f);
		gm.CmdTriggerTransition(false);
		
		yield return new WaitForSeconds(1);
		gm.CmdNextPlayerTurn(0);
		//NextPlayerTurn(true);
	}

	[ClientRpc] public void RpcNextPlayerTurn()
	{
		//Debug.Log($"<color=magenta>-- PLAYER {NetworkManager.Singleton.LocalClientId}'s TURN</color>");
		if (_player == null)
			_player = PlayerControls.Instance;
		else
			Debug.Log("<color=red>PlayerControls.Instance is NULL</color");
		_player.YourTurn();
	}
	public void NextPlayerTurn(bool firstTurn=false)
	{
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

	void LoadMinigame(string minigameName)
	{
		//gm.LoadPreviewMinigame(minigameName);
	}
}

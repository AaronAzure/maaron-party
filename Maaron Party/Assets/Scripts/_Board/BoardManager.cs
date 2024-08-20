using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Experimental.AI;

public class BoardManager : NetworkBehaviour
{
	public static BoardManager Instance;
	[SerializeField] private PlayerControls playerPrefab;
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
		//return dataUi;
	}


	private void Start() 
	{
		gm = GameManager.Instance;

		//!Debug.Log($"<color=magenta>===> {NetworkManager.Singleton.LocalClientId}</color>");
		//!Debug.Log($"<color=magenta>===> BoardManager.Start()</color>");
		//CmdSpawnPlayer(connectionToClient);

		if (isServer)
			GameNetworkManager.Instance.BoardManagerStart();

		//!Debug.Log($"<color=cyan>isOwned = {isOwned} | isLocalPlayer={isLocalPlayer} | isClient = {isClient}</color>");
		if (isClient)
		{
			//!Debug.Log($"<color=cyan>PlayerControls.Instance = {PlayerControls.Instance}</color>");
			_player = PlayerControls.Instance;
			_player.SetStartNode(startNode);
			_player.RemoteStart(spawnPos);
		}

		/* only host can start game */
		//if (!IsHost) return;
		//nPlayers = gm.nPlayers.Value;
		
		//players = new PlayerControls[nPlayers];
		//StartCoroutine( StartGameCo() );
	}

	[Command(requiresAuthority=false)] public void CmdSpawnPlayer(NetworkConnectionToClient conn)
	{
		Debug.Log($"<color=white>{conn.connectionId} = CmdSpawnPlayer()</color>");
		PlayerControls player = Instantiate(playerPrefab);

        // Apply data from the message however appropriate for your game
        // Typically Player would be a component you write with syncvars or properties
        //player.hairColor = message.hairColor;
        //player.eyeColor = message.eyeColor;
        //player.name = message.name;
        //player.race = message.race;

        // call this to use this gameobject as the primary controller
        NetworkServer.AddPlayerForConnection(conn, player.gameObject);


		//var networkObject = NetworkManager.SpawnManager.InstantiateAndSpawn(
		//	playerToSpawn, (ulong) clientId, position:spawnPos.position + new Vector3(-2 + 2*clientId,0,0), destroyWithScene:true);
		//var p = networkObject.GetComponent<PlayerControls>();
		//if (!gm.hasStarted)
		//	RpcSpawnPlayer(clientId);
			//p.SetStartNode(startNode);
		//Debug.Log($"{p.name} Joined");
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

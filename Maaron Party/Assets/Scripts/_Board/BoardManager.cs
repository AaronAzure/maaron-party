using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BoardManager : NetworkBehaviour
{
	public static BoardManager Instance;
	//[SerializeField] private PlayerControls playerToSpawn;
	[SerializeField] private NetworkObject playerToSpawn;
	[SerializeField] private Transform spawnPos;
	[SerializeField] private PlayerControls[] players;
	int nTurn;
	int nPlayerOrder;
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
		gm.TriggerTransition(false);

		//var networkObject = NetworkManager.SpawnManager.InstantiateAndSpawn(
		//	playerToSpawn, OwnerClientId, position:spawnPos.position + new Vector3(-2 + 2*(int)OwnerClientId,0,0));
		//var p = networkObject.GetComponent<PlayerControls>();
		//p.SetModel((int) OwnerClientId);
		//p.SetId((int) OwnerClientId);
		//if (!gm.hasStarted)
		//	p.SetStartNode(startNode);
		//Debug.Log($"{p.name} Joined");
		//players[i] = p;
		Debug.Log($"==> {ClientObject.Instance.id}");
		SpawnPlayerServerRpc((int) ClientObject.Instance.id);

		if (!IsHost) return;
		nPlayers = gm.nPlayers.Value;
		players = new PlayerControls[nPlayers];
		//for (int i=0 ; i<nPlayers ; i++)
		//{
		//	//var p = Instantiate(playerToSpawn, spawnPos.position + new Vector3(-2 + 2*i,0,0) , Quaternion.identity);
		//	//p.GetComponent<NetworkObject>().SpawnWithOwnership((ulong) i);
		//	var networkObject = NetworkManager.SpawnManager.InstantiateAndSpawn(
		//		playerToSpawn, (ulong) i);
		//		//playerToSpawn, (ulong) i, position:spawnPos.position + new Vector3(-2 + 2*i,0,0));
		//	var p = networkObject.GetComponent<PlayerControls>();

		//	//p.transform.position = spawnPos.position + new Vector3(-2 + 2*i,0,0);
		//	//var p = Instantiate(playerToSpawn, spawnPos.position + new Vector3(-2 + 2*i,0,0) , Quaternion.identity);
		//	p.SetModel(i);
		//	p.SetId(i);
		//	if (!gm.hasStarted)
		//	{
		//		p.SetStartNode(startNode);
		//	}
		//	players[i] = p;
		//	Debug.Log($"{p.name} Joined");
		//}
		//NextPlayerTurn(true);
	}

	[ServerRpc(RequireOwnership=false)] public void SpawnPlayerServerRpc(int clientId)
	{
		var networkObject = NetworkManager.SpawnManager.InstantiateAndSpawn(
			playerToSpawn, (ulong) clientId, position:spawnPos.position + new Vector3(-2 + 2*clientId,0,0));
		var p = networkObject.GetComponent<PlayerControls>();
		p.SetModel(clientId);
		//p.SetId(clientId);
		if (!gm.hasStarted)
			p.SetStartNode(startNode);
		Debug.Log($"{p.name} Joined");
	}

	public void NextPlayerTurn(bool firstTurn=false)
	{
		if (!firstTurn)
			nPlayerOrder = ++nPlayerOrder;
			//nPlayerOrder = ++nPlayerOrder % nPlayers;
		if (nPlayerOrder >= 0 && nPlayerOrder < players.Length)
			players[nPlayerOrder].YourTurn();
		else if (nPlayerOrder >= nPlayers)
			LoadMinigame("TestMinigame");
	}

	void LoadMinigame(string minigameName)
	{
		gm.LoadPreviewMinigame(minigameName);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Org.BouncyCastle.Crypto.Macs;

public class GameNetworkManager : NetworkManager
{
	public static GameNetworkManager Instance;
	public Transform spawnHolder;
	//GameObject ball;
	int nPlayers;
	[SerializeField] private GameObject buttons;
	[SerializeField] private Button hostBtn;
	[SerializeField] private Button clientBtn;
	[SerializeField] private Button startBtn;


	[Space] [Header("Scenes")]
	[Scene] [SerializeField] private string lobbyScene;
	[Scene] [SerializeField] private string boardScene;



	public override void Awake() 
	{
		base.Awake();
		Instance = this;
		
		hostBtn.onClick.AddListener(() => {
			_START_HOST();
		});	
		clientBtn.onClick.AddListener(() => {
			_START_CLIENT();
		});	
		startBtn.onClick.AddListener(() => {
			StartGame();
		});	
	}

	public void _START_HOST()
	{
		buttons.SetActive(false);
		StartHost();
		startBtn.gameObject.SetActive(true);
	}
	public void _START_CLIENT()
	{
		buttons.SetActive(false);
		StartClient();
	}
	public void StartGame()
	{
		GameManager.Instance.StartGame();
		nPlayers = NetworkServer.connections.Count;
		Debug.Log($"<color=magenta>NetworkServer.connections.Count = {NetworkServer.connections.Count}</color>");
		if (GameManager.Instance != null)
			startBtn.gameObject.SetActive(false);
	}

	public override void OnServerAddPlayer(NetworkConnectionToClient conn)
	{
		base.OnServerAddPlayer(conn);
		// add player at correct spawn position
		//Transform start = spawnHolder;
		//GameObject player = Instantiate(playerPrefab, start.position, start.rotation);
		//player.name = $"__PLAYER {nPlayers++}";
		//NetworkServer.AddPlayerForConnection(conn, player);

		// spawn ball if two players
		//if (numPlayers == 2)
		//{
		//	ball = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "Ball"));
		//	NetworkServer.Spawn(ball);
		//}
	}

	public override void OnServerDisconnect(NetworkConnectionToClient conn)
	{
		// destroy ball
		//if (ball != null)
		//	NetworkServer.Destroy(ball);

		// call base functionality (actually destroys the player)
		base.OnServerDisconnect(conn);
	}
}

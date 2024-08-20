using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.SceneManagement;

public class GameNetworkManager : NetworkManager
{
	public struct CreateMMOCharacterMessage : NetworkMessage
	{
		public Race race;
		public string name;
		public Color hairColor;
		public Color eyeColor;
	}

	public enum Race
	{
		None,
		Elvish,
		Dwarvish,
		Human
	}

	#region Variables
	public static GameNetworkManager Instance;
	public Transform spawnHolder;
	//GameObject ball;
	int nPlayers;
	[SerializeField] private GameObject buttons;
	[SerializeField] private Button hostBtn;
	[SerializeField] private Button clientBtn;
	[SerializeField] private Button startBtn;

	[Scene] [SerializeField] private Animator anim;


	[Space] [Header("Scenes")]
	[SerializeField] private int minPlayers=2;
	[Scene] [SerializeField] private string lobbyScene;
	[Scene] [SerializeField] private string boardScene;

	[Space] [SerializeField] private LobbyObject lobbyPlayerPrefab;
	[SerializeField] private List<LobbyObject> lobbyPlayers = new();
	//private Dictionary<NetworkConnection, GameObject> lobbyPlayers = new();
	
	[Space] [SerializeField] private GameObject boardPlayerPrefab;

	[Space] [SerializeField] private MinigameControls gamePlayerPrefab;


	#endregion



	#region Methods
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

	public override void OnStartServer()
    {
        base.OnStartServer();

        NetworkServer.RegisterHandler<CreateMMOCharacterMessage>(OnCreateCharacter);
    }

	public override void OnStopServer()
	{
		base.OnStopServer();
		lobbyPlayers.Clear();
	}

	public void AddConnection(LobbyObject lo)
	{
		lobbyPlayers.Add(lo);
	}
	public void RemoveConnection(LobbyObject lo)
	{
		if (lobbyPlayers.Contains(lo))
			lobbyPlayers.Remove(lo);
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
		StartBoardGame();
		nPlayers = NetworkServer.connections.Count;
		Debug.Log($"<color=magenta>NetworkServer.connections.Count = {NetworkServer.connections.Count}</color>");
		//if (GameManager.Instance != null)
		startBtn.gameObject.SetActive(false);
	}

	//private void DestroyLobbyPlayers()
	//{
	//	foreach (LobbyObject lo in lobbyPlayers)
	//	{
	//		NetworkServer.Destroy(lo.connectionToClient.identity.gameObject);
	//	}
	//}
	//public override void SpawnBoardPlayers()
	public override void ServerChangeScene(string newSceneName)
	{
		if (SceneManager.GetActiveScene().name == lobbyScene)
		{
			foreach (LobbyObject lo in lobbyPlayers)
			{
				NetworkServer.Destroy(lo.connectionToClient.identity.gameObject);
			
				//GameObject player = Instantiate(boardPlayerPrefab);
				//NetworkServer.ReplacePlayerForConnection(lo.connectionToClient, player);
			}
		}
		base.ServerChangeScene(newSceneName);
	}


	public void StartBoardGame()
	{
		StartCoroutine( StartBoardGameCo() );
	}
	
	IEnumerator StartBoardGameCo()
	{
		GameManager.Instance.CmdTriggerTransition(true);
		
		yield return new WaitForSeconds(0.5f);
		ServerChangeScene("TestBoard");
		
		//while (NetworkServer.isLoadingScene)
		//	yield return null;
		//CmdTriggerTransition(false);
		//SceneManager.LoadScene("TestBoard", LoadSceneMode.Single);
		//NetworkManager.Singleton.SceneManager.LoadScene("TestBoard", LoadSceneMode.Single);
	}




	void OnCreateCharacter(NetworkConnectionToClient conn, CreateMMOCharacterMessage message)
    {
        // playerPrefab is the one assigned in the inspector in Network
        // Manager but you can use different prefabs per race for example
        GameObject gameobject = Instantiate(playerPrefab);

        // Apply data from the message however appropriate for your game
        // Typically Player would be a component you write with syncvars or properties
        //LobbyObject player = gameobject.GetComponent();
        //player.hairColor = message.hairColor;
        //player.eyeColor = message.eyeColor;
        //player.name = message.name;
        //player.race = message.race;

        // call this to use this gameobject as the primary controller
        NetworkServer.AddPlayerForConnection(conn, gameobject);
    }
	public override void OnServerAddPlayer(NetworkConnectionToClient conn)
	{
		Debug.Log($"<color=yellow>OnServerAddPlayer</color>");
		//base.OnServerAddPlayer(conn);
		
		// add player at correct spawn position
		if (spawnHolder != null)
		{
			Transform start = spawnHolder;
			GameObject player = Instantiate(playerPrefab, start.position, start.rotation);
			player.name = $"_{playerPrefab.name} [connId={conn.connectionId}]_";
			NetworkServer.AddPlayerForConnection(conn, player);
		}
		//player.name = $"__PLAYER {nPlayers++}";

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
	#endregion
}

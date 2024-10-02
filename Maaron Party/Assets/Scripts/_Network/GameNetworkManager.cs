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
	private GameManager gm {get{return GameManager.Instance;}}
	//GameObject ball;
	[SerializeField] private GameObject buttons;
	[SerializeField] private Button hostBtn;
	[SerializeField] private Button clientBtn;
	[SerializeField] private Button startBtn;

	[Scene] [SerializeField] private Animator anim;

	
	[Space] [Header("Network")]
	[SerializeField] private List<NetworkConnectionToClient> conns = new();
	//bool isInTransition;
	[Space] [SerializeField] private int fixedGame=-1;
	[SerializeField] private bool skipSideQuests;


	[Space] [Header("Scenes")]
	[SerializeField] private int minPlayers=2;
	[Scene] [SerializeField] private string lobbyScene;
	[Scene] [SerializeField] private string boardScene;
	[Scene] [SerializeField] private string practiceScene;
	int nMinigame;
	[Scene] [SerializeField] private string[] minigameScenes;

	[Space] [SerializeField] private GameObject previewObj;


	[Space] [SerializeField] private LobbyObject lobbyPlayerPrefab;
	[SerializeField] private PlayerControls boardPlayerPrefab;
	[SerializeField] private MinigameControls gamePlayerPrefab;

	[Space] [SerializeField] private List<LobbyObject> lobbyPlayers = new();
	[SerializeField] private List<PlayerControls> boardControls = new();
	[SerializeField] private List<MinigameControls> minigameControls = new();
	//bool isLoadingScene


	#endregion



	#region Network Methods
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
		boardControls.Clear();
		minigameControls.Clear();
		conns.Clear();
	}

	public void AddConnection(LobbyObject lo)
	{
		if (!lobbyPlayers.Contains(lo))
			lobbyPlayers.Add(lo);
	}
	public void RemoveConnection(LobbyObject lo)
	{
		if (lobbyPlayers.Contains(lo))
			lobbyPlayers.Remove(lo);
	}
	public void AddBoardConnection(PlayerControls pc)
	{
		if (!boardControls.Contains(pc))
			boardControls.Add(pc);
	}
	public void RemoveBoardConnection(PlayerControls pc)
	{
		if (boardControls.Contains(pc))
			boardControls.Remove(pc);
	}
	public void AddMinigameConnection(MinigameControls mc)
	{
		if (!minigameControls.Contains(mc))
			minigameControls.Add(mc);
	}
	public void RemoveMinigameConnection(MinigameControls mc)
	{
		if (minigameControls.Contains(mc))
			minigameControls.Remove(mc);
	}

	#endregion

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
		//nPlayers = NetworkServer.connections.Count;
		Debug.Log($"<color=magenta>NetworkServer.connections.Count = {NetworkServer.connections.Count}</color>");
		startBtn.gameObject.SetActive(false);
	}


	public override void ServerChangeScene(string newSceneName)
	{
		// transitioning from lobby to board
		if (lobbyScene.Contains(SceneManager.GetActiveScene().name))
		{
			for (int i = 0; i < lobbyPlayers.Count; i++)
			{
				var conn = lobbyPlayers[i].connectionToClient;
				PlayerControls player = Instantiate(boardPlayerPrefab);
				player.characterInd = lobbyPlayers[i].characterInd;
				player.id = i;

				NetworkServer.ReplacePlayerForConnection(conn, player.gameObject);
			}
			lobbyPlayers.Clear();
		}
		// transitioning from board to board
		else if (SceneManager.GetActiveScene().name.Contains("Board") && newSceneName.Contains("Board"))
		{
			Debug.Log($"<color=yellow>RELOADING</color>");
			for (int i = 0; i < boardControls.Count; i++)
			{
				var conn = boardControls[i].connectionToClient;
				PlayerControls player = Instantiate(boardPlayerPrefab);
				player.characterInd = boardControls[i].characterInd;
				player.id = i;

				NetworkServer.ReplacePlayerForConnection(conn, player.gameObject);
			}
			boardControls.Clear();
		}
		// transitioning from board to minigame
		else if (SceneManager.GetActiveScene().name.Contains("Board"))
		{
			Debug.Log($"<color=yellow>STARTING MINIGAME</color>");
			for (int i = 0; i < boardControls.Count; i++)
			{
				var conn = boardControls[i].connectionToClient;
				MinigameControls player = Instantiate(gamePlayerPrefab);
				player.characterInd = boardControls[i].characterInd;
				player.id = i;

				NetworkServer.ReplacePlayerForConnection(conn, player.gameObject);
			}
			boardControls.Clear();
		}
		// transitioning from minigame to board
		else if (SceneManager.GetActiveScene().name.Contains("Minigame"))
		{
			for (int i = 0; i < minigameControls.Count; i++)
			{
				var conn = minigameControls[i].connectionToClient;
				PlayerControls player = Instantiate(boardPlayerPrefab);
				player.characterInd = minigameControls[i].characterInd;
				player.id = i;

				NetworkServer.ReplacePlayerForConnection(conn, player.gameObject);
			}
			minigameControls.Clear();
		}
		base.ServerChangeScene(newSceneName);
	}


	#region Board

	public void StartBoardGame()
	{
		StartCoroutine( StartBoardGameCo() );
	}
	
	IEnumerator StartBoardGameCo()
	{
		gm.CmdTriggerTransition(true);
		
		yield return new WaitForSeconds(0.5f);
		ServerChangeScene("TestBoard");
		//SceneManager.LoadScene("TestBoard", LoadSceneMode.Single);
		//NetworkManager.Singleton.SceneManager.LoadScene("TestBoard", LoadSceneMode.Single);
	}

	public void UnparentBoardControls()
	{
		for (int i=0 ; i<boardControls.Count ; i++)
		{
			if (boardControls[i] != null)
				boardControls[i].MediateRemoteStart();
		}
	}

	[SerializeField] int nPlayerOrder; 
	public void NextBoardPlayerTurn()
	{
		//Debug.Log($"<color=white>NextBoardPlayerTurn() = {nPlayerOrder} < {boardControls.Count}</color>");
		if (nPlayerOrder < boardControls.Count)
			boardControls[nPlayerOrder++].YourTurn();
		else
			StartCoroutine(StartMiniGameCo());
	}

	public NetworkConnectionToClient GetLosingPlayer()
	{
		if (boardControls != null && boardControls.Count > 0)
			return boardControls[boardControls.Count - 1] != null ? boardControls[boardControls.Count - 1].netIdentity.connectionToClient : null;
		return null;
	}

	public override void OnServerSceneChanged(string sceneName)
	{
		base.OnServerSceneChanged(sceneName);
		Debug.Log($"==> Scene Loaded = {sceneName}");
		gm.TriggerTransitionDelay(false);
	}
	#endregion


	#region Minigame
	public int GetNumPlayers()
	{
		return minigameControls.Count;
	}
	IEnumerator StartMiniGameCo()
	{
		gm.CmdTriggerTransition(true);
		
		yield return new WaitForSeconds(0.5f);
		nPlayerOrder = 0;
		//nTurn++;
		gm.IncreaseTurnNum();
		if (skipSideQuests)
		{
			//yield return new WaitForSeconds(0.5f);
			ServerChangeScene("TestBoard");
		}
		else
		{
			yield return new WaitForSeconds(0.5f);
			ServerChangeScene(minigameScenes[fixedGame == -1 ? nMinigame++ % minigameScenes.Length : fixedGame]);
		}
		
		//while (NetworkServer.isLoadingScene)
		//	yield return null;

		//gm.CmdTriggerTransition(false);
	}

	/// <summary>
	/// Called when all preview (on all clients) have loaded
	/// </summary>
	public void LoadPreviewMinigame()
	{
		gm.StartMinigame(minigameScenes[fixedGame == -1 ? nMinigame++ % minigameScenes.Length : fixedGame]);
	}
	public void ReloadPreviewMinigame()
	{
		Debug.Log($"<color=yellow>STARTING MINIGAME</color>");
		for (int i = 0; i < minigameControls.Count; i++)
		{
			var conn = minigameControls[i].connectionToClient;
			int temp = minigameControls[i].characterInd;
			minigameControls.Remove(minigameControls[i]);

			MinigameControls player = Instantiate(gamePlayerPrefab);
			player.characterInd = temp;
			player.id = i;

			NetworkServer.ReplacePlayerForConnection(conn, player.gameObject);
		}
		//gm.StartMinigame(minigameScene);
		//gm.CmdReloadPreviewMinigameUnload();
		//StartCoroutine(LoadPreviewMinigameCo());
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
		//Debug.Log($"<color=yellow>OnServerAddPlayer</color>");
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
		if (!conns.Contains(conn))
			conns.Add(conn);

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
		if (conns.Contains(conn))
			conns.Remove(conn);
	}
	#endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameNetworkManager : NetworkManager
{
	#region Variables
	public static GameNetworkManager Instance;
	public Transform spawnHolder;
	private GameManager gm {get{return GameManager.Instance;}}
	private PreviewManager pm {get{return PreviewManager.Instance;}}
	//GameObject ball;
	[SerializeField] private GameObject buttons;
	[SerializeField] private GameObject hostLostUi;

	[SerializeField] private Animator anim;

	
	[Space] [Header("Network")]
	[SerializeField] private List<NetworkConnectionToClient> conns = new();
	//bool isInTransition;
	[Space] [SerializeField] private int fixedGame=-1;
	[SerializeField] private bool skipSideQuests;
	public bool skipIntro;
	public bool skipBoard;


	[Space] [Header("Scenes")]
	[SerializeField] private int minPlayers=2;
	[SerializeField] private string lobbyScene;
	[SerializeField] private string boardScene;
	[SerializeField] private string practiceScene;
	int nMinigame;
	private string minigameName;
	[SerializeField] private string[] minigameScenes;

	[Space] [SerializeField] private GameObject previewObj;


	[Space] [SerializeField] private LobbyObject lobbyPlayerPrefab;
	[SerializeField] private PlayerControls boardPlayerPrefab;
	[SerializeField] private MinigameControls gamePlayerPrefab;
	//[SerializeField] private PreviewControls previewPlayerPrefab;

	[Space] [SerializeField] private List<LobbyObject> lobbyPlayers = new();
	[SerializeField] private List<PlayerControls> boardControls = new();
	[SerializeField] private List<MinigameControls> minigameControls = new();
	//[SerializeField] private List<PreviewControls> previewControls = new();
	public List<int> playerOrder;
	//bool isLoadingScene


	#endregion



	#region Network Methods
	public override void Awake() 
	{
		base.Awake();
		Instance = this;
		
		//hostBtn.onClick.AddListener(() => {
		//	_START_HOST();
		//});	
		//clientBtn.onClick.AddListener(() => {
		//	_START_CLIENT();
		//});	
		//startBtn.onClick.AddListener(() => {
		//	StartGame();
		//});	
	}

	public override void OnStartServer()
    {
        base.OnStartServer();

        //NetworkServer.RegisterHandler<CreateMMOCharacterMessage>(OnCreateCharacter);
    }

	public override void OnStopServer()
	{
		base.OnStopServer();
		lobbyPlayers.Clear();
		boardControls.Clear();
		minigameControls.Clear();
		conns.Clear();
	}
	public override void OnClientDisconnect()
	{
		if (lobbyScene.Contains(SceneManager.GetActiveScene().name))
		{
			buttons.SetActive(true);
		}
		else
			hostLostUi.SetActive(true);
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

	public void ToggleBoardControlCam(int id, bool active)
	{
		if (boardControls != null && id >= 0 && id < boardControls.Count)
			boardControls[id].CmdCamToggle(active);
	}
	public void RewardBoardControl(int id, int reward, bool isStar)
	{
		if (boardControls != null && id >= 0 && id < boardControls.Count)
			boardControls[id].CmdNodeEffect(reward, isStar);
	}
	public void AddBoardConnection(PlayerControls pc)
	{
		if (!boardControls.Contains(pc))
			boardControls.Add(pc);
	}
	public void RemoveBoardConnection(PlayerControls pc)
	{
		Debug.Log($"<color=#FF9900>PLAYER LOST</color>");
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
		if (pm != null && pm.gameObject.activeInHierarchy)
			pm.CmdCheckReady();
	}

	//public void AddPreviewConnection(PreviewControls pc)
	//{
	//	Debug.Log($"<color=green>Preview joined!</color>");
	//	if (!previewControls.Contains(pc))
	//		previewControls.Add(pc);
	//}
	//public void RemovePreviewConnection(PreviewControls pc)
	//{
	//	if (previewControls.Contains(pc))
	//		previewControls.Remove(pc);
	//}

	#endregion

	//public void _START_HOST()
	//{
	//	buttons.SetActive(false);
	//	lobbyUi.SetActive(false);
	//	//StartHost();
	//	//startBtn.gameObject.SetActive(true);
	//	SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, maxConnections);
	//}
	//public void _START_CLIENT()
	//{
	//	//buttons.SetActive(false);
	//	//StartClient();

	//}
	//public void StartGame()
	//{
	//	StartBoardGame();
	//	//nPlayers = NetworkServer.connections.Count;
	//	Debug.Log($"<color=magenta>NetworkServer.connections.Count = {NetworkServer.connections.Count}</color>");
	//	startBtn.gameObject.SetActive(false);
	//}


	#region ServerChangeScene
	bool inPreview;
	public override void ServerChangeScene(string newSceneName)
	{
		// transitioning from lobby to board
		if (lobbyScene.Contains(SceneManager.GetActiveScene().name))
		{
			//* Add board controls
			List<int> temp = new();
			for (int i = 0; i < lobbyPlayers.Count; i++)
			{
				if (lobbyPlayers[i] != null)
				{
					temp.Add(i);
					var conn = lobbyPlayers[i].connectionToClient;
					PlayerControls player = Instantiate(boardPlayerPrefab);
					player.characterInd = lobbyPlayers[i].characterInd;
					player.id = i;
					player.boardOrder = i;

					NetworkServer.ReplacePlayerForConnection(conn, player.gameObject);
				}
				else
					boardControls.Add(null);
			}
			SetPlayerOrder(temp);
			lobbyPlayers.Clear();
		}
		// transitioning from board to board
		else if (SceneManager.GetActiveScene().name.Contains("Board") && newSceneName.Contains("Board"))
		{
			//Debug.Log($"<color=yellow>RELOADING</color>");
			//* Add board controls
			int temp = boardControls.Count;
			for (int i = 0; i < temp; i++)
			{
				if (boardControls[i] != null)
				{
					var conn = boardControls[i].connectionToClient;
					PlayerControls player = Instantiate(boardPlayerPrefab);
					player.characterInd = boardControls[i].characterInd;
					player.id = i;

					NetworkServer.ReplacePlayerForConnection(conn, player.gameObject);
				}
				else
					boardControls.Add(null);
			}
			boardControls.Clear();
		}
		// transitioning from board to minigame
		else if (SceneManager.GetActiveScene().name.Contains("Board"))
		{
			//* Add minigame controls
			for (int i = 0; i < boardControls.Count; i++)
			{
				if (boardControls[i] != null)
				{
					var conn = boardControls[i].connectionToClient;
					MinigameControls player = Instantiate(gamePlayerPrefab);
					player.characterInd = boardControls[i].characterInd;
					player.boardOrder = boardControls[i].boardOrder;
					player.id = i;
					
					NetworkServer.ReplacePlayerForConnection(conn, player.gameObject);
				}
				else
					minigameControls.Add(null);
			}
			boardControls.Clear();
		}
		// transitioning from minigame to minigame
		else if (SceneManager.GetActiveScene().name.Contains("Minigame") && newSceneName.Contains("Minigame"))
		{
			//* Add minigame controls
			int temp = minigameControls.Count;
			for (int i = 0; i < temp; i++)
			{
				if (minigameControls[i] != null)
				{
					var conn = minigameControls[i].connectionToClient;
					MinigameControls player = Instantiate(gamePlayerPrefab);
					player.characterInd = minigameControls[i].characterInd;
					player.id = i;
					player.boardOrder = minigameControls[i].boardOrder;

					NetworkServer.ReplacePlayerForConnection(conn, player.gameObject);
				}
				else
					minigameControls.Add(null);
			}
			minigameControls.Clear();
		}
		// transitioning from minigame to board
		else if (SceneManager.GetActiveScene().name.Contains("Minigame"))
		{
			//* Add board controls
			for (int i = 0; i < minigameControls.Count; i++)
			{
				if (minigameControls[i] != null)
				{
					var conn = minigameControls[i].connectionToClient;
					PlayerControls player = Instantiate(boardPlayerPrefab);
					player.characterInd = minigameControls[i].characterInd;
					player.boardOrder = minigameControls[i].boardOrder;
					player.id = i;

					NetworkServer.ReplacePlayerForConnection(conn, player.gameObject);
				}
				else
					boardControls.Add(null);
			}
			minigameControls.Clear();
		}

		base.ServerChangeScene(newSceneName);
	}
	#endregion


	#region Board

	public void SetPlayerOrder(List<int> order) => playerOrder = order;

	public int GetNumPlayers() => boardControls.Count;

	public void StartBoardGame()
	{
		StartCoroutine( StartBoardGameCo() );
	}
	
	IEnumerator StartBoardGameCo()
	{
		gm.CmdTriggerTransition(true);
		
		yield return new WaitForSeconds(0.5f);

		ServerChangeScene(boardScene);
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
	public bool StillHavePlayerTurns() => nPlayerOrder < boardControls.Count;
	public void NextBoardPlayerTurn()
	{
		// no player turns
		if (skipBoard)
			StartCoroutine(StartMiniGameCo());
		// still have player turns
		else if (nPlayerOrder < boardControls.Count)
		{
			if (skipIntro)
			{
				if (boardControls[ nPlayerOrder ] != null)
					boardControls[ nPlayerOrder++ ].YourTurn();
				else
				{
					++nPlayerOrder;
					BoardManager.Instance.NextPlayerTurn();
				}
			}
			else
			{
				if (boardControls[ playerOrder[nPlayerOrder ]] != null)
					boardControls[ playerOrder[nPlayerOrder++] ].YourTurn();
				else
				{
					++nPlayerOrder;
					BoardManager.Instance.NextPlayerTurn();
				}
			}
		}
		// all player turns done
		else
			StartCoroutine(StartMiniGameCo());
	}

	public NetworkConnectionToClient GetLosingPlayer()
	{
		int lowest = 99999;
		int id = 0;
		for (int i=0 ; i<boardControls.Count ; i++)
		{
			if (boardControls[i] != null)
			{
				if (boardControls[i].GetCoins() + boardControls[i].GetStars() * 1000 < lowest)
				{
					lowest = boardControls[i].GetCoins() + boardControls[i].GetStars() * 1000;
					id = i;
				}
			}
		}
		if (boardControls != null && boardControls.Count > 0)
			return boardControls[id] != null ? boardControls[id].netIdentity.connectionToClient : null;
		return null;
	}
	public void PunishNonWinners()
	{
		int highest = -1;
		int id = 0;
		for (int i=0 ; i<boardControls.Count ; i++)
		{
			if (boardControls[i] != null)
			{
				if (boardControls[i].GetCoins() + boardControls[i].GetStars() * 1000 > highest)
				{
					highest = boardControls[i].GetCoins() + boardControls[i].GetStars() * 1000;
					id = i;
				}
			}
		}
		for (int i=0 ; i<boardControls.Count ; i++)
		{
			if (boardControls[i] != null)
				if (id != i)
					boardControls[i].CmdLose();
		}
	}
	public void ShowWinner()
	{
		int highest = -1;
		int id = 0;
		for (int i=0 ; i<boardControls.Count ; i++)
		{
			if (boardControls[i] != null)
			{
				if (boardControls[i].GetCoins() + boardControls[i].GetStars() * 1000 > highest)
				{
					highest = boardControls[i].GetCoins() + boardControls[i].GetStars() * 1000;
					id = i;
				}
			}
		}
		boardControls[id].CmdWin();
	}
	public int GetWinningPlayer()
	{
		int highest = -1;
		int id = 0;
		for (int i=0 ; i<boardControls.Count ; i++)
		{
			if (boardControls[i] != null)
			{
				if (boardControls[i].GetCoins() + boardControls[i].GetStars() * 1000 > highest)
				{
					highest = boardControls[i].GetCoins() + boardControls[i].GetStars() * 1000;
					id = i;
				}
			}
		}
		return id;
	}

	public override void OnServerSceneChanged(string sceneName)
	{
		base.OnServerSceneChanged(sceneName);
		//if (PreviewManager.Instance == null)
		if (!inPreview)
			gm.TriggerTransitionDelay(false);
	}
	#endregion


	#region Minigame
	public int GetNumMinigamePlayers() => minigameControls.Count;
	
	public int[] GetMinigamePlayerInfo(int ind)
	{
		if (ind >= 0 && ind < minigameControls.Count && minigameControls[ind] != null)
		{
			int[] details = new int[5]; // characterInd, board order, coins, stars, manas
			details[0] = minigameControls[ind].characterInd;
			details[1] = minigameControls[ind].boardOrder;
			details[2] = gm.GetCoins(ind);
			details[3] = gm.GetStars(ind);
			details[4] = gm.GetMana(ind);
			return details;
		}
		return null;
	}


	bool previewProfileLoaded;
	public void PreviewManagerLoaded() 
	{
		if (!previewProfileLoaded)
		{
			previewProfileLoaded = true;
			int[] characterInds = new int[boardControls.Count];
			for (int i=0; i >= 0 && i < boardControls.Count && boardControls[i] != null ; i++)
				characterInds[i] = boardControls[i].characterInd;
			//Debug.Log($"<color=yellow>PreviewManagerLoaded = {characterInds.Length}</color>");
			gm.SetProfilePic(characterInds);
		}
		minigameName = minigameScenes[fixedGame == -1 ? nMinigame++ % minigameScenes.Length : fixedGame];
		ServerChangeScene(minigameName);
	}
	public void PreviewManagerUnLoaded() 
	{
		inPreview = false;
		ServerChangeScene(minigameName);
	}
	int nSaved;
	public void IncreasePlayerDataSaved() => ++nSaved;
	IEnumerator StartMiniGameCo()
	{
		nSaved = 0;
		foreach (PlayerControls p in boardControls)
			if (p != null)
				p.CmdSaveData();
		gm.CmdTriggerTransition(true);
		
		yield return new WaitForSeconds(0.5f);
		while (nSaved < boardControls.Count)
			yield return null;

		nPlayerOrder = 0;
		//nTurn++;
		gm.IncreaseTurnNum();
		if (skipSideQuests)
		{
			//yield return new WaitForSeconds(0.5f);
			ServerChangeScene(boardScene);
		}
		else
		{
			yield return new WaitForSeconds(0.5f);
			gm.CmdTogglePreviewManager(true);
			//gm.SetupPreviewManager();
			
			//yield return new WaitForEndOfFrame(); yield return new WaitForEndOfFrame();
			//minigameName = minigameScenes[fixedGame == -1 ? nMinigame++ % minigameScenes.Length : fixedGame];
			//ServerChangeScene(minigameName);
		}
	}
	Coroutine actualMinigameCo;
	public void StartActualMiniGame() => actualMinigameCo = StartCoroutine(StartActualMiniGameCo());
	IEnumerator StartActualMiniGameCo()
	{
		yield return new WaitForSeconds(0.5f);
		gm.CmdTriggerTransition(true);
		
		yield return new WaitForSeconds(0.5f);
		if (skipSideQuests)
		{
			//yield return new WaitForSeconds(0.5f);
			ServerChangeScene(boardScene);
		}
		else
		{
			yield return new WaitForSeconds(0.5f);
			gm.CmdTogglePreviewManager(false);
		}
		actualMinigameCo = null;
	}


	public void ReloadPreviewMinigame()
	{
		inPreview = true;
		if (actualMinigameCo == null)
			ServerChangeScene(minigameName);
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

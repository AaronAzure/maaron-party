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
	private BoardManager bm {get{return BoardManager.Instance;}}
	private PreviewManager pm {get{return PreviewManager.Instance;}}
	//GameObject ball;
	[SerializeField] private GameObject buttons;
	[SerializeField] private GameObject hostLostUi;

	[SerializeField] private Animator anim;

	
	[Space] [Header("Network")]
	//[SerializeField] private List<NetworkConnectionToClient> conns = new();
	[Space] [SerializeField] private int fixedGame=-1;
	[SerializeField] private bool skipSideQuests;
	public bool skipIntro;
	public bool skipBoard;
	//[SerializeField] private GameObject gmObj;


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
	public void Awake() 
	{
		Instance = this;
	}

	//public override void OnStartServer()
	//{
	//	base.OnStartServer();
	//}

	//public override void OnStopServer()
	//{
	//	base.OnStopServer();
	//	lobbyPlayers.Clear();
	//	boardControls.Clear();
	//	minigameControls.Clear();
	//	conns.Clear();
	//}
	//public override void OnClientDisconnect()
	//{
	//	if (lobbyScene.Contains(SceneManager.GetActiveScene().name))
	//	{
	//		buttons.SetActive(true);
	//	}
	//	else
	//		hostLostUi.SetActive(true);
	//}


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
			boardControls[id].CamToggleServerRpc(active);
	}
	public void RewardBoardControl(int id, int reward, bool isStar)
	{
		if (boardControls != null && id >= 0 && id < boardControls.Count)
			boardControls[id].NodeEffectServerRpc(reward, isStar);
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
			pm.CheckReadyServerRpc();
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

	public void _START_HOST()
	{
		var success = StartHost();
		if (success)
		{
			DebugSync();
		}
		//buttons.SetActive(false);
		//lobbyUi.SetActive(false);
		//startBtn.gameObject.SetActive(true);
		//SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, maxConnections);
	}
	public void _START_CLIENT()
	{
		//buttons.SetActive(false);
		var success = StartClient();
		if (success)
		{
			DebugSync();
		}
	}

	private void DebugSync()
	{
		SceneManager.OnLoad += (ulong clientId, string sceneName, LoadSceneMode loadSceneMode, AsyncOperation asyncOperation) => {
			Debug.Log($"<color=magenta>== ({clientId}) SceneManager.OnLoad ({sceneName}) ==</color>");
		};
		SceneManager.OnLoadComplete += (ulong clientId, string sceneName, LoadSceneMode loadSceneMode) => {
			Debug.Log($"<color=cyan>== ({clientId}) SceneManager.OnLoadComplete ({sceneName}) ==</color>");
		};
		SceneManager.OnSceneEvent += SceneManager_OnSceneEvent;
		OnClientConnectedCallback += (ulong id) => OnClientConnect(id);
		ConnectionApprovalCallback += (ConnectionApprovalRequest a, ConnectionApprovalResponse b) => {
			Debug.Log($"<color=yellow>== client ({a.ClientNetworkId}) connected! ==</color>");
		};
	}

	#region OnSceneEvent
	private void SceneManager_OnSceneEvent(SceneEvent sceneEvent)
	{
		Debug.Log($"<color=#EC9A33>== ({LocalClientId}) SceneManager.OnLoad ({sceneEvent.SceneEventType}) ==</color>");
		// Both client and server receive these notifications
		switch (sceneEvent.SceneEventType)
		{
			// Handle server to client Load Notifications
			case SceneEventType.Load:
				{
					// This event provides you with the associated AsyncOperation
					// AsyncOperation.progress can be used to determine scene loading progression
					var asyncOperation = sceneEvent.AsyncOperation;
					// Since the server "initiates" the event we can simply just check if we are the server here
					if (IsServer)
					{
						// Handle server side load event related tasks here
					}
					else
					{
						// Handle client side load event related tasks here
					}                        
					break;
				}
			// Handle server to client unload notifications
			case SceneEventType.Unload:
				{
					// You can use the same pattern above under SceneEventType.Load here
					break;
				}
			// Handle client to server LoadComplete notifications
			case SceneEventType.LoadComplete:
				{
					// This will let you know when a load is completed
					// Server Side: receives thisn'tification for both itself and all clients
					if (IsServer)
					{                            
						if (sceneEvent.ClientId == LocalClientId)
						{
							// Handle server side LoadComplete related tasks here
						}
						else
						{
							// Handle client LoadComplete **server-side** notifications here
						}
					}
					else // Clients generate thisn'tification locally
					{
						// Handle client side LoadComplete related tasks here
					}

					// So you can use sceneEvent.ClientId to also track when clients are finished loading a scene
					break;
				}
			// Handle Client to Server Unload Complete Notification(s)
			case SceneEventType.UnloadComplete:
				{
					// This will let you know when an unload is completed
					// You can follow the same pattern above as SceneEventType.LoadComplete here

					// Server Side: receives thisn'tification for both itself and all clients
					// Client Side: receives thisn'tification for itself

					// So you can use sceneEvent.ClientId to also track when clients are finished unloading a scene
					break;
				}
			// Handle Server to Client Load Complete (all clients finished loading notification)
			case SceneEventType.LoadEventCompleted:
				{
					// This will let you know when all clients have finished loading a scene
					// Received on both server and clients
					foreach (var clientId in sceneEvent.ClientsThatCompleted)
					{
						// Handle any server-side tasks here
						if (IsServer)
						{
							// board scene
							if (bm != null)
							{
								bm.StartUp();
								bm.SetUpPlayers();
								//if (IsHost)
								//	bm.SetUpPlayer();
							}
						}
						// Handle any client-side tasks here
						else
						{
							// board scene
							if (bm != null)
							{
								//bm.SetUpPlayer();
							}
						}
					}
					break;
				}
			// Handle Server to Client unload Complete (all clients finished unloading notification)
			case SceneEventType.UnloadEventCompleted:
				{
					// This will let you know when all clients have finished unloading a scene
					// Received on both server and clients
					foreach (var clientId in sceneEvent.ClientsThatCompleted)
					{
						// Example of parsing through the clients that completed list
						if (IsServer)
						{
							// Handle any server-side tasks here
						}
						else
						{
							// Handle any client-side tasks here
						}
					}
					break;
				}
		}
	}
	#endregion
	private void OnClientConnect(ulong clientId)
	{
		Debug.Log($"<color=yellow>== client ({clientId}) joined! ==</color>");
		if (IsServer)
		{
			Debug.Log($"<color=yellow>== ConnectedClientsIds ({ConnectedClientsIds.Count}) ==</color>");
			Debug.Log($"<color=yellow>== PendingClients ({PendingClients.Count}) ==</color>");
			//if (PendingClients.Count >= ConnectedClientsIds.Count)
			//{
			//	StartGame();
			//}
		}
	}

	
	public void StartGame()
	{
		//gmObj.SetActive(true);
		StartBoardGame();
		//nPlayers = NetworkServer.connections.Count;
		//Debug.Log($"<color=magenta>NetworkServer.connections.Count = {NetworkServer.connections.Count}</color>");
		//startBtn.gameObject.SetActive(false);
	}


	#region ServerChangeScene
	bool inPreview;
	//public override void ServerChangeScene(string newSceneName)
	//{
	//	// transitioning from lobby to board
	//	if (lobbyScene.Contains(SceneManager.GetActiveScene().name))
	//	{
	//		//* Add board controls
	//		List<int> temp = new();
	//		for (int i = 0; i < lobbyPlayers.Count; i++)
	//		{
	//			if (lobbyPlayers[i] != null)
	//			{
	//				temp.Add(i);
	//				var conn = lobbyPlayers[i].connectionToClient;
	//				PlayerControls player = Instantiate(boardPlayerPrefab);
	//				player.characterInd = lobbyPlayers[i].characterInd;
	//				player.id = i;
	//				player.boardOrder = i;

	//				NetworkServer.ReplacePlayerForConnection(conn, player.gameObject);
	//			}
	//			else
	//				boardControls.Add(null);
	//		}
	//		SetPlayerOrder(temp);
	//		lobbyPlayers.Clear();
	//	}
	//	// transitioning from board to board
	//	else if (SceneManager.GetActiveScene().name.Contains("Board") && newSceneName.Contains("Board"))
	//	{
	//		//Debug.Log($"<color=yellow>RELOADING</color>");
	//		//* Add board controls
	//		int temp = boardControls.Count;
	//		for (int i = 0; i < temp; i++)
	//		{
	//			if (boardControls[i] != null)
	//			{
	//				var conn = boardControls[i].connectionToClient;
	//				PlayerControls player = Instantiate(boardPlayerPrefab);
	//				player.characterInd = boardControls[i].characterInd;
	//				player.id = i;

	//				NetworkServer.ReplacePlayerForConnection(conn, player.gameObject);
	//			}
	//			else
	//				boardControls.Add(null);
	//		}
	//		boardControls.Clear();
	//	}
	//	// transitioning from board to minigame
	//	else if (SceneManager.GetActiveScene().name.Contains("Board"))
	//	{
	//		//* Add minigame controls
	//		for (int i = 0; i < boardControls.Count; i++)
	//		{
	//			if (boardControls[i] != null)
	//			{
	//				var conn = boardControls[i].connectionToClient;
	//				MinigameControls player = Instantiate(gamePlayerPrefab);
	//				player.characterInd = boardControls[i].characterInd;
	//				player.boardOrder = boardControls[i].boardOrder;
	//				player.id = i;
					
	//				NetworkServer.ReplacePlayerForConnection(conn, player.gameObject);
	//			}
	//			else
	//				minigameControls.Add(null);
	//		}
	//		boardControls.Clear();
	//	}
	//	// transitioning from minigame to minigame
	//	else if (SceneManager.GetActiveScene().name.Contains("Minigame") && newSceneName.Contains("Minigame"))
	//	{
	//		//* Add minigame controls
	//		int temp = minigameControls.Count;
	//		for (int i = 0; i < temp; i++)
	//		{
	//			if (minigameControls[i] != null)
	//			{
	//				var conn = minigameControls[i].connectionToClient;
	//				MinigameControls player = Instantiate(gamePlayerPrefab);
	//				player.characterInd = minigameControls[i].characterInd;
	//				player.id = i;
	//				player.boardOrder = minigameControls[i].boardOrder;

	//				NetworkServer.ReplacePlayerForConnection(conn, player.gameObject);
	//			}
	//			else
	//				minigameControls.Add(null);
	//		}
	//		minigameControls.Clear();
	//	}
	//	// transitioning from minigame to board
	//	else if (SceneManager.GetActiveScene().name.Contains("Minigame"))
	//	{
	//		//* Add board controls
	//		for (int i = 0; i < minigameControls.Count; i++)
	//		{
	//			if (minigameControls[i] != null)
	//			{
	//				var conn = minigameControls[i].connectionToClient;
	//				PlayerControls player = Instantiate(boardPlayerPrefab);
	//				player.characterInd = minigameControls[i].characterInd;
	//				player.boardOrder = minigameControls[i].boardOrder;
	//				player.id = i;

	//				NetworkServer.ReplacePlayerForConnection(conn, player.gameObject);
	//			}
	//			else
	//				boardControls.Add(null);
	//		}
	//		minigameControls.Clear();
	//	}

	//	base.ServerChangeScene(newSceneName);
	//}
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
		if (gm != null)
			gm.TriggerTransition(true);
		
		Debug.Log($"<color=yellow>fading in</color>");
		yield return new WaitForSeconds(0.5f);

		//todo ServerChangeScene(boardScene);
		StartBoardGameServerRpc();
		//NetworkManager.Singleton.SceneManager.LoadScene("TestBoard 1", LoadSceneMode.Single);
	}

	[ServerRpc] void StartBoardGameServerRpc()
	{
		string m_SceneName = "TestBoard 1";
		Debug.Log($"<color=yellow>IsServer = {IsServer} | IsHost = {IsHost}</color>");

		var status = SceneManager.LoadScene(m_SceneName, LoadSceneMode.Single);
		if (status != SceneEventProgressStatus.Started)
		{
			Debug.LogWarning($"Failed to load {m_SceneName} " +
				$"with a {nameof(SceneEventProgressStatus)}: {status}");
		}
	}

	//public void UnparentBoardControls()
	//{
	//	for (int i=0 ; i<boardControls.Count ; i++)
	//	{
	//		if (boardControls[i] != null)
	//			boardControls[i].MediateRemoteStart();
	//	}
	//}

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
				{
					boardControls[ nPlayerOrder++ ].YourTurnClientRpc(
						new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { (ulong) nPlayerOrder }}}
					);

				}
				else
				{
					++nPlayerOrder;
					bm.NextPlayerTurn();
				}
			}
			else
			{
				if (boardControls[ playerOrder[nPlayerOrder ]] != null)
				{
					boardControls[ playerOrder[nPlayerOrder++] ].YourTurnClientRpc(
						new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { (ulong) nPlayerOrder }}}
					);

				}
				else
				{
					++nPlayerOrder;
					bm.NextPlayerTurn();
				}
			}
		}
		// all player turns done
		else
			StartCoroutine(StartMiniGameCo());
	}

	public ulong GetLosingPlayer()
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
			return boardControls[id] != null ? boardControls[id].OwnerClientId : 0;
		return 0;
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
					boardControls[i].LoseServerRpc();
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
		boardControls[id].WinServerRpc();
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

	// todo public override void OnServerSceneChanged(string sceneName)
	//{
	//	base.OnServerSceneChanged(sceneName);
	//	//if (PreviewManager.Instance == null)
	//	if (!inPreview)
	//		gm.TriggerTransitionDelay(false);
	//}
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
				characterInds[i] = boardControls[i].characterInd.Value;
			//Debug.Log($"<color=yellow>PreviewManagerLoaded = {characterInds.Length}</color>");
			gm.SetProfilePic(characterInds);
		}
		minigameName = minigameScenes[fixedGame == -1 ? nMinigame++ % minigameScenes.Length : fixedGame];
		//todo ServerChangeScene(minigameName);
	}
	public void PreviewManagerUnLoaded() 
	{
		inPreview = false;
		//todo ServerChangeScene(minigameName);
	}
	int nSaved;
	public void IncreasePlayerDataSaved() => ++nSaved;
	IEnumerator StartMiniGameCo()
	{
		nSaved = 0;
		foreach (PlayerControls p in boardControls)
			if (p != null)
				p.SaveDataServerRpc();
		gm.TriggerTransitionServerRpc(true);
		
		yield return new WaitForSeconds(0.5f);
		while (nSaved < boardControls.Count)
			yield return null;

		nPlayerOrder = 0;
		//nTurn++;
		gm.IncreaseTurnNum();
		if (skipSideQuests)
		{
			//yield return new WaitForSeconds(0.5f);
			//todo ServerChangeScene(boardScene);
		}
		else
		{
			yield return new WaitForSeconds(0.5f);
			gm.TogglePreviewManagerServerRpc(true);
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
		gm.TriggerTransitionServerRpc(true);
		
		yield return new WaitForSeconds(0.5f);
		if (skipSideQuests)
		{
			//yield return new WaitForSeconds(0.5f);
			//todo ServerChangeScene(boardScene);
		}
		else
		{
			yield return new WaitForSeconds(0.5f);
			gm.TogglePreviewManagerServerRpc(false);
		}
		actualMinigameCo = null;
	}


	public void ReloadPreviewMinigame()
	{
		inPreview = true;
		//todo if (actualMinigameCo == null) ServerChangeScene(minigameName);
	}


	//public override void OnServerAddPlayer(NetworkConnectionToClient conn)
	//{
	//	//Debug.Log($"<color=yellow>OnServerAddPlayer</color>");
	//	//base.OnServerAddPlayer(conn);
		
	//	// add player at correct spawn position
	//	if (spawnHolder != null)
	//	{
	//		Transform start = spawnHolder;
	//		GameObject player = Instantiate(playerPrefab, start.position, start.rotation);
	//		player.name = $"_{playerPrefab.name} [connId={conn.connectionId}]_";
	//		NetworkServer.AddPlayerForConnection(conn, player);
	//	}
	//	//player.name = $"__PLAYER {nPlayers++}";
	//	if (!conns.Contains(conn))
	//		conns.Add(conn);

	//	// spawn ball if two players
	//	//if (numPlayers == 2)
	//	//{
	//	//	ball = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "Ball"));
	//	//	NetworkServer.Spawn(ball);
	//	//}
	//}

	//public override void OnServerDisconnect(NetworkConnectionToClient conn)
	//{
	//	// destroy ball
	//	//if (ball != null)
	//	//	NetworkServer.Destroy(ball);

	//	// call base functionality (actually destroys the player)
	//	base.OnServerDisconnect(conn);
	//	if (conns.Contains(conn))
	//		conns.Remove(conn);
	//}
	#endregion
}

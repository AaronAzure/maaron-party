using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
	#region Variables
	public static GameManager Instance;
	//public int nPlayers {get; private set;}
	//public NetworkVariable<List<ulong>> players = new NetworkVariable<List<ulong>>(
	//	new(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	//public NetworkVariable<int> nPlayers = new NetworkVariable<int>(
	//	0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	//public NetworkList<ulong> players;
	//public NetworkList<int> playerModels;
	[SyncVar] public int nPlayers;
	private Scene m_LoadedScene;


	[Space] [Header("Lobby Manager")]
	public Transform spawnHolder;


	[Space] [Header("In game references")]
	[SerializeField] private List<ushort> currNodes;
	[SerializeField] private List<int> coins;
	[SerializeField] private List<int> stars;
	public bool hasStarted {get; private set;}
	public bool lobbyCreated {get; private set;}
	[SerializeField] Animator anim;

	#endregion


	#region Methods
	public void Awake() 
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
		DontDestroyOnLoad(this);
	}

	public void Start() 
	{
		currNodes = new();
		coins = new();
		stars = new();
		if (BoardManager.Instance == null)
			TriggerTransition(false);
	}

	public void StartHost()
	{
		//NetworkManager.Singleton.StartHost();
		//if (buttons != null) buttons.SetActive(false);
		//if (startBtn != null)
		//	startBtn.gameObject.SetActive(true);
	}
	public void StartClient()
	{
		//NetworkManager.Singleton.StartClient();
		//if (buttons != null) buttons.SetActive(false);
	}

	#endregion
	
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~ NETWORK ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	#region Network

	[Command] public void CmdJoinGame(ulong id)
	{
		//if (!IsHost) return;
		//nPlayers.Value++;
		////Debug.Log($"Player {id} joined!!");
		//if (!players.Contains(id))
		//	players.Add(id);
	}
	[Command(requiresAuthority=false)] public void CmdLeftGame(ulong id)
	{
		//if (!IsHost) return;
		//if (!lobbyCreated)
		//	nPlayers.Value--;
		//if (players != null && players.Contains(id))
		//	players.Remove(id);
	}

	[Command(requiresAuthority=false)] public void CmdSetPlayerModel(int ind)
	{
		//Debug.Log($"<color=blue>CmdSetPlayerModel = {ind}</color>");
		//playerModels.Add(ind);
	}
	[ClientRpc] public void RpcSetPlayerModel()
	{
		//Debug.Log($"<color=blue>SetPlayerModelClientRpc</color>");
		//LobbyObject.Instance.CmdSendPlayerModel();
	}

	//public override void OnNetworkSpawn()
	//{
	//	base.OnNetworkSpawn();
	//	NetworkManager.Singleton.SceneManager.OnLoadComplete += this.OnLoadComplete;
	//	NetworkManager.Singleton.SceneManager.OnUnloadComplete += this.OnUnloadComplete;
	//	NetworkManager.SceneManager.OnSceneEvent += SceneManager_OnSceneEvent;
	//}
	//private void SceneManager_OnSceneEvent(SceneEvent sceneEvent)
	//{
	//    var clientOrServer = sceneEvent.ClientId == NetworkManager.ServerClientId ? "server" : "client";
	//    switch (sceneEvent.SceneEventType)
	//    {
	//        case SceneEventType.LoadComplete:
	//            {
	//                // We want to handle this for only the server-side
	//                if (sceneEvent.ClientId == NetworkManager.ServerClientId)
	//                {
	//                    // *** IMPORTANT ***
	//                    // Keep track of the loaded scene, you need this to unload it
	//                    m_LoadedScene = sceneEvent.Scene;
	//                }
	//                Debug.Log($"Loaded the {sceneEvent.SceneName} scene on " +
	//                    $"{clientOrServer}-({sceneEvent.ClientId}).");
	//                break;
	//            }
	//        case SceneEventType.UnloadComplete:
	//            {
	//                Debug.Log($"Unloaded the {sceneEvent.SceneName} scene on " +
	//                    $"{clientOrServer}-({sceneEvent.ClientId}).");
	//                break;
	//            }
	//        case SceneEventType.LoadEventCompleted:
	//        case SceneEventType.UnloadEventCompleted:
	//            {
	//                var loadUnload = sceneEvent.SceneEventType == SceneEventType.LoadEventCompleted ? "Load" : "Unload";
	//                Debug.Log($"{loadUnload} event completed for the following client " +
	//                    $"identifiers:({sceneEvent.ClientsThatCompleted})");
	//                if (sceneEvent.ClientsThatTimedOut.Count > 0)
	//                {
	//                    Debug.LogWarning($"{loadUnload} event timed out for the following client " +
	//                        $"identifiers:({sceneEvent.ClientsThatTimedOut})");
	//                }
	//                break;
	//            }
	//    }
	//}

	public void StartGame()
	{
		lobbyCreated = true;
		//npla
		//for (int i=0 ; i<NetworkManager.Singleton.ConnectedClientsIds.Count ; i++)
		//{
		//	SetPlayerModelClientRpc(
		//		new ClientRpcParams { 
		//			Send = new ClientRpcSendParams { 
		//				TargetClientIds = new List<ulong> {NetworkManager.Singleton.ConnectedClientsIds[i]}
		//			}
		//		}
		//	);
		//}

		//string s = "<color=cyan>NetworkManager.Singleton.ConnectedClientsIds: ";
		//foreach (ulong x in NetworkManager.Singleton.ConnectedClientsIds)
		//	s += $"|{x}| ";
		//Debug.Log(s + "</color>");
		//startBtn.gameObject.SetActive(false);
		StartCoroutine( StartGameCo() );
	}
	
	IEnumerator StartGameCo()
	{
		CmdTriggerTransition(true);
		
		yield return new WaitForSeconds(0.5f);
		//GameNetworkManager.Instance.ServerChangeScene("TestBoard");
		
		//while (NetworkServer.isLoadingScene)
		//	yield return null;
		//CmdTriggerTransition(false);
		//SceneManager.LoadScene("TestBoard", LoadSceneMode.Single);
		//NetworkManager.Singleton.SceneManager.LoadScene("TestBoard", LoadSceneMode.Single);
	}

	[Command(requiresAuthority=false)] public void CmdNextPlayerTurn(ulong id)
	{
		//BoardManager.Instance.NextPlayerTurnClientRpc(
		//	new ClientRpcParams { 
		//		Send = new ClientRpcSendParams { 
		//			TargetClientIds = new List<ulong> {id}
		//		}
		//	}
		//);
	}
	[Command(requiresAuthority=false)] public void CmdLoadMinigame()
	{
		StartCoroutine(LoadMinigameCo());
	}
	IEnumerator LoadMinigameCo()
	{
		CmdTriggerTransition(true);
		yield return new WaitForSeconds(0.5f);
		//NetworkManager.Singleton.SceneManager.LoadScene("TestMinigame", LoadSceneMode.Single);
	}

	#endregion
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~ NETWORK ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


	public void IncreaseNumPlayers()
	{
		//nPlayers.Value++;
	}
	public void DecreaseNumPlayers()
	{
		//nPlayers.Value--;
	}

	//* --------------------
	//* ------- save -------
	public void SaveCurrNode(ushort nodeId, int playerId)
	{
		if (currNodes == null)
			currNodes = new();
		while (currNodes.Count <= playerId)
			currNodes.Add(0);
		currNodes[playerId] = nodeId;
	}
	public ushort GetCurrNode(int playerId)
	{
		return currNodes[playerId];
	}

	public void SaveCoins(int newCoin, int playerId)
	{
		if (coins == null)
			coins = new();
		while (coins.Count <= playerId)
			coins.Add(0);
		coins[playerId] = newCoin;
	}
	public int GetCoins(int playerId)
	{
		return coins[playerId];
	}

	public void SaveStars(int newStar, int playerId)
	{
		if (stars == null)
			stars = new();
		while (stars.Count <= playerId)
			stars.Add(0);
		stars[playerId] = newStar;
	}
	public int GetStars(int playerId)
	{
		return stars[playerId];
	}
	//* ------- save -------
	//* --------------------

	public void TriggerTransition(bool fadeIn)
	{
		anim.SetTrigger(fadeIn ? "in" : "out");
	}
	[Command(requiresAuthority=false)] public void CmdTriggerTransition(bool fadeIn)
	{
		anim.SetTrigger(fadeIn ? "in" : "out");
	}

	string minigameName;
	bool previewLoaded;
	bool unloaded;
	[Command(requiresAuthority=false)] public void CmdLoadPreviewMinigame(string minigameName)
	{
		hasStarted = true;
		StartCoroutine( LoadPreviewMinigameCo(minigameName) );
	}
	IEnumerator LoadPreviewMinigameCo(string minigameName)
	{
		yield return new WaitForSeconds(1.5f);
		CmdTriggerTransition(true);

		yield return new WaitForSeconds(0.5f);
		previewLoaded = false;
		this.minigameName = minigameName;
		//SceneEventProgressStatus status = NetworkManager.Singleton.SceneManager.LoadScene("TestPreview", LoadSceneMode.Single);
		
		while (!previewLoaded)
			yield return null;
	}

	[Command(requiresAuthority=false)] public void CmdReloadPreviewMinigame()
	{
		//NetworkManager.Singleton.SceneManager.UnloadScene(m_LoadedScene);
		//NetworkManager.Singleton.SceneManager.LoadScene("TestMinigame", LoadSceneMode.Additive);
		//SceneManager.UnloadSceneAsync(minigameName);
		//SceneManager.LoadSceneAsync(minigameName, LoadSceneMode.Additive);
		StartCoroutine( ReloadPreviewMinigameCo() );
	}
	IEnumerator ReloadPreviewMinigameCo()
	{
		//CmdTriggerTransition(true);
		yield return new WaitForSeconds(0.5f);
		unloaded = false;
		//NetworkManager.Singleton.SceneManager.UnloadScene(m_LoadedScene);
		//NetworkManager.Singleton.SceneManager.LoadScene("TestPreview", LoadSceneMode.Single);
		
		while (!unloaded)
			yield return null;
		//NetworkManager.Singleton.SceneManager.LoadScene(minigameName, LoadSceneMode.Additive);

		//SceneManager.LoadScene(1);
		//SceneManager.LoadSceneAsync(minigameName, LoadSceneMode.Additive);
	}


	#region minigame
	public void StartMinigame(string minigameName) 
	{
		Debug.Log("<color=green>StartMinigame</color>");
		RpcStartMinigame(minigameName);
	}
	[ClientRpc] private void RpcStartMinigame(string minigameName) 
	{
		Debug.Log($"<color=green>isClientOnly={isClientOnly} | isServer={isServer}</color>");
		if (isClientOnly)
			StartCoroutine(StartMiniGameCo(minigameName));
	} 
	IEnumerator StartMiniGameCo(string minigameName)
	{
		AsyncOperation async = SceneManager.LoadSceneAsync(minigameName, LoadSceneMode.Additive);

		while (!async.isDone)
			yield return null;
		if (isServer)
			CmdTriggerTransition(false);
	}

	#endregion

	public int GetPrizeValue(int place)
	{
		//switch (place)
		//{
		//	case 0: return nPlayers.Value == 2 ? 3 : nPlayers.Value == 3 ? 0 : 0 ;
		//	case 1: return nPlayers.Value == 2 ? 15 : nPlayers.Value == 3 ? 5 : 3 ;
		//	case 2: return nPlayers.Value == 2 ? 15 : nPlayers.Value == 3 ? 15 : 5 ;
		//	case 3: return nPlayers.Value == 2 ? 15 : nPlayers.Value == 3 ? 15 : 15 ;
		//}
		return 0;
	}
	public void AwardMinigamePrize(int[] rewards)
	{
		for (int i=0 ; i<rewards.Length ; i++)
		{
			if (i < coins.Count)
			{
				coins[i] += rewards[i];
			}
		}
	}


	public void ReturnToBoard(string minigameName)
	{
		//SceneManager.LoadScene(0);
	}
}

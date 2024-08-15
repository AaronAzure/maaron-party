using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using FishNet.Managing;
using FishNet;
using FishNet.Object.Synchronizing;
using FishNet.Managing.Scened;
using FishNet.Component.Animating;

public class GameManager : NetworkBehaviour
{
	public static GameManager Instance;
	private NetworkManager nm;
	//public NetworkVariable<int> nPlayers = new NetworkVariable<int>(
	//	0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	//public NetworkList<ulong> players;
	//public NetworkList<int> playerModels;
	public readonly SyncVar<int> nPlayers = new();
	//[SerializeField] private readonly SyncList<int> characterModels = new();
	[SerializeField] private readonly SyncDictionary<NetworkConnection, int> characterModels = new();
	private Scene m_LoadedScene;
	public List<LobbyObject> inits = new List<LobbyObject>();


	[Space] [Header("Lobby Manager")]
	public Transform spawnHolder;
	[SerializeField] private GameObject buttons;
	[SerializeField] private Button hostBtn;
	[SerializeField] private Button clientBtn;
	[SerializeField] private Button startBtn;


	[Space] [Header("In game references")]
	[SerializeField] private List<ushort> currNodes;
	[SerializeField] private List<int> coins;
	[SerializeField] private List<int> stars;
	public bool hasStarted {get; private set;}
	public bool lobbyCreated {get; private set;}
	[SerializeField] NetworkAnimator anim;


	private void Awake() 
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
		//DontDestroyOnLoad(this);

		if (hostBtn != null)
		{
			hostBtn.onClick.AddListener(() => {
				StartHost();
			});	
		}
		if (clientBtn != null)
		{
			clientBtn.onClick.AddListener(() => {
				StartClient();
			});	
		}
		if (startBtn != null)
		{
			startBtn.onClick.AddListener(() => {
				StartGame();
			});	
		}

		//players = new NetworkList<ulong>(
		//	default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner
		//);
		//playerModels = new NetworkList<int>(
		//	default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner
		//);
	}

	private void Start() 
	{
		currNodes = new();
		coins = new();
		stars = new();
		if (BoardManager.Instance == null)
			TriggerTransition(false);
		nm = FindObjectOfType<NetworkManager>();
	}

	public void StartHost()
	{
		nm.ServerManager.StartConnection();
		nm.ClientManager.StartConnection();;

		//NetworkManager.Singleton.StartHost();
		if (buttons != null) buttons.SetActive(false);
		if (startBtn != null)
			startBtn.gameObject.SetActive(true);
	}
	public void StartClient()
	{
		nm.ClientManager.StartConnection();;
		//NetworkManager.Singleton.StartClient();
		if (buttons != null) buttons.SetActive(false);
	}

	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~ NETWORK ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

	[ServerRpc(RequireOwnership=false)] public void SetPlayerModelServerRpc(NetworkConnection conn, int ind)
	{
		// new addition
		if (!characterModels.ContainsKey(conn))
			characterModels.Add(conn, ind);
		// rewrite existing
		else
			characterModels[conn] = ind;
	}
	[ServerRpc(RequireOwnership=false)] public void TestServer(string msg)
	{
		Debug.Log($"<color=magenta>To Server {msg}</color>");
		TestObserver();
	}
	[ObserversRpc] public void TestObserver()
	{
		Debug.Log("<color=magenta>To Observer</color>");
	}
	private void Update() {
		if (Input.GetKeyDown(KeyCode.A))
		{
			Debug.Log("---------");
			TestServer("hello");
		}
	}

	public void StartGame(string sceneName="TestBoard")
	{
		lobbyCreated = true;
		nPlayers.Value = InstanceFinder.ClientManager.Clients.Count;
		string s = "<color=cyan>ClientManager.Clients.Keys: ";
		TestServer("hello");
		//foreach (int key in InstanceFinder.ClientManager.Clients.Keys)
			//s += $"|{key}| ";
		foreach (NetworkConnection conn in InstanceFinder.ClientManager.Clients.Values)
			s += $"|{conn}| ";
		s += "</color>";

		//s = "<color=cyan>characterModels: ";
		//for (int i=0 ; i<characterModels.Count ; i++)
		//{
		//	s += $"|{characterModels[i]}| ";
		//}
		//s += "</color>";
		Debug.Log(s);

		startBtn.gameObject.SetActive(false);
		StartCoroutine( StartGameCo(sceneName) );
	}
	IEnumerator StartGameCo(string sceneName)
	{
		TriggerTransition(true);
		//TriggerTransitionServerRpc(true);
		yield return new WaitForSeconds(0.5f);
		SceneLoadData sld = new SceneLoadData(sceneName);
		InstanceFinder.SceneManager.LoadGlobalScenes(sld);

		SceneUnloadData uld = new SceneUnloadData("_TestLobby");
		InstanceFinder.SceneManager.UnloadGlobalScenes(uld);
		//NetworkManager.Singleton.SceneManager.LoadScene("TestBoard", LoadSceneMode.Single);
	}

	//[ServerRpc(RequireOwnership=false)] public void NextPlayerTurnServerRpc(ulong id)
	//{
	//	BoardManager.Instance.NextPlayerTurnClientRpc(
	//		new ClientRpcParams { 
	//			Send = new ClientRpcSendParams { 
	//				TargetClientIds = new List<ulong> {id}
	//			}
	//		}
	//	);
	//}
	//[ServerRpc(RequireOwnership=false)] public void LoadMinigameServerRpc()
	//{
	//	StartCoroutine(LoadMinigameCo());
	//}
	//IEnumerator LoadMinigameCo()
	//{
	//	TriggerTransitionServerRpc(true);
	//	yield return new WaitForSeconds(0.5f);
	//	NetworkManager.Singleton.SceneManager.LoadScene("TestMinigame", LoadSceneMode.Single);
	//}

	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~ NETWORK ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


	//public void IncreaseNumPlayers()
	//{
	//	nPlayers.Value++;
	//}
	//public void DecreaseNumPlayers()
	//{
	//	nPlayers.Value--;
	//}

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
	//[ServerRpc(RequireOwnership=false)] public void TriggerTransitionServerRpc(bool fadeIn) // broadcast
	//{
	//	TriggerTransitionClientRpc(fadeIn);
	//}
	//[ClientRpc(RequireOwnership=false)] private void TriggerTransitionClientRpc(bool fadeIn)
	//{
	//	anim.SetTrigger(fadeIn ? "in" : "out");
	//}

	string minigameName;
	bool previewLoaded;
	bool unloaded;
	[ServerRpc(RequireOwnership=false)] public void LoadPreviewMinigameServerRpc(string minigameName)
	{
		hasStarted = true;
		StartCoroutine( LoadPreviewMinigameCo(minigameName) );
	}
	IEnumerator LoadPreviewMinigameCo(string minigameName)
	{
		//yield return new WaitForSeconds(1.5f);
		//TriggerTransitionServerRpc(true);

		//yield return new WaitForSeconds(0.5f);
		//previewLoaded = false;
		//this.minigameName = minigameName;
		//SceneEventProgressStatus status = NetworkManager.Singleton.SceneManager.LoadScene("TestPreview", LoadSceneMode.Single);
		
		//while (!previewLoaded)
			yield return null;
		//NetworkManager.Singleton.SceneManager.LoadScene(minigameName, LoadSceneMode.Additive);

		//* LOCAL
		//SceneManager.LoadScene(1);
		//SceneManager.LoadSceneAsync(minigameName, LoadSceneMode.Additive);
	}

	[ServerRpc(RequireOwnership=false)] public void ReloadPreviewMinigameServerRpc()
	{
		//NetworkManager.Singleton.SceneManager.UnloadScene(m_LoadedScene);
		//NetworkManager.Singleton.SceneManager.LoadScene("TestMinigame", LoadSceneMode.Additive);
		//SceneManager.UnloadSceneAsync(minigameName);
		//SceneManager.LoadSceneAsync(minigameName, LoadSceneMode.Additive);
		StartCoroutine( ReloadPreviewMinigameCo() );
	}
	IEnumerator ReloadPreviewMinigameCo()
	{
		////TriggerTransitionServerRpc(true);
		//yield return new WaitForSeconds(0.5f);
		//unloaded = false;
		//NetworkManager.Singleton.SceneManager.UnloadScene(m_LoadedScene);
		////NetworkManager.Singleton.SceneManager.LoadScene("TestPreview", LoadSceneMode.Single);
		
		//while (!unloaded)
			yield return null;
		//NetworkManager.Singleton.SceneManager.LoadScene(minigameName, LoadSceneMode.Additive);

		////SceneManager.LoadScene(1);
		////SceneManager.LoadSceneAsync(minigameName, LoadSceneMode.Additive);
	}

	
	private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
	{
		Debug.Log("OnLoadComplete clientId: " + clientId + " scene: " + sceneName + " mode: " + loadSceneMode);
		previewLoaded = true;
	}
	private void OnUnloadComplete(ulong clientId, string sceneName)
	{
		Debug.Log("OnLoadComplete clientId: " + clientId + " scene: " + sceneName);
		unloaded = true;
	}

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

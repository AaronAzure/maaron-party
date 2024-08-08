using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
	public static GameManager Instance;
	//public int nPlayers {get; private set;}
	//public NetworkList<ulong> players = new NetworkList<ulong>();
	//public NetworkVariable<List<ulong>> players = new NetworkVariable<List<ulong>>(
	//	new(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	public NetworkVariable<int> nPlayers = new NetworkVariable<int>(
		0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


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
	[SerializeField] Animator anim;


	private void Awake() 
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
		DontDestroyOnLoad(this);

		hostBtn.onClick.AddListener(() => {
			StartHost();
		});	
		clientBtn.onClick.AddListener(() => {
			StartClient();
		});	
		startBtn.onClick.AddListener(() => {
			StartGame();
		});	
	}

	private void Start() 
	{
		currNodes = new();
		coins = new();
		stars = new();
		if (BoardManager.Instance == null)
			TriggerTransition(false);
	}

	public void StartHost()
	{
		NetworkManager.Singleton.StartHost();
		if (buttons != null) buttons.SetActive(false);
		if (startBtn != null)
			startBtn.gameObject.SetActive(true);
	}
	public void StartClient()
	{
		NetworkManager.Singleton.StartClient();
		if (buttons != null) buttons.SetActive(false);
	}
	//public void StartGame()
	//{
	//	NetworkManager.Singleton.SceneManager.LoadScene("TestBoard", LoadSceneMode.Single);
	//}

	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~ NETWORK ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

	[ServerRpc(RequireOwnership=false)] public void JoinGameServerRpc()
	{
		if (!IsHost) return;
		nPlayers.Value++;
		//Debug.Log($"Player {id} joined!!");
		//if (!players.Value.Contains(id))
		//	players.Value.Add(id);
	}
	[ServerRpc(RequireOwnership=false)] public void LeftGameServerRpc()
	{
		if (!IsHost) return;
		if (!lobbyCreated)
			nPlayers.Value--;
		//if (players != null && players.Value.Contains(id))
		//{
		//	players.Value.Remove(id);
		//}
	}

	public void StartGame()
	{
		lobbyCreated = true;
		StartCoroutine( StartGameCo() );
	}
	IEnumerator StartGameCo()
	{
		//TriggerTransition(true);
		TriggerTransitionServerRpc(true);
		yield return new WaitForSeconds(0.5f);
		NetworkManager.Singleton.SceneManager.LoadScene("TestBoard", LoadSceneMode.Single);
	}

	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~ NETWORK ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


	public void IncreaseNumPlayers()
	{
		nPlayers.Value++;
	}
	public void DecreaseNumPlayers()
	{
		nPlayers.Value--;
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
	[ServerRpc(RequireOwnership=false)] public void TriggerTransitionServerRpc(bool fadeIn) // broadcast
	{
		TriggerTransitionClientRpc(fadeIn);
	}
	[ClientRpc(RequireOwnership=false)] public void TriggerTransitionClientRpc(bool fadeIn)
	{
		anim.SetTrigger(fadeIn ? "in" : "out");
	}

	public void LoadPreviewMinigame(string minigameName)
	{
		hasStarted = true;
		StartCoroutine( LoadPreviewMinigameCo(minigameName) );
	}

	string minigameName;
	IEnumerator LoadPreviewMinigameCo(string minigameName)
	{
		yield return new WaitForSeconds(1.5f);
		TriggerTransition(true);

		yield return new WaitForSeconds(0.5f);
		this.minigameName = minigameName;
		SceneManager.LoadScene(1);
		SceneManager.LoadSceneAsync(minigameName, LoadSceneMode.Additive);
	}
	public void ReloadPreviewMinigame()
	{
		SceneManager.UnloadSceneAsync(minigameName);
		SceneManager.LoadSceneAsync(minigameName, LoadSceneMode.Additive);
	}

	public int GetPrizeValue(int place)
	{
		switch (place)
		{
			case 0: return nPlayers.Value == 2 ? 3 : nPlayers.Value == 3 ? 0 : 0 ;
			case 1: return nPlayers.Value == 2 ? 15 : nPlayers.Value == 3 ? 5 : 3 ;
			case 2: return nPlayers.Value == 2 ? 15 : nPlayers.Value == 3 ? 15 : 5 ;
			case 3: return nPlayers.Value == 2 ? 15 : nPlayers.Value == 3 ? 15 : 15 ;
		}
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
		SceneManager.LoadScene(0);
	}
}

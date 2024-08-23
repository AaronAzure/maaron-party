using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.ProBuilder.MeshOperations;

public class GameManager : NetworkBehaviour
{
	#region Variables
	public static GameManager Instance;
	private GameNetworkManager nm {get{return GameNetworkManager.Instance;}}
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

	#endregion
	
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~ NETWORK ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	#region Network


	#endregion
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~ NETWORK ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


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


	[SyncVar] public string minigameName;

	#region minigame
	public void StartMinigame(string minigameName) // host side
	{
		RpcStartMinigame(minigameName);
	}
	[ClientRpc] private void RpcStartMinigame(string minigameName) 
	{
		StartCoroutine(StartMiniGameCo(minigameName));
	} 
	IEnumerator StartMiniGameCo(string minigameName)
	{
		this.minigameName = minigameName;

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

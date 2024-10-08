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


	[Space] [Header("Lobby Manager")]
	public Transform spawnHolder;


	[Space] [Header("In game references")]
	[SerializeField] private List<ushort> currNodes;
	//[SyncVar] [SerializeField] private Dictionary<int, int> traps;
	public readonly SyncDictionary<int, int> traps = new SyncDictionary<int, int>();
	[SyncVar] [SerializeField] private List<int> coins;
	[SyncVar] [SerializeField] private List<int> stars;
	[SyncVar] [SerializeField] private List<List<int>> items;
	[SyncVar] public int nTurn=1; 
	public bool gameStarted; 
	public int prevStarInd=-1; 
	[SyncVar] public int maxTurns=20; 
	[SyncVar] public int turretReady; 
	[SyncVar] public int turretRot; 
	//private int nPlayers {get{return GameObject.FindGameObjectsWithTag("Player").Length;}}
	//public bool hasStarted {get; private set;}
	//public bool lobbyCreated {get; private set;}
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
		//traps = new SyncDictionary<int, int>();
		coins = new();
		stars = new();
		items = new();
		nTurn = 1;
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

	public void SaveTrap(int nodeId, int playerId)
	{
		//if (traps == null)
		//	traps = new();
		//while (traps.Count <= playerId)
		//	traps.Add(0);
		if (!traps.ContainsKey(nodeId))
			traps.Add(nodeId, playerId);
	}
	//public Dictionary<int, int> GetTraps()
	//{
	//	return traps;
	//}

	[Command(requiresAuthority=false)] public void CmdSaveCoins(int newCoin, int playerId)
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
	[Command(requiresAuthority=false)] public void CmdSaveItems(List<int> newItems, int playerId)
	{
		if (items == null)
			items = new();
		while (items.Count <= playerId)
			items.Add(new());
		items[playerId] = newItems;
	}
	public List<int> GetItems(int playerId)
	{
		return items[playerId];
	}

	[Command(requiresAuthority=false)] public void CmdSaveStars(int newStar, int playerId)
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

	#region Board

	[Command(requiresAuthority=false)] public void CmdHitPlayersAtNode(int nodeId) => RpcHitPlayersAtNode(nodeId);
	[ClientRpc] private void RpcHitPlayersAtNode(int nodeId) => NodeManager.Instance.GetNode(nodeId).HitPlayers(-10);

	#endregion

	public void TriggerTransition(bool fadeIn)
	{
		anim.SetTrigger(fadeIn ? "in" : "out");
	}
	[Command(requiresAuthority=false)] public void CmdTriggerTransition(bool fadeIn)
	{
		anim.SetTrigger(fadeIn ? "in" : "out");
	}
	public void TriggerTransitionDelay(bool fadeIn)
	{
		StartCoroutine(TriggerTransitionDelayCo(fadeIn));
	}
	private IEnumerator TriggerTransitionDelayCo(bool fadeIn)
	{
		yield return new WaitForSeconds(0.5f);
		anim.SetTrigger(fadeIn ? "in" : "out");
	}


	[SyncVar] public string minigameName;

	#region minigame
	public void IncreaseTurnNum()
	{
		nTurn++;
	}
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
		int nPlayers = NetworkServer.connections.Count;
		switch (place)
		{
			// 3, 0, 0
			case 0: return nPlayers == 2 ? 3 : nPlayers == 3 ? 0 : 0 ;
			// 15, 5, 3
			case 1: return nPlayers == 2 ? 15 : nPlayers == 3 ? 5 : 3 ;
			// -, 15, 5
			case 2: return nPlayers == 2 ? 15 : nPlayers == 3 ? 15 : 5 ;
			// -, -, 15
			case 3: return nPlayers == 2 ? 15 : nPlayers == 3 ? 15 : 15 ;
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
}

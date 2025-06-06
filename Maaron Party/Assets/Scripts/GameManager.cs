using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
	#region Variables
	public static GameManager Instance;
	//private GameNetworkManager nm {get{return GameNetworkManager.Instance;}}
	[SerializeField] private PreviewManager pm;


	[Space] [Header("Lobby Manager")]
	public Transform spawnHolder;


	[Space] [Header("Minigame rewards")]
	public GameObject rewardUi;
	public GameObject[] dataUis;


	[Space] [Header("In game references")]
	[SerializeField] private List<ushort> currNodes;
	public readonly SyncDictionary<int, int[]> traps = new SyncDictionary<int, int[]>();
	[SyncVar] [SerializeField] private List<int> placements;
	[SyncVar] [SerializeField] private List<int> coins;
	[SyncVar] [SerializeField] private List<int> stars;
	[SyncVar] [SerializeField] private List<int> manas;
	[SyncVar] [SerializeField] private List<int> doorTolls;
	[SyncVar] [SerializeField] private List<List<int>> items;
	
	[Space] [SyncVar] public int nTurn=1; 
	[SyncVar] public int maxTurns=20; 
	
	[Space] [SyncVar] public bool isPractice; 
	[SerializeField] private GameObject pmObj;

	[Space] public bool gameStarted; 
	public int prevStarInd=-1; 
	[SyncVar] public int turretReady; 
	[SyncVar] public int turretRot; 
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
		placements = new();
		coins = new();
		stars = new();
		manas = new();
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

	public void SaveTrap(int nodeId, int playerId, int characterInd, int trapId)
	{
		if (!traps.ContainsKey(nodeId))
			traps.Add(nodeId, new int[3]{playerId, characterInd, trapId});
		// overwrite trap
		else
			traps[nodeId] = new int[3]{playerId, characterInd, trapId};
	}

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
	[Command(requiresAuthority=false)] public void CmdSaveMana(int newMana, int playerId)
	{
		if (manas == null)
			manas = new();
		while (manas.Count <= playerId)
			manas.Add(0);
		manas[playerId] = newMana;
	}
	public int GetMana(int playerId)
	{
		return manas[playerId];
	}

	[Command(requiresAuthority=false)] public void CmdSavePlacements(int newPlacement, int playerId)
	{
		if (placements == null)
			placements = new();
		while (placements.Count <= playerId)
			placements.Add(0);
		placements[playerId] = newPlacement;
	}
	public int GetPlacements(int playerId)
	{
		if (playerId < 0 || placements == null || playerId >= placements.Count)
			return 0;
		return placements[playerId];
	}
	//* ------- save -------
	//* --------------------

	#region Board

	[Command(requiresAuthority=false)] public void CmdHitPlayersAtNode(int nodeId, int penalty) => RpcHitPlayersAtNode(nodeId, penalty);
	[ClientRpc] private void RpcHitPlayersAtNode(int nodeId, int penalty) => NodeManager.Instance.GetNode(nodeId).HitPlayers(penalty);

	[Command(requiresAuthority=false)] public void CmdHitPlayersStarsAtNode(int nodeId) => RpcHitPlayersStarsAtNode(nodeId);
	[ClientRpc] private void RpcHitPlayersStarsAtNode(int nodeId) => NodeManager.Instance.GetNode(nodeId).HitPlayersStars();

	[Command(requiresAuthority=false)] public void CmdSetupDoorTolls(int nDoors)
	{
		doorTolls = new();
		for (int i=0 ; i<nDoors ; i++)
		{
			doorTolls.Add(0);
			if (BoardManager.Instance != null)
				BoardManager.Instance.CmdSetNewToll(i, 1);
		}
	}
	[Command(requiresAuthority=false)] public void CmdSetDoorToll(int ind, int newToll)
	{
		if (doorTolls == null || ind < 0 || ind >= doorTolls.Count)
			return;
		RpcSetDoorToll(ind, newToll);
		if (BoardManager.Instance != null)
			BoardManager.Instance.CmdSetNewToll(ind, newToll);
	}
	[ClientRpc] public void RpcSetDoorToll(int ind, int newToll)
	{
		if (doorTolls == null || ind < 0 || ind >= doorTolls.Count)
			return;
		doorTolls[ind] = newToll;
	}
	public int GetDoorToll(int ind)
	{
		if (doorTolls == null || ind < 0 || ind >= doorTolls.Count)
			return 0;
		return doorTolls[ind] == 0 ? 1 : doorTolls[ind];
	}

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
	public void SetProfilePic(int[] inds) => RpcSetProfilePic(inds);
	[ClientRpc] public void RpcSetProfilePic(int[] inds) => pm.SetProfilePic(inds);
	public void IncreaseTurnNum() => nTurn++;
	

	[Command(requiresAuthority=false)] public void CmdTogglePreviewManager(bool active) => RpcTogglePreviewManager(active);
	[ClientRpc] void RpcTogglePreviewManager(bool active) => pmObj.SetActive(active);


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
			// 10, 3, 2
			case 1: return nPlayers == 2 ? 10 : nPlayers == 3 ? 3 : 2 ;
			// -, 10, 3
			case 2: return nPlayers == 2 ? 10 : nPlayers == 3 ? 10 : 3 ;
			// -, -, 10
			case 3: return nPlayers == 2 ? 10 : nPlayers == 3 ? 10 : 10 ;
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
	public void AwardManaPrize(int id)
	{
		if (id >= 0 && id < manas.Count)
			manas[id] = Mathf.Clamp(manas[id] + 1, 0, 5);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Linq;

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
	//public readonly SyncDictionary<int, int[]> traps = new SyncDictionary<int, int[]>();
	public Dictionary<int, int[]> traps = new Dictionary<int, int[]>();
	[SerializeField] private NetworkList<int> placements = new NetworkList<int>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	[SerializeField] private NetworkList<int> coins = new NetworkList<int>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	[SerializeField] private NetworkList<int> stars = new NetworkList<int>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	[SerializeField] private NetworkList<int> manas = new NetworkList<int>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	[SerializeField] private NetworkList<int> doorTolls = new NetworkList<int>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	[SerializeField] private NetworkVariable<List<List<int>>> items =
		new NetworkVariable<List<List<int>>>(new List<List<int>>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	
	[Space] public NetworkVariable<int> nTurn = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner); 
	public NetworkVariable<int> maxTurns = new NetworkVariable<int>(20, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner); 
	
	[Space] public NetworkVariable<bool> isPractice = new NetworkVariable<bool>(false); 
	[SerializeField] private GameObject pmObj;

	[Space] public bool gameStarted; 
	public int prevStarInd=-1; 
	public NetworkVariable<int> turretReady = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner); 
	public NetworkVariable<int> turretRot = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner); 
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
		//placements = new();
		//coins = new();
		//stars = new();
		//manas = new();
		//items = new();
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

	[ServerRpc] public void SaveCoinsServerRpc(int newCoin, int playerId)
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
	[ServerRpc] public void SaveItemsServerRpc(int[] newItems, int playerId)
	{
		//if (items == null)
		//	items = new();
		while (items.Value.Count <= playerId)
			items.Value.Add(new());
		items.Value[playerId] = new List<int>(newItems);
	}
	public List<int> GetItems(int playerId)
	{
		return items.Value[playerId];
	}

	[ServerRpc] public void SaveStarsServerRpc(int newStar, int playerId)
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
	[ServerRpc] public void SaveManaServerRpc(int newMana, int playerId)
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

	[ServerRpc] public void SavePlacementsServerRpc(int newPlacement, int playerId)
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

	[ServerRpc] public void HitPlayersAtNodeServerRpc(int nodeId, int penalty) => HitPlayersAtNodeClientRpc(nodeId, penalty);
	[ClientRpc] private void HitPlayersAtNodeClientRpc(int nodeId, int penalty) => NodeManager.Instance.GetNode(nodeId).HitPlayers(penalty);

	[ServerRpc] public void HitPlayersStarsAtNodeServerRpc(int nodeId) => HitPlayersStarsAtNodeClientRpc(nodeId);
	[ClientRpc] private void HitPlayersStarsAtNodeClientRpc(int nodeId) => NodeManager.Instance.GetNode(nodeId).HitPlayersStars();

	[ServerRpc] public void SetupDoorTollsServerRpc(int nDoors)
	{
		doorTolls = new();
		for (int i=0 ; i<nDoors ; i++)
		{
			doorTolls.Add(0);
			if (BoardManager.Instance != null)
				BoardManager.Instance.SetNewTollServerRpc(i, 1);
		}
	}
	[ServerRpc] public void SetDoorTollServerRpc(int ind, int newToll)
	{
		if (doorTolls == null || ind < 0 || ind >= doorTolls.Count)
			return;
		SetDoorTollClientRpc(ind, newToll);
		if (BoardManager.Instance != null)
			BoardManager.Instance.SetNewTollServerRpc(ind, newToll);
	}
	[ClientRpc] public void SetDoorTollClientRpc(int ind, int newToll)
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
	[ServerRpc] public void TriggerTransitionServerRpc(bool fadeIn)
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


	public NetworkVariable<string> minigameName = new();

	#region minigame
	public void SetProfilePic(int[] inds) => SetProfilePicClientRpc(inds);
	[ClientRpc] public void SetProfilePicClientRpc(int[] inds) => pm.SetProfilePic(inds);
	public void IncreaseTurnNum() => nTurn.Value++;
	

	[ServerRpc] public void TogglePreviewManagerServerRpc(bool active) => TogglePreviewManagerClientRpc(active);
	[ClientRpc] void TogglePreviewManagerClientRpc(bool active) => pmObj.SetActive(active);


	public void StartMinigame(string minigameName) // host side
	{
		StartMinigameClientRpc(minigameName);
	}
	[ClientRpc] private void StartMinigameClientRpc(string minigameName) 
	{
		StartCoroutine(StartMiniGameCo(minigameName));
	} 
	IEnumerator StartMiniGameCo(string minigameName)
	{
		this.minigameName.Value = minigameName;

		AsyncOperation async = SceneManager.LoadSceneAsync(minigameName, LoadSceneMode.Additive);

		while (!async.isDone)
			yield return null;
		if (IsServer)
			TriggerTransitionServerRpc(false);
	}

	#endregion

	public int GetPrizeValue(int place)
	{
		int nPlayers = NetworkManager.Singleton.ConnectedClients.Count;
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

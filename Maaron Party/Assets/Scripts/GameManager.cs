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
	public NetworkVariable<int> nConnected = new(0); 
	[SerializeField] private List<ushort> currNodes = new();
	//public readonly SyncDictionary<int, int[]> traps = new SyncDictionary<int, int[]>();
	public Dictionary<int, int[]> traps = new Dictionary<int, int[]>();
	[SerializeField] private List<int> placements = new();
	[SerializeField] private List<int> coins = new();
	[SerializeField] private List<int> stars = new();
	[SerializeField] private List<int> manas = new();
	[SerializeField] private List<int> doorTolls = new();
	//[SerializeField] private NetworkVariable<List<List<int>>> items =
	//	new(new()r);
	
	[Space] public NetworkVariable<int> nTurn = new(1); 
	public NetworkVariable<int> maxTurns = new(20); 
	
	[Space] public NetworkVariable<bool> isPractice = new(false); 
	[SerializeField] private GameObject pmObj;

	[Space] public bool gameStarted; 
	public int prevStarInd=-1; 
	public NetworkVariable<int> turretReady = new(0); 
	public NetworkVariable<int> turretRot = new(0); 
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
		if (BoardManager.Instance == null)
			TriggerTransition(false);
	}

	#endregion
	
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~ NETWORK ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	#region Network

	[ServerRpc(RequireOwnership=false)] public void ConnectServerRpc() 
	{
		nConnected.Value++;
		Debug.Log($"<color=white>== ConnectServerRpc ({nConnected.Value}) ==</color>");
	} 


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
		//if (coins == null)
		//	coins = new();
		//while (coins.Value.Count <= playerId)
		//	coins.Value.Add(0);
		//coins.Value[playerId] = newCoin;
	}
	public int GetCoins(int playerId)
	{
		return 0;
		//return coins.Value[playerId];
	}
	[ServerRpc] public void SaveItemsServerRpc(int[] newItems, int playerId)
	{
		//if (items == null)
		//	items = new();
		//while (items.Value.Count <= playerId)
		//	items.Value.Add(new());
		//items.Value[playerId] = new List<int>(newItems);
	}
	public List<int> GetItems(int playerId)
	{
		return null;
		//return items.Value[playerId];
	}

	[ServerRpc] public void SaveStarsServerRpc(int newStar, int playerId)
	{
		//if (stars == null)
		//	stars = new();
		//while (stars.Value.Count <= playerId)
		//	stars.Value.Add(0);
		//stars.Value[playerId] = newStar;
	}
	public int GetStars(int playerId)
	{
		return 0;
		//return stars.Value[playerId];
	}
	[ServerRpc] public void SaveManaServerRpc(int newMana, int playerId)
	{
		//if (manas == null)
		//	manas = new();
		//while (manas.Value.Count <= playerId)
		//	manas.Value.Add(0);
		//manas.Value[playerId] = newMana;
	}
	public int GetMana(int playerId)
	{
		return 0;
		//return manas.Value[playerId];
	}

	[ServerRpc] public void SavePlacementsServerRpc(int newPlacement, int playerId)
	{
		//if (placements == null)
		//	placements = new();
		//while (placements.Value.Count <= playerId)
		//	placements.Value.Add(0);
		//placements.Value[playerId] = newPlacement;
	}
	public int GetPlacements(int playerId)
	{
		return 0;
		//if (playerId < 0 || placements == null || playerId >= placements.Value.Count)
		//	return 0;
		//return placements.Value[playerId];
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

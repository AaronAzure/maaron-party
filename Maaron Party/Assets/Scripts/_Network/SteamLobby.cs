using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;


public class SteamLobby : MonoBehaviour
{
	public static SteamLobby Instance;
	private LobbyManager lm {get{return LobbyManager.Instance;}}
	[SerializeField] private GameNetworkManager nm;

	[Space] [SerializeField] private GameObject buttons;
	[SerializeField] private GameObject lobbyUi;
	[SerializeField] private Button hostBtn;
	[SerializeField] private Button clientBtn;
	[SerializeField] private Button lobbyBtn;
	[SerializeField] private Button startBtn;


	
	#region Steam
	protected Callback<LobbyCreated_t> lobbyCreated;
	protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
	protected Callback<LobbyEnter_t> lobbyEnter;
	protected Callback<LobbyMatchList_t> lobbyList;
	protected Callback<LobbyDataUpdate_t> lobbyUpdate;

	private const string HostAddrKey="HostAddress";
	public List<CSteamID> lobbyIds = new List<CSteamID>();

	

	private void Awake() {
		Instance = this;
	}
	public void Start()
	{
		if (SteamManager.Initialized)
		{
			lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
			lobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
			lobbyList = Callback<LobbyMatchList_t>.Create(OnGetLobbyList);
			lobbyUpdate = Callback<LobbyDataUpdate_t>.Create(OnLobbyUpdate);
			gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameJoinLobbyJoinRequested);
		}

		hostBtn.onClick.AddListener(() => {
			_START_HOST();
		});	
		clientBtn.onClick.AddListener(() => {
			_START_CLIENT();
		});	
		startBtn.onClick.AddListener(() => {
			StartGame();
		});	
	}

	public void _START_HOST()
	{
		buttons.SetActive(false);
		lobbyUi.SetActive(false);
		//StartHost();
		//startBtn.gameObject.SetActive(true);
		SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 4);
	}
	public void _START_CLIENT()
	{
		//buttons.SetActive(false);
		//StartClient();

	}
	public void StartGame()
	{
		nm.StartBoardGame();
		//nPlayers = NetworkServer.connections.Count;
		//Debug.Log($"<color=magenta>NetworkServer.connections.Count = {NetworkServer.connections.Count}</color>");
		startBtn.gameObject.SetActive(false);
	}

	private void OnLobbyCreated(LobbyCreated_t callback)
	{
		//buttons.SetActive(false);
		//lobbyUi.SetActive(false);
		// fail
		if (callback.m_eResult != EResult.k_EResultOK)
		{
			buttons.SetActive(true);
			return;
		}
		nm.StartHost();
		startBtn.gameObject.SetActive(true);

		SteamMatchmaking.SetLobbyData(
			new CSteamID(callback.m_ulSteamIDLobby), 
			HostAddrKey, 
			SteamUser.GetSteamID().ToString()
		);
		SteamMatchmaking.SetLobbyData(
			new CSteamID(callback.m_ulSteamIDLobby), 
			"name", 
			SteamFriends.GetPersonaName().ToString() + "'s Lobby"
		);
	}
	private void OnGameJoinLobbyJoinRequested(GameLobbyJoinRequested_t callback)
	{
		SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
	}

	private void OnLobbyEntered(LobbyEnter_t callback)
	{
		// all connected clients

		// only clients
		//if (NetworkServer.active) return;

		string hostAddr = SteamMatchmaking.GetLobbyData(
			new CSteamID(callback.m_ulSteamIDLobby),
			HostAddrKey
		);

		//nm.networkAddress = hostAddr;
		buttons.SetActive(false);
		lobbyUi.SetActive(false);
		nm.StartClient();
	}
	
	public void GetLobbyList()
	{
		if (lobbyIds.Count > 0)
			lobbyIds.Clear();

		SteamMatchmaking.AddRequestLobbyListResultCountFilter(60);
		SteamMatchmaking.RequestLobbyList();
	}
	private void OnGetLobbyList(LobbyMatchList_t callback)
	{
		if (lm.lobbies.Count > 0)
			lm.DestroyLobbies();

		for (int i=0 ; i<callback.m_nLobbiesMatching ; i++)
		{
			CSteamID lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
			lobbyIds.Add(lobbyId);
			SteamMatchmaking.RequestLobbyData(lobbyId);
		}
	}
	private void OnLobbyUpdate(LobbyDataUpdate_t callback)
	{
		lm.DisplayLobbies(lobbyIds, callback);
	}
	public void JoinLobby(CSteamID lobbyID)
	{
		buttons.SetActive(false);
		lobbyUi.SetActive(false);
		SteamMatchmaking.JoinLobby(lobbyID);
	}
	public void ShowLobbies()
	{
		lobbyUi.SetActive(true);
	}
	#endregion
}

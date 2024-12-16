using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.SceneManagement;

public class LobbyRelay : MonoBehaviour
{
	public static LobbyRelay Instance;
	private IAuthenticationService aut {get{return AuthenticationService.Instance;}}
	private IRelayService relay {get{return RelayService.Instance;}}
	private GameNetworkManager nm => GameNetworkManager.Instance;
	//private NetworkManager nm => NetworkManager.Singleton;
	private ILobbyService ls => LobbyService.Instance;

	
	[Space] [Header("Ui")]
	[SerializeField] private GameObject loadingUi;
	[SerializeField] private GameObject buttonUi;
	[SerializeField] private GameObject hostingUi;
	[SerializeField] private GameObject joiningUi;
	[SerializeField] private GameObject lobbyUi;
	[SerializeField] private TextMeshProUGUI lobbyNameTmp;


	[Space] [Header("Lobby")]
	public static Lobby hostLobby;
	public static Lobby joinedLobby;
	[SerializeField] private float heartBeatTimer=20;
	[SerializeField] private float lobbyPollTimer=1.5f;
	[SerializeField] private TMP_InputField lobbyNameInput;
	[SerializeField] private LobbyContainer[] lobbyItems; // max 10 lobbies
	[SerializeField] private LobbyPlayer[] players; // max 4 players
	bool startGame=false;
	//[SerializeField] private TMP_Dropdown gameModeInput;

	
	[Space] [Header("Interactive")]
	[SerializeField] private GameObject startButton;
	[SerializeField] private Button hostBtn;
	[SerializeField] private Button joinBtn;
	[SerializeField] private TextMeshProUGUI lobbyCode;
	[SerializeField] private TMP_InputField joinCodeInput;


	private void Awake() {
		Instance = this;
		hostLobby = joinedLobby = null;
	}

	
	// Start is called before the first frame update
	async void Start()
	{
		try {
			ShowLoading();
			Debug.Log("Logging on");
			var options = new InitializationOptions();
			options.SetProfile($"{Random.Range(1000,int.MaxValue)}");
			await UnityServices.InitializeAsync(options);

			aut.SignedIn += () => {
				Debug.Log("Signed in " + aut.PlayerId);
				ShowMainMenu();
			};
			
			await aut.SignInAnonymouslyAsync();

			//if (hostBtn != null)
			//	hostBtn.onClick.AddListener(() => CreateRelay() );
			//if (joinBtn != null)
			//	joinBtn.onClick.AddListener(() => JoinRelay() );
		} catch (AuthenticationException e) {
			Debug.LogError(e);
			Start();
		}
	}

	public async void ShowLobby(bool pollPlayers=true)
	{
		buttonUi?.SetActive(false);
		loadingUi?.SetActive(false);
		lobbyUi.SetActive(true);
		startButton.SetActive(hostLobby != null);
		if (pollPlayers)
		{
			try {
				pollingLobby = true;
				Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
				lobbyPollTimer = 2.5f;
				ListPlayers();
				joinedLobby = lobby;
				pollingLobby = false;
			} catch (LobbyServiceException e) {
				Debug.LogError(e);
				lobbyPollTimer = 2.5f;
				pollingLobby = false;
			}
		}
	}
	public void ShowMainMenu()
	{
		loadingUi?.SetActive(false);
		lobbyUi.SetActive(false);
		buttonUi?.SetActive(true);
	}
	public void ShowLoading()
	{
		buttonUi?.SetActive(false);
		lobbyUi.SetActive(false);
		loadingUi?.SetActive(true);
	}


	#region Lobby

	private Player GetPlayer()
	{
		return new Player {
			Data = new Dictionary<string, PlayerDataObject> {
				{"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, AuthenticationService.Instance.PlayerId)},
			}
		};
	}

	public void UpdateLobbyData(string key, string newValue)
	{
		try {
			//LobbyService
		} catch (LobbyServiceException e) {
			Debug.LogError(e);
		}
	}
	public async void CreateLobby()
	{
		try {
			ShowLoading();
			int maxPlayers = 4;

			string lobbyName = lobbyNameInput.text;
			if (lobbyName == "" || lobbyName == null)
				lobbyName = "MyLobby"; 
			CreateLobbyOptions options = new CreateLobbyOptions {
				IsPrivate = false,
				Player = GetPlayer(),
				Data = new Dictionary<string, DataObject> {
					//{"GameMode", new DataObject(DataObject.VisibilityOptions.Public, lobbyName)},
					//{"KillerId", new DataObject(DataObject.VisibilityOptions.Public, AuthenticationService.Instance.PlayerId)},
					{"Start", new DataObject(DataObject.VisibilityOptions.Member, "0")}
				}
			};
			//currentKillerId = AuthenticationService.Instance.PlayerId;

			//createLobbyObj.SetActive(false);
			var lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
			joinedLobby = hostLobby = lobby;
			Debug.Log($"Created Lobby = {lobbyName}, Id = {hostLobby.Id}, code = {hostLobby.LobbyCode}");
			
			//lobbyTitleTxt.text = hostLobby.Name;
			//foreach (LobbyContainer l in players)
			//	l.killerBtn.interactable = true;
			//EnterJoinedLobby();
			ShowLobby();
			lobbyNameTmp.text = $"{lobbyName}";
		} catch (LobbyServiceException e) {
			Debug.LogError(e);
			ShowMainMenu();
		}
	}
	public async void QuickJoinLobby()
	{
		try {
			ShowLoading();
			await ls.QuickJoinLobbyAsync();
			ShowLobby();
		} catch (LobbyServiceException e) {
			Debug.LogError(e);
			ShowMainMenu();
		}
	}
	public async void JoinLobbyBySelection(string lobbyId)
	{
		try {
			ShowLoading();
			JoinLobbyByIdOptions options = new JoinLobbyByIdOptions {
				Player = GetPlayer()
			};
			var lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);
			joinedLobby = lobby;
			lobbyNameTmp.text = joinedLobby.Name;
			
			ShowLobby();
			Debug.Log($"Joined Lobby!");
		} catch (LobbyServiceException e) {
			Debug.LogError(e);
			ShowMainMenu();
		}
	}
	public void LeaveLobby()
	{
		try {
			//ls.QuickJoinLobbyAsync();
		} catch (LobbyServiceException e) {
			Debug.LogError(e);
		}
	}
	public void DeleteLobby()
	{
		try {
			//ls.QuickJoinLobbyAsync();
		} catch (LobbyServiceException e) {
			Debug.LogError(e);
		}
	}
	public void KickPlayerFromLobby()
	{
		try {
			//ls.QuickJoinLobbyAsync();
		} catch (LobbyServiceException e) {
			Debug.LogError(e);
		}
	}
	#endregion



	#region List Lobbies
	bool findingLobby;
	public async void ListLobbies()
	{
		if (findingLobby) return;
		try {
			findingLobby = true;
			QueryLobbiesOptions options = new QueryLobbiesOptions {
				Count = 10,
				Filters = new List<QueryFilter> {
					new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
				},
				Order = new List<QueryOrder> {
					new QueryOrder(false, QueryOrder.FieldOptions.Created)
				}
			};
			
			QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(options);
			Debug.Log($"<color=magenta>Found {queryResponse.Results.Count} Lobbies</color>");

			for (int i=0 ; i<lobbyItems.Length ; i++)
			{
				if (i < queryResponse.Results.Count)
				{
					lobbyItems[i].gameObject.SetActive(true);
					lobbyItems[i].lobbyNameTxt.text = queryResponse.Results[i].Name;
					lobbyItems[i].nPlayersTxt.text = $"{queryResponse.Results[i].Players.Count}/{queryResponse.Results[i].MaxPlayers}";
					//lobbyItems[i].gameModeTxt.text = queryResponse.Results[i].Data["GameMode"].Value;
					lobbyItems[i].lobbyId = queryResponse.Results[i].Id;
				}
				else
				{
					lobbyItems[i].gameObject.SetActive(false);
				}
			}
			findingLobby = false;
		} catch (LobbyServiceException e) {
			Debug.LogError(e);
			findingLobby = false;
		}
	}
	#endregion


	#region HeartBeat
	bool pollingLobby;
	private void FixedUpdate() 
	{
		if (!startGame)
		{
			HeartBeatLobby();	
			if (!pollingLobby)
				PollLobby();	
		}
	}
	async void HeartBeatLobby()
	{
		if (hostLobby != null)
		{
			if (heartBeatTimer > 0)	
				heartBeatTimer -= Time.fixedDeltaTime;	
			else
			{
				await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
				heartBeatTimer = 20;
			}
		}
	}

	private async void PollLobby()
	{
		if (joinedLobby != null)
		{
			if (lobbyPollTimer > 0)		
				lobbyPollTimer -= Time.fixedDeltaTime; 
			else
			{
				try {
					pollingLobby = true;
					Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
					lobbyPollTimer = 1.5f;
					ListPlayers();
					joinedLobby = lobby;
					if (hostLobby == null)
						CheckIfStart();
					pollingLobby = false;
				} catch (LobbyServiceException e) {
					Debug.LogError(e);
					lobbyPollTimer = 1.5f;
					pollingLobby = false;
				}
			}
		}
	}

	public void ListPlayers()
	{
		for (int i=0 ; i<players.Length ; i++)
		{
			if (i < joinedLobby.Players.Count)
			{
				players[i].gameObject.SetActive(true);
				players[i].playerNameTxt.text = joinedLobby.Players[i].Id;
				players[i].playerId = joinedLobby.Players[i].Data["PlayerName"].Value;
			}
			else
			{
				players[i].gameObject.SetActive(false);
			}
		}
	}
	#endregion

	public async void _START_GAME()
	{
		try {
			ShowLoading();
			Allocation a = await relay.CreateAllocationAsync(4);

			var data = new RelayServerData(a, "dtls");
			nm.GetComponent<UnityTransport>().SetRelayServerData(data);

			string joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);
			await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions {
				Data = new Dictionary<string, DataObject> {
					{ "Start", new DataObject(DataObject.VisibilityOptions.Member, joinCode)}
				}
			});
			joinedLobby = hostLobby;

			// start host
			nm._START_HOST();

		} catch (RelayServiceException e) {
			Debug.LogError(e);
			ShowLobby(false);
		}
	}
	public async void StartClient()
	{
		try {
			Debug.Log("Starting Client!");
			ShowLoading();

			JoinAllocation a = await relay.JoinAllocationAsync(joinedLobby.Data["Start"].Value);

			var data = new RelayServerData(a, "dtls");
			nm.GetComponent<UnityTransport>().SetRelayServerData(data);

			// start host
			Debug.Log("Started!");
			nm._START_CLIENT();

			//string joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);
			//await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions {
			//	Data = new Dictionary<string, DataObject> {
			//		{ "Start", new DataObject(DataObject.VisibilityOptions.Member, joinCode)}
			//	}
			//});

		} catch (RelayServiceException e) {
			Debug.LogError(e);
			ShowLobby(false);
		}
	}

	private void CheckIfStart()
	{
		if (!startGame && joinedLobby.Data["Start"].Value != "0")
		{
			Debug.Log("Server game has begun!");
			startGame = true;
			StartClient();
			//nm._START_CLIENT();
		}
	}
}

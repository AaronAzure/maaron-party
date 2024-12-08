using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using Unity.Networking.Transport;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class LobbyRelay : MonoBehaviour
{
	private IAuthenticationService aut {get{return AuthenticationService.Instance;}}
	private IRelayService relay {get{return RelayService.Instance;}}
	//private GameNetworkManager nm => GameNetworkManager.Instance;
	private NetworkManager nm => NetworkManager.Singleton;
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
	bool startGame=false;
	//[SerializeField] private TMP_Dropdown gameModeInput;

	
	[Space] [Header("Interactive")]
	[SerializeField] private Button hostBtn;
	[SerializeField] private Button joinBtn;
	[SerializeField] private TextMeshProUGUI lobbyCode;
	[SerializeField] private TMP_InputField joinCodeInput;


	
	// Start is called before the first frame update
	async void Start()
	{
		try {
			Debug.Log("Logging on");
			var options = new InitializationOptions();
			options.SetProfile($"{Random.Range(1000,int.MaxValue)}");
			await UnityServices.InitializeAsync(options);

			aut.SignedIn += () => {
				Debug.Log("Signed in " + aut.PlayerId);
			};
			
			await aut.SignInAnonymouslyAsync();

			buttonUi?.SetActive(true);
			loadingUi?.SetActive(false);
			if (hostBtn != null)
				hostBtn.onClick.AddListener(() => CreateRelay() );
			if (joinBtn != null)
				joinBtn.onClick.AddListener(() => JoinRelay() );
		} catch (AuthenticationException e) {
			Debug.LogError(e);
			Start();
		}
	}

	
	async void CreateRelay() // plus host
	{
		try {
			// try to create lobby
			buttonUi?.SetActive(false);
			hostingUi?.SetActive(true);
			Allocation a = await relay.CreateAllocationAsync(4);

			var data = new RelayServerData(a, "dtls");
			nm.GetComponent<UnityTransport>().SetRelayServerData(data);

			// start host
			nm.StartHost();
			//nm.StartStandardHost(); // without relay

			//todo nm.StartRelayHost(nm.maxConnections); // with relay
			// created lobby, set lobby settings
			string joinCode = await relay.GetJoinCodeAsync(a.AllocationId);

			hostingUi?.SetActive(false);
			lobbyUi?.SetActive(true);
			lobbyCode.text = $"Lobby Code: {joinCode}";

		} catch (RelayServiceException e) {
			buttonUi?.SetActive(true);
			hostingUi?.SetActive(false);
			Debug.LogError(e);
		}
	}

	async void JoinRelay()
	{
		if (joinCodeInput.text == "" || joinCodeInput.text.Length < 6)
    	{
        	Debug.LogError("Please input a join code.");
        	return;
    	}

		Debug.Log($"<color=magenta>Joining with {joinCodeInput.text}</color>");
		try {
			// try to join lobby
			buttonUi?.SetActive(false);
			joiningUi?.SetActive(true);
			JoinAllocation a = await relay.JoinAllocationAsync(joinCodeInput.text[..6]);

			// joined lobby, set lobby settings
			joiningUi?.SetActive(false);
			var data = new RelayServerData(a, "dtls");
			nm.GetComponent<UnityTransport>().SetRelayServerData(data);
			
			// start host
			nm.StartClient();
			//nm.JoinStandardServer(); // without relay
			//todo nm.JoinRelayServer(); // with relay

		} catch (RelayServiceException e) {
			buttonUi?.SetActive(true);
			joiningUi?.SetActive(false);
			Debug.LogError(e);
		}
	}

	//public async void OnJoinCode()
	//{
	//	try
	//	{
	//		string joinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);
	//		Debug.Log("Host - Got join code: " + joinCode);
	//	}
	//	catch (RelayServiceException ex)
	//	{
	//		Debug.LogError(ex.Message + "\n" + ex.StackTrace);
	//	}
	//}


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
			buttonUi?.SetActive(false);
			loadingUi?.SetActive(true);
			int maxPlayers = 4;

			string lobbyName = lobbyNameInput.text;
			if (lobbyName == "" || lobbyName == null)
				lobbyName = "MyLobby"; 
			Debug.Log($"Creating Lobby = {lobbyName}");
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
			joinedLobby = hostLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
			Debug.Log($"Created Lobby = {lobbyName}, Id = {hostLobby.Id}, code = {hostLobby.LobbyCode}");
			
			//lobbyTitleTxt.text = hostLobby.Name;
			//foreach (LobbyContainer l in players)
			//	l.killerBtn.interactable = true;
			//EnterJoinedLobby();
			loadingUi?.SetActive(false);
			lobbyUi.SetActive(true);
			lobbyNameTmp.text = $"{lobbyName}";
		} catch (LobbyServiceException e) {
			Debug.LogError(e);
			buttonUi?.SetActive(true);
			loadingUi?.SetActive(false);
		}
	}
	public async void QuickJoinLobby()
	{
		try {
			await ls.QuickJoinLobbyAsync();
		} catch (LobbyServiceException e) {
			Debug.LogError(e);
		}
	}
	public async void JoinLobbyByCode()
	{
		try {
			//Debug.Log($"Attempting to Join Lobby ({l.lobbyId})");

			JoinLobbyByIdOptions options = new JoinLobbyByIdOptions {
				Player = GetPlayer()
			};
			joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync("0", options);
			//lobbyTitleTxt.text = joinedLobby.Name;

			//EnterJoinedLobby();
			//Debug.Log($"Joined Lobby!");
		} catch (LobbyServiceException e) {
			Debug.LogError(e);
		}
	}
	public void JoinLobbyBySelection()
	{
		try {
			//ls.QuickJoinLobbyAsync();
		} catch (LobbyServiceException e) {
			Debug.LogError(e);
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


	public async void ListLobbies()
	{
		try {
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
		} catch (LobbyServiceException e) {
			Debug.LogError(e);
		}
	}

	#endregion

	#region HeartBeat

	private void FixedUpdate() 
	{
		if (!startGame)
		{
			HeartBeatLobby();	
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
				Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
				lobbyPollTimer = 1.5f;
				//ListPlayers();
				//if (hostLobby == null)
				//	CheckIfStart();
				joinedLobby = lobby;
			}
		}
	}

	#endregion
}

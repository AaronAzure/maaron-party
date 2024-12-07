using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ED.SC;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using TMPro;
using Unity.Netcode;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using Unity.Netcode.Transports.UTP;

public class LobbyManager : MonoBehaviour
{
	public static LobbyManager Instance;
	public static Lobby hostLobby;
	public static Lobby joinedLobby;
	[SerializeField] private float heartBeatTimer=20;
	[SerializeField] private float lobbyPollTimer=1.5f;
	[SerializeField] private string playerName;
	[SerializeField] string currentKillerId;
	[SerializeField] string playerId;
	public string gameMode="KillerHost"; // RandomKiller, ChooseKiller
	bool startGame=false;
	

	[Space] [Header("Lobby UI")]
	[SerializeField] private LobbyContainer[] lobbyItems; // max 10 lobbies
	[SerializeField] private GameObject createLobbyObj;
	[SerializeField] private TMP_InputField lobbyNameInput;
	[SerializeField] private TMP_Dropdown gameModeInput;
	[SerializeField] private GameObject mainUi;
	[SerializeField] private GameObject lobbyUi;
	[SerializeField] private GameObject gameUi;
	[SerializeField] private TextMeshProUGUI lobbyTitleTxt;
	[SerializeField] private LobbyContainer[] players; // max 10 lobbies
	[SerializeField] private GameObject startGameBtn;


	private void Awake() 
	{
		if (Instance == null)
			Instance = this;		
		else
			Destroy(gameObject);
	}

	async void Start()
	{
		var options = new InitializationOptions();
    	options.SetProfile($"{Random.Range(1000,1000000000)}");
		await UnityServices.InitializeAsync(options);

		AuthenticationService.Instance.SignedIn += () => {
			Debug.Log("Signed In " + AuthenticationService.Instance.PlayerId);
		};
		await AuthenticationService.Instance.SignInAnonymouslyAsync();
		playerId = AuthenticationService.Instance.PlayerId;
	}

	public async void UpdateLobbyData(string key, string newValue)
	{
		try {
			hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions {
				Data = new Dictionary<string, DataObject> {
					//{ "GameMode", new DataObject(DataObject.VisibilityOptions.Public, newGameMode)},
					{ key, new DataObject(DataObject.VisibilityOptions.Public, newValue)}
				}
			});
			if (key == "KillerId")
			{
				currentKillerId = newValue;
			}
			joinedLobby = hostLobby;
		} catch (LobbyServiceException e) {
			Debug.LogError(e);
		}
	}

	private Player GetPlayer()
	{
		return new Player {
			Data = new Dictionary<string, PlayerDataObject> {
				{"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, AuthenticationService.Instance.PlayerId)},
			}
		};
	}

	public void ToggleCreateLobbyUi()
	{
		createLobbyObj.SetActive(!createLobbyObj.activeSelf);
	}

	public async void CreateLobby()
	{
		try {
			int maxPlayers = 4;

			string lobbyName = gameModeInput.options[gameModeInput.value].text;
			if (lobbyName == "" || lobbyName == null)
				lobbyName = "MyLobby"; 
			CreateLobbyOptions options = new CreateLobbyOptions {
				IsPrivate = false,
				Player = GetPlayer(),
				Data = new Dictionary<string, DataObject> {
					{"GameMode", new DataObject(DataObject.VisibilityOptions.Public, lobbyName)},
					{"KillerId", new DataObject(DataObject.VisibilityOptions.Public, AuthenticationService.Instance.PlayerId)},
					{"Start", new DataObject(DataObject.VisibilityOptions.Member, "0")}
				}
			};
			currentKillerId = AuthenticationService.Instance.PlayerId;

			createLobbyObj.SetActive(false);
			joinedLobby = hostLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyNameInput.text, maxPlayers, options);
			Debug.Log($"Created Lobby = {lobbyNameInput.text}, Id = {hostLobby.Id}, code = {hostLobby.LobbyCode}");
			
			lobbyTitleTxt.text = hostLobby.Name;
			foreach (LobbyContainer l in players)
				l.killerBtn.interactable = true;
			EnterJoinedLobby();
		} catch (LobbyServiceException e) {
			Debug.LogError(e);
		}
	}

	[Command] async void QuickJoinLobby()
	{
		try {
			await LobbyService.Instance.QuickJoinLobbyAsync();
		} catch (LobbyServiceException e) {
			Debug.LogError(e);
		}
	}

	//* DEFAULT METHOD
	public async void JoinLobbyByCode(LobbyContainer l)
	{
		try {
			Debug.Log($"Attempting to Join Lobby ({l.lobbyId})");

			JoinLobbyByIdOptions options = new JoinLobbyByIdOptions {
				Player = GetPlayer()
			};
			joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(l.lobbyId, options);
			lobbyTitleTxt.text = joinedLobby.Name;

			EnterJoinedLobby();
			Debug.Log($"Joined Lobby!");
		} catch (LobbyServiceException e) {
			Debug.LogError(e);
		}
	}

	public async void JoinLobbyBySelection(string lobbyId)
	{
		try {
			Debug.Log("Attempting to Join Lobby");

			JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions {
				Player = GetPlayer()
			};
			joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyId, options);
			lobbyTitleTxt.text = joinedLobby.Name;
			
			Debug.Log($"Joined Lobby!");
			EnterJoinedLobby();
		} catch (LobbyServiceException e) {
			Debug.LogError(e);
		}
	}

	[Command] async void LeaveLobby()
	{
		try {
			await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
			Debug.Log($"Left Lobby!");
		} catch (LobbyServiceException e) {
			Debug.LogError(e);
		}
	}

	[Command] async void KickPlayer(int ind)
	{
		try {
			await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[ind].Id);
			Debug.Log($"Kicked Player!");
		} catch (LobbyServiceException e) {
			Debug.LogError(e);
		}
	}
	[Command] async void DeleteLobby()
	{
		try {
			await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
			Debug.Log($"Delete Lobby!");
		} catch (LobbyServiceException e) {
			Debug.LogError(e);
		}
	}

	public async void START_HOST()
	{
		try {
			if (GameManager.Instance != null)
				GameManager.Instance.nPlayers = joinedLobby.Players.Count;
			Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

			RelayServerData data = new RelayServerData(allocation, "dtls");
			NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(data);

			NetworkManager.Singleton.StartHost();

			string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
			await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions {
				Data = new Dictionary<string, DataObject> {
					{ "Start", new DataObject(DataObject.VisibilityOptions.Member, joinCode)}
				}
			});
			joinedLobby = hostLobby;
			EnterGame();
		} catch (RelayServiceException e) {
			Debug.LogError(e);
			startGame = false;
		}
		//NetworkManager.Singleton.StartHost();
	}
	public async void START_CLIENT()
	{
		try {
			if (GameManager.Instance != null)
				GameManager.Instance.nPlayers = joinedLobby.Players.Count;
			currentKillerId = joinedLobby.Data["KillerId"].Value;

			string joinCode = joinedLobby.Data["Start"].Value[..6];
			JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

			RelayServerData data = new RelayServerData(allocation, "dtls");
			NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(data);

			NetworkManager.Singleton.StartClient();
			EnterGame();
		} catch (RelayServiceException e) {
			Debug.LogError(e);
			startGame = false;
		}
		//NetworkManager.Singleton.StartClient();
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
					lobbyItems[i].playersTxt.text = $"{queryResponse.Results[i].Players.Count}/{queryResponse.Results[i].MaxPlayers}";
					lobbyItems[i].gameModeTxt.text = queryResponse.Results[i].Data["GameMode"].Value;
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


	public void ListPlayers()
	{
		for (int i=0 ; i<players.Length ; i++)
		{
			if (i < joinedLobby.Players.Count)
			{
				players[i].gameObject.SetActive(true);
				players[i].playerNameTxt.text = joinedLobby.Players[i].Id;
				players[i].playerId = joinedLobby.Players[i].Data["PlayerName"].Value;
				players[i].killerImg.color = (joinedLobby.Data["KillerId"].Value != joinedLobby.Players[i].Id ? 
					(hostLobby == null ? Color.clear : Color.white) : 
					Color.red
				);
			}
			else
			{
				players[i].gameObject.SetActive(false);
			}
		}
	}



	private void EnterJoinedLobby()
	{
		startGameBtn.SetActive(hostLobby != null);
		ListPlayers();
		mainUi.SetActive(false);
		lobbyUi.SetActive(true);
	}
	private void EnterGame()
	{
		mainUi.SetActive(false);
		createLobbyObj.SetActive(false);
		lobbyUi.SetActive(false);
		gameUi.SetActive(true);
	}


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
				heartBeatTimer = 20;
				await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
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
				lobbyPollTimer = 1.5f;
				Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
				ListPlayers();
				if (hostLobby == null)
					CheckIfStart();
				joinedLobby = lobby;
			}
		}
	}
	private void CheckIfStart()
	{
		if (!startGame && joinedLobby.Data["Start"].Value != "0")
		{
			startGame = true;
			START_CLIENT();
		}
	}

	public bool IsKiller()
	{
		//Debug.Log(currentKillerId);
		return currentKillerId == playerId;
	}
}

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

public class LobbyRelay : MonoBehaviour
{
	private IAuthenticationService aut {get{return AuthenticationService.Instance;}}
	private IRelayService relay {get{return RelayService.Instance;}}
	private GameNetworkManager nm => GameNetworkManager.Instance;

	
	[Space] [Header("Ui")]
	[SerializeField] private GameObject loadingUi;
	[SerializeField] private GameObject buttonUi;
	[SerializeField] private GameObject hostingUi;
	[SerializeField] private GameObject joiningUi;
	[SerializeField] private GameObject lobbyUi;

	
	[Space] [Header("Interactive")]
	[SerializeField] private Button hostBtn;
	[SerializeField] private Button joinBtn;
	[SerializeField] private TextMeshProUGUI lobbyCode;
	[SerializeField] private TMP_InputField joinCodeInput;
	private NetworkDriver hostDriver;


	
	// Start is called before the first frame update
	async void Start()
	{
		await UnityServices.InitializeAsync();

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
	}

	
	async void CreateRelay() // plus host
	{
		try {
			// try to create lobby
			buttonUi?.SetActive(false);
			hostingUi?.SetActive(true);
			Allocation a = await relay.CreateAllocationAsync(nm.ConnectedClients.Count);

			// created lobby, set lobby settings
			string joinCode = await relay.GetJoinCodeAsync(a.AllocationId);
			var data = new RelayServerData(a, "dtls");
			var settings = new NetworkSettings();
    		settings.WithRelayParameters(ref data);
			//todo nm.GetComponent<UtpTransport>();


			hostDriver = NetworkDriver.Create(settings);

			// Bind to the Relay server.
			if (hostDriver.Bind(NetworkEndpoint.AnyIpv4) != 0)
			{
				Debug.LogError("Host client failed to bind");
			}
			else
			{
				if (hostDriver.Listen() != 0)
				{
					Debug.LogError("Host client failed to listen");
				}
				else
				{
					Debug.Log("Host client bound to Relay server");
				}
			}


			hostingUi?.SetActive(false);
			lobbyUi?.SetActive(true);
			lobbyCode.text = $"Lobby Code: {joinCode}";

			// start host
			//nm.StartStandardHost(); // without relay
			//todo nm.StartRelayHost(nm.maxConnections); // with relay

		} catch (RelayServiceException e) {
			buttonUi?.SetActive(true);
			hostingUi?.SetActive(false);
			Debug.LogError(e);
		}
	}

	public void OnBindHost(Allocation a)
	{
    	Debug.Log("Host - Binding to the Relay server using UTP.");

    	// Extract the Relay server data from the Allocation response.
    	var relayServerData = new RelayServerData(a, "udp");

    	// Create NetworkSettings using the Relay server data.
    	var settings = new NetworkSettings();
    	settings.WithRelayParameters(ref relayServerData);
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
			JoinAllocation a = await relay.JoinAllocationAsync(joinCodeInput.text);

			// joined lobby, set lobby settings
			joiningUi?.SetActive(false);
			var data = new RelayServerData(a, "dtls");
			var settings = new NetworkSettings();
    		settings.WithRelayParameters(ref data);
			
			// start host
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
}

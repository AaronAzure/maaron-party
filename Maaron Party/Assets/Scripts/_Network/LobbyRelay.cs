using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Utp;
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
			joinBtn.onClick.AddListener(() => JoinRelay(joinCodeInput.text) );
	}

	
	async void CreateRelay() // plus host
	{
		try {
			// try to create lobby
			buttonUi?.SetActive(false);
			hostingUi?.SetActive(true);
			Allocation a = await relay.CreateAllocationAsync(nm.maxConnections);

			// created lobby, set lobby settings
			string joinCode = await relay.GetJoinCodeAsync(a.AllocationId);
			var data = new RelayServerData(a, "dtls");
			var settings = new NetworkSettings();
    		settings.WithRelayParameters(ref data);

			hostingUi?.SetActive(false);
			lobbyUi?.SetActive(true);
			lobbyCode.text = $"Lobby Code: {joinCode}";

			// start host
			//nm.StartStandardHost(); // without relay
			nm.StartRelayHost(nm.maxConnections); // with relay

		} catch (RelayServiceException e) {
			buttonUi?.SetActive(true);
			hostingUi?.SetActive(false);
			Debug.LogError(e);
		}
	}

	async void JoinRelay(string joinCode)
	{
		Debug.Log($"<color=magenta>Joining with {joinCode}</color>");
		try {
			// try to join lobby
			buttonUi?.SetActive(false);
			joiningUi?.SetActive(true);
			JoinAllocation a = await relay.JoinAllocationAsync(joinCode);
			joiningUi?.SetActive(false);

			// joined lobby, set lobby settings
			var data = new RelayServerData(a, "dtls");
			var settings = new NetworkSettings();
    		settings.WithRelayParameters(ref data);
			
			// start host
			//nm.JoinStandardServer(); // without relay
			nm.JoinRelayServer(); // with relay

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

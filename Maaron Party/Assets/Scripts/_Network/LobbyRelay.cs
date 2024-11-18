using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Utp;

public class LobbyRelay : MonoBehaviour
{
	private IAuthenticationService aut {get{return AuthenticationService.Instance;}}
	private IRelayService relay {get{return RelayService.Instance;}}
	
	// Start is called before the first frame update
	async void Start()
	{
		await UnityServices.InitializeAsync();

		aut.SignedIn += () => {
			Debug.Log("Signed in " + aut.PlayerId);
		};
		
		await aut.SignInAnonymouslyAsync();
	}
	async void CreateRelay()
	{
		try {
			Allocation a = await relay.CreateAllocationAsync(NetworkServer.maxConnections-1);

			string joinCode = await relay.GetJoinCodeAsync(a.AllocationId);

			//RelayNetworkManager.singleton.GetComponent<UtpTransport>().
		} catch (RelayServiceException e) {
			Debug.LogError(e);
		}
	}

	async void JoinRelay(string joinCOde)
	{
		try {
			await relay.JoinAllocationAsync(joinCOde);
		} catch (RelayServiceException e) {
			Debug.LogError(e);
		}
	}
}

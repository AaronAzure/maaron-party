using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TestScript : NetworkBehaviour
{
	public override void OnNetworkSpawn()
	{
		GameManager.Instance.ConnectServerRpc();
		//GameNetworkManager.Instance.StartGame();
		name = $"_ PLAYER {OwnerClientId} _";
		DontDestroyOnLoad(this);
	}
}

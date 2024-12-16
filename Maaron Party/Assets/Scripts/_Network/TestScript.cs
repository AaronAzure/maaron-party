using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TestScript : NetworkBehaviour
{
	public override void OnNetworkSpawn()
	{
		//GameNetworkManager.Instance.StartGame();
	}
}

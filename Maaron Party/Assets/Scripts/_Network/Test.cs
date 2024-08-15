using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using FishNet;

public class Test : NetworkBehaviour
{
    [ServerRpc(RequireOwnership=false)] public void TestServerRpc(string msg)
	{
		Debug.Log($"<color=magenta>To Server :{msg}</color>");
		TestObserverRpc();
	}
	[ObserversRpc] public void TestObserverRpc()
	{
		Debug.Log("<color=magenta>To Observer</color>");
	}
	private void Update() 
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			TestServerRpc($"{base.LocalConnection.ClientId}");
		}
	}
}

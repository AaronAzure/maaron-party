using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class NetcodeTest : NetworkBehaviour
{
	[SerializeField] private NetworkVariable<int> hp;
	[SerializeField] private NetworkVariable<int[]> coins;


	//[GenerateSerializationForTypeAttribute(typeof(System.Int32[]))]
	[GenerateSerializationForType(typeof(int[]))]
	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		hp = new();
		coins = new(new int[4], NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	}
}

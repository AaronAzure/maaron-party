using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ClientObject : NetworkBehaviour
{
    public static ClientObject Instance;
	public ulong id {get; private set;}

	private void Start() 
	{
		id = OwnerClientId;
		if (IsOwner)
		{
			Instance = this;
			transform.parent = null;
			DontDestroyOnLoad(gameObject);
		}
		else
			Destroy(gameObject);
	}
}

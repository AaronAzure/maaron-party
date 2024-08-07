using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyObject : MonoBehaviour
{
	private void Start() 
	{
		transform.SetParent(LobbyManager.Instance.spawnHolder, true);
		transform.localScale = Vector3.one;
	}
}

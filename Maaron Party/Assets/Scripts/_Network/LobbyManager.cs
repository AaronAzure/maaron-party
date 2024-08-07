using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
	public static LobbyManager Instance;
	public Transform spawnHolder;

	private void Awake() 
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}
}

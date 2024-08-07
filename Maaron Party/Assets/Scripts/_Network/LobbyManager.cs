using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class LobbyManager : MonoBehaviour
{
	public static LobbyManager Instance;
	public Transform spawnHolder;
	[SerializeField] private GameObject buttons;
	[SerializeField] private Button hostBtn;
	[SerializeField] private Button clientBtn;


	private void Awake() 
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
		hostBtn.onClick.AddListener(() => {
			StartHost();
		});	
		clientBtn.onClick.AddListener(() => {
			StartClient();
		});	
	}

	public void StartHost()
	{
		NetworkManager.Singleton.StartHost();
		if (buttons != null) buttons.SetActive(false);
	}
	public void StartClient()
	{
		NetworkManager.Singleton.StartClient();
		if (buttons != null) buttons.SetActive(false);
	}
}

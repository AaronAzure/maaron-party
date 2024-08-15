using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FishNet.Connection;
using FishNet.Object;
using FishNet;

public class NetworkStarter : NetworkBehaviour
{
	public static NetworkStarter Instance;
	public Transform spawnHolder;
	[SerializeField] private GameObject buttons;
	[SerializeField] private Button hostBtn;
	[SerializeField] private Button clientBtn;
	[SerializeField] private Button startBtn;
	private GameManager gm;

	private void Awake() 
	{
		Instance = this;
	}

	private void Start() 
	{
		if (hostBtn != null)
		{
			hostBtn.onClick.AddListener(() => {
				StartHost();
			});	
		}
		if (clientBtn != null)
		{
			clientBtn.onClick.AddListener(() => {
				StartClient();
			});	
		}
		if (startBtn != null)
		{
			startBtn.onClick.AddListener(() => {
				StartGame();
			});	
		}
	}

	public void StartHost()
	{
		InstanceFinder.NetworkManager.ServerManager.StartConnection();
		InstanceFinder.NetworkManager.ClientManager.StartConnection();

		if (buttons != null) buttons.SetActive(false);
		if (startBtn != null)
			startBtn.gameObject.SetActive(true);
	}
	public void StartClient()
	{
		InstanceFinder.NetworkManager.ClientManager.StartConnection();
		//NetworkManager.Singleton.StartClient();
		if (buttons != null) buttons.SetActive(false);
	}
	public void StartGame()
	{
		gm = GameManager.Instance;
		if (GameManager.Instance != null)
		{
			if (startBtn != null) startBtn.gameObject.SetActive(false);
			gm.StartGame();
		}
	}
}

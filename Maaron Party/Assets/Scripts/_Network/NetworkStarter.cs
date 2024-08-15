using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FishNet.Connection;
using FishNet.Object;
using FishNet;

public class NetworkStarter : NetworkBehaviour
{
	[SerializeField] private GameObject buttons;
	[SerializeField] private Button hostBtn;
	[SerializeField] private Button clientBtn;
	[SerializeField] private Button startBtn;
	[SerializeField] private GameManager gm;

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
		gm.gameObject.SetActive(true);

		if (buttons != null) buttons.SetActive(false);
		if (startBtn != null)
			startBtn.gameObject.SetActive(true);
	}
	public void StartClient()
	{
		InstanceFinder.NetworkManager.ClientManager.StartConnection();
		gm.gameObject.SetActive(true);
		//NetworkManager.Singleton.StartClient();
		if (buttons != null) buttons.SetActive(false);
	}
	public void StartGame()
	{
		if (startBtn != null) startBtn.gameObject.SetActive(false);
		gm.StartGame();
	}
}

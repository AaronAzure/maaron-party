using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour
{
	public static LobbyManager Instance;
	public Transform spawnHolder;
	[SerializeField] private GameObject buttons;
	[SerializeField] private Button hostBtn;
	[SerializeField] private Button clientBtn;
	[SerializeField] private Button startBtn;


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
		startBtn.onClick.AddListener(() => {
			StartGame();
		});	
	}

	public void StartHost()
	{
		NetworkManager.Singleton.StartHost();
		if (buttons != null) buttons.SetActive(false);
		if (startBtn != null)
			startBtn.gameObject.SetActive(true);
	}
	public void StartClient()
	{
		NetworkManager.Singleton.StartClient();
		if (buttons != null) buttons.SetActive(false);
	}
	public void StartGame()
	{
		NetworkManager.Singleton.SceneManager.LoadScene("TestBoard", UnityEngine.SceneManagement.LoadSceneMode.Single);
	}
}

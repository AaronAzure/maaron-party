using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;

public class LobbyManager : MonoBehaviour
{
	public static LobbyManager Instance;
	//[SerializeField] private SteamLobby steamLobby;
	[SerializeField] private Transform lobbyHolder;
	[SerializeField] private RectTransform lobbyUi;
	[SerializeField] private LobbyContainer lobbyDataPrefab;
	public List<GameObject> lobbies {get; private set;}


	private void Awake() {
		if (Instance == null) Instance = this;
		lobbies = new List<GameObject>();
	}

	public void _GET_LOBBIES()
	{
		SteamLobby.Instance.GetLobbyList();
		SteamLobby.Instance.ShowLobbies();
	}

	public void DisplayLobbies(List<CSteamID> lobbyIds, LobbyDataUpdate_t result)
	{
		if (lobbyIds != null && lobbyIds.Count > 0)
			lobbyUi.sizeDelta = new Vector2(lobbyUi.rect.width, 10 + lobbyIds.Count * 110);
		for (int i=0 ; i<lobbyIds.Count ; i++)
		{
			if (lobbyIds[i].m_SteamID == result.m_ulSteamIDLobby)
			{
				GameObject obj = Instantiate(lobbyDataPrefab.gameObject, lobbyHolder);

				LobbyContainer lobby = obj.GetComponent<LobbyContainer>();
				//lobby.lobbyId = (CSteamID)lobbyIds[i].m_SteamID;
				//lobby.lobbyName = SteamMatchmaking.GetLobbyData((CSteamID)lobbyIds[i].m_SteamID, "name");
				//lobby.SetLobbyData();
				lobbies.Add(obj);
			}
		}
	}

	public void DestroyLobbies()
	{
		foreach (GameObject lobby in lobbies)
			Destroy(lobby);
		lobbies.Clear();
	}
}

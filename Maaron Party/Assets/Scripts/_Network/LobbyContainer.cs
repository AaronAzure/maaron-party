using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using TMPro;

public class LobbyContainer : MonoBehaviour
{
	public CSteamID lobbyId;
	public string lobbyName;
	[SerializeField] TextMeshProUGUI lobbyTxt;

	public void SetLobbyData()
	{
		if (lobbyName == "")
			lobbyTxt.text = "Untitled Lobby :<";
		else
			lobbyTxt.text = lobbyName;
	}

	public void _JOIN_LOBBY()
	{
		GameNetworkManager.Instance.JoinLobby(lobbyId);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.ComponentModel;

public class LobbyContainer : MonoBehaviour
{
	public TextMeshProUGUI playerNameTxt;
	public string playerId;

	[Space] public TextMeshProUGUI lobbyNameTxt;
	public TextMeshProUGUI nPlayersTxt;
	public string lobbyId;

	//public void SetLobbyData()
	//{
	//	if (lobbyName == "")
	//		lobbyTxt.text = "Untitled Lobby :<";
	//	else
	//		lobbyTxt.text = lobbyName;
	//}

	//public void _JOIN_LOBBY()
	//{
	//	SteamLobby.Instance.JoinLobby(lobbyId);
	//}
}

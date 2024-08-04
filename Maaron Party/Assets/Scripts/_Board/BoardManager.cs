using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
	public static BoardManager Instance;
	[SerializeField] private PlayerControls[] players;
	int nTurn;
	int nPlayerOrder;
	int nPlayers;

	[Space] [Header("MUST REFERENCE PER BOARD")]
	[SerializeField] private Node startNode;

	private void Awake() 
	{
		Instance = this;		
	}

	private void Start() 
	{
		for (int i=0 ; i<players.Length ; i++)
		{
			if (players[i] != null)
			{
				players[i].SetModel(i);
				players[i].SetStartNode(startNode);
			}
		}
		nPlayers = players.Length;
		NextPlayerTurn(true);
	}

	public void NextPlayerTurn(bool firstTurn=false)
	{
		if (!firstTurn)
			nPlayerOrder = ++nPlayerOrder % nPlayers;
		if (nPlayerOrder >= 0 && nPlayerOrder < players.Length)
			players[nPlayerOrder].YourTurn();
	}
}

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
	GameManager gm;


	[Space] [Header("Universal")]
	[SerializeField] private Transform dataUi;


	[Space] [Header("MUST REFERENCE PER BOARD")]
	[SerializeField] private Node startNode;


	private void Awake() 
	{
		Instance = this;		
	}

	public Transform GetUiLayout()
	{
		return dataUi;
	}

	private void Start() 
	{
		gm = GameManager.Instance;
		for (int i=0 ; i<players.Length ; i++)
		{
			if (players[i] != null && players[i].gameObject.activeInHierarchy)
			{
				players[i].SetModel(i);
				players[i].SetId(i);
				if (!gm.hasStarted)
				{
					players[i].SetStartNode(startNode);
					gm.IncreaseNumPlayers();
				}
			}
		}
		nPlayers = players.Length;
		NextPlayerTurn(true);
	}

	public void NextPlayerTurn(bool firstTurn=false)
	{
		if (!firstTurn)
			nPlayerOrder = ++nPlayerOrder;
			//nPlayerOrder = ++nPlayerOrder % nPlayers;
		if (nPlayerOrder >= 0 && nPlayerOrder < players.Length)
			players[nPlayerOrder].YourTurn();
		else if (nPlayerOrder >= nPlayers)
			LoadMinigame("TestMinigame");
	}

	void LoadMinigame(string minigameName)
	{
		gm.LoadPreviewMinigame(minigameName);
	}
}

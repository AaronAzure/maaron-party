using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;
	public int nPlayers {get; private set;}


	[Space] [Header("In game references")]
	[SerializeField] private List<ushort> currNodes;
	[SerializeField] private List<int> coins;
	[SerializeField] private List<int> stars;
	public bool hasStarted {get; private set;}
	[SerializeField] Animator anim;


	private void Awake() 
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
		DontDestroyOnLoad(this);
	}

	private void Start() 
	{
		currNodes = new();
		coins = new();
		stars = new();
		if (BoardManager.Instance == null)
			TriggerTransition(false);
	}

	public void IncreaseNumPlayers()
	{
		nPlayers++;
	}
	public void DecreaseNumPlayers()
	{
		nPlayers--;
	}

	//* --------------------
	//* ------- save -------
	public void SaveCurrNode(ushort nodeId, int playerId)
	{
		if (currNodes == null)
			currNodes = new();
		while (currNodes.Count <= playerId)
			currNodes.Add(0);
		currNodes[playerId] = nodeId;
	}
	public ushort GetCurrNode(int playerId)
	{
		return currNodes[playerId];
	}

	public void SaveCoins(int newCoin, int playerId)
	{
		if (coins == null)
			coins = new();
		while (coins.Count <= playerId)
			coins.Add(0);
		coins[playerId] = newCoin;
	}
	public int GetCoins(int playerId)
	{
		return coins[playerId];
	}

	public void SaveStars(int newStar, int playerId)
	{
		if (stars == null)
			stars = new();
		while (stars.Count <= playerId)
			stars.Add(0);
		stars[playerId] = newStar;
	}
	public int GetStars(int playerId)
	{
		return stars[playerId];
	}
	//* ------- save -------
	//* --------------------

	public void TriggerTransition(bool fadeIn)
	{
		anim.SetTrigger(fadeIn ? "in" : "out");
	}

	public void LoadPreviewMinigame(string minigameName)
	{
		hasStarted = true;
		StartCoroutine( LoadPreviewMinigameCo(minigameName) );
	}

	string minigameName;
	IEnumerator LoadPreviewMinigameCo(string minigameName)
	{
		yield return new WaitForSeconds(1.5f);
		TriggerTransition(true);

		yield return new WaitForSeconds(0.5f);
		this.minigameName = minigameName;
		SceneManager.LoadScene(1);
		SceneManager.LoadSceneAsync(minigameName, LoadSceneMode.Additive);
	}
	public void ReloadPreviewMinigame()
	{
		SceneManager.UnloadSceneAsync(minigameName);
		SceneManager.LoadSceneAsync(minigameName, LoadSceneMode.Additive);
	}

	public int GetPrizeValue(int place)
	{
		switch (place)
		{
			case 0: return nPlayers == 2 ? 3 : nPlayers == 3 ? 0 : 0 ;
			case 1: return nPlayers == 2 ? 15 : nPlayers == 3 ? 5 : 3 ;
			case 2: return nPlayers == 2 ? 15 : nPlayers == 3 ? 15 : 5 ;
			case 3: return nPlayers == 2 ? 15 : nPlayers == 3 ? 15 : 15 ;
		}
		return 0;
	}
	public void AwardMinigamePrize(int[] rewards)
	{
		for (int i=0 ; i<rewards.Length ; i++)
		{
			if (i < coins.Count)
			{
				coins[i] += rewards[i];
			}
		}
	}


	public void ReturnToBoard(string minigameName)
	{
		SceneManager.LoadScene(0);
	}
}

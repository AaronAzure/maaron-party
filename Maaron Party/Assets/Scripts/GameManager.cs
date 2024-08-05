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
	public bool hasStarted {get; private set;}


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
	}

	public void IncreaseNumPlayers()
	{
		nPlayers++;
	}
	public void DecreaseNumPlayers()
	{
		nPlayers--;
	}

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


	public void LoadPreviewMinigame(string minigameName)
	{
		hasStarted = true;
		StartCoroutine( LoadPreviewMinigameCo(minigameName) );
	}

	IEnumerator LoadPreviewMinigameCo(string minigameName)
	{
		yield return new WaitForSeconds(2f);
		SceneManager.LoadScene(1);
		SceneManager.LoadSceneAsync(minigameName, LoadSceneMode.Additive);
	}

	public void ReturnToBoard(string minigameName)
	{
		SceneManager.LoadScene(0);
	}
}

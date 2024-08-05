using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;
	public int nPlayers {get; private set;}

	private void Awake() 
	{
		Instance = this;
		DontDestroyOnLoad(this);
	}

	private void Start() 
	{

	}

	public void IncreaseNumPlayers()
	{
		nPlayers++;
	}
	public void DecreaseNumPlayers()
	{
		nPlayers--;
	}

	public void LoadPreviewMinigame(string minigameName)
	{
		StartCoroutine( LoadPreviewMinigameCo(minigameName) );
	}

	IEnumerator LoadPreviewMinigameCo(string minigameName)
	{
		yield return new WaitForSeconds(2f);
		SceneManager.LoadScene(1);
		SceneManager.LoadSceneAsync(minigameName, LoadSceneMode.Additive);
	}
}

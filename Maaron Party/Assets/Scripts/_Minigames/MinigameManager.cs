using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MinigameManager : MonoBehaviour
{
	public static MinigameManager Instance;
	private GameManager gm;
	[SerializeField] private MinigameControls player;
	[SerializeField] private Transform spawnHolder;
	[SerializeField] private Transform spawnPos;
	[SerializeField] private TextMeshProUGUI timerTxt;
	private int nPlayers;

	
	[Space] [Header("Specific Rules")]
	[SerializeField] private int timer=30;
	Coroutine countdownCo;
	[SerializeField] private bool lastManStanding=true;
	[SerializeField] private bool playersCanMove=true;
	[SerializeField] private bool playersCanJump;

	
	[Space] [Header("Results")]
	[SerializeField] private int[] rewards;
	int nOut;



	private void Awake() 
	{
		Instance = this;
	}	

	private void Start() 
	{
		gm = GameManager.Instance;
		if (gm != null)
		{
			nPlayers = gm.nPlayers;
			if (PreviewManager.Instance == null)
				gm.TriggerTransition(false);
		}
		for (int i=0 ; i<nPlayers ; i++)
		{
			/* Distance around the circle */  
			float radians = 2 * Mathf.PI / nPlayers * i;
			
			/* Get the vector direction */ 
			float vertical = Mathf.Sin(radians);
			float horizontal = Mathf.Cos(radians); 
			
			Vector3 spawnDir = new Vector3 (horizontal, 0, vertical);
			
			/* Get the spawn position */ 
			Vector3 spawnP = spawnPos.position + spawnDir * 3; // Radius is just the distance away from the point
			
			/* Now spawn */
			
			/* Rotate the enemy to face towards player */
			var obj = Instantiate(player, spawnP, Quaternion.identity, spawnHolder);
			obj.transform.LookAt(spawnPos);
			obj.SetModel(i);
			obj.SetId(i);
			obj.canMove = playersCanMove;
			obj.canJump = playersCanJump;
		}
		rewards = new int[nPlayers];
		for (int i=0 ; i<rewards.Length ; i++)
			rewards[i] = -1;

		timerTxt.text = $"{timer}";
		countdownCo = StartCoroutine( CountdownCo() );
		if (PreviewManager.Instance != null)
			PreviewManager.Instance.TriggerTransition(false);
	}

	IEnumerator CountdownCo()
	{
		yield return new WaitForSeconds(1);
		timerTxt.text = $"{--timer}";

		if (timer > 0)
			StartCoroutine( CountdownCo() );
		// game over
		else
			GameOver();
	} 

	bool gameFin;
	public void PlayerEliminated(int id)
	{
		if (id < rewards.Length)
			rewards[id] = gm.GetPrizeValue(nOut++);
		if (lastManStanding && nOut == nPlayers - 1)
		{
			StopCoroutine( countdownCo );
			GameOver();
		}
	}
	private void GameOver()
	{
		if (!gameFin)
		{
			gameFin = true;
			StartCoroutine( MinigameOverCo() );
		}
	}
	private IEnumerator MinigameOverCo()
	{
		// practice
		if (PreviewManager.Instance != null)
		{
			yield return new WaitForSeconds(0.5f);
			PreviewManager.Instance.TriggerTransition(true);

			yield return new WaitForSeconds(0.5f);
			gm.ReloadPreviewMinigame();
		}
		// real
		else
		{
			for (int i=0 ; i<rewards.Length ; i++)
				if (rewards[i] == -1)
					rewards[i] = gm.GetPrizeValue(nPlayers - 1);
			string d = "Prizes: ";
			for (int i=0 ; i<rewards.Length ; i++)
				d += $"{rewards[i]} ";
			Debug.Log(d);

			yield return new WaitForSeconds(0.5f);
			gm.TriggerTransition(true);
			gm.AwardMinigamePrize(rewards);

			yield return new WaitForSeconds(0.5f);
			gm.ReturnToBoard("");
		}
	}
}

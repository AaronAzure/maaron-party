using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MingameManager : MonoBehaviour
{
	public static MingameManager Instance;
	private GameManager gm;
	[SerializeField] private MinigameControls player;
	[SerializeField] private Transform spawnPos;
	[SerializeField] private TextMeshProUGUI timerTxt;
	private int nPlayers;

	
	[Space] [Header("Specific Rules")]
	[SerializeField] private int timer=30;
	[SerializeField] private bool playersCanMove=true;
	[SerializeField] private bool playersCanJump;


	private void Awake() 
	{
		Instance = this;
	}	

	private void Start() 
	{
		gm = GameManager.Instance;
		nPlayers = gm.nPlayers;
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
			var obj = Instantiate(player, spawnP, Quaternion.identity);
			obj.transform.LookAt(spawnPos);
			obj.SetModel(i);
			obj.canMove = playersCanMove;
			obj.canJump = playersCanJump;
		}

		timerTxt.text = $"{timer}";
		StartCoroutine( CountdownCo() );
	}

	IEnumerator CountdownCo()
	{
		yield return new WaitForSeconds(1);
		timerTxt.text = $"{--timer}";

		if (timer > 0)
			StartCoroutine( CountdownCo() );
		// game over
		else
			MinigameOver();
	} 

	private void MinigameOver()
	{

		gm.ReturnToBoard("");
	}
}

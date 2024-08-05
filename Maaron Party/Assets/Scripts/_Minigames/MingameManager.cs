using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MingameManager : MonoBehaviour
{
	public static MingameManager Instance;
	[SerializeField] private bool playersCanMove=true;
	[SerializeField] private bool playersCanJump;
	[SerializeField] private MinigameControls player;
	[SerializeField] private Transform spawnPos;
	private int nPlayers;

	private void Awake() 
	{
		Instance = this;
	}	

	private void Start() 
	{
		nPlayers = GameManager.Instance.nPlayers;
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
	}
}

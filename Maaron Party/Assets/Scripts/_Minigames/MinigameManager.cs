using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mirror;

public class MinigameManager : NetworkBehaviour
{
	public static MinigameManager Instance;
	private GameNetworkManager nm {get {return GameNetworkManager.Instance;}}
	private GameManager gm {get {return GameManager.Instance;}}
	private MinigameControls _player {get {return MinigameControls.Instance;}}
	//[SerializeField] private NetworkObject playerToSpawn;
	[SyncVar] public int nBmReady; 
	[SerializeField] private Transform spawnHolder;
	[SerializeField] private Transform spawnPos;
	[SerializeField] private TextMeshProUGUI timerTxt;

	
	[Space] [Header("Specific Rules")]
	[SerializeField] private int timer=30;
	Coroutine countdownCo;
	[SerializeField] private bool lastManStanding=true;
	[SerializeField] private bool playersCanMove=true;
	[SerializeField] private bool playersCanJump;

	
	[Space] [Header("Results")]
	[SerializeField] private int[] rewards;
	int nOut;
	int nPlayers=4;



	private void Awake() 
	{
		Instance = this;
	}	

	private void Start() 
	{
		if (gm != null)
		{
			//nPlayers = gm.nPlayers.Value;
			//if (PreviewManager.Instance == null)
			//	gm.TriggerTransitionServerRpc(false);
		}
		
		// Spawn players
		//SpawnPlayerServerRpc((int) NetworkManager.Singleton.LocalClientId);
		rewards = new int[nPlayers];
		for (int i=0 ; i<rewards.Length ; i++)
			rewards[i] = -1;

		timerTxt.text = $"{timer}";
		countdownCo = StartCoroutine( CountdownCo() );
		if (PreviewManager.Instance != null)
			PreviewManager.Instance.CmdTriggerTransition(false);
		CmdReadyUp();
	}

	[Command(requiresAuthority=false)] public void CmdReadyUp()
	{
		++nBmReady;
		Debug.Log($"<color=white>{nBmReady} >= {nm.numPlayers}</color>");
		if (nBmReady >= nm.numPlayers)
			RpcSetUpPlayer();
	} 
	[ClientRpc] private void RpcSetUpPlayer()
	{
		Debug.Log($"<color=white>Setting Up</color>");
		_player.SetSpawn();
		//if (isServer)
		//	StartCoroutine( StartGameCo() );
	}

	public Vector3 GetPlayerSpawn(int id)
	{
		/* Distance around the circle */  
		float radians = Mathf.PI + (2 * Mathf.PI / nPlayers * id);
		
		/* Get the vector direction */ 
		float vertical = Mathf.Sin(radians);
		float horizontal = Mathf.Cos(radians); 
		
		Vector3 spawnDir = new Vector3 (horizontal, 0, vertical);
		
		/* Get the spawn position */ 
		Vector3 spawnP = spawnPos.position + spawnDir * 3; // Radius is just the distance away from the point
		return spawnP;

		//var networkObject = NetworkManager.SpawnManager.InstantiateAndSpawn(
		//	playerToSpawn, (ulong) clientId, position:spawnP, destroyWithScene:true);
		
	}
	[Command(requiresAuthority=false)] public void CmdSpawnPlayer(int clientId)
	{
		/* Distance around the circle */  
		float radians = 180 + (2 * Mathf.PI / nPlayers * clientId);
		
		/* Get the vector direction */ 
		float vertical = Mathf.Sin(radians);
		float horizontal = Mathf.Cos(radians); 
		
		Vector3 spawnDir = new Vector3 (horizontal, 0, vertical);
		
		/* Get the spawn position */ 
		Vector3 spawnP = spawnPos.position + spawnDir * 3; // Radius is just the distance away from the point

		//var networkObject = NetworkManager.SpawnManager.InstantiateAndSpawn(
		//	playerToSpawn, (ulong) clientId, position:spawnP, destroyWithScene:true);
		RpcInitPlayer();

		//MinigameControls obj = networkObject.GetComponent<MinigameControls>();
	}
	[ClientRpc] private void RpcInitPlayer()
	{ 
		//_player.transform.LookAt(spawnPos);
		_player.canMove = playersCanMove;
		_player.canJump = playersCanJump;
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
	[Command(requiresAuthority=false)] public void CmdPlayerEliminated(int id)
	{
		if (id < rewards.Length)
			rewards[id] = gm.GetPrizeValue(nOut++);
		RpcPlayerEliminated((ulong) id);
		if (lastManStanding && nOut == nPlayers - 1)
		{
			StopCoroutine( countdownCo );
			GameOver();
		}
	}
	[ClientRpc] private void RpcPlayerEliminated(ulong id)
	{
		_player.RpcDeath(id);
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
			PreviewManager.Instance.CmdTriggerTransition(true);

			yield return new WaitForSeconds(0.5f);
			gm.CmdReloadPreviewMinigame();
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
			//gm.TriggerTransition(true);
			gm.AwardMinigamePrize(rewards);

			yield return new WaitForSeconds(0.5f);
			gm.ReturnToBoard("");
		}
	}
}

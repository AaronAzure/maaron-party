using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mirror;

public class MinigameManager : NetworkBehaviour
{
	public static MinigameManager Instance;
	private PreviewManager pm {get {return PreviewManager.Instance;}}
	private GameNetworkManager nm {get {return GameNetworkManager.Instance;}}
	private GameManager gm {get {return GameManager.Instance;}}
	private MinigameControls _player {get {return MinigameControls.Instance;}}
	//[SerializeField] private NetworkObject playerToSpawn;
	[SyncVar] public int nBmReady; 
	[SerializeField] private TextMeshProUGUI timerTxt;
	[SerializeField] private TextMeshProUGUI countDownTxt;
	private int countDownTimer=-1;
	
	[Space] [SerializeField] private Transform spawnPos;

	[Space] [SerializeField] private bool spawnInLine;
	[SerializeField] private Transform spawnPosA;
	[SerializeField] private Transform spawnPosB;

	
	[Space] [Header("Specific Rules")]
	[SerializeField] private MinigameController ctr;
	[SerializeField] private int timer=30;
	Coroutine countdownCo;
	[SerializeField] private bool lastManStanding=true;
	[SerializeField] private bool playersCanMove=true;
	[SerializeField] private bool playersCanJump;
	[SerializeField] private bool playersCanKb;

	
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
		//nPlayers = GameObject.FindGameObjectsWithTag("Player").Length;
		nPlayers = nm.GetNumMinigamePlayers();
		if (isServer)
		{
			rewards = new int[nPlayers];
			for (int i=0 ; i<rewards.Length ; i++)
				rewards[i] = -1;
		}
		if (ctr != null)
			ctr.enabled = false;

		timerTxt.text = $"{timer}";
		CmdReadyUp();
	}

	[Command(requiresAuthority=false)] public void CmdReadyUp()
	{
		++nBmReady;
		Debug.Log($"<color=white>{nBmReady} >= {nm.numPlayers}</color>");
		if (nBmReady >= nm.numPlayers)
		{
			pm.CmdSetup();
			RpcSetUpPlayer();
			StartCoroutine(CountDownCo());
		}
	} 
	[ClientRpc] private void RpcSetUpPlayer()
	{
		//!Debug.Log($"<color=white>Setting Up</color>");
		_player.canMove = playersCanMove;
		_player.canJump = playersCanJump;
		_player.canKb = playersCanKb;
		_player.SetSpawn();
		if (pm != null && pm.gameObject.activeInHierarchy)
		{
			_player.StartGame();
		}
		else
		{
			countDownTimer = 3;
			countDownTxt.text = $"{3}";
		}
		countdownCo = StartCoroutine( GameTimerCo() );
		if (pm != null && pm.gameObject.activeInHierarchy)
			pm.CmdTriggerTransition(false);
	}

	public Vector3 GetPlayerSpawn(int id)
	{
		if (spawnInLine)
		{
			/* Distance around the circle */  
			if (nPlayers == 2)
			{
				float dist = (id + 1f) / 3f;
				Vector3 diff = spawnPosB.position - spawnPosA.position;
				return spawnPosA.position + diff * dist;
			}
			else
			{
				float dist = (float)id / (float)(nPlayers-1);
				Vector3 diff = spawnPosB.position - spawnPosA.position;
				return spawnPosA.position + diff * dist;
			}
		}
		// in a circle
		else
		{
			/* Distance around the circle */  
			float radians = 2 * Mathf.PI * ((float)id / (float)nPlayers);
			
			/* Get the vector direction */ 
			float vertical = Mathf.Sin(radians);
			float horizontal = Mathf.Cos(radians); 
			
			Vector3 spawnDir = new Vector3 (horizontal, 0, vertical);
			
			/* Get the spawn position */ 
			Vector3 spawnP = spawnPos.position + spawnDir * 3; // Radius is just the distance away from the point
			return spawnP;
		}
	}
	IEnumerator CountDownCo()
	{
		yield return new WaitForSeconds(pm != null && !pm.gameObject.activeInHierarchy ? 4 : 1);
		ctr.enabled = true;
	}



	IEnumerator GameTimerCo(bool gameStart=false)
	{
		yield return new WaitForSeconds(1);
		if (gameStart)
			_player.StartGame();
		if (pm != null && !pm.gameObject.activeInHierarchy && countDownTimer >= 0)
		{
			--countDownTimer;
			countDownTxt.text = countDownTimer >= 0 ? countDownTimer == 0 ? "Start!" : $"{countDownTimer}" : "";
			if (countDownTimer > 0)
				StartCoroutine( GameTimerCo() );
			else
				StartCoroutine( GameTimerCo(true) );
		}
		else
		{
			timerTxt.text = $"{--timer}";

			if (timer > 0)
				StartCoroutine( GameTimerCo() );
			// game over
			else if (isServer)
				GameOver();
		}
	} 

	bool gameFin;
	[Command(requiresAuthority=false)] public void CmdPlayerEliminated(int id)
	{
		if (id < rewards.Length)
			rewards[id] = gm.GetPrizeValue(nOut++);
		//RpcPlayerEliminated((ulong) id);
		if (lastManStanding && nOut >= nPlayers - 1)
		{
			StopCoroutine( countdownCo );
			GameOver();
		}
	}
	private void GameOver()
	{
		if (!gameFin)
		{
			if (ctr != null)
				ctr.enabled = false;
			gameFin = true;
			_player.EndGame();
			StartCoroutine( MinigameOverCo() );
		}
	}
	private IEnumerator MinigameOverCo()
	{
		// practice
		if (pm != null && pm.gameObject.activeInHierarchy)
		{
			yield return new WaitForSeconds(0.5f);
			pm.CmdTriggerTransition(true);

			yield return new WaitForSeconds(0.5f);
			//gm.CmdReloadPreviewMinigame();
			nm.ReloadPreviewMinigame();
		}
		// real
		else if (isServer)
		{
			for (int i=0 ; i<rewards.Length ; i++)
				if (rewards[i] == -1)
					rewards[i] = gm.GetPrizeValue(nPlayers - 1);
			string d = "Prizes: ";
			for (int i=0 ; i<rewards.Length ; i++)
				d += $"{rewards[i]} ";
			Debug.Log(d);

			// show reward
			yield return new WaitForSeconds(3);


			yield return new WaitForSeconds(0.5f);
			//gm.CmdTriggerTransition(true);
			gm.AwardMinigamePrize(rewards);

			yield return new WaitForSeconds(0.5f);
			nm.StartBoardGame();
			//gm.ReturnToBoard("");
		}
	}
}

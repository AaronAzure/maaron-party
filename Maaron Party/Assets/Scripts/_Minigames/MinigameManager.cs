using System.Collections;
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
	[SerializeField] private Transform mainCam;

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
	[SerializeField] private RewardUi[] rewardUis;
	int nOut;



	private void Awake() 
	{
		Instance = this;
	}	

	public int GetNumPlayers() => GameObject.FindGameObjectsWithTag("Player").Length;

	private void Start() 
	{
		if (isServer)
		{
			rewards = new int[GetNumPlayers()];
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
		if (nBmReady >= nm.numPlayers)
		{
			if (pm != null && pm.gameObject.activeInHierarchy)
				pm.CmdSetup();
			RpcSetUpPlayer();
			StartCoroutine(CountDownCo());
		}
	} 
	[ClientRpc] private void RpcSetUpPlayer()
	{
		_player.canMove = playersCanMove;
		_player.canJump = playersCanJump;
		_player.canKb = playersCanKb;
		_player.SetSpawn();
		// practice game
		if (pm != null && pm.gameObject.activeInHierarchy)
		{
			if (mainCam != null)
				pm.MatchPreviewCamera(mainCam.rotation);
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
			if (GetNumPlayers() == 2)
			{
				float dist = (id + 1f) / 3f;
				Vector3 diff = spawnPosB.position - spawnPosA.position;
				return spawnPosA.position + diff * dist;
			}
			else
			{
				float dist = (float)id / (float)(GetNumPlayers()-1);
				Vector3 diff = spawnPosB.position - spawnPosA.position;
				return spawnPosA.position + diff * dist;
			}
		}
		// in a circle
		else
		{
			/* Distance around the circle */  
			float radians = 2 * Mathf.PI * ((float)id / (float)GetNumPlayers());
			
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
				countdownCo = StartCoroutine( GameTimerCo() );
			else
				countdownCo = StartCoroutine( GameTimerCo(true) );
		}
		else
		{
			timerTxt.text = $"{--timer}";

			if (timer > 0)
				countdownCo = StartCoroutine( GameTimerCo() );
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
		if (lastManStanding && nOut >= GetNumPlayers() - 1)
		{
			if (countdownCo != null) StopCoroutine( countdownCo );
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
			if (countdownCo != null) StopCoroutine( countdownCo );
			for (int i=0 ; i<rewards.Length ; i++)
				if (rewards[i] == -1)
					rewards[i] = gm.GetPrizeValue(GetNumPlayers() - 1);
			string d = "Prizes: ";
			for (int i=0 ; i<rewards.Length ; i++)
				d += $"{rewards[i]} ";
			Debug.Log(d);
			CmdChangeText("Game!");

			yield return new WaitForSeconds(1f);
			CmdChangeText("");
			for (int i=0 ; i<rewardUis.Length && i<GetNumPlayers() ; i++)
			{
				int[] details = nm.GetMinigamePlayerInfo(i);
				CmdShowRewards(i, details[0], details[1], details[2], details[3], details[4]);
				CmdShowPrizeText(i, rewards[i]);
			}

			// show reward
			yield return new WaitForSeconds(2f);
			gm.AwardMinigamePrize(rewards);
			for (int i=0 ; i<rewardUis.Length && i<GetNumPlayers() ; i++)
			{
				int[] details = nm.GetMinigamePlayerInfo(i);
				CmdShowRewards(i, details[0], details[1], details[2], details[3], details[4]);
				CmdShowPrizeText(i, 0);
			}

			yield return new WaitForSeconds(1f);
			nm.StartBoardGame();
		}
	}


	[Command(requiresAuthority=false)] void CmdChangeText(string newTxt) => RpcChangeText(newTxt);
	[ClientRpc] void RpcChangeText(string newTxt) => countDownTxt.text = newTxt;

	[Command(requiresAuthority=false)] void CmdShowPrizeText(int ind, int prize) => RpcShowPrizeText(ind, prize);
	[ClientRpc] void RpcShowPrizeText(int ind, int prize) => rewardUis[ind].ShowPrize(prize);

	[Command(requiresAuthority=false)] void CmdShowRewards(int ind,
		int characterInd, int order, int coins, int stars, int manas)
	{
		RpcShowRewards(ind, characterInd, order, coins, stars, manas);
	}
	[ClientRpc] void RpcShowRewards(int ind,
		int characterInd, int order, int coins, int stars, int manas)
	{
		if (ind >= 0 && ind < rewardUis.Length && rewardUis[ind] != null)
		{
			rewardUis[ind].gameObject.SetActive(true);
			rewardUis[ind].SetUp(characterInd, order, coins, stars, manas);
		}
		else
		{
			Debug.LogError($"reward Uis ind={ind} is missing");
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using Cinemachine;

public class BoardManager : NetworkBehaviour
{
	public static BoardManager Instance;
	[SerializeField] private PlayerControls playerPrefab;
	[SerializeField] private Transform spawnPos;
	[SerializeField] private PlayerControls[] players;
	private PlayerControls _player;
	GameManager gm {get{return GameManager.Instance;}}
	GameNetworkManager nm {get{return GameNetworkManager.Instance;}}


	[Space] [Header("Universal")]
	[SerializeField] private Transform dataUi;
	[SerializeField] private TextMeshProUGUI turnTxt;
	[SerializeField] private GameObject newStockUi;
	[SerializeField] private CinemachineVirtualCamera starCam;
	[SerializeField] private Animator maaronAnim;
	[SerializeField] private ParticleSystem maaronSpotlightPs;
	[SyncVar] public int nBmReady; 
	[SyncVar] public int n;
	bool isChoosingStar;


	[Space] [Header("MUST REFERENCE PER BOARD")]
	[SerializeField] private Node startNode;
	[SerializeField] private starNodes[] starNodes;


	private void Awake() 
	{
		Instance = this;		
	}

	public void SetUiLayout(Transform ui)
	{
		if (ui != null)
		{
			ui.parent = dataUi;
			ui.localScale = Vector3.one;
		}
	}

	private void Start() 
	{
		CmdReadyUp();
		turnTxt.text = $"Turn: {gm.nTurn}/{gm.maxTurns}";
		//string s = $"<color=#FF8D07>";
		//s += $"NetworkServer.connections.Count = {NetworkServer.connections.Count} | ";
		//s += $"GameNetworkManager.Instance.numPlayers = {GameNetworkManager.Instance.numPlayers} | ";
		//s += "</color>";
		//Debug.Log(s);
	}

	[Command(requiresAuthority=false)] public void CmdReadyUp()
	{
		++nBmReady;
		Debug.Log($"<color=white>{nBmReady} >= {nm.numPlayers}</color>");
		if (nBmReady >= nm.numPlayers)
		{
			RpcSetUpPlayer();
			nm.UnparentBoardControls();
		}
	} 
	[ClientRpc] private void RpcSetUpPlayer()
	{
		Debug.Log($"<color=white>Setting Up</color>");
		_player = PlayerControls.Instance;
		if (gm.nTurn == 1)
			_player.SetStartNode(startNode);
		//_player.RemoteStart(spawnPos);
		if (isServer)
			StartCoroutine( StartGameCo() );
	}
	public Transform GetSpawnPos()
	{
		return spawnPos;
	}
	public int GetNth()
	{
		return n++;
	}
	
	IEnumerator StartGameCo()
	{
		//yield return new WaitForSeconds(0.5f);
		//gm.CmdTriggerTransition(false);
		
		yield return new WaitForSeconds(1);
		if (!gm.gameStarted)
		{
			gm.gameStarted = true;
			yield return ChooseStarCo(0);
		}
		// turn 2+
		else
			StartCoroutine( SetupStarNode(gm.prevStarInd) );
		nm.NextBoardPlayerTurn();

		if (gm.nTurn == 5)
		{
			yield return new WaitForSeconds(2);
			Debug.Log("<color=cyan>gm.nTurn == 5</color>");
			CmdNewStock();
		}
		if (gm.nTurn == gm.maxTurns - 5)
		{
			yield return new WaitForSeconds(2);
			Debug.Log("<color=cyan>gm.nTurn == gm.maxTurns - 5</color>");
			CmdNewStock();
		}
	}


	#region Star
	public void ChooseStar() => StartCoroutine( ChooseStarCo() );
	IEnumerator ChooseStarCo(int fixedInd=-1, bool changeLoc=false)
	{
		if (changeLoc)
			CmdSetStarNode(gm.prevStarInd, false);
		int rng = fixedInd == -1 ? Random.Range(0, starNodes.Length) : fixedInd;
		while (rng == gm.prevStarInd || 
			(gm.prevStarInd >= 0 && gm.prevStarInd < starNodes.Length &&
			starNodes[gm.prevStarInd].invalid != null && starNodes[gm.prevStarInd].invalid != starNodes[rng].node))
		{
			rng = Random.Range(0, starNodes.Length);
		}
		gm.prevStarInd = rng;
		starCam.m_Follow = starNodes[rng].node.transform;

		yield return new WaitForSeconds(1f);
		CmdToggleStarCam(true);

		yield return new WaitForSeconds(1f);
		StartCoroutine( SetupStarNode(rng) );
		
		yield return new WaitForSeconds(4);
		CmdToggleStarCam(false);
	}
	[Command(requiresAuthority=false)] void CmdToggleStarCam(bool active) => RpcToggleStarCam(active);
	[ClientRpc] void RpcToggleStarCam(bool active) => starCam.gameObject.SetActive(active);
	private IEnumerator SetupStarNode(int ind)
	{
		CmdSetMaaron(ind, true);
		
		yield return new WaitForSeconds(2);
		CmdSetStarNode(starNodes[ind].node.nodeId, true);
		CmdToggleSpotlight(true);
	}

	// set maaron location
	[Command(requiresAuthority=false)] void CmdSetMaaron(int nodeId, bool isStarSpace) => RpcSetMaaron(nodeId, isStarSpace);
	[ClientRpc] void RpcSetMaaron(int nodeId, bool isStarSpace) 
	{ 
		maaronAnim.gameObject.SetActive(isStarSpace);
		maaronAnim.gameObject.transform.position = starNodes[nodeId].node.maaronPos.position;
		maaronAnim.transform.LookAt(starNodes[nodeId].node.transform.position, Vector3.up);
	}

	// toggle spotlight for maaron
	[Command(requiresAuthority=false)] void CmdToggleSpotlight(bool active) => RpcToggleSpotlight(active);
	[ClientRpc] void RpcToggleSpotlight(bool active) 
	{
		if (active)
			maaronSpotlightPs.Play();
		else
			maaronSpotlightPs.Stop();
	}

	// set node space (star)
	[Command(requiresAuthority=false)] void CmdSetStarNode(int nodeId, bool isStarSpace) => RpcSetStarNode(nodeId, isStarSpace);
	[ClientRpc] void RpcSetStarNode(int nodeId, bool isStarSpace) => NodeManager.Instance.GetNode(nodeId).ToggleStarNode(isStarSpace);

	// new stock text
	[Command(requiresAuthority=false)] void CmdNewStock() => RpcNewStock();
	[ClientRpc] void RpcNewStock() => newStockUi.SetActive(true);
	
	#endregion


	public void NextPlayerTurn()
	{
		CmdNextPlayerTurn(); // calls to server
	}
	[Command(requiresAuthority=false)] private void CmdNextPlayerTurn()
	{
		Debug.Log($"<color=cyan>CmdNextPlayerTurn()</color>");
		nm.NextBoardPlayerTurn();
		//if (!firstTurn)
		//	nPlayerOrder = ++nPlayerOrder;
		//	//nPlayerOrder = ++nPlayerOrder % nPlayers;
		//	players[nPlayerOrder].YourTurn();
		//if (nPlayerOrder.Value >= 0 && nPlayerOrder.Value < players.Length)
		//	gm.CmdNextPlayerTurn((ulong) ++nPlayerOrder.Value);
		//else if (nPlayerOrder.Value >= nPlayers)
		//{
		//	//CmdDisablePlayer();
		//	gm.CmdLoadPreviewMinigame("TestMinigame");
		//}
			//gm.CmdLoadMinigame();
			//LoadMinigame("TestMinigame");
	}
}

[System.Serializable]
public class starNodes
{
	public Node node;
	public Node invalid;
}
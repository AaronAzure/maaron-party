using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class PreviewManager : NetworkBehaviour
{
	public static PreviewManager Instance;
	[SerializeField] private Animator anim;
	private GameManager gm {get{return GameManager.Instance;}}
	private GameNetworkManager nm {get{return GameNetworkManager.Instance;}}
	[SyncVar] public int nReady;
	public Transform readyLayoutHolder;
	[SerializeField] private Button[] readyBtns;
	[SyncVar] bool[] readys;



	private void Awake()
	{
		Debug.Log($"<color=cyan>PreviewManager = OnEnable() {nm.GetNumMinigamePlayers()}</color>");
		Instance = this;
	} 
	//private void OnDisable() => Instance = null;
	

	[Command(requiresAuthority=false)] public void CmdReadyUp()
	{
		++nReady;
		if (nReady > nm.GetNumMinigamePlayers())
			nm.StartActualMiniGame();
	}

	public void Setup() 
	{
		Debug.Log($"<color=cyan>PreviewManager = Setup() {nm.GetNumMinigamePlayers()}</color>");
		CmdSetup();
	}
	[Command(requiresAuthority=false)] public void CmdSetup() 
	{
		nReady = 0;
		Debug.Log($"<color=cyan>PreviewManager = CmdSetup() {nm.GetNumMinigamePlayers()}</color>");
		RpcToggleReadyButton(true);
		for (int i=0 ; i<readyBtns.Length && i<nm.GetNumMinigamePlayers() ; i++)
			RpcSetReadyButton(i);
	}


	[ClientRpc] void RpcSetReadyButton(int i)
	{
		Debug.Log($"<color=cyan>PreviewManager = RpcSetReadyButton({i}) {MinigameControls.Instance.id == i}</color>");
		if (i >= 0 && i < readyBtns.Length && readyBtns[i] != null)
			readyBtns[i].interactable = MinigameControls.Instance.id == i;
	}
	[ClientRpc] void RpcToggleReadyButton(bool active)
	{
		Debug.Log($"<color=cyan>PreviewManager = RpcToggleReadyButton({active}) {nm.GetNumMinigamePlayers()}</color>");
		for (int i=0 ; i<readyBtns.Length && i<nm.GetNumMinigamePlayers() ; i++)
			if (readyBtns[i] != null)
				readyBtns[i].gameObject.SetActive(active);
	}





	[Command(requiresAuthority=false)] public void CmdTriggerTransition(bool fadeIn)
	{
		RpcTriggerTransition(fadeIn);
	}
	[ClientRpc] private void RpcTriggerTransition(bool fadeIn)
	{
		anim.SetTrigger(fadeIn ? "in" : "out");
	}
}

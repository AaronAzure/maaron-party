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
	bool hasSetup;
	bool started;



	private void Awake()
	{
		Debug.Log($"<color=cyan>PreviewManager = OnEnable() {nm.GetNumMinigamePlayers()}</color>");
		Instance = this;
	} 
	private void OnEnable() 
	{
		if (isServer) 
		{
			started = true;
			nm.PreviewManagerLoaded();
		}
	}
	
	private void OnDisable() 
	{
		if (started) nm.PreviewManagerUnLoaded();
		started = hasSetup = false;
	}
	

	[Command(requiresAuthority=false)] public void CmdReadyUp()
	{
		++nReady;
		if (nReady >= nm.GetNumMinigamePlayers())
			nm.StartActualMiniGame();
	}

	[Command(requiresAuthority=false)] public void CmdSetup() 
	{
		Debug.Log($"<color=cyan>PreviewManager = CmdSetup() {nm.GetNumMinigamePlayers()}</color>");
		if (!hasSetup)
		{
			hasSetup = true;
			nReady = 0;
			RpcToggleReadyButton(true);
			for (int i=0 ; i<readyBtns.Length && i<nm.GetNumMinigamePlayers() ; i++)
				RpcSetReadyButton(i);
		}
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

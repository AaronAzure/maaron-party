using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PreviewManager : NetworkBehaviour
{
	public static PreviewManager Instance;
	[SerializeField] private Animator anim;
	private GameManager gm {get{return GameManager.Instance;}}
	private GameNetworkManager nm {get{return GameNetworkManager.Instance;}}
	[SyncVar] public int nReady;
	public Transform readyLayoutHolder;

	private void OnEnable()
	{
		Instance = this;
		nReady = 0;
	} 
	private void OnDisable() => Instance = null;
	

	[Command(requiresAuthority=false)] public void CmdReadyUp()
	{
		++nReady;
		if (nReady > nm.GetNumPreviewPlayers())
			nm.StartActualMiniGame();
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

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
	[SyncVar] public int nManagerReady;

	private void Awake() 
	{
		Instance = this;
	}

	// Start is called before the first frame update
	void Start()
	{
		//gm.CmdTriggerTransition(false);
		CmdReadyUp();
	}

	[Command(requiresAuthority=false)] public void CmdReadyUp()
	{
		++nManagerReady;
		Debug.Log($"<color=white>{nManagerReady} >= {nm.numPlayers}</color>");
		if (nManagerReady >= nm.numPlayers)
		{
			nm.LoadPreviewMinigame();
			gm.CmdTriggerTransition(false);
		}
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

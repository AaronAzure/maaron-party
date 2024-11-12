using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class PreviewManager : NetworkBehaviour
{
	public static PreviewManager Instance;
	[SerializeField] private Animator anim;
	//private GameManager gm {get{return GameManager.Instance;}}
	private GameNetworkManager nm {get{return GameNetworkManager.Instance;}}
	[SyncVar] public int nReady;
	public Transform readyLayoutHolder;
	[SerializeField] private Button[] readyBtns;
	[SerializeField] private PreviewControls[] pcs;
	[SerializeField] private Transform previewCam;
	bool hasSetup;
	bool started;



	private void Awake()
	{
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
		nReady = 0;
	}

	public void MatchPreviewCamera(Quaternion rot)
	{
		previewCam.rotation = rot;
	}
	

	[Command(requiresAuthority=false)] public void CmdCheckReady()
	{
		if (nReady >= nm.GetNumMinigamePlayers())
			nm.StartActualMiniGame();
	}
	[Command(requiresAuthority=false)] public void CmdReadyUp()
	{
		++nReady;
		if (nReady >= nm.GetNumMinigamePlayers())
			nm.StartActualMiniGame();
	}

	[Command(requiresAuthority=false)] public void CmdSetup() 
	{
		if (!hasSetup)
		{
			hasSetup = true;
			nReady = 0;
			RpcToggleReadyButton(true, nm.GetNumMinigamePlayers());
			for (int i=0 ; i<readyBtns.Length && i<nm.GetNumMinigamePlayers() ; i++)
				RpcSetReadyButton(i);
		}
	}


	#region Ready Btn
	[ClientRpc] void RpcSetReadyButton(int i)
	{
		if (i >= 0 && i < readyBtns.Length && readyBtns[i] != null)
			readyBtns[i].interactable = MinigameControls.Instance.id == i;
	}
	[ClientRpc] void RpcToggleReadyButton(bool active, int n)
	{
		for (int i=0 ; i<readyBtns.Length && i<n ; i++)
			if (readyBtns[i] != null)
				readyBtns[i].gameObject.SetActive(active);
	}
	#endregion



	#region Profiles
	public void SetProfilePic(int[] inds)
	{
		for (int i=0 ; i<pcs.Length && i<inds.Length ; i++)
			if (pcs[i] != null)
				pcs[i].Setup(inds[i]);
	}
	#endregion



	#region Transition
	[Command(requiresAuthority=false)] public void CmdTriggerTransition(bool fadeIn)
	{
		RpcTriggerTransition(fadeIn);
	}
	[ClientRpc] private void RpcTriggerTransition(bool fadeIn)
	{
		anim.SetTrigger(fadeIn ? "in" : "out");
	}
	#endregion
}

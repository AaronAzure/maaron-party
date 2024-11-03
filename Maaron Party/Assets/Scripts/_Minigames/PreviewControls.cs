using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PreviewControls : NetworkBehaviour
{
	public static PreviewControls Instance;

	private GameNetworkManager nm { get { return GameNetworkManager.Instance; } }
	public Button btn;
	[SerializeField] private GameObject readyObj;
	[SerializeField] private GameObject[] profilePics;
	[SerializeField] private Image profileBg;

	private void OnDisable() 
	{
		readyObj.SetActive(false);
	}

	public void Setup(int ind)
	{
		Debug.Log($"<color=cyan>PreviewControls = Setup({ind})</color>");
		if (profilePics != null) profilePics[ind].SetActive(true);
		//if (profileBg != null) profileBg.color = ind == 0 ? new Color(0.7f,0.13f,0.13f) : ind == 1 ? new Color(0.4f,0.7f,0.3f) 
		//		: ind == 2 ? new Color(0.85f,0.85f,0.5f) : new Color(0.7f,0.5f,0.8f);
	}

	public void _READY_UP()
	{
		CmdReady();
	}
	[Command(requiresAuthority=false)] void CmdReady() 
	{
		PreviewManager.Instance.CmdReadyUp();
		RpcReady();
	}
	[ClientRpc] void RpcReady()
	{
		btn.interactable = false;
		readyObj.SetActive(true);
	}
	[Command(requiresAuthority=false)] public void CmdUnparent() => RpcUnparent();
	[ClientRpc] void RpcUnparent() => transform.parent = MinigameManager.Instance.transform;
}

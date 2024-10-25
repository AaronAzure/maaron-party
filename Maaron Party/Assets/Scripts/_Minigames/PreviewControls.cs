using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PreviewControls : NetworkBehaviour
{
	public static PreviewControls Instance;

	private GameNetworkManager nm { get { return GameNetworkManager.Instance; } }
	public Button btn;
	[SyncVar] public int characterInd=-1;
	[SyncVar] public int id=-1;
	[SerializeField] private GameObject readyObj;

	//public override void OnStartClient()
	//{
	//	base.OnStartClient();
	//	btn.interactable = isOwned;
	//	if (isOwned)
	//		Instance = this;	
	//	nm.AddPreviewConnection(this);
	//}
	//public override void OnStopClient()
	//{
	//	//Debug.Log($"<color=#FF9900>PLAYER DISCONNECT ({isOwned}) | {isServer} | {yourTurn}</color>");
	//	base.OnStopClient();
	//	if (isOwned)
	//		nm.RemovePreviewConnection(this);
	//	// if disconnect and not ready
	//	//if (isServer && yourTurn)
	//	//	bm.NextPlayerTurn();
	//}

	//private void Start() 
	//{
	//	if (PreviewManager.Instance != null)
	//	{
	//		transform.parent = PreviewManager.Instance.readyLayoutHolder;
	//		transform.localScale = Vector3.one;
	//	}
	//	name = $"__ {id} __";
	//}

	public void _READY_UP()
	{
		CmdReady();
	}
	[Command(requiresAuthority=false)] void CmdReady() => RpcReady();
	[ClientRpc] void RpcReady()
	{
		btn.interactable = false;
		readyObj.SetActive(true);
		PreviewManager.Instance.CmdReadyUp();
	}
	[Command(requiresAuthority=false)] public void CmdUnparent() => RpcUnparent();
	[ClientRpc] void RpcUnparent() => transform.parent = MinigameManager.Instance.transform;
}

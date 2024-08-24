using UnityEngine;
using Mirror;

public class TrickyTile : NetworkBehaviour
{
	[SerializeField] private MeshRenderer mesh;
	[SerializeField] private Material[] mats;
	[SerializeField] private Material emptyMat;


	[Command(requiresAuthority=false)] public void CmdSetMaterial(int matInd) 
	{
		//Debug.Log("<color=yellow>CmdSetMaterial</color>");
		RpcSetMaterial(matInd);
	}
	[ClientRpc] private void RpcSetMaterial(int matInd) 
	{
		//Debug.Log("<color=cyan>RpcSetMaterial</color>");
		mesh.material = mats[matInd];
	}
	[Command(requiresAuthority=false)] public void CmdClearMaterial() 
	{
		//Debug.Log("<color=yellow>CmdClearMaterial</color>");
		RpcClearMaterial();
	}
	[ClientRpc] private void RpcClearMaterial() 
	{
		//Debug.Log("<color=cyan>RpcClearMaterial</color>");
		mesh.material = emptyMat;
	}
}

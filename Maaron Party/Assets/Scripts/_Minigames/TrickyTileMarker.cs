using UnityEngine;
using Mirror;

public class TrickyTileMarker : NetworkBehaviour
{
    [SerializeField] private MeshRenderer mesh;
	[SerializeField] private Material[] mats;
	[SerializeField] private Material emptyMat;


	[Command(requiresAuthority=false)] public void CmdSetMaterial(int matInd) => RpcSetMaterial(matInd);
	[ClientRpc] private void RpcSetMaterial(int matInd) => mesh.material = mats[matInd];
	[Command(requiresAuthority=false)] public void CmdClearMaterial() => RpcClearMaterial();
	[ClientRpc] private void RpcClearMaterial() => mesh.material = emptyMat;
}

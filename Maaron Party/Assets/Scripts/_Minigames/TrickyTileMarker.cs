using UnityEngine;
using Unity.Netcode;

public class TrickyTileMarker : NetworkBehaviour
{
    [SerializeField] private MeshRenderer mesh;
	[SerializeField] private Material[] mats;
	[SerializeField] private Material emptyMat;


	[ServerRpc] public void SetMaterialServerRpc(int matInd) => SetMaterialClientRpc(matInd);
	[ClientRpc] private void SetMaterialClientRpc(int matInd) => mesh.material = mats[matInd];
	[ServerRpc] public void ClearMaterialServerRpc() => ClearMaterialClientRpc();
	[ClientRpc] private void ClearMaterialClientRpc() => mesh.material = emptyMat;
}

using UnityEngine;
using Unity.Netcode;

public class TrickyTile : NetworkBehaviour
{
	[SerializeField] private Animator anim;


	[ServerRpc] public void TriggerTrapServerRpc() => TriggerTrapClientRpc();
	[ClientRpc] private void TriggerTrapClientRpc() => anim.SetTrigger("open");
}

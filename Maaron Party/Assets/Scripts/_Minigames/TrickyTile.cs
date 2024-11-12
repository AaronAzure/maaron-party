using UnityEngine;
using Mirror;

public class TrickyTile : NetworkBehaviour
{
	[SerializeField] private Animator anim;


	[Command(requiresAuthority=false)] public void CmdTriggerTrap() => RpcTriggerTrap();
	[ClientRpc] private void RpcTriggerTrap() => anim.SetTrigger("open");
}

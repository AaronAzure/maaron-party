using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class PreviewManager : NetworkBehaviour
{
	public static PreviewManager Instance;
	[SerializeField] private Animator anim;
	private GameManager gm;

	private void Awake() 
	{
		Instance = this;
	}

	// Start is called before the first frame update
	void Start()
	{
		gm = GameManager.Instance;
		gm.TriggerTransition(false);
	}

	[ServerRpc(RequireOwnership=false)] public void TriggerTransitionServerRpc(bool fadeIn)
	{
		//TriggerTransitionClientRpc(fadeIn);
	}
	//[ClientRpc(RequireOwnership=false)] private void TriggerTransitionClientRpc(bool fadeIn)
	//{
	//	anim.SetTrigger(fadeIn ? "in" : "out");
	//}
}

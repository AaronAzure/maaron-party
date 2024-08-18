using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class LobbyObject : NetworkBehaviour
{
	#region Variables
	public static LobbyObject Instance;
	private GameManager gm;
	[SerializeField] private GameObject buttons;
	[SerializeField] private TextMeshProUGUI characterTxt;
	[SerializeField] private int maxCharacters=4;
	[SerializeField] private Image pfp;
	//public NetworkVariable<int> characterInd = new NetworkVariable<int>(
	//	0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	#endregion


	//public override void OnNetworkSpawn()
	//{
	//	characterInd.OnValueChanged += (int prevInd, int newInd) => {
	//		ChangeName(newInd);
	//	};
	//}
	//public override void OnNetworkDespawn()
	//{
	//	if (gm != null && IsOwner)
	//		gm.LeftGameServerRpc(OwnerClientId);
	//}

	private void ChangeName(int ind)
	{
		gameObject.name = $"__ PLAYER {ind} __";
		switch (ind)
		{
			case 0: 
				characterTxt.text = "Green";
				break;
			case 1: 
				characterTxt.text = "Orange";
				break;
			case 2: 
				characterTxt.text = "Pink";
				break;
			case 3: 
				characterTxt.text = "Blue";
				break;
		}
		pfp.color = ind == 0 ? new Color(0,1,0) : ind == 1 ? new Color(1,0.6f,0) 
				: ind == 2 ? new Color(1,0.5f,0.8f) : Color.blue;
	}

	private void Start() 
	{
		gm = GameManager.Instance;
		transform.SetParent(gm.spawnHolder, true);
		transform.localScale = Vector3.one;

		//if (IsOwner)
		//{
		//	Instance = this;
		//	Debug.Log("INSTANCE CREATED " + name );
		//	//Debug.Log($"=>  {OwnerClientId}");
		//	buttons.SetActive(true);
		//	gm.JoinGameServerRpc(OwnerClientId);
		//	characterInd.Value = (int) OwnerClientId;
		//}
		//ChangeName(characterInd.Value);
	}

	[Command(requiresAuthority=false)] public void CmdSendPlayerModel()
	{
		//Debug.Log($"<color=blue>SendPlayerModelServerRpc</color>");
		//gm.SetPlayerModelServerRpc(characterInd.Value);
	}

	public void CHARACTER_IND_INC()
	{
		//characterInd.Value = (characterInd.Value + 1) % maxCharacters;
	}
	public void CHARACTER_IND_DEC()
	{
		//characterInd.Value = characterInd.Value == 0 ? maxCharacters - 1 : characterInd.Value - 1;
	}
}

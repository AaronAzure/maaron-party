using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class LobbyObject : NetworkBehaviour
{
	[SerializeField] private GameObject buttons;
	[SerializeField] private TextMeshProUGUI characterTxt;
	[SerializeField] private int maxCharacters=4;
	public NetworkVariable<int> characterInd = new NetworkVariable<int>(
		0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


	public override void OnNetworkSpawn()
	{
		characterInd.OnValueChanged += (int prevInd, int newInd) => {
			ChangeName(newInd);
		};
	}

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
	}

	private void Start() 
	{
		transform.SetParent(LobbyManager.Instance.spawnHolder, true);
		transform.localScale = Vector3.one;
		ChangeName((int) OwnerClientId);
		if (IsOwner)
		{
			buttons.SetActive(true);
		}
	}

	public void CHARACTER_IND_INC()
	{
		characterInd.Value = (characterInd.Value + 1) % maxCharacters;
	}
	public void CHARACTER_IND_DEC()
	{
		characterInd.Value = characterInd.Value == 0 ? maxCharacters - 1 : characterInd.Value - 1;
	}
}

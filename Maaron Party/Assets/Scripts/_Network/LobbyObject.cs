using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FishNet.Object;
using TMPro;
using FishNet.Object.Synchronizing;
using FishNet.Connection;

public class LobbyObject : NetworkBehaviour
{
	public static LobbyObject Instance;
	private GameManager gm;
	[SerializeField] private GameObject buttons;
	[SerializeField] private TextMeshProUGUI characterTxt;
	[SerializeField] private int maxCharacters=4;
	[SerializeField] private Image pfp;
	[SerializeField] private Image bg;
	[SerializeField] private readonly SyncVar<int> characterInd = new SyncVar<int>(-1);


	public override void OnStartClient()
	{
		base.OnStartClient();
		characterInd.OnChange += ChangeName;
		characterInd.Value = OwnerId;
		//ChangeName(characterInd.Value);
		name = $"__ PLAYER {OwnerId} __";
		Debug.Log($"==> {name} JOINED!!");
		gm = GameManager.Instance;

		if (IsOwner)
		{
			Instance = this;
			buttons.SetActive(true);
			bg.color = new Color(0.25f, 0.25f, 0.25f, 0.7843f);
			StartCoroutine(ReparentUiCo());
		}
		else
		{
			this.transform.SetParent(gm.spawnHolder, true);
			this.transform.localScale = Vector3.one;
			this.enabled = false;
		}
	}

	public override void OnStopClient()
	{
		base.OnStopClient();
		Debug.Log("<color=red>DISCONNECTED</color>");
	}

	IEnumerator ReparentUiCo()
	{
		yield return null;
		gm = GameManager.Instance;
		this.transform.SetParent(gm.spawnHolder, true);
		this.transform.localScale = Vector3.one;
	}

	private void ChangeName(int prev, int next, bool asServer)
	{
		gameObject.name = $"__ PLAYER {next} __";
		switch (next)
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
		pfp.color = next == 0 ? new Color(0,1,0) : next == 1 ? new Color(1,0.6f,0) 
				: next == 2 ? new Color(1,0.5f,0.8f) : Color.blue;
	}

	[ServerRpc(RequireOwnership=false)] public void SendPlayerModelServerRpc()
	{
		Debug.Log($"<color=magenta>SendPlayerModelServerRpc {OwnerId}</color>");
		//gm.SetPlayerModelServerRpc(characterInd.Value);
	}
	[TargetRpc] public void SendPlayerModel(NetworkConnection conn, int newAmmo)
	{
		
		//Debug.Log($"<color=blue>SendPlayerModelServerRpc</color>");
		//gm.SetPlayerModelServerRpc(characterInd.Value);
	}

	public int GetCharacterInd()
	{
		return characterInd.Value;
	}
	public void CHARACTER_IND_INC()
	{
		CharacterIndInc(this);
	}
	[ServerRpc] public void CharacterIndInc(LobbyObject script)
	{
		script.characterInd.Value = (characterInd.Value + 1) % maxCharacters;
	}
	public void CHARACTER_IND_DEC()
	{
		CharacterIndDec(this);
	}
	[ServerRpc] public void CharacterIndDec(LobbyObject script)
	{
		script.characterInd.Value = characterInd.Value == 0 ? maxCharacters - 1 : characterInd.Value - 1;
	}
}

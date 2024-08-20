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
	private GameNetworkManager nm;
	private GameManager gm;
	[SerializeField] private GameObject buttons;
	[SerializeField] private TextMeshProUGUI characterTxt;
	[SerializeField] private int maxCharacters=4;
	[SerializeField] private Image pfp;
	[SerializeField] private Image bg;
	[SerializeField] private GameObject readyUi;
	[SerializeField] private Button readyBtn;
	[SyncVar] public int characterInd=-1;
	[SyncVar] public bool _isReady=false;
	#endregion



	#region Methods
	private void Awake() 
	{
		nm = GameNetworkManager.Instance;
	}
	public override void OnStartClient()
	{
		//Debug.Log($"{name} Joined");
		nm.AddConnection(this);
	}
	public override void OnStopClient()
	{
		base.OnStopClient();
		nm.RemoveConnection(this);
		//Debug.Log($"{name} Left");
	}

	private void Start() 
	{
		gm = GameManager.Instance;
		transform.SetParent(gm.spawnHolder, true);
		transform.localScale = Vector3.one;
		
		readyBtn.interactable = isLocalPlayer;
		if (isLocalPlayer)
		{
			Instance = this;
			Debug.Log($"<color=cyan>INSTANCE CREATED {name} |{netId}|</color>");
			bg.color = new Color(0.25f, 0.25f, 0.25f, 0.7843f);
			//Debug.Log($"=>  {OwnerClientId}");
			buttons.SetActive(true);
			readyBtn.onClick.AddListener(() => {
				_isReady = !_isReady;
				CmdUpdateDisplay(_isReady);
			});
			readyBtn.gameObject.SetActive(true);
			
			//gm.JoinGameServerRpc(OwnerClientId);
			characterInd = (int) netId % maxCharacters;
		}
		CmdUpdateUi(characterInd);
	}

	[Command(requiresAuthority=false)] public void CmdUpdateUi(int ind)
	{
		RpcUpdateUi(ind);
	}

	[ClientRpc] public void RpcUpdateUi(int ind)
	{
		//gameObject.name = $"__ PLAYER {ind} __";
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

	[Command(requiresAuthority=false)] public void CmdSendPlayerModel()
	{
		//Debug.Log($"<color=blue>SendPlayerModelServerRpc</color>");
		//gm.SetPlayerModelServerRpc(characterInd.Value);
	}

	public void CHARACTER_IND_INC()
	{
		characterInd = (characterInd + 1) % maxCharacters;
		CmdUpdateUi((characterInd + 1) % maxCharacters);
	}
	public void CHARACTER_IND_DEC()
	{
		characterInd = characterInd == 0 ? maxCharacters - 1 : characterInd - 1;
		CmdUpdateUi(characterInd == 0 ? maxCharacters - 1 : characterInd - 1);
	}

	[Command(requiresAuthority = false)] public void CmdUpdateDisplay(bool next)
	{
		readyUi.SetActive(next);
		RpcUpdateDisplay(next);
		//Debug.Log("<color=green>HERE!!</color>");
	}
	[ClientRpc] private void RpcUpdateDisplay(bool next)
	{
		//Debug.Log("<color=green>UPDATED!!</color>");
		readyUi.SetActive(next);
	}

	#endregion
}

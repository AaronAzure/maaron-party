using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
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
	[SerializeField] private GameObject[] profileUis;
	[SerializeField] private GameObject readyUi;
	[SerializeField] private Button readyBtn;
	public int characterInd=-1;
	public bool _isReady=false;
	#endregion



	#region Methods
	private void Awake() 
	{
		nm = GameNetworkManager.Instance;
	}
	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		nm.AddConnection(this);
	}
	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();
		nm.RemoveConnection(this);
	}

	private void Start() 
	{
		gm = GameManager.Instance;
		transform.SetParent(gm.spawnHolder, true);
		transform.localScale = Vector3.one;
		
		readyBtn.interactable = IsOwner;
		//if (isLocalPlayer)
		if (IsOwner)
		{
			Instance = this;
			profileUis[0].SetActive(true);
			bg.color = new Color(0.25f, 0.25f, 0.25f, 0.7843f);
			buttons.SetActive(true);
			readyBtn.onClick.AddListener(() => {
				_isReady = !_isReady;
				UpdateDisplayServerRpc(_isReady);
			});
			readyBtn.gameObject.SetActive(true);
			
			//characterInd = (int) netId % maxCharacters;
			characterInd = (int) OwnerClientId % maxCharacters;
		}
		switch (characterInd)
		{
			case 0: 
				characterTxt.text = "Red";
				break;
			case 1: 
				characterTxt.text = "Green";
				break;
			case 2: 
				characterTxt.text = "Yellow";
				break;
			case 3: 
				characterTxt.text = "Periwinkle";
				break;
		}
		for (int i=0 ; i<profileUis.Length ; i++)
			profileUis[i].SetActive(i == characterInd);
		pfp.color = characterInd == 0 ? new Color(0.7f,0.13f,0.13f) : characterInd == 1 ? new Color(0.4f,0.7f,0.3f) 
				: characterInd == 2 ? new Color(0.85f,0.85f,0.5f) : new Color(0.7f,0.5f,0.8f);
		UpdateUiServerRpc(characterInd);
	}

	[ServerRpc] public void UpdateUiServerRpc(int ind)
	{
		UpdateUiClientRpc(ind);
	}

	[ClientRpc] public void UpdateUiClientRpc(int ind)
	{
		//gameObject.name = $"__ PLAYER {ind} __";
		switch (ind)
		{
			case 0: 
				characterTxt.text = "Red";
				break;
			case 1: 
				characterTxt.text = "Green";
				break;
			case 2: 
				characterTxt.text = "Yellow";
				break;
			case 3: 
				characterTxt.text = "Periwinkle";
				break;
		}
		for (int i=0 ; i<profileUis.Length ; i++)
			profileUis[i].SetActive(i == ind);
		pfp.color = ind == 0 ? new Color(0.7f,0.13f,0.13f) : ind == 1 ? new Color(0.4f,0.7f,0.3f) 
				: ind == 2 ? new Color(0.85f,0.85f,0.5f) : new Color(0.7f,0.5f,0.8f);
	}

	[ServerRpc] public void SendPlayerModelServerRpc()
	{
		//Debug.Log($"<color=blue>SendPlayerModelServerRpc</color>");
		//gm.SetPlayerModelServerRpc(characterInd.Value);
	}

	public void CHARACTER_IND_INC()
	{
		characterInd = (characterInd + 1) % maxCharacters;
		int ind = characterInd;
		switch (ind)
		{
			case 0: 
				characterTxt.text = "Red";
				break;
			case 1: 
				characterTxt.text = "Green";
				break;
			case 2: 
				characterTxt.text = "Yellow";
				break;
			case 3: 
				characterTxt.text = "Periwinkle";
				break;
		}
		for (int i=0 ; i<profileUis.Length ; i++)
			profileUis[i].SetActive(i == ind);
		pfp.color = ind == 0 ? new Color(0.7f,0.13f,0.13f) : ind == 1 ? new Color(0.4f,0.7f,0.3f) 
				: ind == 2 ? new Color(0.85f,0.85f,0.5f) : new Color(0.7f,0.5f,0.8f);
		UpdateUiServerRpc(characterInd);
	}
	public void CHARACTER_IND_DEC()
	{
		characterInd = characterInd == 0 ? maxCharacters - 1 : characterInd - 1;
		int ind = characterInd;
		switch (ind)
		{
			case 0: 
				characterTxt.text = "Red";
				break;
			case 1: 
				characterTxt.text = "Green";
				break;
			case 2: 
				characterTxt.text = "Yellow";
				break;
			case 3: 
				characterTxt.text = "Periwinkle";
				break;
		}
		for (int i=0 ; i<profileUis.Length ; i++)
			profileUis[i].SetActive(i == ind);
		pfp.color = ind == 0 ? new Color(0.7f,0.13f,0.13f) : ind == 1 ? new Color(0.4f,0.7f,0.3f) 
				: ind == 2 ? new Color(0.85f,0.85f,0.5f) : new Color(0.7f,0.5f,0.8f);
		UpdateUiServerRpc(characterInd);
	}

	[ServerRpc] public void UpdateDisplayServerRpc(bool next)
	{
		readyUi.SetActive(next);
		UpdateDisplayClientRpc(next);
	}
	[ClientRpc] private void UpdateDisplayClientRpc(bool next)
	{
		readyUi.SetActive(next);
	}

	#endregion
}

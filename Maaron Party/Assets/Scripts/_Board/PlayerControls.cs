using System.Collections;
using System.Collections.Generic;
using TMPro;
using Rewired;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerControls : NetworkBehaviour
{
	public static PlayerControls Instance;
	private Player player;
	private int playerId;
	[SerializeField] private Node currNode;
	[SerializeField] private Node nextNode;
	[SerializeField] private float moveSpeed=2.5f;
	[SerializeField] private float rotateSpeed=5f;
	private Vector3 startPos;
	private float time;
	[SyncVar] public int characterInd=-1;
	[SyncVar] public int id=-1;

	
	//[Space] [Header("Network")]
	//public NetworkVariable<ulong> id = new NetworkVariable<ulong>(
	//	0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	//[SerializeField] private NetworkObject nwObj;
	
	[Space] [Header("Model")]
	[SerializeField] private Transform model;
	[SerializeField] private GameObject[] models;
	[SerializeField] private Transform vCam;

	[Space] [SerializeField] private GameObject bonusObj;
	[SerializeField] private TextMeshPro bonusTxt;
	
	[Space] [SerializeField] private GameObject penaltyObj;
	[SerializeField] private TextMeshPro penaltyTxt;

	[Space] [SerializeField] private Direction up;
	[SerializeField] private Direction down;
	[SerializeField] private Direction left;
	[SerializeField] private Direction right;
	[SerializeField] private Direction upLeft;
	[SerializeField] private Direction upRight;
	[SerializeField] private Direction downLeft;
	[SerializeField] private Direction downRight;


	[Space] [SerializeField] private int movesLeft;
	[SerializeField] private TextMeshPro movesLeftTxt;

	
	
	[Space] [Header("Stats")]
	//[SerializeField] private NetworkVariable<int> coins = new NetworkVariable<int>(
	//	10, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	////[SerializeField] private int coins=10;
	//private NetworkVariable<int> coinsT = new NetworkVariable<int>(0,
	//	NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	[SerializeField] private int stars;
	private int starsT;
	[SerializeField] private float currencyT;


	[Space] [Header("UI")]
	[SerializeField] private GameObject canvas;
	[SerializeField] private Transform dataUi;
	[SerializeField] private Image dataImg;
	[SerializeField] private TextMeshProUGUI coinTxt;
	[SerializeField] private TextMeshProUGUI starTxt;
	private BoardManager bm
	{
		get 
		{
			return BoardManager.Instance;
		}
	}
	private GameNetworkManager nm
	{
		get 
		{
			return GameNetworkManager.Instance;
		}
	}
	private GameManager gm
	{
		get 
		{
			return GameManager.Instance;
		}
	}


	[Space] [Header("States")]
	[SerializeField] private bool isAtFork;
	private bool isCurrencyAsync;


	[Space] [Header("HACKS")]
	[SerializeField] private int controlledRoll=-1;


	private void Awake() 
	{
		DontDestroyOnLoad(this);	
	}
	public override void OnStartClient()
	{
		base.OnStartClient();
		if (isOwned)
			Instance = this;	
		nm.AddBoardConnection(this);
	}
	public override void OnStopClient()
	{
		base.OnStopClient();
		if (isOwned)
			nm.RemoveBoardConnection(this);
	}

	private void OnEnable() 
	{
		HidePaths();
	}

	public void RemoteStart(Transform spawnPos) 
	{
		//name = $"__ PLAYER {characterInd} __";
		if (vCam != null)
			vCam.parent = null;
		
		CmdSetModel(characterInd);
		transform.position = spawnPos.position + new Vector3(-4 + 2*id,0,0);
		
		if (!isOwned) {
			enabled = false;
			return;
		}
		//id.Value = OwnerClientId;
		startPos = this.transform.position;
		if (gm.hasStarted)
		{
			LoadData();
		}
		else
		{
			//coinsT.Value = coins.Value;
			starsT = stars;
		}
		//coinTxt.text = $"{coins.Value}";
		starTxt.text = $"{stars}";
	}

	[Command(requiresAuthority = false)] public void CmdSetModel(int ind)
	{
		RpcSetModel(ind);
	}

	[ClientRpc] public void RpcSetModel(int ind)
	{
		name = $"__ PLAYER {ind} __";
		transform.parent = bm.transform;
		for (int i=0 ; i<models.Length ; i++)
			models[i].SetActive(false);
		if (models != null && ind >= 0 && ind < models.Length)
			models[ind].SetActive(true);

		if (bm != null)
			bm.SetUiLayout(dataUi);
		dataUi.gameObject.SetActive(true);
		dataImg.color = ind == 0 ? new Color(0,1,0) : ind == 1 ? new Color(1,0.6f,0) 
			: ind == 2 ? new Color(1,0.5f,0.8f) : Color.blue;
	}

	public void SetStartNode(Node startNode)
	{
		nextNode = startNode;
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		//if (!IsOwner) return;
		//if (coins.Value != coinsT.Value)
		//{
		//	if (currencyT < 0.1f)
		//	{
		//		currencyT += Time.fixedDeltaTime;
		//	} 
		//	else
		//	{
		//		coinsT.Value = coinsT.Value < coins.Value ? coinsT.Value + 1 : coinsT.Value - 1;
		//		coinTxt.text = $"{coinsT.Value}";
		//		currencyT = 0;
		//	}
		//	if (coins.Value == coinsT.Value)
		//		isCurrencyAsync = false;
		//}
		//else if (stars != starsT)
		//{
		//	if (currencyT < 0.1f)
		//	{
		//		currencyT += Time.fixedDeltaTime;
		//	} 
		//	else
		//	{
		//		starsT = starsT < stars ? starsT + 1 : starsT - 1;
		//		starTxt.text = $"{starsT}";
		//		currencyT = 0;
		//	}
		//	if (stars == starsT)
		//		isCurrencyAsync = false;
		//}

		if (isAtFork) {}
		else if (movesLeft > 0)
		{
			if (transform.position != nextNode.transform.position)
			{
				var lookPos = nextNode.transform.position + - transform.position;
				lookPos.y = 0;
				var rotation = Quaternion.LookRotation(lookPos);
				model.rotation = Quaternion.Slerp(model.rotation, rotation, Time.fixedDeltaTime * rotateSpeed);

				transform.position = Vector3.Lerp(startPos, nextNode.transform.position, Mathf.SmoothStep(0,1,time));
				if (time < 1)
					time += Time.fixedDeltaTime * moveSpeed;
			}
			else
			{
				time = 0;
				UpdateMovesLeft(movesLeft-1);
				if (movesLeft <= 0)
				{
					currNode = nextNode; 
					movesLeftTxt.text = "";
					StartCoroutine( NodeEffectCo() );
					//if (nextNode.nextNodes.Count == 1)
					//	nextNode = nextNode.nextNodes[0];
					//else
					//	nextNode = null;
				}
				else
				{
					startPos = transform.position;
					// more than one path
					if (nextNode.nextNodes.Count > 1)
					{
						isAtFork = true;
						HidePaths();
						for (int i=0 ; i<nextNode.nextNodes.Count ; i++)
							RevealPaths(nextNode.nextNodes[i].transform.position, i);
					}
					// single path
					else
					{
						nextNode = nextNode.nextNodes[0];
					}
				}
			}
		}
	}


	public void YourTurn()
	{
		TargetYourTurn(netIdentity.connectionToClient);
	}
	[TargetRpc] public void TargetYourTurn(NetworkConnectionToClient target)
	{
		//!Debug.Log($"<color=yellow>target</color>");
		vCam.gameObject.SetActive(true);
		if (canvas != null)
			canvas.SetActive(true);
		this.enabled = true;
	}
	public void EndTurn()
	{
		vCam.gameObject.SetActive(false);
		if (canvas != null)
			canvas.SetActive(false);
		this.enabled = false;
		SaveData();
		//!Debug.Log($"<color=yellow>TURN ENDED</color>");
	}

	private void SaveData()
	{
		gm.SaveCurrNode(currNode.nodeId, playerId);
		//gm.SaveCoins(coins.Value, playerId);
		gm.SaveStars(stars, playerId);
	}
	private void LoadData()
	{
		currNode = NodeManager.Instance.GetNode( gm.GetCurrNode(playerId) );
		startPos = transform.position = currNode.transform.position;
		//coinsT.Value = coins.Value = gm.GetCoins(playerId);
		starsT = stars = gm.GetStars(playerId);
	}

	public void ROLL_DICE()
	{
		if (currNode != null)
		{
			if (currNode.nextNodes.Count > 1)
			{
				isAtFork = true;
				for (int i=0 ; i<currNode.nextNodes.Count ; i++)
					RevealPaths(currNode.nextNodes[i].transform.position, i);
				nextNode = currNode;
			}
			else
			{
				isAtFork = false;
				nextNode = currNode.nextNodes[0];
			}
		}

		int rng = controlledRoll != -1 ? controlledRoll : Random.Range(1, 11);
		Debug.Log($"<color=magenta>ROLLED {rng}</color>");
		UpdateMovesLeft( rng );

		if (canvas != null)
			canvas.SetActive(false);
	}

	public void NodeEffect(int bonus)
	{
		//coins.Value = Mathf.Clamp(coins.Value + bonus, 0, 999);
		//isCurrencyAsync = true;
		if (bonus > 0)
		{
			bonusObj.SetActive(false);
			bonusObj.SetActive(true);
			bonusTxt.text = $"+{bonus}";
		}
		else if (bonus < 0)
		{
			penaltyObj.SetActive(false);
			penaltyObj.SetActive(true);
			penaltyTxt.text = $"{bonus}";
		}
	}

	private IEnumerator NodeEffectCo()
	{
		// no event
		if (currNode.GetNodeEffect(this))
			yield return new WaitForSeconds(0.5f);
		else 
			yield return new WaitForSeconds(0.5f);

		while (isCurrencyAsync)
			yield return new WaitForSeconds(0.1f);

		EndTurn();
		bm.NextPlayerTurn();
	}

	void HidePaths()
	{
		if (up.gameObject.activeSelf)
			up.gameObject.SetActive(false);
		if (down.gameObject.activeSelf)
			down.gameObject.SetActive(false);
		if (left.gameObject.activeSelf)
			left.gameObject.SetActive(false);
		if (right.gameObject.activeSelf)
			right.gameObject.SetActive(false);
		if (upLeft.gameObject.activeSelf)
			upLeft.gameObject.SetActive(false);
		if (upRight.gameObject.activeSelf)
			upRight.gameObject.SetActive(false);
		if (downLeft.gameObject.activeSelf)
			downLeft.gameObject.SetActive(false);
		if (downRight.gameObject.activeSelf)
			downRight.gameObject.SetActive(false);
	}
	void RevealPaths(Vector3 nextPos, int ind)
	{
		Vector3 toPos = (nextPos - startPos).normalized;
		bool goingUp = toPos.z >= 0.33f;
		bool goingDown = toPos.z <= -0.33f;
		bool goingRight = toPos.x >= 0.33f;
		bool goingLeft = toPos.x <= -0.33f;

		if (goingUp && !goingLeft && !goingRight)
		{
			up.gameObject.SetActive(true);
			up.index = ind;
		}
		else if (goingDown && !goingLeft && !goingRight)
		{
			down.gameObject.SetActive(true);
			down.index = ind;
		}
		else if (goingLeft && !goingUp && !goingDown)
		{
			left.gameObject.SetActive(true);
			left.index = ind;
		}
		else if (goingRight && !goingUp && !goingDown)
		{
			right.gameObject.SetActive(true);
			right.index = ind;
		}
		else if (goingUp && goingLeft)
		{
			upLeft.gameObject.SetActive(true);
			upLeft.index = ind;
		}
		else if (goingUp && goingRight)
		{
			upRight.gameObject.SetActive(true);
			upRight.index = ind;
		}
		else if (goingDown && goingLeft)
		{
			downLeft.gameObject.SetActive(true);
			downLeft.index = ind;
		}
		else if (goingDown && goingRight)
		{
			downRight.gameObject.SetActive(true);
			downRight.index = ind;
		}
	}

	public void ChoosePath(int ind)
	{
		nextNode = nextNode.nextNodes[ind];
		HidePaths();
		isAtFork = false;
	}
	void UpdateMovesLeft(int x)
	{
		movesLeft = x;
		movesLeftTxt.text = $"{movesLeft}";
	}

	IEnumerator MoveCo()
	{
		yield return new WaitForSeconds(2);
		UpdateMovesLeft( Random.Range(1, 11) );
	}
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using Rewired;
using UnityEngine;
using UnityEngine.UI;
using FishNet.Object;
using FishNet.Connection;
using FishNet;
using FishNet.Object.Synchronizing;

public class PlayerControls : NetworkBehaviour
{
	public static PlayerControls Instance;
	private NetworkConnection conn;
	private Player player;
	private int playerId;
	[SerializeField] private Node currNode;
	[SerializeField] private Node nextNode;
	[SerializeField] private float moveSpeed=2.5f;
	[SerializeField] private float rotateSpeed=5f;
	private Vector3 startPos;
	private float time;

	
	[Space] [Header("Network")]
	private readonly SyncVar<int> _characterInd = new SyncVar<int>(-1);
	//public NetworkVariable<ulong> id = new NetworkVariable<ulong>(
	//	0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	[SerializeField] private NetworkObject nwObj;
	
	
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
	//private NetworkVariable<int> coinsT = new NetworkVariable<int>(0,
	//	NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	//[SerializeField] private int coins=10;
	[SerializeField] private int stars;
	private int starsT;
	[SerializeField] private float currencyT;


	[Space] [Header("UI")]
	[SerializeField] private GameObject canvas;
	[SerializeField] private Transform dataUi;
	[SerializeField] private Image dataImg;
	[SerializeField] private TextMeshProUGUI coinTxt;
	[SerializeField] private TextMeshProUGUI starTxt;
	private BoardManager bm;
	private GameManager gm;


	[Space] [Header("States")]
	[SerializeField] private bool isAtFork;
	private bool isCurrencyAsync;


	[Space] [Header("HACKS")]
	[SerializeField] private int controlledRoll=-1;

	public override void OnStartClient()
	{
		base.OnStartClient();
		//coinsT.OnValueChanged += (int prevCoins, int newCoins) => {
		//	coinTxt.text = coinTxt.text = $"{newCoins}";
		//};
		_characterInd.OnChange += SetModel;
	}

	public override void OnStopClient()
	{
		base.OnStopClient();
		Debug.Log($"<color=red>DISCONNECTED {base.LocalConnection.ClientId}</color>");
	}

	private void OnEnable() 
	{
		HidePaths();
	}

	private void Start() 
	{
		if (vCam != null)
			vCam.parent = null;
		bm = BoardManager.Instance;
		gm = GameManager.Instance;
		dataUi.SetParent(bm.GetUiLayout());
		dataUi.localScale = Vector3.one;
		_characterInd.Value = gm.GetCharacterModel(base.Owner);
		conn = InstanceFinder.ClientManager.Connection;
		//dataImg.color = OwnerClientId == 0 ? new Color(0,1,0) : OwnerClientId == 1 ? new Color(1,0.6f,0) 
		//	: OwnerClientId == 2 ? new Color(1,0.5f,0.8f) : Color.blue;
		//SetModelServerRpc( gm.GetCharacterModel(conn) );\
		Debug.Log($"<color=cyan>{name} = {base.Owner.IsLocalClient}</color>");
		
		if (base.Owner.IsLocalClient)
			Instance = this;
		else
		{
			this.enabled = false;
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

	public void SetId(int id)
	{
		playerId = id;
	}

	private void SetModel(int prev, int next, bool asServer)
	{
		for (int i=0 ; i<models.Length ; i++)
			models[i].SetActive(false);
		if (models != null && next >= 0 && next < models.Length)
			models[next].SetActive(true);
		dataImg.color = next == 0 ? new Color(0,1,0) : next == 1 ? new Color(1,0.6f,0) 
			: next == 2 ? new Color(1,0.5f,0.8f) : Color.blue;
	}

	[ServerRpc(RequireOwnership=false)] public void SetModelServerRpc(int next)
	{
		SetModelObserverRpc(next);
	}
	[ObserversRpc] public void SetModelObserverRpc(int next)
	{
		//Debug.Log($"__ PLAYER {id.Value} |{ind}|__");
		//name = $"__ PLAYER {id.Value} __";
		for (int i=0 ; i<models.Length ; i++)
			models[i].SetActive(false);
		if (models != null && next >= 0 && next < models.Length)
			models[next].SetActive(true);
		dataImg.color = next == 0 ? new Color(0,1,0) : next == 1 ? new Color(1,0.6f,0) 
			: next == 2 ? new Color(1,0.5f,0.8f) : Color.blue;
	}

	public void SetStartNode(Node startNode)
	{
		nextNode = startNode;
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if (!IsOwner) return;
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
		else if (stars != starsT)
		{
			if (currencyT < 0.1f)
			{
				currencyT += Time.fixedDeltaTime;
			} 
			else
			{
				starsT = starsT < stars ? starsT + 1 : starsT - 1;
				starTxt.text = $"{starsT}";
				currencyT = 0;
			}
			if (stars == starsT)
				isCurrencyAsync = false;
		}

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
		isCurrencyAsync = true;
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

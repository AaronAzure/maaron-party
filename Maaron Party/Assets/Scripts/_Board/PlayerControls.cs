using System.Collections;
using System.Collections.Generic;
using TMPro;
using Rewired;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
	private Player player;
	private int playerId;
	[SerializeField] private Node currNode;
	[SerializeField] private Node nextNode;
	[SerializeField] private float moveSpeed=2.5f;
	[SerializeField] private float rotateSpeed=5f;
	private Vector3 startPos;
	private float time;

	
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
	[SerializeField] private int coins=10;
	private int coinsT;
	[SerializeField] private int stars;
	private int starsT;
	[SerializeField] private float currencyT;


	[Space] [Header("UI")]
	[SerializeField] private GameObject canvas;
	[SerializeField] private Transform dataUi;
	[SerializeField] private TextMeshProUGUI coinTxt;
	[SerializeField] private TextMeshProUGUI starTxt;
	private BoardManager bm;
	private GameManager gm;


	[Space] [Header("States")]
	[SerializeField] private bool isAtFork;
	private bool isCurrencyAsync;


	[Space] [Header("HACKS")]
	[SerializeField] private int controlledRoll=-1;

	
	private void OnEnable() 
	{
		//if (nextNode == null)
		//	this.enabled = false;
		startPos = this.transform.position;
		HidePaths();
	}

	private void Start() 
	{
		if (vCam != null)
			vCam.parent = null;
		bm = BoardManager.Instance;
		gm = GameManager.Instance;
		if (gm.hasStarted)
		{
			LoadData();
		}
		else
		{
			coinsT = coins;
			starsT = stars;
		}
		if (bm != null)
		{
			dataUi.parent = bm.GetUiLayout();
			dataUi.localScale = Vector3.one;
		}
		coinTxt.text = $"{coins}";
		starTxt.text = $"{stars}";
	}

	public void SetId(int id)
	{
		playerId = id;
	}

	public void SetModel(int ind)
	{
		for (int i=0 ; i<models.Length ; i++)
			models[i].SetActive(false);
		if (models != null && ind >= 0 && ind < models.Length)
			models[ind].SetActive(true);
	}

	public void SetStartNode(Node startNode)
	{
		nextNode = startNode;
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if (coins != coinsT)
		{
			if (currencyT < 0.1f)
			{
				currencyT += Time.fixedDeltaTime;
			} 
			else
			{
				coinsT = coinsT < coins ? coinsT + 1 : coinsT - 1;
				coinTxt.text = $"{coinsT}";
				currencyT = 0;
			}
			if (coins == coinsT)
				isCurrencyAsync = false;
		}
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
		gm.SaveCoins(coins, playerId);
		gm.SaveStars(stars, playerId);
	}
	private void LoadData()
	{
		currNode = NodeManager.Instance.GetNode( gm.GetCurrNode(playerId) );
		startPos = transform.position = currNode.transform.position;
		coinsT = coins = gm.GetCoins(playerId);
		starsT = stars = gm.GetStars(playerId);
	}

	public void ROLL_DICE()
	{
		if (currNode != null)
		{
			if (currNode.nextNodes.Count > 1)
			{
				isAtFork = true;
				HidePaths();
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
		coins = Mathf.Clamp(coins + bonus, 0, 999);
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
			yield return new WaitForSeconds(1);
		else 
			yield return new WaitForSeconds(0.5f);

		while (isCurrencyAsync)
			yield return new WaitForSeconds(0.1f);

		EndTurn();
		bm.NextPlayerTurn();
	}

	void HidePaths()
	{
		up.gameObject.SetActive(false);
		down.gameObject.SetActive(false);
		left.gameObject.SetActive(false);
		right.gameObject.SetActive(false);
		upLeft.gameObject.SetActive(false);
		upRight.gameObject.SetActive(false);
		downLeft.gameObject.SetActive(false);
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
		if (goingDown && !goingLeft && !goingRight)
		{
			down.gameObject.SetActive(true);
			down.index = ind;
		}
		if (goingLeft && !goingUp && !goingDown)
		{
			left.gameObject.SetActive(true);
			left.index = ind;
		}
		if (goingRight && !goingUp && !goingDown)
		{
			right.gameObject.SetActive(true);
			right.index = ind;
		}
		if (goingUp && goingLeft)
		{
			upLeft.gameObject.SetActive(true);
			upLeft.index = ind;
		}
		if (goingUp && goingRight)
		{
			upRight.gameObject.SetActive(true);
			upRight.index = ind;
		}
		if (goingDown && goingLeft)
		{
			downLeft.gameObject.SetActive(true);
			downLeft.index = ind;
		}
		if (goingDown && goingRight)
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

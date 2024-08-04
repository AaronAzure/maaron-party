using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
	[SerializeField] private Node currNode;
	[SerializeField] private Node nextNode;
	[SerializeField] private float moveSpeed=2.5f;
	[SerializeField] private float rotateSpeed=5f;
	private Vector3 startPos;
	private float time;

	
	[Space] [Header("Model")]
	[SerializeField] private Transform model;
	[SerializeField] private Transform vCam;
	[SerializeField] private Direction up;
	[SerializeField] private Direction down;
	[SerializeField] private Direction left;
	[SerializeField] private Direction right;
	[SerializeField] private Direction upLeft;
	[SerializeField] private Direction upRight;
	[SerializeField] private Direction downLeft;
	[SerializeField] private Direction downRight;


	[Space] [SerializeField] private int movesLeft;
	[SerializeField] private TextMeshPro movesLeftTxt;


	[Space] [Header("UI")]
	[SerializeField] private GameObject canvas;


	[Space] [Header("States")]
	[SerializeField] private bool isAtFork;

	
	private void OnEnable() 
	{
		if (nextNode == null)
			this.enabled = false;
		startPos = this.transform.position;
	}

	private void Start() 
	{
		//UpdateMovesLeft( Random.Range(1, 11) );
		if (canvas != null)
			canvas.SetActive(true);
		if (vCam != null)
			vCam.parent = null;
	}

	// Update is called once per frame
	void FixedUpdate()
	{
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
				UpdateMovesLeft(movesLeft-1);
				
				time = 0;
				if (movesLeft <= 0)
					if (canvas != null)
						canvas.SetActive(true);
			}
		}
	}


	public void ROLL_DICE()
	{
		UpdateMovesLeft( Random.Range(1, 11) );
		if (canvas != null)
			canvas.SetActive(false);
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
		
		//Debug.Log($"<color=magenta> moving {(goingUp ? "UP" : "")}{(goingDown ? "DOWN" : "")} {(goingRight ? "RIGHT" : "")}{(goingLeft ? "LEFT" : "")}</color>");
		//// up
		//if ((toPos.x > -0.333f && toPos.x < 0.333f) && toPos.z > 0.667f) {Debug.Log("<color=magenta> moving UP</color>");}
		//// down
		//else if ((toPos.x > -0.333f && toPos.x < 0.333f) && toPos.z < -0.667f) {Debug.Log("<color=magenta> moving DOWN</color>");}
		//// left
		//else if (toPos.x < -0.667f && (toPos.z > -0.333f && toPos.z < 0.333f)) {Debug.Log("<color=magenta> moving LEFT</color>");}
		//// right
		//else if (toPos.x > 0.667f && (toPos.z > -0.333f && toPos.z < 0.333f)) {Debug.Log("<color=magenta> moving RIGHT</color>");}
		//// up left
		//else if (toPos.x < -0.667f && toPos.z > 0.667f) {Debug.Log("<color=magenta> moving UP LEFT</color>");}
		//// up right
		//else if (toPos.x > 0.667f && toPos.z > 0.667f) {Debug.Log("<color=magenta> moving UP RIGHT</color>");}
		//// down left
		//else if (toPos.x < -0.667f && toPos.z < -0.667f) {Debug.Log("<color=magenta> moving DOWN LEFT</color>");}
		//// down right
		//else if (toPos.x > 0.667f && toPos.z < -0.667f) {Debug.Log("<color=magenta> moving DOWN RIGHT</color>");}
	}

	IEnumerator MoveCo()
	{
		yield return new WaitForSeconds(2);
		UpdateMovesLeft( Random.Range(1, 11) );
	}
}

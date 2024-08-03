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
	[SerializeField] private GameObject up;
	[SerializeField] private GameObject down;
	[SerializeField] private GameObject left;
	[SerializeField] private GameObject right;
	[SerializeField] private GameObject upLeft;
	[SerializeField] private GameObject upRight;
	[SerializeField] private GameObject downLeft;
	[SerializeField] private GameObject downRight;


	[Space] [SerializeField] private int movesLeft;
	[SerializeField] private TextMeshPro movesLeftTxt;


	[Space] [Header("UI")]
	[SerializeField] private GameObject canvas;

	
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
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if (movesLeft > 0)
		{
			if (transform.position != nextNode.transform.position)
			{
				var lookPos = nextNode.transform.position + - transform.position;
				lookPos.y = 0;
				var rotation = Quaternion.LookRotation(lookPos);
				model.rotation = Quaternion.Slerp(model.rotation, rotation, Time.fixedDeltaTime * rotateSpeed);

				transform.position = Vector3.Lerp(startPos, nextNode.transform.position, time);
				if (time < 1)
					time += Time.fixedDeltaTime * moveSpeed;
			}
			else
			{
				nextNode = nextNode.nextNodes[Random.Range(0, nextNode.nextNodes.Count) ];
				startPos = transform.position;
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

	void UpdateMovesLeft(int x)
	{
		movesLeft = x;
		movesLeftTxt.text = $"{movesLeft}";


		Vector3 toPos = (nextNode.transform.position - startPos).normalized;
		bool goingUp = toPos.z >= 0.33f;
		bool goingDown = toPos.z <= -0.33f;
		bool goingRight = toPos.x >= 0.33f;
		bool goingLeft = toPos.x <= -0.33f;

		up.SetActive(goingUp && !goingLeft && !goingRight);
		down.SetActive(goingDown && !goingLeft && !goingRight);
		left.SetActive(goingLeft && !goingUp && !goingDown);
		right.SetActive(goingRight && !goingUp && !goingDown);
		upLeft.SetActive(goingUp && goingLeft);
		upRight.SetActive(goingUp && goingRight);
		downLeft.SetActive(goingDown && goingLeft);
		downRight.SetActive(goingDown && goingRight);
		
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class MinigameControls : MonoBehaviour
{
	private Player player;
	[HideInInspector] public int playerId;
	
	[SerializeField] private float moveSpeed=3f;
	[SerializeField] private float maxSpeed=7f;
	[SerializeField] private float rotateSpeed=5f;
	public bool canMove;
	public bool canJump;

	
	[Space] [Header("Model")]
	[SerializeField] private Transform model;
	[SerializeField] private GameObject[] models;
	[SerializeField] private Rigidbody rb;



	// Start is called before the first frame update
	void Start()
	{
		player = ReInput.players.GetPlayer(playerId);
	}

	public void SetModel(int ind)
	{
		for (int i=0 ; i<models.Length ; i++)
			models[i].SetActive(false);
		if (models != null && ind >= 0 && ind < models.Length)
			models[ind].SetActive(true);
	}

	private void FixedUpdate() 
	{
		if (canMove)
			Move();
	}

	private void Move()
	{
		float moveX = player.GetAxis("Move Horizontal");
		float moveZ = player.GetAxis("Move Vertical");

		Vector3 moveDir = Vector3.forward * moveZ + Vector3.right * moveX;
		//float spd = moveSpeed;
		//float maxSpd = maxSpeed;
		rb.AddForce(moveDir.normalized * moveSpeed, ForceMode.Impulse);

		if (rb.velocity.magnitude > maxSpeed)
			rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
		//rb.velocity = new Vector3(moveX, rb.velocity.y, moveZ);
		if (moveX != 0 || moveZ != 0)
			Rotate(moveX, moveZ);
	}

	private void Rotate(float x, float z)
	{
		Vector3 lookPos = new Vector3(x, 0, z);
		var rotation = Quaternion.LookRotation(lookPos);
		model.rotation = Quaternion.Slerp(model.rotation, rotation, Time.fixedDeltaTime * rotateSpeed);
	}
}

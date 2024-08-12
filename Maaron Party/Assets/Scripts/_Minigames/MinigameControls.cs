using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using Rewired;

public class MinigameControls : NetworkBehaviour
{
	public static MinigameControls Instance;
	public GameManager gm;
	public MinigameManager mm;
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



	//public override void OnNetworkSpawn()
	//{
	//	if (IsOwner)
	//		Instance = this;
	//}

	// Start is called before the first frame update
	void Start()
	{
		gm = GameManager.Instance;
		//SetModel( gm.playerModels[(int) OwnerClientId] );
		mm = MinigameManager.Instance;
		transform.parent = mm.transform;
		//if (!IsOwner) enabled = false;

		//playerId = (int) OwnerClientId;
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
		if (!IsOwner) return;
		if (canMove)
			Move();
	}

	private void Move()
	{
		float moveX = player.GetAxis("Move Horizontal");
		float moveZ = player.GetAxis("Move Vertical");

		var dir = new Vector3(moveX, 0, moveZ).normalized * moveSpeed;
		rb.velocity = new Vector3(dir.x, rb.velocity.y, dir.z);

		if (moveX != 0 || moveZ != 0)
			Rotate(moveX, moveZ);
	}

	private void Rotate(float x, float z)
	{
		Vector3 lookPos = new Vector3(x, 0, z);
		var rotation = Quaternion.LookRotation(lookPos);
		model.rotation = Quaternion.Slerp(model.rotation, rotation, Time.fixedDeltaTime * rotateSpeed);
	}
	//[ClientRpc(RequireOwnership = false)] public void DeathClientRpc(ulong targetId)
	//{
	//	if (OwnerClientId == targetId)
	//	{
	//		this.enabled = false;
	//		gameObject.SetActive(false);
	//	}
	//}

	private void OnTriggerEnter(Collider other) 
	{
		if (IsOwner && enabled && other.gameObject.CompareTag("Death"))
		{
			MinigameManager.Instance.PlayerEliminatedServerRpc(playerId);
			this.enabled = false;
			gameObject.SetActive(false);
		}
	}
}

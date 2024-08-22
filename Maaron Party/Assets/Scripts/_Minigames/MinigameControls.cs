using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Rewired;

public class MinigameControls : NetworkBehaviour
{
	#region Variables

	public static MinigameControls Instance;
	private GameManager gm { get { return GameManager.Instance; } }
	private GameNetworkManager nm { get { return GameNetworkManager.Instance; } }
	private MinigameManager mm { get { return MinigameManager.Instance; } }
	private Player player;
	[HideInInspector] public int playerId;
	
	[SerializeField] private float moveSpeed=3f;
	[SerializeField] private float maxSpeed=7f;
	[SerializeField] private float rotateSpeed=5f;
	public bool canMove;
	public bool canJump;

	
	[Space] [Header("Model")]
	[SerializeField] private Transform model;
	[SyncVar] public int characterInd;
	[SyncVar] public int id;
	[SerializeField] private GameObject[] models;
	[SerializeField] private Rigidbody rb;

	#endregion

	private void Awake() 
	{
		DontDestroyOnLoad(this);	
	}
	public override void OnStartClient()
	{
		base.OnStartClient();
		if (isOwned)
			Instance = this;	
		nm.AddMinigameConnection(this);
	}
	public override void OnStopClient()
	{
		base.OnStopClient();
		if (isOwned)
			nm.RemoveMinigameConnection(this);
	}

	// Start is called before the first frame update
	void Start()
	{
		SetModel( characterInd );
		//transform.parent = mm.transform;
		//if (!IsOwner) enabled = false;

		//playerId = (int) OwnerClientId;
		player = ReInput.players.GetPlayer(playerId);
	}

	public void SetSpawn()
	{
		transform.position = mm.GetPlayerSpawn(id);
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
		//if (!IsOwner) return;
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
	[ClientRpc] public void RpcDeath(ulong targetId)
	{
		//if (OwnerClientId == targetId)
		//{
		//	this.enabled = false;
		//	gameObject.SetActive(false);
		//}
	}

	private void OnTriggerEnter(Collider other) 
	{
		//if (IsOwner && enabled && other.gameObject.CompareTag("Death"))
		//{
		//	MinigameManager.Instance.PlayerEliminatedServerRpc(playerId);
		//	this.enabled = false;
		//	gameObject.SetActive(false);
		//}
	}
}

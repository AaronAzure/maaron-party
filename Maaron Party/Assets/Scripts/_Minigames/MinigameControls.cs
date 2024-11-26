using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Rewired;

public class MinigameControls : NetworkBehaviour
{
	#region Variables

	public static MinigameControls Instance;
	private GameNetworkManager nm { get { return GameNetworkManager.Instance; } }
	private MinigameManager mm { get { return MinigameManager.Instance; } }
	private Player player;
	[HideInInspector] public int playerId;
	
	[SerializeField] private float moveSpeed=3f;
	[SerializeField] private float maxSpeed=7f;
	[SerializeField] private float rotateSpeed=5f;
	[SerializeField] private float kbForce=5f;
	public bool canMove;
	public bool canJump;
	public bool canKb;
	private bool isReceivingKb;
	private Coroutine kbCo;
	private bool gameStarted;

	
	[Space] [Header("Model")]
	[SerializeField] private Transform model;
	public int characterInd;
	public int id;
	public int boardOrder;
	[SerializeField] private GameObject[] models;
	[SerializeField] private GameObject[] ragdolls;
	[SerializeField] private Animator anim;
	[SerializeField] private Rigidbody rb;
	[SerializeField] private Collider col;

	#endregion


	#region Methods
	private void Awake() 
	{
		DontDestroyOnLoad(this);	
	}
	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		//Debug.Log($"<color=magenta>MinigameControls = StartClient ({Instance != null})</color>");
		if (IsOwner)
			Instance = this;	
		nm.AddMinigameConnection(this);
	}
	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();
		if (IsOwner)
			nm.RemoveMinigameConnection(this);
	}

	// Start is called before the first frame update
	void Start()
	{
		if (!IsOwner) {
			enabled = false;
			return;
		}
		transform.Rotate(0,180,0);	

		player = ReInput.players.GetPlayer(0);
	}
	[ServerRpc] public void SetModelServerRpc() => SetModelClientRpc();
	[ClientRpc] public void SetModelClientRpc()
	{
		name = $"__ PLAYER {id} __";
		col.enabled = true;
		transform.parent = mm.transform;
		for (int i=0 ; i<models.Length ; i++)
			models[i].SetActive(false);
		if (models != null && characterInd >= 0 && characterInd < models.Length)
			models[characterInd].SetActive(true);

		if (IsOwner)
			anim = models[characterInd].GetComponent<Animator>();
	}

	public void SetSpawn()
	{
		SetModelServerRpc();
		rb.velocity = Vector3.zero;
		col.enabled = rb.useGravity = true;
		transform.position = mm.GetPlayerSpawn(id);
		ReactivateServerRpc();
		//Debug.Log($"<color=magenta>MinigameControls = Setting Up ({transform.position})</color>");
		//gameObject.SetActive(true);
		model.rotation = Quaternion.identity;
		model.Rotate(0,180,0);
	}
	public void StartGame() => gameStarted = true;
	
	[ServerRpc] private void ReactivateServerRpc() => ReactivateClientRpc();
	[ClientRpc] private void ReactivateClientRpc() => gameObject.SetActive(true);
	 

	public void SetModel(int ind)
	{
		for (int i=0 ; i<models.Length ; i++)
			models[i].SetActive(false);
		if (models != null && ind >= 0 && ind < models.Length)
			models[ind].SetActive(true);
	}

	private void FixedUpdate() 
	{
		if (!IsOwner || !gameStarted) return;
		if (canMove && !isReceivingKb)
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
		if (anim != null) anim.SetFloat("moveSpeed", rb.velocity.magnitude);
	}

	private void Rotate(float x, float z)
	{
		Vector3 lookPos = new Vector3(x, 0, z);
		var rotation = Quaternion.LookRotation(lookPos);
		model.rotation = Quaternion.Slerp(model.rotation, rotation, Time.fixedDeltaTime * rotateSpeed);
	}
	[ServerRpc] public void DeathServerRpc(Vector3 src, int ind)
	{
		DeathClientRpc(src, ind);
	}
	[ClientRpc] public void DeathClientRpc(Vector3 src, int ind)
	{
		model.gameObject.SetActive(false);

		Rigidbody[] bones = ragdolls[ind].GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody bone in bones) {
			bone.velocity = src * UnityEngine.Random.Range(12,18);
		}
		ragdolls[ind].transform.rotation = Quaternion.LookRotation(new Vector3(src.x, 0, src.z));
		ragdolls[ind].SetActive(true);
		//col.enabled = false;
	}
	public void EndGame()
	{
		gameStarted = false;
		rb.velocity = new Vector3(0,rb.velocity.y,0);
	}

	private void OnTriggerEnter(Collider other) 
	{
		if (IsOwner && gameStarted && enabled && other.gameObject.CompareTag("Death"))
		{
			mm.PlayerEliminatedServerRpc(id);
			gameStarted = false;
			DeathServerRpc(((transform.position - other.transform.position - Vector3.down * 3).normalized + Vector3.up).normalized, characterInd);
		}
	}

	private void OnCollisionEnter(Collision other) 
	{
		if (IsOwner && canKb && !isReceivingKb && gameStarted && enabled && other.gameObject.CompareTag("Player"))
		{
			if (kbCo == null)
				kbCo = StartCoroutine(ReceiveKbCo((transform.position - other.transform.position).normalized));
		}
	}
	IEnumerator ReceiveKbCo(Vector3 dir)
	{
		isReceivingKb = true;
		rb.AddForce(dir * kbForce, ForceMode.Impulse);

		yield return new WaitForSeconds(0.1f);
		isReceivingKb = false;
		rb.velocity = new Vector3(0, rb.velocity.y, 0);
		kbCo = null;
	}
	#endregion
}

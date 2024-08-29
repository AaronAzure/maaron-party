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
	private bool gameStarted;

	
	[Space] [Header("Model")]
	[SerializeField] private Transform model;
	[SyncVar] public int characterInd;
	[SyncVar] public int id;
	[SerializeField] private GameObject[] models;
	[SerializeField] private Animator anim;
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
		//SetModel( characterInd );
		//transform.parent = mm.transform;
		//if (!IsOwner) enabled = false;
		transform.Rotate(0,180,0);	
		if (!isOwned) {
			enabled = false;
			return;
		}

		player = ReInput.players.GetPlayer(0);
	}
	[Command(requiresAuthority = false)] public void CmdSetModel()
	{
		RpcSetModel();
	}
	[ClientRpc] public void RpcSetModel()
	{
		name = $"__ PLAYER {id} __";
		transform.parent = mm.transform;
		for (int i=0 ; i<models.Length ; i++)
			models[i].SetActive(false);
		if (models != null && characterInd >= 0 && characterInd < models.Length)
			models[characterInd].SetActive(true);

		if (isOwned)
			anim = models[characterInd].GetComponent<Animator>();
	}

	public void SetSpawn()
	{
		CmdSetModel();
		rb.velocity = Vector3.zero;
		transform.position = mm.GetPlayerSpawn(id);
		rb.useGravity = true;
		CmdReactivate();
		//gameObject.SetActive(true);
		model.rotation = Quaternion.identity;
		model.Rotate(0,180,0);
		gameStarted = true;
	}
	[Command] private void CmdReactivate() => RpcReactivate();
	[ClientRpc] private void RpcReactivate() => gameObject.SetActive(true);
	 

	public void SetModel(int ind)
	{
		for (int i=0 ; i<models.Length ; i++)
			models[i].SetActive(false);
		if (models != null && ind >= 0 && ind < models.Length)
			models[ind].SetActive(true);
	}

	private void FixedUpdate() 
	{
		if (!isOwned || !gameStarted) return;
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
		if (anim != null) anim.SetFloat("moveSpeed", rb.velocity.magnitude);
	}

	private void Rotate(float x, float z)
	{
		Vector3 lookPos = new Vector3(x, 0, z);
		var rotation = Quaternion.LookRotation(lookPos);
		model.rotation = Quaternion.Slerp(model.rotation, rotation, Time.fixedDeltaTime * rotateSpeed);
	}
	[Command] public void CmdDeath()
	{
		RpcDeath();
	}
	[ClientRpc] public void RpcDeath()
	{
		gameObject.SetActive(false);
	}
	public void EndGame()
	{
		gameStarted = false;
	}

	private void OnTriggerEnter(Collider other) 
	{
		if (isOwned && gameStarted && enabled && other.gameObject.CompareTag("Death"))
		{
			mm.CmdPlayerEliminated(id);
			gameStarted = false;
			CmdDeath();
			gameObject.SetActive(false);
			
		}
	}
}

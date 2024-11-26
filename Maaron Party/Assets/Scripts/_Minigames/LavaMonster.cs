using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class LavaMonster : NetworkBehaviour
{
	[SerializeField] private Transform model;
	[SerializeField] private Transform[] points;
	[SerializeField] private float moveSpeed=9;
	[SerializeField] private float rotateSpeed=7.5f;

	[Space] [SerializeField] private GameObject[] fireTrails;
	[SerializeField] private Vector3 offset;
	[SerializeField] private float timeRepeat=1;
	private Vector3 startPos;
	private Vector3 destPos;
	private bool canMove;
	private bool isRest;
	float timer;
	int prevP;

	private void Start() 
	{
		startPos = transform.position;
		prevP = Random.Range(0, points.Length);
		var p = points[prevP].position;
		destPos = new Vector3(p.x, 0, p.z);
		canMove = true;
		if (IsServer) 
			SetupServerRpc();
	}
	[ServerRpc] void SetupServerRpc() => SetupClientRpc();
	[ClientRpc] void SetupClientRpc()
	{
		for (int i=0 ; i<fireTrails.Length ; i++)
			if (fireTrails[i] != null)
				fireTrails[i].transform.parent = null;
	}

	public void FixedUpdateAction() 
	{
		if (canMove)
		{
			if (transform.position != destPos)
			{
				transform.position = Vector3.MoveTowards(transform.position, destPos, moveSpeed * Time.fixedDeltaTime);

				if (Vector3.Distance(transform.position, destPos) < 0.01f)
				{
					canMove = false;
					StartCoroutine(RestCo());
				}
				// still moving
				else
				{
					if (timer < timeRepeat)
						timer += Time.fixedDeltaTime;
					else
					{
						timer = 0;
						SpawnFireTrailServerRpc();
					}
					var lookPos = destPos - transform.position;
					lookPos.y = 0;
					var rotation = Quaternion.LookRotation(lookPos);
					model.rotation = Quaternion.Slerp(model.rotation, rotation, Time.fixedDeltaTime * rotateSpeed);
				}
			}
		}
	}
	[ServerRpc] void SpawnFireTrailServerRpc() => SpawnFireTrailClientRpc();
	[ClientRpc] private void SpawnFireTrailClientRpc()
	{
		//Debug.Log("<color=red>SPAWNING FIRE TRAIL!!</color>");
		for (int i=0 ; i<fireTrails.Length ; i++)
		{
			if (fireTrails[i] != null && !fireTrails[i].activeSelf)
			{
				fireTrails[i].SetActive(true);
				fireTrails[i].transform.position = this.transform.position + offset;
				break;
			}
		}
	}

	//private void RotateDirection(Vector3 dir)
	//	=> model.rotation = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));

	IEnumerator RestCo()
	{
		yield return new WaitForSeconds(1);
		startPos = transform.position;

		int rng = Random.Range(0, points.Length);
		while (prevP == rng)
			rng = Random.Range(0, points.Length);
		prevP = rng;

		var p = points[prevP].position;
		destPos = new Vector3(p.x, 0, p.z);
		canMove = true;
	}
}

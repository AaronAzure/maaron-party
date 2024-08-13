using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet;

public class LavaMonster : NetworkBehaviour
{
	[SerializeField] private Transform[] points;
	[SerializeField] private float moveSpeed=9;
	private Vector3 startPos;
	private Vector3 destPos;
	private bool canMove;
	private bool isRest;
	float timer;
	int prevP;

	public override void OnStartClient()
	{
		base.OnStartClient();
		
	}

	private void Start() 
	{
		startPos = transform.position;
		prevP = Random.Range(0, points.Length);
		var p = points[prevP].position;
		destPos = new Vector3(p.x, 0, p.z);
		canMove = true;
	}

	public void FixedUpdateAction() 
	{
		if (canMove)
		{
			if (transform.position != destPos)
			{
				transform.position = Vector3.MoveTowards(transform.position, destPos, moveSpeed * Time.fixedDeltaTime);

				if (Vector3.Distance(transform.position, destPos) < 0.001f)
				{
					canMove = false;
					StartCoroutine(RestCo());
				}
			}
		}

	}

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

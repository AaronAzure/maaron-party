using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
	[SerializeField] private Node currNode;
	[SerializeField] private Node nextNode;
	[SerializeField] private float moveSpeed=2.5f;
	[SerializeField] private float rotateSpeed=5f;
	private Vector3 startPos;
	private float time;
	
	private void OnEnable() 
	{
		if (nextNode == null)
			this.enabled = false;
		startPos = this.transform.position;
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if (transform.position != nextNode.transform.position)
		{
			var lookPos = nextNode.transform.position + - transform.position;
			lookPos.y = 0;
			var rotation = Quaternion.LookRotation(lookPos);
			transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.fixedDeltaTime * rotateSpeed);

			transform.position = Vector3.Lerp(startPos, nextNode.transform.position, time);
			if (time < 1)
				time += Time.fixedDeltaTime * moveSpeed;
		}
		else
		{
			nextNode = nextNode.nextNodes[0];
			startPos = transform.position;
			time = 0;
		}
	}
}

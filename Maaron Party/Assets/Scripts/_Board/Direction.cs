using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Direction : MonoBehaviour
{
	public int index;
	[SerializeField] private PlayerControls p;
	[SerializeField] private MeshRenderer meshRenderer;
	[SerializeField] private Material normalMat;
	[SerializeField] private Material highlightMat;

	private void OnDisable() 
	{
		meshRenderer.material = normalMat;
	}

    private void OnMouseOver() 
	{
		meshRenderer.material = highlightMat;
		if (Input.GetMouseButtonDown(0))
		{
			p.ChoosePath(index);
		}
	}

	private void OnMouseExit() 
	{
		meshRenderer.material = normalMat;
	}
}

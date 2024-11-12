using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Direction1 : MonoBehaviour
{
	public int index;
	[SerializeField] private MeshRenderer meshRenderer;
	[SerializeField] private Material normalMat;
	[SerializeField] private Material highlightMat;

    private void OnMouseOver() 
	{
		meshRenderer.material = highlightMat;
		if (Input.GetMouseButtonDown(0))
		{
			//p.ChoosePath(index);
			//Debug.Log($"CLICKED {this.name}");
		}
	}

	private void OnMouseExit() 
	{
		meshRenderer.material = normalMat;
	}
}

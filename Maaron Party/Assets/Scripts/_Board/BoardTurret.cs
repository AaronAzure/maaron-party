using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BoardTurret : MonoBehaviour
{
	GameManager gm {get{return GameManager.Instance;}}
	[SerializeField] private SkinnedMeshRenderer[] renderers;
	[SerializeField] private Material[] emissionMats;
	[SerializeField] private GameObject cam;
	[SerializeField] private Transform turret;
	
	public void RemoteStart(int n, int rot) 
	{
		for (int i=0 ; i<n ; i++)
			if (i<renderers.Length && i<emissionMats.Length)
				renderers[i].material = emissionMats[i];
		RotateTurret(rot);
	}

	public void IncreaseReady(int n)
	{
		if (n < renderers.Length && renderers[n] != null &&
			n < emissionMats.Length && emissionMats[n] != null)
			renderers[n].material = emissionMats[n];
	}

	public void RotateTurret(int n)
	{
		turret.localRotation = Quaternion.Euler(45 - 90 * n, 90, 0);
	}

	public void ToggleCam(bool active) => cam.SetActive(active);
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardTurret : MonoBehaviour
{
	GameManager gm {get{return GameManager.Instance;}}
	[SerializeField] private SkinnedMeshRenderer[] renderers;
	[SerializeField] private Material[] emissionMats;
	[SerializeField] private GameObject cam;
	
	public void RemoteStart(int n) 
	{
		for (int i=0 ; i<n ; i++)
		{
			if (i<renderers.Length && i<emissionMats.Length)
				renderers[i].material = emissionMats[i];
		}
	}

	public void IncreaseReady(int n)
	{
		if (n < renderers.Length && renderers[n] != null &&
			n < emissionMats.Length && emissionMats[n] != null)
			renderers[n].material = emissionMats[n];
		//n = n + 1 >= 5 ? 0 : n + 1;
	}

	public void ToggleCam(bool active) => cam.SetActive(active);
}

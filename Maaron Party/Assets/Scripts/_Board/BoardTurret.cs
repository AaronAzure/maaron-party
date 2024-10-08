using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BoardTurret : MonoBehaviour
{
	//GameManager gm {get{return GameManager.Instance;}}
	[SerializeField] private ParticleSystem[] lightnings;
	[SerializeField] private SkinnedMeshRenderer[] renderers;
	[SerializeField] private Material[] normalMats;
	[SerializeField] private Material[] emissionMats;
	[SerializeField] private GameObject cam;
	[SerializeField] private Transform turret;
	[SerializeField] private Animator anim;

	[Space] [SerializeField] private SkinnedMeshRenderer turretRenderer;
	[SerializeField] private Material turretMat;
	[SerializeField] private Material turretEmissionMat;


	float _t;
	float init;
	float end;

	
	private void FixedUpdate() 
	{
		if (_t < 1)
		{
			_t += Time.fixedDeltaTime;
			turret.localRotation = Quaternion.Euler(45 - 90 * Mathf.Lerp(init, end, Mathf.SmoothStep(0,1,_t)), 90, 0);
		}
		else
			this.enabled = false;
	}
	public void RemoteStart(int n, int rot) 
	{
		for (int i=0 ; i<n ; i++)
		{
			if (i<renderers.Length && i<emissionMats.Length)
				renderers[i].material = emissionMats[i];
			if (i<lightnings.Length)
				lightnings[i].Play();
		}
		turret.localRotation = Quaternion.Euler(45 - 90 * rot, 90, 0);
		//RotateTurret(rot);
	}

	public void IncreaseReady(int n)
	{
		//Debug.Log($"<color=yellow>== TURRET {n-1}</color>");
		if (n-1 < renderers.Length && renderers[n-1] != null &&
			n-1 < emissionMats.Length && emissionMats[n-1] != null)
			renderers[n-1].material = emissionMats[n-1];
		if (n-1 < lightnings.Length)
			lightnings[n-1].Play();
		if (n == 5)
			StartCoroutine(FireTurretCo());
	}
	IEnumerator FireTurretCo()
	{
		yield return new WaitForSeconds(0.5f);
		anim.SetTrigger("fire");
		turretRenderer.material = turretEmissionMat;

		yield return new WaitForSeconds(7f);
		for (int i=0 ; i<5 ; i++)
		{
			if (i<renderers.Length && i<normalMats.Length)
				renderers[i].material = normalMats[i];
			if (i<lightnings.Length)
				lightnings[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
		}
		turretRenderer.material = turretMat;
	}

	public void RotateTurret(int n, int m)
	{
		_t = 0;
		init = n;
		end = m;
		this.enabled = true;
		//turret.localRotation = Quaternion.Euler(45 - 90 * n, 90, 0);
	}

	public void ToggleCam(bool active) => cam.SetActive(active);
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinemachineShake : MonoBehaviour
{
	public static CinemachineShake Instance;
	float intensity;
	float duration;
	float _t;
	CinemachineBasicMultiChannelPerlin bmcp;


	void Awake()
	{
	    Instance = this;
	}
	private void OnEnable() 
	{
		if (_t <= 0) this.enabled = false;
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if (_t > 0)
		{
			_t -= Time.fixedDeltaTime;
			if (bmcp != null)
				bmcp.m_AmplitudeGain = Mathf.Lerp(intensity, 0f, 1 - _t/duration);
			if (_t <= 0)
				this.enabled = false;
		}
	}
	public void ShakeCam(float intensity, float duration)
	{
		if (CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera != null)
		{
			bmcp = CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
			this.intensity = bmcp.m_AmplitudeGain = intensity;
			this.duration = _t = duration;
			this.enabled = true;
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameCamera : MonoBehaviour
{
	private void Start() 
	{
		if (SceneManager.sceneCount > 1)
			Destroy(this.gameObject);
	}
}

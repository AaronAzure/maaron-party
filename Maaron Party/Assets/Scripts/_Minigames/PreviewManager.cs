using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewManager : MonoBehaviour
{
	public static PreviewManager Instance;
	[SerializeField] private Animator anim;
	private GameManager gm;

	private void Awake() 
	{
		Instance = this;
	}

	// Start is called before the first frame update
	void Start()
	{
		gm = GameManager.Instance;
		gm.TriggerTransition(false);
	}

	public void TriggerTransition(bool fadeIn)
	{
		anim.SetTrigger(fadeIn ? "in" : "out");
	}
}

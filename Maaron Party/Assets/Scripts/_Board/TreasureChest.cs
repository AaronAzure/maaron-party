using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    [SerializeField] private Animator anim;
	[SerializeField] private GameObject highlightObj;
	[SerializeField] Collider col;
	private bool canChoose;
	[SerializeField] private TreasureChest[] otherChests;
	[HideInInspector] public int ind;
	BoardManager bm {get{return BoardManager.Instance;}}

	public void ToggleChooseable(bool active) 
	{
		canChoose = active;
		col.enabled = active;
		this.enabled = active;
		highlightObj.SetActive(false);
	}

	public void OpenChest()
	{
		anim.SetTrigger("open");
	}


	private void OnMouseOver() 
	{
		if (canChoose)
		{
			highlightObj.SetActive(true);
			if (Input.GetMouseButtonDown(0))
			{
				bm.CmdSelectChest(ind);
				foreach (TreasureChest chest in otherChests)
					chest.ToggleChooseable(false);
			}
		}
	}

	private void OnMouseExit() 
	{
		if (canChoose)
		{
			highlightObj.SetActive(false);
		}
	}
}

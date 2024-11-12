using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrickyTilesManager : MinigameController
{
	//[SerializeField] Animator[] anims;
	[SerializeField] private Material emptyMat;
	[SerializeField] private Material[] mats;
	[SerializeField] private TrickyTile[] anims;
	[SerializeField] private TrickyTileMarker[] markers;
	[SerializeField] private float cycle=3;
	int nRound;
	int nTraps=1;
	bool isTrapOpen;
	public List<int> selectedTraps = new();

	private void OnEnable() 
	{
		ChooseTraps();	
	}
	void FixedUpdate()
	{
		if (!isTrapOpen)
		{
			timer += Time.fixedDeltaTime;
			if (timer >= cycle)
			{
				isTrapOpen = true;
				timer = 0;
				foreach (int ind in selectedTraps)
					anims[ind].CmdTriggerTrap();
				for (int i = 0; i < selectedTraps.Count; i++)
					markers[i].CmdClearMaterial();
			}
		}
		else
		{
			timer += Time.fixedDeltaTime;
			if (timer > 1)
			{
				ChooseTraps();	
				timer = 0;
				isTrapOpen = false;
			}
		}
	}

	void ChooseTraps()
	{
		List<int> temp = new();
		for (int i = 0; i < anims.Length; i++)
			temp.Add(i);
		
		selectedTraps.Clear();
		for (int i = 0; i < nTraps; i++)
		{
			int rng = temp[Random.Range(0, temp.Count)];
			selectedTraps.Add(rng);
			temp.Remove(rng);
		}

		for (int i = 0; i < markers.Length; i++)
		{
			if (i<selectedTraps.Count)
				markers[i].CmdSetMaterial(selectedTraps[i]);
			else
				markers[i].CmdClearMaterial();
		}

		nRound++;
		if (nRound == 1) nTraps++;
		if (nRound == 2) nTraps++;
		if (nRound == 3) nTraps++;
		if (nRound == 5) nTraps++;
		if (nRound == 7) nTraps++;
		if (nRound == 9) nTraps++;
		if (nRound == 12) nTraps++;
	}
}

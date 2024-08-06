using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaMonsterManager : MonoBehaviour
{
	[SerializeField] private LavaMonster[] monsters;
	[SerializeField] private float delay=10;
	private int ind;
	private float timer;

	// Update is called once per frame
	void FixedUpdate()
	{
		if (ind == 0)
		{
			timer += Time.fixedDeltaTime;
			if (timer > 1.5f)
			{
				if (ind < monsters.Length)
					ind++;
				timer = 0;
			}
		}
		else
		{
			timer += Time.fixedDeltaTime;
			if (timer > delay)
			{
				if (ind < monsters.Length)
					ind++;
				timer = 0;
			}
			//for (int i=0 ; i<ind ; i++)
			if (ind >= 1)
				monsters[0].FixedUpdateAction();
			if (ind >= 2)
				monsters[1].FixedUpdateAction();
			if (ind >= 3)
				monsters[2].FixedUpdateAction();
			if (ind >= 4)
				monsters[3].FixedUpdateAction();
		}
	}
}

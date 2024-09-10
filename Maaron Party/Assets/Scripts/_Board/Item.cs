using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
	public static Item instance;
	
	public int ind;
	[SerializeField] private Sprite spellThorn1;
	[SerializeField] private Sprite spellThorn2;
	[SerializeField] private Sprite spellThorn3;
	[SerializeField] private Sprite spellFire1;
	[SerializeField] private Sprite spellFire2;
	[SerializeField] private Sprite spellFire3;
	[SerializeField] private Sprite spellSpeed1;
	[SerializeField] private Sprite spellSpeed2;
	[SerializeField] private Sprite spellSpeed3;

	private void Awake() 
	{
		if (instance == null)
			instance = this;
	}

	public Sprite GetSprite(int n)
	{
		return n switch
		{
			0 => spellThorn1,
			1 => spellThorn2,
			2 => spellThorn3,
			3 => spellFire1,
			4 => spellFire2,
			5 => spellFire3,
			6 => spellSpeed1,
			7 => spellSpeed2,
			8 => spellSpeed3,
			_ => null,
		};
	}
}

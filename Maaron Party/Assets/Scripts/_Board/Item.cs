using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
	public static Item instance;
	
	public int ind;
	[SerializeField] private Image img;
	
	[Space] [SerializeField] private PlayerControls p;
	[SerializeField] private Sprite emptySpr;
	[SerializeField] private Sprite spellThorn1;
	[SerializeField] private Sprite spellThorn2;
	[SerializeField] private Sprite spellThorn3;
	[SerializeField] private Sprite spellFire1;
	[SerializeField] private Sprite spellFire2;
	[SerializeField] private Sprite spellFire3;
	[SerializeField] private Sprite spellSpeed1;
	[SerializeField] private Sprite spellSpeed2;
	[SerializeField] private Sprite spellSpeed3;
	[SerializeField] private Sprite spellShield1;

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
			9 => spellShield1,
			_ => null,
		};
	}

	public void SetImage()
	{
		if (img != null)
			img.sprite = GetSprite(ind);
	}

	public void _USE_SPELL(int slot)
	{
		if (p != null) 
		{
			switch (ind)
			{
				case -1: break;
				case 0: p._USE_SPELL(slot, 0); break;
				case 1: p._USE_SPELL(slot, 1); break;

				case 6: p.UseDashSpell(4); break;
				case 7: p.UseDashSpell(8); break;
				case 8: p.UseDashSpell(12); break;

				case 9: p.UseShieldSpell(); break;

				default: p._USE_SPELL(slot, ind); break;
			}
		}
	}
}

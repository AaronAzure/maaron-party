using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Item : MonoBehaviour, IPointerEnterHandler
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

	[Space] [Header("Ui")]
	[SerializeField] private bool isUsuable;
	[SerializeField] private TextMeshProUGUI titleTxt;
	[SerializeField] private TextMeshProUGUI descTxt;
	[SerializeField] private TextMeshProUGUI manaTxt;

	private void Awake() 
	{
		if (instance == null)
			instance = this;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (isUsuable)
		{
			if (titleTxt != null) titleTxt.text = GetTitle(ind);
			if (descTxt != null) descTxt.text = GetDesc(ind);
			if (manaTxt != null) manaTxt.text = $"Mana Cost: {GetManaCost(ind)}";
		}
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
		if (p != null && HasEnoughMana(ind)) 
		{
			//p.ConsumeMana(GetManaCost(ind));
			switch (ind)
			{
				case -1: break;
				case 0: p._USE_SPELL(slot, 0); break;
				case 1: p._USE_SPELL(slot, 1); break;

				case 6: p.UseDashSpell(4, 2); break;
				case 7: p.UseDashSpell(8, 3); break;
				case 8: p.UseDashSpell(12, 4); break;

				case 9: p.UseShieldSpell(); break;

				// target node (fire)
				default: p._USE_SPELL(slot, ind); break;
			}
		}
	}

	public string GetTitle(int ind)
	{
		return ind switch
		{
			0 => "Thorn I",
			1 => "Thorn II",
			2 => "Thorn III",
			3 => "Fire II",
			4 => "Fire III",
			5 => "Fire IV",
			6 => "Speed II",
			7 => "Speed III",
			8 => "Speed IV",
			9 => "Shield II",
			_ => "",
		};
	}
	public string GetDesc(int ind)
	{
		return ind switch
		{
			0 => "Sets a trap, steal <b>10</b> coins from any opponents that lands on it >:)",
			1 => "Sets a trap, steal <b>20</b> coins from any opponents that lands on it >:)",
			2 => "Sets a trap, steal <b>Golden Watermelon</b> from any opponents that lands on it >:)",
			3 => "Shoots a fireball at a space, destroy <b>15</b> coins from all opponents on that space >:)",
			4 => "Shoots a fireball at a space, destroy <b>25</b> coins from all opponents on that space >:)",
			5 => "Shoots a fireball at a space, destroy <b>Golden Watermelon</b> coins from all opponents on that space >:)",
			6 => "Enhance yourself, and immediately move <b>4</b> spaces, then proceed with your turn >:)",
			7 => "Enhance yourself, and immediately move <b>8</b> spaces, then proceed with your turn >:)",
			8 => "Enhance yourself, and immediately move <b>12</b> spaces, then proceed with your turn >:)",
			9 => "spellShield1",
			_ => "",
		};
	}

	public int GetPrice(int ind)
	{
		return ind switch
		{
			0 => 5,		//spellThorn1
			1 => 10,	//spellThorn2
			2 => 20,	//spellThorn3
			3 => 10,	//spellFire1
			4 => 20,	//spellFire2
			5 => 40,	//spellFire3
			6 => 10,	//spellSpeed1
			7 => 20,	//spellSpeed2
			8 => 40,	//spellSpeed3
			9 => 10,	//spellShield1
			_ => 5,
		};
	}

	public int GetManaCost(int ind)
	{
		return ind switch
		{
			0 => 1, 	//spellThorn1
			1 => 2, 	//spellThorn2
			2 => 3, 	//spellThorn3
			3 => 2, 	//spellFire1
			4 => 3, 	//spellFire2
			5 => 4, 	//spellFire3
			6 => 2, 	//spellSpeed1
			7 => 3, 	//spellSpeed2
			8 => 4, 	//spellSpeed3
			9 => 2, 	//spellShield1
			_ => 1,
		};
	}

	public bool HasEnoughMana(int ind)
	{
		return ind switch
		{
			0 => p.GetMana() >= 1, 	//spellThorn1
			1 => p.GetMana() >= 2, 	//spellThorn2
			2 => p.GetMana() >= 3, 	//spellThorn3
			3 => p.GetMana() >= 2, 	//spellFire1
			4 => p.GetMana() >= 3, 	//spellFire2
			5 => p.GetMana() >= 4, 	//spellFire3
			6 => p.GetMana() >= 2, 	//spellSpeed1
			7 => p.GetMana() >= 3, 	//spellSpeed2
			8 => p.GetMana() >= 4, 	//spellSpeed3
			9 => p.GetMana() >= 2, 	//spellShield1
			_ => p.GetMana() >= 1,
		};
	}
}

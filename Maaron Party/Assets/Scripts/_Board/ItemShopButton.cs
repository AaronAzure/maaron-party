using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ItemShopButton : MonoBehaviour
//public class ItemShopButton : MonoBehaviour, IPointerEnterHandler
{
	public int ind;
	[SerializeField] private Image img;

	enum Availability {always, turn5, last5}
	[SerializeField] private Availability availability;
	GameManager gm {get{return GameManager.Instance;}}
	[SerializeField] private bool isReplace;

	[Space] [Header("Shop Desc")]
	[SerializeField] private bool isShop;
	[SerializeField] private BuyButton buyBtn;
	[SerializeField] private TextMeshProUGUI titleTxt;
	private string title;
	[SerializeField] private TextMeshProUGUI descTxt;
	private string desc;
	[SerializeField] private TextMeshProUGUI manaTxt;
	private int manaCost;
	[SerializeField] private TextMeshProUGUI priceTxt;
	private int priceCost;

	private void OnEnable() 
	{
		if (img != null)
			img.sprite = Item.instance.GetSprite(ind);
		if (!isReplace)
			gameObject.SetActive(availability == Availability.turn5 ? gm.nTurn.Value >= 5 :
				availability == Availability.last5 ? gm.nTurn.Value >= gm.maxTurns.Value - 4 : 
				availability == Availability.always);
		title = Item.instance.GetTitle(ind);
		desc = Item.instance.GetDesc(ind);
		manaCost = Item.instance.GetManaCost(ind);
		priceCost = Item.instance.GetPrice(ind);
	}

	public void _CHECK_OUT_ITEM()
	{
		if (isShop && Item.instance != null)
		{
			if (buyBtn != null) 
			{
				buyBtn.itemInd = ind;
				buyBtn.cost = Item.instance.GetPrice(ind);
				if (!buyBtn.gameObject.activeSelf)
					buyBtn.gameObject.SetActive(true);
			}
			if (titleTxt != null) titleTxt.text = title;
			if (descTxt != null) descTxt.text = desc;
			if (manaTxt != null) manaTxt.text = $"Mana: <sprite name=\"mana\">{manaCost}";
			if (priceTxt != null) priceTxt.text = $"Buy (<sprite name=\"coin\">{priceCost})";
		}
		else
			Debug.Log($"<color=red>Item.instance != null = {Item.instance != null}</color>");
	}

	public void _BUY_ITEM()
	{
		_CHECK_OUT_ITEM();
		//if (PlayerControls.Instance.GetCoins() >= priceCost)
		//	PlayerControls.Instance._BUY_ITEM(ind, priceCost);
		//else
		//	PlayerControls.Instance.NoCoinAlert();
	}

	//public void OnPointerEnter(PointerEventData eventData)
	//{
	//	_CHECK_OUT_ITEM();
	//}
}

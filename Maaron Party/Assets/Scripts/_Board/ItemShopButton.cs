using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemShopButton : MonoBehaviour
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
	[SerializeField] private TextMeshProUGUI descTxt;
	[SerializeField] private TextMeshProUGUI manaTxt;
	[SerializeField] private TextMeshProUGUI priceTxt;

	private void OnEnable() 
	{
		if (img != null)
			img.sprite = Item.instance.GetSprite(ind);
		if (!isReplace)
			gameObject.SetActive(availability == Availability.turn5 ? gm.nTurn >= 5 :
				availability == Availability.last5 ? gm.nTurn >= gm.maxTurns - 4 : 
				availability == Availability.always);
		//if (isShop && Item.instance != null)
		//{
		//	if (titleTxt != null) titleTxt.text = Item.instance.GetTitle(ind);
		//	if (descTxt != null) descTxt.text = Item.instance.GetDesc(ind);
		//	if (manaTxt != null) manaTxt.text = $"{Item.instance.GetManaCost(ind)}";
		//	if (priceTxt != null) priceTxt.text = $"{Item.instance.GetPrice(ind)}";
		//}
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
			if (titleTxt != null) titleTxt.text = Item.instance.GetTitle(ind);
			if (descTxt != null) descTxt.text = Item.instance.GetDesc(ind);
			if (manaTxt != null) manaTxt.text = $"Mana: {Item.instance.GetManaCost(ind)}";
			if (priceTxt != null) priceTxt.text = $"Buy ({Item.instance.GetPrice(ind)})";
		}
	}

	//public void OnPointerEnter(PointerEventData eventData)
	//{
	//	if (isShop && Item.instance != null)
	//	{
	//		if (titleTxt != null) titleTxt.text = Item.instance.GetTitle(ind);
	//		if (descTxt != null) descTxt.text = Item.instance.GetDesc(ind);
	//		if (manaTxt != null) manaTxt.text = $"{Item.instance.GetManaCost(ind)}";
	//		if (priceTxt != null) priceTxt.text = $"{Item.instance.GetPrice(ind)}";
	//	}
	//}
}

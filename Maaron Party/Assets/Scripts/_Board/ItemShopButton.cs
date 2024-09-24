using UnityEngine;
using UnityEngine.UI;

public class ItemShopButton : MonoBehaviour
{
	[SerializeField] int ind;
	[SerializeField] private Image img;

	enum Availability {always, turn5, last5}
	[SerializeField] private Availability availability;
	GameManager gm {get{return GameManager.Instance;}}

	private void Awake() 
	{
		if (img != null)
			img.sprite = Item.instance.GetSprite(ind);
		gameObject.SetActive(availability == Availability.turn5 ? gm.nTurn >= 5 :
			availability == Availability.last5 ? gm.nTurn >= gm.maxTurns - 5 : 
			availability == Availability.always);
	}
}

using UnityEngine;
using UnityEngine.UI;

public class ItemShopButton : MonoBehaviour
{
	public int ind;
	[SerializeField] private Image img;

	enum Availability {always, turn5, last5}
	[SerializeField] private Availability availability;
	GameManager gm {get{return GameManager.Instance;}}
	[SerializeField] private bool isReplace;

	private void OnEnable() 
	{
		if (img != null)
			img.sprite = Item.instance.GetSprite(ind);
		if (!isReplace)
			gameObject.SetActive(availability == Availability.turn5 ? gm.nTurn >= 5 :
				availability == Availability.last5 ? gm.nTurn >= gm.maxTurns - 4 : 
				availability == Availability.always);
	}
}

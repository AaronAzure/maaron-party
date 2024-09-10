using UnityEngine;
using UnityEngine.UI;

public class ItemShopButton : MonoBehaviour
{
	[SerializeField] int ind;
	[SerializeField] private Image img;

	private void Awake() 
	{
		if (img != null)
			img.sprite = Item.instance.GetSprite(ind);
	}
}

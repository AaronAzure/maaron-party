using UnityEngine;

public class BuyButton : MonoBehaviour
{
	public int itemInd;
	public int cost;
	PlayerControls pc { get { return PlayerControls.Instance; } }

	public void _BUY_ITEM()
	{
		if (pc.GetCoins() >= cost)
			pc._BUY_ITEM(itemInd, cost);
	}
}

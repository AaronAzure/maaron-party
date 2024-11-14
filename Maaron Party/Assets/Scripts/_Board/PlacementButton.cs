using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class PlacementButton : NetworkBehaviour
{
	PlayerControls pc {get{return PlayerControls.Instance;}}
	GameManager gm {get{return GameManager.Instance;}}
	BoardManager bm {get{return BoardManager.Instance;}}
	[SyncVar] public int placement;
	[SerializeField] private int ind;
	[SerializeField] private Image img;
	[SerializeField] private Image glow;
	[SerializeField] private Image pic;
	[SerializeField] private Button btn;
	[SerializeField] private TextMeshProUGUI txt;

	[Space] [SerializeField] private Sprite[] profilePics;
	[SerializeField] private Sprite cardFront;


	public void SetPlacement(int n, int ind) 
	{
		this.ind = ind;
		placement = n;
		switch (n)
		{
			case 0: txt.text = "1st"; break;
			case 1: txt.text = "2nd"; break;
			case 2: txt.text = "3rd"; break;
			case 3: txt.text = "4th"; break;
		}
	}
	
	public void _CHOOSE_CARD()
	{
		bm.DoneChoosingPlacement(placement);
	}
	public void ChooseCard()
	{
		bm.CmdRevealPlacementCard(ind, pc.id, pc.characterInd, placement, connectionToClient);
		pc.CmdSetOrder(placement);
		glow.enabled = btn.enabled = false;
		img.sprite = cardFront;
	}

	public void RevealCard(int characterInd)
	{
		if (characterInd >= 0 && characterInd < profilePics.Length)
		{
			pic.sprite = profilePics[characterInd];
			pic.gameObject.SetActive(true);
		}
		glow.enabled = btn.enabled = false;
		img.sprite = cardFront;
		txt.gameObject.SetActive(true);
	}
}

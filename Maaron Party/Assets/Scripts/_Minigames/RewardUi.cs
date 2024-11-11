using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardUi : MonoBehaviour
{

	[Space] [Header("Main")]
	[SerializeField] private RectTransform rect;
	[SerializeField] private GameObject[] profilePics;
	[SerializeField] private Image profileBg;
	[SerializeField] private TextMeshProUGUI coinTxt;
	[SerializeField] private TextMeshProUGUI starTxt;
	[SerializeField] private TextMeshProUGUI manaTxt;
	[SerializeField] private Slider manaSlds;

	[Space] [SerializeField] private GameObject textbox;
	[SerializeField] private TextMeshProUGUI textboxTxt;

	[Space] [SerializeField] private GameObject manaTextbox;
	[SerializeField] private TextMeshProUGUI manaTextboxTxt;


	public void SetUp(int characterInd, int order, int coins, int stars, int manas)
	{
		if (rect != null) rect.anchoredPosition = new Vector2(50, -90 - (180 * order));
		Debug.Log($"<color=cyan>rect = {rect.anchoredPosition} | order = {order}</color>");
		if (coinTxt != null) coinTxt.text = $"{coins}";
		if (starTxt != null) starTxt.text = $"{stars}";
		if (manaTxt != null) manaTxt.text = $"{manas}/5";
		if (manaSlds != null) manaSlds.value = manas;
		if (profilePics != null) profilePics[characterInd].SetActive(true);
		if (profileBg != null) profileBg.color = characterInd == 0 ? new Color(0.7f,0.13f,0.13f) : characterInd == 1 ? new Color(0.4f,0.7f,0.3f) 
				: characterInd == 2 ? new Color(0.85f,0.85f,0.5f) : new Color(0.7f,0.5f,0.8f);
	}

	public void ShowPrize(int prize)
	{
		textbox.SetActive(true);
		textboxTxt.text = $"+{prize}";
	}

	public void ShowManaPrize(int prize)
	{
		manaTextbox.SetActive(true);
		manaTextboxTxt.text = $"+{prize}";
	}
}

using System.Collections;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UiDialogue : MonoBehaviour
{
    public static UiDialogue Instance;
	[SerializeField] private Animator anim;
	[SerializeField] private TextMeshProUGUI speakerTxt;
	[SerializeField] private TextMeshProUGUI dialogueTxt;
	[SerializeField] private Button nextBtn;

	[Space] [SerializeField] private float textSpd=2;
	private string sentence;
	private FixedString128Bytes[] sentences;
	private int nSent;
	
	private void Awake() 
	{
		Instance = this;
		gameObject.SetActive(false);
	}

	public void CloseDialogue()
	{
		anim.SetTrigger("close");
	}
	public void ToggleButton(bool active)
	{
		nextBtn.gameObject.SetActive(active);
	}
	public void SetSentence(bool active, string speaker="", FixedString128Bytes[] sent=null)
	{
		speakerTxt.text = speaker;
		sentences = sent;
		nSent = 0;
		
		if (active)
		{
			gameObject.SetActive(false);
			gameObject.SetActive(active);
			StartCoroutine( DisplaySentenceCo() );
		}
		else
			anim.SetTrigger("close");
	}
	public void _NEXT_SENTENCE()
	{
		BoardManager.Instance.NextDialogue();
		BoardManager.Instance.NextDialogueServerRpc();
	}
	public void NextSentence()
	{
		nSent++;
		//gameObject.SetActive(active);
		if (sentences != null && nSent < sentences.Length)
			StartCoroutine( DisplaySentenceCo() );
		else
		{
			nextBtn.gameObject.SetActive(false);
			BoardManager.Instance.EndDialogue();
			BoardManager.Instance.EndDialogueServerRpc();
		}
	}

	IEnumerator DisplaySentenceCo()
	{
		dialogueTxt.text = "";
		if (sentences == null || nSent >= sentences.Length)
			yield break;
			
		yield return null;
		foreach (char letter in sentences[nSent].ConvertToString().ToCharArray())
		{
			dialogueTxt.text += letter;
			yield return null;
		}
	}
}

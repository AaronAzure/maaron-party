using UnityEngine;

public class Maaron : MonoBehaviour
{
	public static Maaron Instance;

	private void Awake() 
	{
		Instance = this;
	}
}

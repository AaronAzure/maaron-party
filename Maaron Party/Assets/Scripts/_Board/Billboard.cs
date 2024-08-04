using UnityEngine;

public class Billboard : MonoBehaviour
{
	public Transform cam;

    void FixedUpdate()
    {
        if (cam != null)
			transform.LookAt(cam);
    }
}

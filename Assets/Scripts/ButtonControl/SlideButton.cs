using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideButton : MonoBehaviour
{
	public void xmenu()
	{
		gameObject.SetActive(false);
	}
	
	public void HideIt()
	{
		if (gameObject.activeSelf)
			gameObject.SetActive(false);
		else
			gameObject.SetActive(true);
	}
}
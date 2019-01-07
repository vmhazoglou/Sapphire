using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour {

	public GameObject Console;
public void ShowHideConsole()
	{
		if (Console.active)
		{
			Console.SetActive(false);
		}
		else
		{
			Console.SetActive(true);
		}
	}
}

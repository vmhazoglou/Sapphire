using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnOnOffLights : MonoBehaviour {
	public GameObject SunLight, RimLight;
	public GameObject SunButton, RimButton;

public void OFSun()
	{
		if (SunButton.GetComponent<Toggle>().isOn)
		{
			SunLight.SetActive(true);
		}
		else { SunLight.SetActive(false); }
	}

	public void OFRim()
	{
		if (RimButton.GetComponent<Toggle>().isOn)
		{
			RimLight.SetActive(true);
		}
		else { RimLight.SetActive(false); }
	}
}

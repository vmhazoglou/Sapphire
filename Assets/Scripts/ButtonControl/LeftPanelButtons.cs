using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MixedRealityToolkit.Examples.UX.Controls;

public class LeftPanelButtons : MonoBehaviour {

	public GameObject File, Representations, Play, Scaling, Labels, Calculate, Settings;

	public void ShowLabels()
	{
		Representations.SetActive(false);
		File.SetActive(false);
		Scaling.SetActive(false);
        if (Labels.activeSelf == false)
            Labels.SetActive(true);
        else
            Labels.SetActive(false);
		Calculate.SetActive(false);
		Settings.SetActive(false);
	}
	public void FileSelectionWindow()
	{
        if (File.activeSelf == false)
            File.SetActive(true);
        else
            File.SetActive(false);
		Representations.SetActive(false);

        /*
		Play.SetActive(false);
		Scaling.SetActive(false);
		Labels.SetActive(false);
		Calculate.SetActive(false);
		Settings.SetActive(false);
        */
		//gameObject.GetComponent<PlayButton>().StopTraj(); // TODO: Get rid of FIND 
		//FrameSlider.GetComponent<SliderGestureControl>().SliderValue = 0;

		//ClearData();

	}
	public void ShowReps()
	{
        if (Representations.activeSelf == false)
            Representations.SetActive(true);
        else
            Representations.SetActive(false);
		File.SetActive(false);

        /*
		Scaling.SetActive(false);
		Labels.SetActive(false);
		Calculate.SetActive(false);
		Settings.SetActive(false);
        */
	}

	public void ShowScale()
	{
        if (Scaling.activeSelf == false)
            Scaling.SetActive(true);
        else
            Scaling.SetActive(false);
		Representations.SetActive(false);
		File.SetActive(false);
		Labels.SetActive(false);
		Calculate.SetActive(false);
		Settings.SetActive(false);
	}

	public void ShowCalculate()
	{
		Scaling.SetActive(false);
		Representations.SetActive(false);
		File.SetActive(false);
		Labels.SetActive(false);
        if (Calculate.activeSelf == false)
            Calculate.SetActive(true);
        else
            Calculate.SetActive(false);
		Settings.SetActive(false);
	}

	public void ShowSettings()
	{
		Scaling.SetActive(false);
		Representations.SetActive(false);
		File.SetActive(false);
		Labels.SetActive(false);
		Calculate.SetActive(false);
        if (Settings.activeSelf == false)
            Settings.SetActive(true);
        else
            Settings.SetActive(false);
	}


	public GameObject FrameSlider;
	public void ClearData()
	{

		Resources.UnloadUnusedAssets();

		GC.Collect();
	}
}

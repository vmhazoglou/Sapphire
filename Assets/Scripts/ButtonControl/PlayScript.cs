using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;
using MixedRealityToolkit.Examples.UX.Controls;

public class PlayScript : MonoBehaviour {

    //atominfo[,] molecule = GameObject.Find("Manager").GetComponent<LoadPDB>().molecule;
    //topinfo top = GameObject.Find("Manager").GetComponent<LoadPDB>().top;

    public int framenum, i;
    public string representation;
    public GameObject PauseButton, PlayButton, MoleculeCollection, HologramCollection, Manager;
    public float speed;

    public void PlayTraj()
    {
		PlayButton.SetActive(false);
        PauseButton.SetActive(true);
		FrameSlider.GetComponent<SliderGestureControl>().SetSpan(0, framenum - 1);
		SpeedSlider.GetComponent<SliderGestureControl>().SetSpan(0, 100);
		SpeedSlider.GetComponent<SliderGestureControl>().SetSliderValue(25);
		//Time.timeScale = 1;
		//GameObject.Find("PlayButton").AddComponent<PlayButton2>();

		// gets the total number of frames from Load
		//framenum = Manager.GetComponent<Load>().framenum;


		StartCoroutine("Traj");
    }

    IEnumerator Traj()
    {
        while (true)
        {
			//MoleculeCollection.transform.rotation = new Quaternion(0, 0, 0, 0);
            //Manager.GetComponent<DestroyPrimitive>().DestroyObjects();
            i = Convert.ToInt32(FrameSlider.GetComponent<SliderGestureControl>().SliderValue);

            if (Manager.GetComponent<Load>().dynamicBonds)
            {
                // TODO: Have bonds form/break depending on distance if dynamic bonds is turned on
            }

            // move the spheres for bns model so they are where they should be for current frame
            transform.parent.GetComponent<BallAndStick>().MoveSpheres(i);
            // update the text below the play button to display the current frame
            //txt.GetComponent<ChangeText>().currentFrame = i;
            //txt.GetComponent<ChangeText>().ChangeIt();

            speed = 100f/ ((SpeedSlider.GetComponent<SliderGestureControl>().SliderValue + 1) * framenum); // TODO: get this from another class which is for a slider or smth later

			// Wait for a small amount of time before proceeding
            yield return new WaitForSeconds(speed);
            // reset slider to 0 if it reaches the max value so it loops
            if (i == FrameSlider.GetComponent<SliderGestureControl>().MaxSliderValue)
            {
				FrameSlider.GetComponent<SliderGestureControl>().SetSliderValue(0);

                // move the spheres to where the should be for frame 0
                transform.parent.GetComponent<BallAndStick>().MoveSpheres(i);
                // make currentframe display 0
                //txt.GetComponent<ChangeText>().currentFrame = i;
                //txt.GetComponent<ChangeText>().ChangeIt();

                //wait for a small amount of time before proceeding
                yield return new WaitForSeconds(speed);
            }

			// increment the slider value to go to the next frame
			FrameSlider.GetComponent<SliderGestureControl>().SetSliderValue((Convert.ToInt32(FrameSlider.GetComponent<SliderGestureControl>().SliderValue)+1));
        }
    }
    /*
    public void Update()
    {
        i = Convert.ToInt32(slider.value);
    }
    */

    public void StopTraj()
    {
        PlayButton.SetActive(true);
        PauseButton.SetActive(false);
        StopCoroutine("Traj");

        //Time.timeScale = 0;// try not both stopping it and adjusting the timescale
    }

	public GameObject SpeedSlider;

	public void ChangeSpeed()
	{
		speed = 1f/ (SpeedSlider.GetComponent<SliderGestureControl>().SliderValue + 1);
	}

    public GameObject FrameSlider;
    public void Slider()
    {
        // Change frame to whatever user selected with slider
		i = Convert.ToInt32(FrameSlider.GetComponent<SliderGestureControl>().SliderValue);
        
        // Move spheres in case it is paused
        transform.parent.GetComponent<BallAndStick>().MoveSpheres(i);
    }
}

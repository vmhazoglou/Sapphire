using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BondDisText : MonoBehaviour {


    Text txt;
    public GameObject canvas;
    public int atom1, atom2;
    public GameObject Manager;

    // Use this for initialization
    void Start()
    {
        float x1, y1, z1, x2, y2, z2;
        x1 = Manager.GetComponent<Load>().molecule[Manager.GetComponent<PlayScript>().i, atom1].x;
        y1 = Manager.GetComponent<Load>().molecule[Manager.GetComponent<PlayScript>().i, atom1].y;
        z1 = Manager.GetComponent<Load>().molecule[Manager.GetComponent<PlayScript>().i, atom1].z;

        x2 = Manager.GetComponent<Load>().molecule[Manager.GetComponent<PlayScript>().i, atom2].x;
        y2 = Manager.GetComponent<Load>().molecule[Manager.GetComponent<PlayScript>().i, atom2].y;
        z2 = Manager.GetComponent<Load>().molecule[Manager.GetComponent<PlayScript>().i, atom2].z;

        Vector3 a = new Vector3(x1, y1, z1);
        Vector3 b = new Vector3(x2, y2, z2);

        Vector3 vector = new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        double distance = Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
        txt = gameObject.GetComponent<Text>();
        txt.text = string.Format("{0}", distance);
        canvas.transform.localScale = (a + b) / 2;
    }

    // TODO:

    // when atoms are clicked, this script (attached to the textbox object) will have the integer values for the atoms associated with it
    // it should then get the frame from PlayButton component on Manager, and can use the function in the Bonds struct in Load to get the distance
    // Make it so you can just input atoms into that function because the function can call molecule in load to calculate the distance
    // this textbox object then moves between the two atoms and changes its text to the distance
}

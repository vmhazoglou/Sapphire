using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cartoon : MonoBehaviour {

    public int beta;
    int a1, a2, a3, a4, frame;
    public GameObject Manager;
    string sym1, sym2, sym3, sym4;

    public void DrawCartoon()
    {
        frame = Manager.GetComponent<PlayScript>().i;

    }

}

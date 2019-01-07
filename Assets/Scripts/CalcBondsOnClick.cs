using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System;

public class CalcBondsOnClick : MonoBehaviour {

    string atom1, atom2;
    public GameObject Manager;

	// Use this for initialization
	void Start () {
		
	}

    void Update()
    {
        //RecognizeAtomClick();
    }
	/*
    void RecognizeAtomClick()
    {
        if (InteractibleManager.Instance.FocusedGameObject == gameObject)
        {
            if(atom1 == null)
            {
                atom1 = gameObject.transform.name;
                string modelregex = @"([0-9]+)";
                Regex model = new Regex(modelregex);
                Match m = model.Match(atom1);

                Manager.GetComponent<ManageBondCalcs>().atom1 = Convert.ToInt32(m);
            }

            if(atom2 == null)
            {
                atom2 = gameObject.transform.name;
                string modelregex = @"([0-9]+)";
                Regex model = new Regex(modelregex);
                Match m = model.Match(atom2);

                Manager.GetComponent<ManageBondCalcs>().atom2 = Convert.ToInt32(m);
            }


        }
    }
	*/
}

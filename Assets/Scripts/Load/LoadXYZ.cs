using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using UnityEngine.UI;

public class LoadXYZ : MonoBehaviour {

    public GameObject Manager, HologramCollection;
    public int framenum, matches;
    public List<string> Models = new List<string>();

    public string modelregex;

    public atominfo[,] molecule;
    string filestring;
    public topinfo top;

    public void hoopla()
    {
        for (int i = 0; i < 10000; i++)
            i = i + i;
    }
    public void LoadMolecule()
    {
        filestring = Manager.GetComponent<OpenFileButton>().filestring;

        // TODO: display an error if filestring is null


        // First we will get the frames using Regex.Split
        GetFrames();

        // Then we get information about each individual atom with each frame. Most important: Atom#, Element, X, Y, Z 
        GetAtomInfo();

        // fill out top.NATOM with the total number of atoms and pass it to Load
        Manager.GetComponent<Load>().top.NATOM = top.NATOM;

        // Fill our molecule struct and pass to Load
        Manager.GetComponent<Load>().molecule = molecule;

        // Fill out number of frames and pass to Load
        Manager.GetComponent<Load>().framenum = framenum;

        // Delete everything here
        molecule = null;
        filestring = null;
        framenum = 0;

        Models.Clear();
    }

    void GetFrames()
    {
		// We are just getting the top line of XYZ because this is the same in every frame
        string topline = @"[0-9]+(\n|\r)";
        Regex a1 = new Regex(topline);
        Match b1 = a1.Match(filestring);

		// The top line of an XYZ file is the number of atoms
        top.NATOM = Convert.ToInt32(b1.ToString());

		// We can use the top line and Regex.Split to create an array of the frames
        string[] substrings = Regex.Split(Manager.GetComponent<OpenFileButton>().filestring, $"^{b1.ToString()}(\n|\r)", RegexOptions.Multiline);

		// Skip the top one bc it's null
        foreach (string match in substrings.Skip(1)) // TODO: Replace some junk in LoadPDB by using LINQ's skip function
        {
            if (!Models.Contains(match))
			{
				Models.Add(match);
			}
        }

    }

    /*
     * 
     * Group 1: Element
     * Group 2: X
     * Group 3: Y
     * Group 4: Z
     *
     * 
     */

    void GetAtomInfo()
    {
        framenum = Models.Count();
        Manager.GetComponent<Load>().molecule = new atominfo[framenum, top.NATOM];
        molecule = new atominfo[framenum, top.NATOM];

        int thisframe = 0;
        foreach (string model in Models)
        {
			// This is to get the Atom#, Element, and X,Y,Z coord
            string reg = @"([A-Z][A-Z]?)\s*(\S*)\s*(\S*)\s*(\S*)";
            Regex r = new Regex(reg);
            Match m = r.Match(model);

            for (int a = 0; a < top.NATOM; a++)
            {
                Vector3 original = new Vector3(float.Parse(m.Groups[2].ToString()), float.Parse(m.Groups[3].ToString()), float.Parse(m.Groups[4].ToString()));
                //original = Fitbox.GetComponent<Fitbox>().toQuat*original;

				// Need this line because the HologramCollection is moved by the Fitbox when loading the app so we are correcting the X,Y,Z for that
                original = original + HologramCollection.transform.position;

                molecule[Models.IndexOf(model), a].x = original.x;
                molecule[Models.IndexOf(model), a].y = original.y;
                molecule[Models.IndexOf(model), a].z = original.z;

                molecule[Models.IndexOf(model), a].element = m.Groups[1].ToString();

                m = m.NextMatch();
            }
            thisframe += 1;
        }
    }

}

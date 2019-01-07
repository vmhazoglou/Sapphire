using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System;
using System.Linq;

public class LoadOrca : MonoBehaviour {

	public GameObject Manager, HologramCollection;
	public int framenum, matches;
	public List<string> Models = new List<string>();

	public string modelregex;

	public atominfo[,] molecule;
	string filestring;
	public topinfo top;

	public void LoadMolecule()
	{
		string filestring = Manager.GetComponent<OpenFileButton>().filestring;

		// TODO: display an error if filestring is null


		// First we will get the frames using Regex.Split
		GetFrames(filestring);
		// Then we get information about each individual atom with each frame. Most important: Atom#, Element, X, Y, Z 
		GetAtoms(filestring);

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

	void GetFrames(string filestring)
	{
		string[] substrings = Regex.Split(filestring, "CARTESIAN COORDINATES \\(ANGSTROEM\\)");

		foreach (string match in substrings.Skip(1))
		{
			string[] substrings2 = Regex.Split(match, "CARTESIAN COORDINATES \\(A.U.\\)");

			if (!Models.Contains(substrings2[0]))
			{
				Models.Add(substrings2[0]);
			}
		}

	}

	void GetAtoms(string filestring)
	{
		string reg = @"Number of atoms\s+\S+\s+([0-9]+)";
		Regex r = new Regex(reg);
		Match m = r.Match(filestring);
		top.NATOM = Convert.ToInt32(m.Groups[1].ToString());
	}

	/*
     * 
     * Group 1: Element
     * Group 2: X
     * Group 3: Y
     * Group 4: Z
     * Group 5: 
     * 
     */

	void GetAtomInfo()
	{
		framenum = Models.Count();
		Manager.GetComponent<Load>().molecule = new atominfo[framenum, top.NATOM];
		molecule = new atominfo[framenum, top.NATOM];

		foreach (string model in Models)
		{
			// This is to get the Atom#, Element, and X,Y,Z coord
			string reg = @"\r?\n?([A-Z]+)\s+(\S+)\s+(\S+)\s+(\S+)\r?\n?";
			Regex r = new Regex(reg);
			Match m = r.Match(model);
			for (int a = 0; a < top.NATOM; a++)
			{
				Vector3 original = new Vector3(float.Parse(m.Groups[2].ToString()) / 25, float.Parse(m.Groups[3].ToString()) / 25, float.Parse(m.Groups[4].ToString()) / 25);
				//original = Fitbox.GetComponent<Fitbox>().toQuat*original;

				// Need this line because the HologramCollection is moved by the Fitbox when loading the app so we are correcting the X,Y,Z for that
				original = original + HologramCollection.transform.position;

				molecule[Models.IndexOf(model), a].x = original.x;
				molecule[Models.IndexOf(model), a].y = original.y;
				molecule[Models.IndexOf(model), a].z = original.z;

				molecule[0, a].element = m.Groups[1].ToString();

				m = m.NextMatch();
			}
		}
	}

}

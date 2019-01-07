using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System;
using System.Linq;

public class LoadMOL2 : MonoBehaviour {

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
		string[] substrings = Regex.Split(filestring, "<TRIPOS>ATOM");

		foreach (string match in substrings.Skip(1))
		{
			string[] substrings2 = Regex.Split(match, "<TRIPOS>BOND");

			if (!Models.Contains(substrings2[0]))
			{
				Models.Add(substrings2[0]);
			}
		}

	}

	void GetAtoms(string filestring)
	{
		string[] lines = Regex.Split(filestring, @"\n");
		string reg = @"\s+([0-9]+)";
		Regex r = new Regex(reg);
		Match m = r.Match(lines[2]);
		top.NATOM = Convert.ToInt32(m.Groups[1].ToString());
	}

	/*
     * 
     * Group 1: AtomNumer
     * Group 2: Elemet Symbol
     * Group 3: X
     * Group 4: Y
     * Group 5: Z
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
			string reg = @"\r?\n?\s+([0-9]+)\s+(\S+)\s+(\S+)\s+(\S+)\s+(\S+)\s+(\S+)\s+(\S+)\s+(\S+)\s+(\S+)\r?\n?";
			Regex r = new Regex(reg);
			Match m = r.Match(model);
			for (int a = 0; a < top.NATOM; a++)
			{
				Vector3 original = new Vector3(float.Parse(m.Groups[3].ToString()) / 25, float.Parse(m.Groups[4].ToString()) / 25, float.Parse(m.Groups[5].ToString()) / 25);
				//original = Fitbox.GetComponent<Fitbox>().toQuat*original;

				// Need this line because the HologramCollection is moved by the Fitbox when loading the app so we are correcting the X,Y,Z for that
				original = original + HologramCollection.transform.position;

				molecule[Models.IndexOf(model), a].x = original.x;
				molecule[Models.IndexOf(model), a].y = original.y;
				molecule[Models.IndexOf(model), a].z = original.z;

				if (m.Groups[2].ToString().StartsWith("H"))
				{
					molecule[0, a].element = "H";
				}
				if (m.Groups[2].ToString().StartsWith("N"))
				{
					molecule[0, a].element = "N";
				}
				if (m.Groups[2].ToString().StartsWith("O"))
				{
					molecule[0, a].element = "O";
				}
				if (m.Groups[2].ToString().StartsWith("C"))
				{
					molecule[0, a].element = "C";
				}
				if (m.Groups[2].ToString().StartsWith("S"))
				{
					molecule[0, a].element = "S";
				}
				if (m.Groups[2].ToString().StartsWith("P"))
				{
					molecule[0, a].element = "P";
				}

				m = m.NextMatch();
			}
		}
	}

}

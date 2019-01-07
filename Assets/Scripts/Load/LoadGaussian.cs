using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;

public class LoadGaussian : MonoBehaviour {

	public GameObject Manager, HologramCollection;
	public int framenum, matches;
	public List<string> Models = new List<string>();

	public string modelregex;

	public atominfo[,] molecule;
	//string filestring;
	public GameObject txt;
	public topinfo top;

	public void LoadMolecule()
	{		
		// display an error if filestring is null

		GetFrames(Manager.GetComponent<OpenFileButton>().filestring);

		GetAtomInfo();

		Manager.GetComponent<Load>().top.NATOM = top.NATOM;
		Manager.GetComponent<Load>().molecule = molecule;
		Manager.GetComponent<Load>().framenum = framenum;

		molecule = null;
		//filestring = null;
		framenum = 0;
		Models.Clear();
	}

	void GetFrames(string filestring)
	{
		string[] substrings = Regex.Split(filestring, "Input orientation");

		foreach (string match in substrings.Skip(1))
		{
			string[] substrings2 = Regex.Split(match, "Distance matrix \\(angstroms\\)");

			if(!Models.Contains(substrings2[0]))
			{
				Models.Add(substrings2[0]);
			}
		}
		
	}

	/*
     * 
     * Group 1: Element
     * Group 2: Atomic number
     * Group 3: X
     * Group 4: Y
     * Group 5: Z
     * 
     */

	void GetAtomInfo()
	{
		top.NATOM = Regex.Matches(Models[0], @"\r?\n?\s*([0-9]+)\s*([0-9])\s*([0-9])\s*(\S*)\s*(\S*)\s*(\S*)\r?\n?").Count;

		framenum = Models.Count();
		Manager.GetComponent<Load>().molecule = new atominfo[framenum, top.NATOM];
		molecule = new atominfo[framenum, top.NATOM];

		foreach (string model in Models)
		{
			string reg = @"\r?\n?\s*([0-9]+)\s*([0-9])\s*([0-9])\s*(\S*)\s*(\S*)\s*(\S*)\r?\n?";
			Regex r = new Regex(reg);
			Match m = r.Match(model);

			for (int a = 0; a < top.NATOM; a++)
			{
				Vector3 original = new Vector3(float.Parse(m.Groups[4].Value) / 25f, float.Parse(m.Groups[5].Value) / 25f, float.Parse(m.Groups[6].Value) / 25f);
				original = original + HologramCollection.transform.position;

				molecule[Models.IndexOf(model), a].x = original.x;
				molecule[Models.IndexOf(model), a].y = original.y;
				molecule[Models.IndexOf(model), a].z = original.z;

				switch (m.Groups[2].ToString())
				{
					case "1":
						molecule[0, a].element = "H";
						break;
					case "6":
						molecule[0, a].element = "C";
						break;
					case "7":
						molecule[0, a].element = "N";
						break;
					case "8":
						molecule[0, a].element = "O";
						break;
					default:
						molecule[0, a].element = "S";
						break;
						// TODO: Add more elements here
						// There is a switch case like this in LoadAMBER as well so copy paste there
				}

				m = m.NextMatch();
			}

			//Debug.Log(molecule[Models.IndexOf(model), 1].x);
		}
	}
}

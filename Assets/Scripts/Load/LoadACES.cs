using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System;
using System.Linq;

public class LoadACES : MonoBehaviour
{

    public GameObject Manager, HologramCollection;
    public int framenum, matches;
    public List<string> Models = new List<string>();

    public string modelregex;

    public atominfo[,] molecule;
    string filestring;
    public GameObject txt;
    public topinfo top;

    public void LoadMolecule()
    {
        filestring = Manager.GetComponent<OpenFileButton>().filestring;

        // display an error if filestring is null

        GetFrames();
        //GetTotalAtoms();
        GetAtomInfo();

        Manager.GetComponent<Load>().top.NATOM = top.NATOM;
        Manager.GetComponent<Load>().molecule = molecule;
        Manager.GetComponent<Load>().framenum = framenum - 1;

        //txt.GetComponent<ChangeText>().totalFrames = framenum - 1;

        molecule = null;
        filestring = null;
        framenum = 0;
        Models.Clear();
    }

    void GetFrames()
    {
        string[] substrings = Regex.Split(Manager.GetComponent<OpenFileButton>().filestring, "Cartesian coordinates corresponding to internal");

        foreach (string match in substrings.Skip(1))
        {
            string[] substrings2 = Regex.Split(match, "Interatomic distance matrix");

            Models.Add(substrings2[0]);
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
        top.NATOM = Regex.Matches(Models[0], @"\r?\n?\s*([A-Z][A-Z]?)\s*([0-9]+)\s*(\S*)\s*(\S*)\s*(\S*)").Count;

        framenum = Models.Count();
        Manager.GetComponent<Load>().molecule = new atominfo[framenum, top.NATOM];
        molecule = new atominfo[framenum, top.NATOM];


        foreach (string model in Models)
        {
            string reg = @"\r?\n?\s*([A-Z][A-Z]?)\s*([0-9]+)\s*(\S*)\s*(\S*)\s*(\S*)";
            Regex r = new Regex(reg);
            Match m = r.Match(model);

            for (int a = 0; a < top.NATOM; a++)
            {
                Vector3 original = new Vector3(float.Parse(m.Groups[3].Value) / 25, float.Parse(m.Groups[4].Value) / 25, float.Parse(m.Groups[5].Value) / 25);
                //original = Fitbox.GetComponent<Fitbox>().toQuat*original;
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

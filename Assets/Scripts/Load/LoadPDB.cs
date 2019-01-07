using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Antlr4;

public class LoadPDB : MonoBehaviour
{
#pragma warning disable 0219
#pragma warning disable 0414
#pragma warning disable 0618

    public GameObject Manager, HologramCollection;
    public int framenum, matches;
    public List<string> Models = new List<string>();

    public string modelregex;

    public atominfo[,] molecule;
    string filestring;
    public Text txt;
    public topinfo top;

    public void LoadMolecule()
    {
        filestring = Manager.GetComponent<OpenFileButton>().filestring;

        // TODO: display an error if filestring is null
        if (filestring == null)
        {
            // Console text  += " filetring is null"
            // `goto` somewhere at end of this LoadMolecule() function
        }

        // First we will get the frames using Regex.Split
        double starttime = Time.realtimeSinceStartup;
        GetFrames();
        txt.text += $"\n GetFrames time : {Time.realtimeSinceStartup - starttime}"; starttime = Time.realtimeSinceStartup;
        // Then we get information about each individual atom with each frame. Most important: Atom#, Element, X, Y, Z 
        GetAtomInfo();
        txt.text += $"\n GetAtomInfo time : {Time.realtimeSinceStartup - starttime}"; starttime = Time.realtimeSinceStartup;

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

    static Regex r = new Regex("MODEL\\s+[0-9]", RegexOptions.Compiled);
    static Regex ter = new Regex("TER", RegexOptions.Compiled);

    
    void GetFrames()
    {

        Antlr4.Runtime.AntlrInputStream inputStream;
        
        double starttime = Time.realtimeSinceStartup;
        //Regex r = new Regex(@"MODEL\\s+[0-9]+");
        txt.text += $"\n 1was : {Time.realtimeSinceStartup - starttime}"; starttime = Time.realtimeSinceStartup;

        string[] substrings = r.Split(filestring);

        txt.text += $"\n 1 : {Time.realtimeSinceStartup - starttime}"; starttime = Time.realtimeSinceStartup;

        if (substrings.Length > 1)
        {
            foreach (string match in substrings.Skip(1))
            {
                //if (!Models.Contains(match))
                {
                    Models.Add(ter.Split(match)[0]);
                }
            }
            txt.text += $"\n 2: {Time.realtimeSinceStartup - starttime}"; starttime = Time.realtimeSinceStartup;

            framenum = Models.Count();
            top.NATOM = Convert.ToInt32(Regex.Match(substrings[1], "TER\\s+([0-9]+)").Groups[1].ToString()) - 1;
            txt.text += $"\n 3 : {Time.realtimeSinceStartup - starttime}"; starttime = Time.realtimeSinceStartup;

        }
        else
        {
            Models.Add("ATOM      1" + Regex.Split(substrings[0], "ATOM      1")[1]);
            framenum = 1;
            top.NATOM = Convert.ToInt32(Regex.Match(substrings[0], "TER\\s+([0-9]+)").Groups[1].ToString()) - 1;

        }
    }
    // (......)(.....)(.....)(.)(...).(.)(....)(.)...(........)(........)(........)(......)(......)......(....)(..)
    /// <summary>
    /// (1-6)    :   "ATOM"/"HETATM"/"TER"          :  CHAR     : Group 1
    /// (7-11)   :   Atom serial number             :  INT      : Group 2
    /// 12       :   BLANK
    /// (13-16)  :   Atom name                      :  CHAR     : Group 3
    /// (17)     :   Alternate location indicator   :  CHAR     : Group 4
    /// (18-20)  :   Residue name                   :  CHAR     : Group 5
    /// 21       :   BLANK
    /// (22)     :   Chain identifier               :  CHAR     : Group 6
    /// (23-26)  :   Residue sequence number        :  INT      : Group 7
    /// (27)     :   Code for insertions of residues:  CHAR     : Group 8
    /// 28-30    :   BLANK
    /// (31-38)  :   X orthogonal A coordinate      :  FLOAT    : Group 9
    /// (39-46)  :   Y orthogonal A coordinate      :  FLOAT    : Group 10
    /// (47-54)  :   Z orthogonal A coordinate      :  FLOAT    : Group 11
    /// (55-60)  :   Occupancy                      :  FLOAT    : Group 12
    /// (61-66)  :   Temperature factor             :  FLOAT    : Group 13
    /// (73-76)  :   Segment identifier             :  CHAR     : Group 14
    /// (77-78)  :   Element symbol                 :  CHAR     : Group 15
    /// </summary>
    /// 

    void GetAtomInfo()
    {
        Manager.GetComponent<Load>().molecule = new atominfo[framenum, top.NATOM];
        molecule = new atominfo[framenum, top.NATOM];
        Vector3 hc = HologramCollection.transform.position;
        double starttime = Time.realtimeSinceStartup;
        foreach (string model in Models)
         {
             string reg = "(......)(.....)(.....)(.)(...).(.)(....)(.)...(........)(........)(........)(......)(......)......(....)(..)";
             Regex r = new Regex(reg);
             Match m = r.Match(model);

             for (int a = 0; a < top.NATOM; a++)
             {
                // original should be unmodified, not divided by 25 here. this is not the place for rescaling. localposition is used in movespheres() already
                Vector3 original = new Vector3(float.Parse(m.Groups[9].ToString().Trim()) , float.Parse(m.Groups[10].ToString().Trim()) , float.Parse(m.Groups[11].ToString().Trim()) );
                //original = Fitbox.GetComponent<Fitbox>().toQuat*original;

                // Need this line because the HologramCollection is moved by the Fitbox when loading the app so we are correcting the X,Y,Z for that
                original = original + hc;

                 molecule[Models.IndexOf(model), a].x = original.x;
                 molecule[Models.IndexOf(model), a].y = original.y;
                 molecule[Models.IndexOf(model), a].z = original.z;

                 molecule[0, a].atomsym = m.Groups[3].ToString().Trim();
                 molecule[0, a].element = m.Groups[15].ToString().Trim();
                 molecule[0, a].resnum = Convert.ToInt32(m.Groups[7].ToString().Trim());
                 molecule[0, a].resid = m.Groups[5].ToString().Trim();

                 m = m.NextMatch();
             }
         }
        

        Debug.Log($"time to load: {Time.realtimeSinceStartup - starttime}");
    }

}
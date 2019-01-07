
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class LoadAMBER : MonoBehaviour
{
#pragma warning disable 0219
#pragma warning disable 0414
#pragma warning disable 0618
    Vector3 offset = new Vector3(0, 0, 2);

// clean this up below

    public int rescale , adjustUnits;

    public string topfilestring, xfilestring;
    public int framenum, totalatoms;
	public List<string> Models = new List<string>(); // changed from public string[] Models
	public string modelregex;
    public atominfo[,] molecule;
    public topinfo top;
    public GameObject sphere, Manager,MoleculeCollection, txt, HologramCollection, Fitbox;
    string temp;
    public string test;
    Match m;
    Dictionary<string, bool> Beta = new Dictionary<string, bool>();
    public int beta;
    bool contained;
    string constr;
    public int totbonds;
    public Dictionary<string, double> bonddistances = new Dictionary<string, double>();
    float x1, x2, y1, y2, z1, z2;
    public double distance;

    public void LoadMolecule()
    {
        if (Manager.GetComponent<Dropdowns>().filetypeSelected == "AMBER Parm")
        {
            topfilestring = Manager.GetComponent<OpenFileButton>().filestring;

            LoadTopology();
        }

        if (Manager.GetComponent<Dropdowns>().filetypeSelected == "AMBER Coord")
        {
            xfilestring = Manager.GetComponent<OpenFileButton>().filestring;

            GetFrameNum();
            GetCrds();
            GetAtomInfo();

            Manager.GetComponent<Load>().molecule = molecule;
            Manager.GetComponent<Load>().top = top;
			Manager.GetComponent<Load>().framenum = framenum;

			molecule = null;
			topfilestring = null;
			xfilestring = null;
			framenum = 0;

			Models.Clear();
		}
    }

    public void GetAtomInfo()
    {
        // split by %FORMAT\(20a4\)\s+\r?\n?
        // then just (....) will get all matches
        // loop through matches per atom to get atom types
        // maybe for now: if water, break;
        // get CA atoms somewhere else for drawing ribbon
        string endfile = Regex.Split(topfilestring, @"%FLAG ATOM_NAME\s+\r?\%FORMAT\(20a4\)\s+\r?\n?")[1];
        string justatomtypes = Regex.Split(topfilestring, @"%FLAG CHARGE")[0];
        string modelregex = @"(....)";
        Regex model = new Regex(modelregex);
        Match m = model.Match(justatomtypes);
        string bla = m.Groups[1].ToString();

        string mm = @"\s*([0-9]+)";
        Regex mmm = new Regex(mm);
        Match b = mmm.Match(bla);

            int a = 0;

        // SECOND %FORMAT(10I8) GIVES YOU ELEMENT NUMBER
        while (b.Success)
            {
                switch (Convert.ToInt32(b.Groups[1].ToString()))
                {
                    case 1:
                        molecule[0, a].element = "H";
                        break;
                    case 6:
                        molecule[0, a].element = "C";
                        break;
                    case 7:
                        molecule[0, a].element = "N";
                        break;
                    case 8:
                        molecule[0, a].element = "O";
                        break;
                    default:
                        molecule[0, a].element = "S";
                        break;
                }
                b = b.NextMatch();
                a++;
            }
    }

    public void LoadTopology()
    {
        string modelregex = @"%FORMAT\(10I8\)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)";
        Regex model = new Regex(modelregex);
        Match m = model.Match(topfilestring);

        top.NATOM = Convert.ToInt32(m.Groups[1].ToString());
        top.NTYPES = Convert.ToInt32(m.Groups[2].ToString());
        top.NBONH = Convert.ToInt32(m.Groups[3].ToString());
        top.MBONA = Convert.ToInt32(m.Groups[4].ToString());
        top.NTHETH = Convert.ToInt32(m.Groups[5].ToString());
        top.MTHETA = Convert.ToInt32(m.Groups[6].ToString());
        top.NPHIH = Convert.ToInt32(m.Groups[7].ToString());
        top.MPHIA = Convert.ToInt32(m.Groups[8].ToString());
        top.NHPARM = Convert.ToInt32(m.Groups[9].ToString());
        top.NPARM = Convert.ToInt32(m.Groups[10].ToString());
        top.NNB = Convert.ToInt32(m.Groups[11].ToString());
        top.NRES = Convert.ToInt32(m.Groups[12].ToString());
        top.NBONA = Convert.ToInt32(m.Groups[13].ToString());
        top.NTHETA = Convert.ToInt32(m.Groups[14].ToString());
        top.NPHIA = Convert.ToInt32(m.Groups[15].ToString());
        top.NUMBND = Convert.ToInt32(m.Groups[16].ToString());
        top.NUMANG = Convert.ToInt32(m.Groups[17].ToString());
        top.NPTRA = Convert.ToInt32(m.Groups[18].ToString());
        top.NATYP = Convert.ToInt32(m.Groups[19].ToString());
        top.NPHB = Convert.ToInt32(m.Groups[20].ToString());
        top.IFPERT = Convert.ToInt32(m.Groups[21].ToString());
        top.NBPER = Convert.ToInt32(m.Groups[22].ToString());
        top.NGPER = Convert.ToInt32(m.Groups[23].ToString());
        top.NDPER = Convert.ToInt32(m.Groups[24].ToString());
        top.MBPER = Convert.ToInt32(m.Groups[25].ToString());
        top.MGPER = Convert.ToInt32(m.Groups[26].ToString());
        top.MDPER = Convert.ToInt32(m.Groups[27].ToString());
        top.IFBOX = Convert.ToInt32(m.Groups[28].ToString());
        top.NMXRS = Convert.ToInt32(m.Groups[29].ToString());
        top.IFCAP = Convert.ToInt32(m.Groups[30].ToString());
        top.NUMEXTRA = Convert.ToInt32(m.Groups[31].ToString());
        top.NCOPY = Convert.ToInt32(m.Groups[32].ToString());
    }


    // you get a block of 3D coordinates for each frame here
    public void GetFrames()
    {
        //GetFrameNum();
        //Models = new string[framenum];

        double lines = top.NATOM * 3 / 10;
		// round down with Math.Floor so that you get all lines except last, then you get the last line using the modulo

		// ******************************************************************
		// ******************************************************************
		// ******************************************************************
		// ******************************************************************
		//***************************************************************
		// TODO: Double check this next line. Might need a new AMBER reader.

		string modelregex = string.Format("((\\s+\\S+){{{0}}}\\r?\\n){{{1}}}(\\s+\\S+){{{2}}}", 10, Math.Floor(lines), top.NATOM * 3 % 10);

		// ******************************************************************
		// ******************************************************************
		// ******************************************************************
		// ******************************************************************
		// ******************************************************************
		// ******************************************************************



		Regex model = new Regex(modelregex);
        Match m = model.Match(xfilestring);

        for (int i = 0; i < framenum; i++)
        {
			Models.Add(m.Groups[0].ToString());
			//Models[i] = m.Groups[0].ToString();
			m = m.NextMatch();
        }
        //return Models;
    }

    public int GetFrameNum()
    {
        double lines = top.NATOM * 3 / 10;
        int line = Convert.ToInt32(Math.Floor(lines));

        string modelregex = string.Format("((\\s+\\S+){{{0}}}\\r?\\n){{{1}}} ", 10, line);
        Regex model = new Regex(modelregex);
        Match m = model.Match(xfilestring);

        while (m.Success)
        {
            framenum++;
            m = m.NextMatch();
        }
        framenum--;

        return framenum++;
    }

    atominfo[,] GetCrds()
    {
        molecule = new atominfo[framenum, top.NATOM];
        //Models = new string[framenum];
        GetFrames();
        rescale = 25;
        for (int i = 0; i < framenum; i++)
        {
            temp = Models[i];
            string pattern = @"\s+(\S+)"; /// (x) (y) (z) {top.NATOM}
            Regex r = new Regex(pattern);
            m = r.Match(temp);
            for (int a = 0; a < top.NATOM; a++)
            {

               // molecule[i, a].atomnum = a+1;// Convert.ToInt32(m.Groups[1].ToString());
                //molecule[i, a].atomsym = m.Groups[2].ToString();
                //molecule[i, a].resid = m.Groups[3].ToString();
                //molecule[i, a].resnum = Convert.ToInt32(m.Groups[5].ToString());

                //var fmt = new NumberFormatInfo();
                //fmt.NegativeSign = "-";
                molecule[i, a].x = float.Parse(m.Groups[1].ToString()) / rescale;
                m = m.NextMatch();

                molecule[i, a].y = float.Parse(m.Groups[1].ToString()) / rescale;
                m = m.NextMatch();

                molecule[i, a].z = float.Parse(m.Groups[1].ToString()) / rescale;

                //molecule[i, a].element = m.Groups[11].ToString();

                m = m.NextMatch();
            }
        }
        return molecule;
    }
}

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;

#pragma warning disable 0618

public class ViewLine : MonoBehaviour
{
    public GameObject HologramCollection, MoleculeCollection, AtomCollection;
    private Dictionary<string, double> bonddistances;
	Vector3 offset;
    atominfo[,] molecule;
    public GameObject Manager, Play;
    public LineRenderer lr;

    public void Line()
    {
        //frame = Play.GetComponent<PlayScript>().i;
        Play.GetComponent<PlayScript>().representation = "Line";
        transform.GetChild(0).gameObject.SetActive(false);

    }

    public Material whiteDiffuseMat;

    public void CreateLines(atominfo[,] molecule)
    {
        //float starttime = Time.realtimeSinceStartup;
        foreach (Bonds k in GetComponent<ThisFileInfo>().Bonds[0])
        {
            GameObject myLine = new GameObject();
            myLine.tag = "Sticks";

            myLine.AddComponent<LineRenderer>();
            myLine.name = string.Format("Line{0}.{1}", k.a1, k.a2);
            lr = myLine.GetComponent<LineRenderer>();
            lr.SetWidth(0.003f, 0.003f);
            lr.material = mat;
            lr.useWorldSpace = false;
            lr.startColor = Colors(molecule[0, k.a1].element);
            lr.endColor = Colors(molecule[0, k.a2].element);
            
            float x1, y1, z1, x2, y2, z2;
            x1 = molecule[0, k.a1].x;
            y1 = molecule[0, k.a1].y;
            z1 = molecule[0, k.a1].z;
            x2 = molecule[0, k.a2].x;
            y2 = molecule[0, k.a2].y;
            z2 = molecule[0, k.a2].z;

            Vector3 firstatom = new Vector3(x1, y1, z1) * 0.5f;
            Vector3 secondatom = new Vector3(x2, y2, z2) * 0.5f;

			offset = new Vector3(0, 0,0f + MoleculeCollection.transform.position.z);

			lr.SetPosition(0, firstatom+offset);
            lr.SetPosition(1, secondatom+offset);
			lr.transform.parent = transform.GetChild(1).gameObject.transform;
        }
        //Debug.Log($"Time: {Time.realtimeSinceStartup - starttime}");
    }

    public Material mat;

    public void MoveLine(int frame, atominfo[,] molecule)
    {
        int atom1, atom2;
        GameObject sphereone, spheretwo;

        if (GetComponent<ThisFileInfo>().dynamicBonds == true)
        {
            // Check for bonds which exist for the frame but were not constructed in a previous frame
            foreach (Bonds k in GetComponent<ThisFileInfo>().Bonds[frame])
            {
                Transform[] children = transform.GetChild(1).transform.GetComponentsInChildren<Transform>();
                string[] childrenNames = new string[children.Length];

                foreach (Transform c in children)
                {
                    childrenNames[Array.IndexOf(children, c)] = c.name;
                }

                if (!childrenNames.Contains($"Line{k.a1}.{k.a2}"))
                {
                    GameObject myLine = new GameObject();

                    myLine.tag = "Sticks";

                    myLine.AddComponent<LineRenderer>();
                    myLine.name = string.Format("Line{0}.{1}", k.a1, k.a2);
                    lr = myLine.GetComponent<LineRenderer>();
                    lr.SetWidth(0.003f, 0.003f);
                    lr.material = mat;
                    lr.useWorldSpace = false;
                    lr.startColor = Colors(molecule[frame, k.a1].element);
                    lr.endColor = Colors(molecule[frame, k.a2].element);

                    float x1, y1, z1, x2, y2, z2;
                    x1 = molecule[frame, k.a1].x;
                    y1 = molecule[frame, k.a1].y;
                    z1 = molecule[frame, k.a1].z;
                    x2 = molecule[frame, k.a2].x;
                    y2 = molecule[frame, k.a2].y;
                    z2 = molecule[frame, k.a2].z;

                    Vector3 firstatom = new Vector3(x1, y1, z1) * 0.5f;
                    Vector3 secondatom = new Vector3(x2, y2, z2) * 0.5f;

                    offset = new Vector3(0, 0, 0f + MoleculeCollection.transform.position.z);

                    lr.transform.parent = transform.GetChild(1).gameObject.transform;

                    lr.SetPosition(0, firstatom + offset);
                    lr.SetPosition(1, secondatom + offset);

                    myLine.transform.localPosition = new Vector3(0, 0, -1);
                    myLine.transform.localRotation = new Quaternion(0,0,0,1);
                    myLine.transform.localScale = Vector3.one;
                }
            }
        }

        // Check for bonds which are extra for this frame and delete them, then update the lines' positions for the frame
        foreach (Transform k in transform.GetChild(1).GetComponentInChildren<Transform>())
        {
            atom1 = Convert.ToInt32(Regex.Match(k.name, $"Line([0-9]+).([0-9]+)").Groups[1].ToString());
            atom2 = Convert.ToInt32(Regex.Match(k.name, $"Line([0-9]+).([0-9]+)").Groups[2].ToString());

            if (GetComponent<ThisFileInfo>().dynamicBonds == true)
            {
                string[] bondNames = new string[GetComponent<ThisFileInfo>().Bonds[frame].Count];
                foreach (Bonds m in GetComponent<ThisFileInfo>().Bonds[frame])
                {
                    bondNames[GetComponent<ThisFileInfo>().Bonds[frame].IndexOf(m)] = $"Line{m.a1}.{m.a2}";
                }

                if (!bondNames.Contains(k.name))
                {
                    Destroy(k.gameObject);
                    goto SkipMove; // line 159
                }
            }

            sphereone = AtomCollection.transform.GetChild(atom1).gameObject;
            spheretwo = AtomCollection.transform.GetChild(atom2).gameObject;

            float x1, y1, z1, x2, y2, z2;
            x1 = sphereone.transform.localPosition.x;
            y1 = sphereone.transform.localPosition.y;
            z1 = sphereone.transform.localPosition.z + 1;
            x2 = spheretwo.transform.localPosition.x;
            y2 = spheretwo.transform.localPosition.y;
            z2 = spheretwo.transform.localPosition.z + 1;

            Vector3 firstatom = new Vector3(x1, y1, z1);
            Vector3 secondatom = new Vector3(x2, y2, z2);

            GameObject myLine = k.gameObject;

            lr = myLine.GetComponent<LineRenderer>();

            lr.SetPosition(0, firstatom);
            lr.SetPosition(1, secondatom);

            SkipMove:
            lr.useWorldSpace = false;
        }

        //Debug.Log($"Move Time: {Time.realtimeSinceStartup - sttime}");
    }

    public bool CheckBond(int atom1, int atom2)
    {
        // Check if Bonds[frame] contains atom1. atom2
        // go if Bonds[frame].atom1 == atom1 && Bonds[frame].atom2 == atom2 then
        // bondexists

        return true;
    }

    public Color DarkGrey, Sulfur;

    Color Colors(string element)
    {
        switch (element)
        {
            case "O":
                return Color.red;
            case "C":
                return DarkGrey;
            case "N":
                return Color.blue;
            case "H":
                return Color.white;
            case "S":
                return Sulfur;
            default:
                return Color.cyan;
        }
    }

    public GameObject[] bonds;

    // unused
    public void ChangeLineThickness(float resizeFactor)
    {
        foreach (Transform bond in transform.GetChild(1).gameObject.transform)
        {
            lr = bond.GetComponent<LineRenderer>();
            lr.SetWidth(0.002f* resizeFactor, 0.002f* resizeFactor);

        }
    }

}

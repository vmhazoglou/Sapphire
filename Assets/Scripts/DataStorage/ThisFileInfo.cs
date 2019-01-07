using System.Collections.Generic;
using UnityEngine;


public class ThisFileInfo : MonoBehaviour
{
    public atominfo[,] molecule;
    public topinfo top;

    public List<Bonds>[] Bonds;
    public List<Angles> Angles;
    public List<Dihedrals> Dihedrals;

    public int FileNumber, TotalFrames;
    public string FileName;
    public bool dynamicBonds;
}
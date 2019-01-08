using UnityEngine;
using Antlr4.Runtime;
using ClassLibrary2;
using System;
using System.Threading;
using System.Threading.Tasks;

public class ParseXYZ : MonoBehaviour
{
    public GameObject Manager, HologramCollection;
    public int framenum, matches;

    public atominfo[,] molecule;
    public string filestring;
    public topinfo top;

    AntlrInputStream istream;
    XYZLexer lexer;
    CommonTokenStream tokens;
    XYZParser parser;
    XYZParser.FileContext fileContext;
    XYZParser.FrameContext firstFrame;

    int atom, frame;
    int atomX, frameX;
    int atomY, frameY;
    int atomZ, frameZ;
    int atomE, frameE;

    public void LoadMolecule()
	{
#if !UNITY_EDITOR
		filestring = Manager.GetComponent<OpenFileButton>().filestring;
#endif

		// First we will get all info from the frames using ANTLR4
		// We start this on new threads and then join them at the end for a small processing speedup

		istream = new AntlrInputStream(filestring);
        lexer = new XYZLexer(istream);
        tokens = new CommonTokenStream(lexer);
        parser = new XYZParser(tokens);

        // takes 0.5 seconds to parse
        fileContext = parser.file();
        //

        firstFrame = fileContext.frame()[0];
        top.NATOM = Convert.ToInt32(firstFrame.header().GetText());
        framenum = fileContext.frame().Length;
        molecule = new atominfo[framenum, top.NATOM];

        //double st = Time.realtimeSinceStartup;

        atomX = 0; frameX = 0;
        atomY = 0; frameY = 0;
        atomZ = 0; frameZ = 0;
        atomE = 0; frameE = 0;
        var X = new Task(() => GetX());
        var Y = new Task(() => GetY());
        var Z = new Task(() => GetZ());
        var E = new Task(() => GetE());

        X.Start();
        Y.Start();
        Z.Start();
        E.Start();

        X.Wait();
        Y.Wait();
        Z.Wait();
        E.Wait();
        
        // Debug.Log($"0:{Time.realtimeSinceStartup - st}");

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
    }

    // Slower, unused
    private void GetAll()
    {
        foreach (XYZParser.FrameContext frameContext in fileContext.frame())
        {
            atom = 0;
            foreach (XYZParser.LineContext lineContext in frameContext.line())
            {
                molecule[frame, atom].x = float.Parse(lineContext.FLOAT()[0].ToString());
                molecule[frame, atom].y = float.Parse(lineContext.FLOAT()[1].ToString());
                molecule[frame, atom].z = float.Parse(lineContext.FLOAT()[2].ToString());
                molecule[frame, atom].element = lineContext.ELEMENT().ToString();

                atom += 1;
            }
            frame += 1;
        }
    }

    private void GetX()
    {
        foreach (XYZParser.FrameContext frameContext in fileContext.frame())
        {
            atomX = 0;
            foreach (XYZParser.LineContext lineContext in frameContext.line())
            {
                molecule[frameX, atomX].x = float.Parse(lineContext.FLOAT()[0].ToString());
                atomX += 1;
            }
            frameX += 1;
        }
    }

    private void GetY()
    {
        foreach (XYZParser.FrameContext frameContext in fileContext.frame())
        {
            atomY = 0;
            foreach (XYZParser.LineContext lineContext in frameContext.line())
            {
                molecule[frameY, atomY].y = float.Parse(lineContext.FLOAT()[1].ToString());
                atomY += 1;
            }
            frameY += 1;
        }
    }

    private void GetZ()
    {
        foreach (XYZParser.FrameContext frameContext in fileContext.frame())
        {
            atomZ = 0;
            foreach (XYZParser.LineContext lineContext in frameContext.line())
            {
                molecule[frameZ, atomZ].z = float.Parse(lineContext.FLOAT()[2].ToString());
                atomZ += 1;
            }
            frameZ += 1;
        }
    }

    private void GetE()
    {
        foreach (XYZParser.FrameContext frameContext in fileContext.frame())
        {
            atomE = 0;
            foreach (XYZParser.LineContext lineContext in frameContext.line())
            {
                molecule[frameE, atomE].element = lineContext.ELEMENT().ToString();
                atomE += 1;
            }
            frameE += 1;
        }
    }
}
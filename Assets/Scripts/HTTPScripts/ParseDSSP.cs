using MixedRealityToolkit.UX.AppBarControl;
using MixedRealityToolkit.UX.BoundingBoxes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Data.AppColors;
using System.Threading.Tasks;

// This script needs to be cleaned up

public class ParseDSSP : MonoBehaviour
{
    public string DSSPfile;
    public float tiltFactor;
    float topY = -99999, bottomY = 99999, leftX = 99999, rightX = -99999, frontZ = 99999, backZ = -99999;

    // This isn't really used much
    public struct SecondaryStructure
    {
        public int resnum, res; // Residue Number according to DSSP. res num according to PDb
        public string aa, structure; // One letter amino acid code. Secondary structure assignment.
        public float phi, psi, xca, yca, zca;
        public Vector3 CA;
    }

    // http://www.cmbi.ru.nl/dssp.html

    /// <Groups>
    /// Group 1:			Resnum (DSSP)
    /// Group 2:			Resnum (PDB)
    /// group 3:			? letter associated with residue number in pdb
    /// Group 4:			one letter aa code
    /// *Group 5:			Secondary structure assignment
    /// Group x:			Additional info on secondary structure assignment (needs its own separate regex)
    /// Groups 5-15:		BP1 BP2  ACC     N-H-->O    O-->H-N    N-H-->O    O-->H-N    TCO  KAPPA ALPHA
    /// Groups 16-17:		PHI/PSI angles
    /// *Groups 18-21:		X-CA   Y-CA   Z-CA
    /// </summary>

    /// <SecondaryStructure>
    /// H		Alpha Helix
    /// B		Beta bridge
    /// E		Strand
    /// G		Helix-3
    /// I		Helix-5
    /// T		Turn
    /// S		Bend
    /// </summary>

    public Text ConsoleText;
    float startTime, finishTime;
    public void ParseFile()
    {
        topY = -99999; bottomY = 99999; leftX = 99999; rightX = -99999; frontZ = 99999; backZ = -99999;
        startTime = Time.realtimeSinceStartup;
        ConsoleText.text += string.Format(" Reading in DSSP file...");
        DSSPfile = GetComponent<HttpTest>().outputfile;
        string pattern = @"([0-9]+)\s+(\S+)\s+([A-Z]+)\s+([A-Z]+)..(.)...........(....)(...)(...)(............)(...........)(...........)(...........)(........)(......)(......)(......)(......)\s+(\S+)\s+(\S+)\s+(\S+..)";

        string getnumAA = @"([0-9]+)\s+[0-9]+\s+[0-9]+\s+[0-9]+\s+[0-9]+\s+TOTAL NUMBER OF RESIDUES";

        string newmolpattern = @"!*             0   0    0      0, 0.0     0, 0.0     0, 0.0     0, 0.0   0.000 360.0 360.0 360.0 360.0    0.0    0.0    0.0";

        string header = @"#  RESIDUE AA STRUCTURE BP1 BP2  ACC     N-H-->O    O-->H-N    N-H-->O    O-->H-N    TCO  KAPPA ALPHA  PHI   PSI    X-CA   Y-CA   Z-CA            CHAIN AUTHCHAIN";

        // get total num of amino acids in the protein complex
        Regex getnumAAr = new Regex(getnumAA);
        int numaa = Convert.ToInt32(getnumAAr.Match(DSSPfile).Groups[1].ToString());
        ConsoleText.text += string.Format("\n Total number of amino acids: {0}", numaa);

        if (Regex.Split(Regex.Split(DSSPfile, header)[1], newmolpattern) == null)
        {
            ConsoleText.text += string.Format("\n string[] molecules is null.");
        }

        string[] molecules = Regex.Split(Regex.Split(DSSPfile, header)[1], newmolpattern);
        int totmolnum = molecules.Length; // index of molecule. total num of molecules in a protein complex

        List<SecondaryStructure[]> MoleculesSecondaryAssignments = new List<SecondaryStructure[]>(); // We want to make lists of the above vectors per molecule
        List<Vector3[]> MoleculesCAPoints = new List<Vector3[]>();

        foreach (string molecule in molecules)
        {
            int te = 0; // temp
            SecondaryStructure[] structure = new SecondaryStructure[Regex.Matches(molecule, pattern).Count];
            Vector3[] points = new Vector3[Regex.Matches(molecule, pattern).Count];

            if (Regex.Matches(molecule, pattern) == null)
            {
                ConsoleText.text += string.Format("\n Error reading DSSP file. Regex pattern returned no matches. Matches is null.");
            }

            foreach (Match matches in Regex.Matches(molecule, pattern))
            {
                structure[te].structure = matches.Groups[5].ToString();

                points[te] = new Vector3(float.Parse(matches.Groups[18].ToString()),
                                        float.Parse(matches.Groups[19].ToString()),
                                        float.Parse(matches.Groups[20].ToString()));
                te++;
            }

            MoleculesSecondaryAssignments.Add(structure);
            MoleculesCAPoints.Add(points);
        }

        GameObject ThisFile = new GameObject();
        ThisFile.name = string.Format("File {0}", GetComponent<Load>().FileNumber);
        ThisFile.transform.parent = GameObject.Find("MoleculeCollection").transform;
        ThisFile.transform.localPosition = Vector3.zero;
        ThisFile.transform.localScale = Vector3.one;
        GameObject RibbonCollection = new GameObject();
        RibbonCollection.name = "RibbonCollection";
        RibbonCollection.transform.parent = ThisFile.transform;
        RibbonCollection.transform.localPosition = Vector3.zero;
        RibbonCollection.transform.localScale = Vector3.one;
        // TODO : Put this in script Ribbon.cs as "DrawRibbon(Vector3 points)" 
        GameObject ThisPDB = new GameObject();
        ThisPDB.name = string.Format("PDB{0}", GetComponent<Load>().FileNumber);
        ThisPDB.transform.parent = RibbonCollection.transform;
        ThisPDB.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        //ThisPDB.transform.localPosition = new Vector3(0.75f, 0, 0) * (numPDB - 1);

        GameObject MoleculeHolder = new GameObject();
        MoleculeHolder.transform.parent = ThisPDB.transform;
        MeshFilter[] meshFilters = new MeshFilter[totmolnum];
        ConsoleText.text += string.Format("\n Total number of molecules: {0}.", totmolnum);
        Vector3 offset = new Vector3(0, 0f, 0f);
        GameObject parent = GameObject.Find("Hologram Collection"); // TODO: Remove find

        for (int tem = 0; tem < totmolnum; tem++)
        {
            // Make a new vector that has more points in it so it is smoother
            // The higher the second number, the more points and thus smoother lines. Changes smoothness exponentially relative to number of amino acids.
            Vector3[] newPoints = LineSmoother.SmoothLine(MoleculesCAPoints[tem], 0.8f * (float)Math.Exp(-10f / (float)numaa));
            if (newPoints == null)
            {
                ConsoleText.text += string.Format("\n Error initializing smoothing array. NewPoints is null.");
            }

            // Second argument adjusts the smoothness. Smaller = smoother
            // TODO: Second argument can be adjusted for changing quality using slider


            // TODO: These three parameters can be adjustable with sliders
            float lineWidth = 0.006f;
            float rescale = 100f;
            int numofsides = 12;

            int[] triangles = new int[6 * numofsides * newPoints.Length];
            Vector3[] quad = new Vector3[newPoints.Length * numofsides];
            Color[] colors = new Color[quad.Length];

            float a = lineWidth;
            float b = lineWidth / 2.5f;

            ConsoleText.text += string.Format("\n Initialized arrays and variables for molecule {0}...", tem);

            //foreach (Vector3 point in newPoints)
            foreach (Vector3 point in newPoints)
            {
                tiltFactor = 30;
                int i = Array.IndexOf(newPoints, point);
                int totnumofsides = i * numofsides;
                int totnumofvertices = totnumofsides * 6;
                int div = Convert.ToInt32(Math.Floor(i / ((float)newPoints.Length / (float)MoleculesCAPoints[tem].Length)));

                // Adds color based on secondary structure assignment
                for (int z = 0; z < numofsides; z++)
                {
                    colors[totnumofsides + z] = Colors(MoleculesSecondaryAssignments[tem][div].structure);
                }
            }

            Parallel.ForEach(newPoints, point =>
            {
                tiltFactor = 30;
                int i = Array.IndexOf(newPoints, point);
                int totnumofsides = i * numofsides;
                int totnumofvertices = totnumofsides * 6;

                int div = Convert.ToInt32(Math.Floor(i / ((float)newPoints.Length / (float)MoleculesCAPoints[tem].Length)));



                // for padding on the z axis:
                if (totnumofsides < (newPoints.Length - 2) * 6 && Mathf.Abs(point.z - newPoints[i + 1].z) > Mathf.Abs(point.x - newPoints[i + 1].x) && Mathf.Abs(point.z - newPoints[i + 1].z) > Mathf.Abs(point.y - newPoints[i + 1].y))
                {
                    tiltFactor = 45;
                }

                // Makes an oval
                // TODO: Find a way to fix thinness of certain axes (the padding on z axis method right above doesn't entirely work)

                for (int side = 0; side < numofsides; side++)
                {
                    float angle = (side * (360f / numofsides)) * Mathf.Deg2Rad;
                    float x = ((float)a * b) / (float)Mathf.Sqrt(((float)Math.Pow(b, 2) + ((float)Math.Pow(a, 2) * (float)Math.Pow(Mathf.Tan(angle), 2))));
                    float y = ((float)a * b) / (float)Mathf.Sqrt((float)Mathf.Pow(a, 2) + ((float)Mathf.Pow(b, 2) / (float)Mathf.Pow(Mathf.Tan(angle), 2)));

                    if (side * (360f / numofsides) > 90 && side * (360f / numofsides) < 270)
                    {
                        x = -1 * x;
                    }

                    if (side * (360f / numofsides) > 180 && side * (360f / numofsides) < 360)
                    {
                        y = -1 * y;
                    }

                    // Determine the max/min vertices for resizing and positioning later
                    if ((point / rescale).y + y > topY) { topY = (point / rescale).y + y; }
                    if ((point / rescale).y + y < bottomY) { bottomY = (point / rescale).y + y; }
                    if ((point / rescale).x + x > rightX) { rightX = (point / rescale).x + x; }
                    if ((point / rescale).x + x < leftX) { leftX = (point / rescale).x + x; }
                    if ((point / rescale).z > backZ) { backZ = (point / rescale).z; }
                    if ((point / rescale).z < frontZ) { frontZ = (point / rescale).z; }

                    quad[totnumofsides + side] = (point / rescale) + offset + new Vector3(x, y, 0);
                }

                // Creates an array for the triangles
                if (i < (newPoints.Length - 1))
                {
                    for (int p = 0; p < totnumofsides; p++)
                    {
                        triangles[(p * 6) + 0] = p + 0;
                        triangles[(p * 6) + 1] = p + numofsides;
                        triangles[(p * 6) + 2] = p + 1;
                        triangles[(p * 6) + 3] = p + 1;
                        triangles[(p * 6) + 4] = p + numofsides;
                        triangles[(p * 6) + 5] = p + numofsides + 1;
                    }
                }
            }
            );

            // Draw the molecule
            GameObject Protein = new GameObject();

            Protein.transform.parent = MoleculeHolder.transform; // remove find
            Protein.name = string.Format("PDB.{0}.molecule.{1}", GetComponent<Load>().FileNumber, tem);

            // some sanity checks
            if (GameObject.Find(string.Format("PDB.{0}.molecule.{1}", GetComponent<Load>().FileNumber, tem)) == null) { ConsoleText.text += string.Format("\n Molecule {0} GameObject does not exist.", tem); }
            if (quad == null) { ConsoleText.text += string.Format("\nQuad is null"); }
            if (triangles == null) { ConsoleText.text += string.Format("\n Triangles is null."); }

            Protein.AddComponent<MeshFilter>();
            Protein.AddComponent<MeshRenderer>();

            //Protein.GetComponent<MeshRenderer>().material = new Material(Shader.Find("VertexColorLightmaps"));
            Protein.GetComponent<MeshFilter>().mesh.vertices = quad;
            Protein.GetComponent<MeshFilter>().mesh.triangles = triangles;
            Protein.GetComponent<MeshFilter>().mesh.colors = colors;

            Protein.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Custom/SpecularHighlight"));

            Protein.GetComponent<MeshFilter>().mesh.RecalculateNormals();

            //meshFilters[tem] = Protein.GetComponent<MeshFilter>();
        }
        /*
		ThisPDB.AddComponent<MeshCollider>();
		ThisPDB.AddComponent<MeshFilter>();
		CombineInstance[] combine = new CombineInstance[meshFilters.Length];
		int inde = 0;
		while (inde < meshFilters.Length)
		{
			combine[inde].mesh = meshFilters[inde].sharedMesh;
			combine[inde].transform = meshFilters[inde].transform.localToWorldMatrix;
			//meshFilters[inde].gameObject.SetActive(false);
			inde++;
		}
		ThisPDB.transform.GetComponent<MeshFilter>().mesh = new Mesh();
		ThisPDB.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
		ThisPDB.transform.gameObject.SetActive(true);
		ThisPDB.transform.GetComponent<MeshCollider>().sharedMesh = ThisPDB.transform.GetComponent<MeshFilter>().mesh;
		ThisPDB.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Custom/SpecularHighlight"));
		*/
        MoleculeHolder.transform.localPosition -= new Vector3((rightX + leftX) / 2f, (topY + bottomY) / 2f, (backZ + frontZ) / 2f);
        ThisPDB.transform.localPosition = Vector3.zero;

        //ThisPDB.AddComponent<BoxCollider>().size = new Vector3(0.5f, 0.5f, 0.5f);
        //ThisPDB.AddComponent<MeshRenderer>().material = GameObject.Find("MoleculeCollection").GetComponent<MeshRenderer>().material;




        if (Math.Abs(topY - bottomY) >= Math.Abs(rightX - leftX))
        {
            if (Math.Abs(backZ - frontZ) >= Math.Abs(topY - bottomY))
            {
                MoleculeHolder.transform.localScale /= Math.Abs(backZ - frontZ) / .3f; // instead of .48f make this localscale of moelculecolelctin
            }
            else
            {
                MoleculeHolder.transform.localScale /= Math.Abs(topY - bottomY) / .3f;
            }
        }
        else
        {
            MoleculeHolder.transform.localScale /= Math.Abs(rightX - leftX) / .3f;
        }

		// change back to interpolated strings
        ConsoleText.text += string.Format("\n Cartoon structure drawn for PDB #{0}.", GetComponent<Load>().FileNumber);
        finishTime = Time.realtimeSinceStartup;
        ConsoleText.text += string.Format("\n Time taken to draw: {0} seconds.\n", (finishTime - startTime));
        Debug.Log($"Time to draw protein: {finishTime - startTime}");

        //ThisPDB.AddComponent<BoxCollider>().size = new Vector3(0.48f / ThisPDB.transform.localScale.x, 0.48f / ThisPDB.transform.localScale.x, 0.48f / ThisPDB.transform.localScale.x);
        //GameObject.Find("RibbonCollection").GetComponent<BoxCollider>().enabled = true;
        //MoleculeHolder.transform.localScale = new Vector3(3, 3, 3);

        RibbonCollection.AddComponent<BoundingBoxRig>().ScaleHandleMaterial = BoundingBoxHandle;
        RibbonCollection.GetComponent<BoundingBoxRig>().RotateHandleMaterial = BoundingBoxHandle;
        RibbonCollection.GetComponent<BoundingBoxRig>().InteractingMaterial = BoundingBoxHandleGrabbed;
        RibbonCollection.GetComponent<BoundingBoxRig>().BoundingBoxPrefab = BoundingBoxBasic;
        RibbonCollection.GetComponent<BoundingBoxRig>().appBarPrefab = AppBar;
        RibbonCollection.AddComponent<MixedRealityToolkit.InputModule.Utilities.Interations.TwoHandManipulatable>().BoundingBoxPrefab = BoundingBoxBasic;

        RibbonCollection.GetComponent<MixedRealityToolkit.InputModule.Utilities.Interations.TwoHandManipulatable>().SetManipulationMode(MixedRealityToolkit.InputModule.Utilities.Interations.TwoHandManipulatable.TwoHandedManipulation.MoveRotateScale);

        ThisPDB.AddComponent<MeshFilter>().mesh = GameObject.Find("MoleculeCollection").GetComponent<MeshFilter>().mesh;

        ThisPDB.AddComponent<MeshCollider>().sharedMesh = ThisPDB.GetComponent<MeshFilter>().mesh;
        ThisPDB.AddComponent<MeshRenderer>().material = GameObject.Find("MoleculeCollection").GetComponent<MeshRenderer>().material;
        ThisPDB.GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Load>().FileNumber++;
    }

    public Material BoundingBoxHandle, BoundingBoxHandleGrabbed;
    public BoundingBox BoundingBoxBasic;
    public AppBar AppBar;

    Color Colors(string structure)
    {
        switch (structure)
        {
            case "H":
                return GetComponent<Colors>().Purple;
            case "B":
                return GetComponent<Colors>().Yellow;
            case "E":
                return GetComponent<Colors>().Yellow;
            case "G":
                return GetComponent<Colors>().Purple;
            case "I":
                return GetComponent<Colors>().Purple;
            case "T":
                return GetComponent<Colors>().Aquamarine;
            case "S":
                return GetComponent<Colors>().Aquamarine;
            default:
                return Color.white;
        }
    }

    // ***
    /// <summary>
    /// H		Alpha Helix
    /// B		Beta bridge
    /// E		Strand
    /// G		Helix-3
    /// I		Helix-5
    /// T		Turn
    /// S		Bend
    /// </summary>
    /// 
}



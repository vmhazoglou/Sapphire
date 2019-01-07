using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using MixedRealityToolkit.UX.BoundingBoxes;
using MixedRealityToolkit.UX.AppBarControl;
using System.Threading.Tasks;

public struct topinfo
{
    public int NATOM, NTYPES, NBONH, MBONA, NTHETH, MTHETA,
        NPHIH, MPHIA, NHPARM, NPARM, NNB, NRES,
        NBONA, NTHETA, NPHIA, NUMBND, NUMANG, NPTRA,
        NATYP, NPHB, IFPERT, NBPER, NGPER, NDPER,
        MBPER, MGPER, MDPER, IFBOX, NMXRS, IFCAP,
        NUMEXTRA, NCOPY;
}

// coordinates
public struct atominfo
{
    public int resnum;
    public string resid, atomsym, element;
    public float x, y, z;
    public float charge;
}
    /*
  NATOM    : total number of atoms
  NTYPES   : total number of distinct atom types
  NBONH    : number of bonds containing hydrogen
  MBONA    : number of bonds not containing hydrogen
  NTHETH   : number of angles containing hydrogen
  MTHETA   : number of angles not containing hydrogen
  NPHIH    : number of dihedrals containing hydrogen
  MPHIA    : number of dihedrals not containing hydrogen
  NHPARM   : currently not used
  NPARM    : used to determine if addles created prmtop
  NNB      : number of excluded atoms
  NRES     : number of residues
  NBONA    : MBONA + number of constraint bonds
  NTHETA   : MTHETA + number of constraint angles
  NPHIA    : MPHIA + number of constraint dihedrals
  NUMBND   : number of unique bond types
  NUMANG   : number of unique angle types
  NPTRA    : number of unique dihedral types
  NATYP    : number of atom types in parameter file, see SOLTY below
  NPHB     : number of distinct 10-12 hydrogen bond pair types
  IFPERT   : set to 1 if perturbation info is to be read in
  NBPER    : number of bonds to be perturbed
  NGPER    : number of angles to be perturbed
  NDPER    : number of dihedrals to be perturbed
  MBPER    : number of bonds with atoms completely in perturbed group
  MGPER    : number of angles with atoms completely in perturbed group
  MDPER    : number of dihedrals with atoms completely in perturbed groups
  IFBOX    : set to 1 if standard periodic box, 2 when truncated octahedral
  NMXRS    : number of atoms in the largest residue
  IFCAP    : set to 1 if the CAP option from edit was specified
  NUMEXTRA : number of extra points found in topology
  NCOPY    : number of PIMD slices / number of beads
  */

public struct Bonds
{
    public int a1, a2;
    public double distance;

    public void Constructor(int atom1, int atom2)
    {
        a1 = atom1;
        a2 = atom2;
    }

    public double CalcDistance(Vector3 a, Vector3 b)
    {
        Vector3 vector = new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        return Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
    }
}

public struct Angles
{
    public int a1, a2, a3;
    public double angle;

    public void Constructor(int atom1, int atom2, int atom3)
    {
        a1 = atom1;
        a2 = atom2;
        a3 = atom3;
    }

    public double CalcAngle(Vector3 one, Vector3 two, Vector3 three)
    {
        Vector3 a = two - one;
        Vector3 b = three - two;
        float num = Vector3.Dot(a, b);
        float denom = Vector3.Magnitude(a) * Vector3.Magnitude(b);
        return 180 / Math.PI * Math.Acos(num / denom);
    }
}

public struct Dihedrals
{
    public int a1, a2, a3, a4;
    public double dih;
    public bool phi, psi, backbone, beta, alpha;

    public void Constructor(int atom1, int atom2, int atom3, int atom4)
    {
        a1 = atom1;
        a2 = atom2;
        a3 = atom3;
        a4 = atom4;
    }

    public double CalcDihedral(Vector3 first, Vector3 second, Vector3 third, Vector3 fourth)
    {
		Vector3 b1 = second - first;
		Vector3 b2 = third - second;
		Vector3 b3 = fourth - third;

		Vector3 n1 = Vector3.Cross(b1, b2);
		Vector3 n2 = Vector3.Cross(b2, b3);
		Vector3 nc = Vector3.Cross(n1, n2);
		double nd = Vector3.Dot(n1, n2);
		Vector3 b22 = b2 / Vector3.Magnitude(b2);
		double arg1 = Vector3.Dot(nc, b22);

		return (180 / Math.PI) * Math.Atan2(arg1, nd);
    }
}

public class Load : MonoBehaviour
{
    public GameObject MoleculeCollection, HologramCollection, Manager, ThisFile, NewLineCollection, FilePrefab, Play, SpherePrefabs, LowResSpherePrefabs;

    public atominfo[,] molecule; // frame, atom
    public topinfo top;

    public List<Bonds>[] Bonds;
    public List<Angles> Angles = new List<Angles>();
    public List<Dihedrals> Dihedrals = new List<Dihedrals>();

    public int framenum;
    public bool dynamicBonds;
    public Text txt;
    public int FileNumber = 0;
    public Text consoletxt;

    delegate Task LoadDel(GameObject ThisFile);
    List<LoadDel> loadDel;

    Dictionary<string, List<string>> aabondlist;

    private void Start()
    {
        // this is on upon startup so that bounding box works properly
        // set it off so user doesn't see it
        // (??))
// *************************************************
        FilePrefab.SetActive(false);
        // Initialize a list of AA names to their corresponding list of bonds
// *********************
        // can we just make these lists uppercase and use nameof(variable)
        aabondlist = new Dictionary<string, List<string>>()
        {
            {"ALA", Ala },
            {"VAL", Val },
            {"ILE", Ile },
            {"LEU", Leu },
            {"MET", Met },
            {"PHE", Phe },
            {"TYR", Tyr },
            {"TRP", Trp },
            {"SER", Ser },
            {"THR", Thr },
            {"ASN", Asn },
            {"GLN", Gln },
            {"CYS", Cys },
            {"GLY", Gly },
            {"PRO", Pro },
            {"ASP", Asp },
            {"GLU", Glu },
            {"ARG", Arg },
            {"HID", Hid },
            {"HIE", Hie },
            {"LYS", Lys },
            {"AS4", As4 },
            {"HIP", Hip },
            {"GL4", Gl4 },
        };

        //LoadACES, LoadAMBERParm, LoadAMBERCoord, LoadGaussian, LoadMOL2, LoadOrca
        loadDel = new List<LoadDel>() { LoadPDB, LoadXYZ };

        // for testing code in Unity
#if UNITY_EDITOR
        Manager.GetComponent<OpenFileButton>().ShowFileOpenPicker();
#endif
    }

    public async void LoadMolecule()
    {
        // testing out tuples
        //var primes = Tuple.Create(1, 2, 3);

        // Check if any file was loaded in
        if (Manager.GetComponent<OpenFileButton>().filestring == null)
        {
            consoletxt.text += "\n No file is selected to load.";
            goto NoFile; // Skips to the end of this function
        }

        // Create a new FileObject

        ThisFile = (GameObject)Instantiate(FilePrefab);
        ThisFile.transform.localScale = Vector3.one;
        ThisFile.SetActive(true);
        ThisFile.transform.parent = MoleculeCollection.transform;
        ThisFile.transform.name = String.Format("File {0}", FileNumber);

        // Get the AtomCollection object
        NewLineCollection = ThisFile.transform.GetChild(1).gameObject;

        // Add mesh details for the bounding box for resizing, rotating, etc.
        NewLineCollection.AddComponent<MeshFilter>().mesh = MoleculeCollection.GetComponent<MeshFilter>().mesh;
        NewLineCollection.AddComponent<MeshCollider>().sharedMesh = NewLineCollection.GetComponent<MeshFilter>().mesh;
        NewLineCollection.AddComponent<MeshRenderer>().material = MoleculeCollection.GetComponent<MeshRenderer>().material;
        NewLineCollection.GetComponent<MeshRenderer>().enabled = false;

// ****************************
        //TODO: Do a try and if there is an exception that the index is too large or not found for array Rep then throw a "consoletxt += filetype not recognized"

        await loadDel[Manager.GetComponent<Dropdowns>().filetypes.IndexOf(Manager.GetComponent<Dropdowns>().filetypeSelected) - 1](ThisFile); 

        ThisFile.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

// ****************************
        // Can we put this data directly in the right place instead of moving it around and then deleting it?
        Manager.GetComponent<OpenFileButton>().filestring = null;
        molecule = null;
        //for (int g = 0; g < framenum; g++)
        //    Bonds[g].Clear();

        FileNumber++;

        NoFile:
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    async Task LoadPDB(GameObject ThisFile)
    {
        //string pdbid = Regex.Match(Manager.GetComponent<OpenFileButton>().filestring, "HEADER\\s+....................................................(....)").Groups[1].ToString();
        //Manager.GetComponent<HttpTest>().PDBID = pdbid;
        //Manager.GetComponent<HttpTest>().enabled = true;
        //Manager.GetComponent<ParseDSSP>();
        double sstartt = Time.realtimeSinceStartup;
        double starttime = Time.realtimeSinceStartup;

        // Parse file for molecular data
        Manager.GetComponent<LoadPDB>().LoadMolecule();

        // Create an array (per frame) of a list of bonds
        Bonds = new List<Bonds>[framenum];

        // Have to create lists for each element of array
        for ( int g = 0; g < framenum; g++)
        {
            Bonds[g] = new List<Bonds>();
        }

        consoletxt.text += $"\n Time for poarse PDB func : {Time.realtimeSinceStartup - starttime}"; starttime = Time.realtimeSinceStartup;

        CenterMolecule();
        consoletxt.text += $"\n Time for center : {Time.realtimeSinceStartup - starttime}";  starttime = Time.realtimeSinceStartup;


        GetBondsPDB(0);
        consoletxt.text += $"\n Time for get bonds : {Time.realtimeSinceStartup - starttime}"; starttime = Time.realtimeSinceStartup;

        dynamicBonds = false; // this needs to be on the file object

        // Save information about this molecule in its FileObject
        ThisFile.GetComponent<ThisFileInfo>().FileNumber = FileNumber;
        ThisFile.GetComponent<ThisFileInfo>().TotalFrames = framenum;
        ThisFile.GetComponent<ThisFileInfo>().molecule = molecule;
        ThisFile.GetComponent<ThisFileInfo>().top = top;
        ThisFile.GetComponent<ThisFileInfo>().Bonds = Bonds;
        ThisFile.GetComponent<ThisFileInfo>().FileName = $"PDB {FileNumber}";

        consoletxt.text += $"\n Time for copy data : {Time.realtimeSinceStartup - starttime}"; starttime = Time.realtimeSinceStartup;

        //  Set representation mode to line
        ThisFile.GetComponent<ViewLine>().Line();
        consoletxt.text += $"\n Time for set linec : {Time.realtimeSinceStartup - starttime}"; starttime = Time.realtimeSinceStartup;

        // Create the lines to represent bonds
        ThisFile.GetComponent<ViewLine>().CreateLines(molecule);
        consoletxt.text += $"\n Time for draw line : {Time.realtimeSinceStartup - starttime}"; starttime = Time.realtimeSinceStartup;

        // Tell the PlayScript how many frames were loaded in
        ThisFile.transform.GetChild(4).GetComponent<PlayScript>().framenum = framenum;
        Debug.Log(top.NATOM);
        // If there are less than 800 atoms (spheres to render), set the representation mode to Ball & Stick
        if (top.NATOM < 200)
        {
            ThisFile.GetComponent<BallAndStick>().SpherePrefabs = SpherePrefabs;
            ThisFile.GetComponent<BallAndStick>().BnS();
            consoletxt.text += $"\n Time for drwa sphere : {Time.realtimeSinceStartup - starttime}"; starttime = Time.realtimeSinceStartup;
        }
        else if (top.NATOM >= 200 && top.NATOM < 800)
        {
            ThisFile.GetComponent<BallAndStick>().SpherePrefabs = LowResSpherePrefabs;
            ThisFile.GetComponent<BallAndStick>().BnS();
        }
        else if (top.NATOM >= 800 && top.NATOM < 1600)
        {
            // Change this to very low res ones. set to 1
            ThisFile.GetComponent<BallAndStick>().SpherePrefabs = LowResSpherePrefabs;

        }
        else if (top.NATOM >=1600)
        {
             // billboard
        }
        // Set the Play object
        Play = ThisFile.transform.GetChild(4).gameObject;

        // If multiple frames were loaded in, set Play as active so we can see the play button and associated sliders
        if (framenum > 1)
        {
            Play.SetActive(true);

            // Set it to play automatically
            ThisFile.transform.GetChild(4).gameObject.GetComponent<PlayScript>().PlayTraj();
        }
        consoletxt.text += $"\n Time for play: {Time.realtimeSinceStartup - starttime}"; starttime = Time.realtimeSinceStartup;

        consoletxt.text += $" Total time to load in PDB : {Time.realtimeSinceStartup - sstartt}";
    }

    async Task LoadXYZ(GameObject ThisFile)
    {
        // Parse file to load in molecular data
        //float st = Time.realtimeSinceStartup;
        Manager.GetComponent<ParseXYZ>().LoadMolecule();
        //Debug.Log(Time.realtimeSinceStartup - st);
        // Create an array (per frame) of a list of bonds
        ThisFile.GetComponent<ThisFileInfo>().Bonds = new List<Bonds>[framenum];

        // Get the bonds based on atom distances
        // Have to create lists for each element of array
        for (int g = 0; g < framenum; g++)
        {
            ThisFile.GetComponent<ThisFileInfo>().Bonds[g] = new List<Bonds>();
            GetBondsDynamic(g);
        }

        // Set this flag to true so our PlayScript/ViewLine components update bonds per frame
        ThisFile.GetComponent<ThisFileInfo>().dynamicBonds = true;

        // Determine the size and center of the molecule and ensure it is in the center of a box
        CenterMolecule();

        // Save information about this molecule in its FileObject
        ThisFile.GetComponent<ThisFileInfo>().FileNumber = FileNumber;
        ThisFile.GetComponent<ThisFileInfo>().molecule = molecule;
        ThisFile.GetComponent<ThisFileInfo>().top = top;
        ThisFile.GetComponent<ThisFileInfo>().TotalFrames = framenum;
        ThisFile.GetComponent<ThisFileInfo>().FileName = $"XYZ {FileNumber}";

        //  Set representation mode to line
        ThisFile.GetComponent<ViewLine>().Line();

        // Create the lines to represent bonds
        ThisFile.GetComponent<ViewLine>().CreateLines(molecule);

        // Tell the PlayScript how many frames were loaded in
        ThisFile.transform.GetChild(4).GetComponent<PlayScript>().framenum = framenum;

        // If there are less than 800 atoms (spheres to render), set the representation mode to Ball & Stick
        if (top.NATOM < 200)
        {
            ThisFile.GetComponent<BallAndStick>().SpherePrefabs = SpherePrefabs;
            ThisFile.GetComponent<BallAndStick>().BnS();
        }
        else if (top.NATOM >= 200 && top.NATOM < 1600)
        {
            ThisFile.GetComponent<BallAndStick>().SpherePrefabs = LowResSpherePrefabs;
        }
        else
        {
            // create billboard circles ; 
            // maybe just give a warning in console about performance up to like 1500 atoms and then give an error "too many atoms to be drawn" if >1500
        }

        // Set the Play object
        Play = ThisFile.transform.GetChild(4).gameObject;

        // If multiple frames were loaded in, set Play as active so we can see the play button and associated sliders
        if (framenum > 1)
        {
            Play.SetActive(true);

            // Set it to play automatically
            ThisFile.transform.GetChild(4).gameObject.GetComponent<PlayScript>().PlayTraj();
        }
    }

    // "PDB", "XYZ", "ACES", "AMBER Parm", "AMBER Coord", "Gaussian", "MOL2", "Orca"

    void LoadACES(GameObject ThisFile)
    {
        // Parse file to load in molecular data
        Manager.GetComponent<LoadACES>().LoadMolecule();

        // Create an array (per frame) of a list of bonds
        ThisFile.GetComponent<ThisFileInfo>().Bonds = new List<Bonds>[framenum];

        // Get the bonds based on atom distances
        // Have to create lists for each element of array
        for (int g = 0; g < framenum; g++)
        {
            ThisFile.GetComponent<ThisFileInfo>().Bonds[g] = new List<Bonds>();
            GetBondsDynamic(g);
        }

        // Set this flag to true so our PlayScript/ViewLine components update bonds per frame
        ThisFile.GetComponent<ThisFileInfo>().dynamicBonds = true;

        // Determine the size and center of the molecule and ensure it is in the center of a box
        CenterMolecule();

        // Save information about this molecule in its FileObject
        ThisFile.GetComponent<ThisFileInfo>().FileNumber = FileNumber;
        ThisFile.GetComponent<ThisFileInfo>().molecule = molecule;
        ThisFile.GetComponent<ThisFileInfo>().top = top;
        ThisFile.GetComponent<ThisFileInfo>().TotalFrames = framenum;
        ThisFile.GetComponent<ThisFileInfo>().FileName = $"ACES {FileNumber}";

        //  Set representation mode to line
        ThisFile.GetComponent<ViewLine>().Line();

        // Create the lines to represent bonds
        ThisFile.GetComponent<ViewLine>().CreateLines(molecule);

        // Tell the PlayScript how many frames were loaded in
        ThisFile.transform.GetChild(4).GetComponent<PlayScript>().framenum = framenum;

        // If there are less than 800 atoms (spheres to render), set the representation mode to Ball & Stick
        if (top.NATOM < 200)
        {
            ThisFile.GetComponent<BallAndStick>().SpherePrefabs = SpherePrefabs;
            ThisFile.GetComponent<BallAndStick>().BnS();
        }
        else if (top.NATOM >= 200 && top.NATOM < 1600)
        {
            ThisFile.GetComponent<BallAndStick>().SpherePrefabs = LowResSpherePrefabs;
        }
        else
        {
            // create billboard circles ; 
            // maybe just give a warning in console about performance up to like 1500 atoms and then give an error "too many atoms to be drawn" if >1500
        }

        // Set the Play object
        Play = ThisFile.transform.GetChild(4).gameObject;

        // If multiple frames were loaded in, set Play as active so we can see the play button and associated sliders
        if (framenum > 1)
        {
            Play.SetActive(true);

            // Set it to play automatically
            ThisFile.transform.GetChild(4).gameObject.GetComponent<PlayScript>().PlayTraj();
        }
    }

    void LoadAMBERParm(GameObject ThisFile)
    {
        Manager.GetComponent<LoadAMBER>().LoadMolecule();
        Manager.GetComponent<Dropdowns>().filetypeSelected = "AMBER Coord";
    }

    void LoadAMBERCoord(GameObject ThisFile)
    {
        Manager.GetComponent<LoadAMBER>().LoadMolecule();
        GetBondsDynamic(0);
        dynamicBonds = true;
        CenterMolecule();
        if (framenum > 1) { Play.SetActive(true); txt.GetComponent<ChangeText>().ChangeIt(); }

        Manager.GetComponent<ViewLine>().Line();
        Manager.GetComponent<ViewLine>().CreateLines(molecule);

        if (top.NATOM < 800) { Manager.GetComponent<BallAndStick>().BnS(); }
    }

    void LoadGaussian(GameObject ThisFile)
    {

        // Parse file to load in molecular data
        Manager.GetComponent<LoadGaussian>().LoadMolecule();

        // Create an array (per frame) of a list of bonds
        ThisFile.GetComponent<ThisFileInfo>().Bonds = new List<Bonds>[framenum];

        // Get the bonds based on atom distances
        // Have to create lists for each element of array
        for (int g = 0; g < framenum; g++)
        {
            ThisFile.GetComponent<ThisFileInfo>().Bonds[g] = new List<Bonds>();
            GetBondsDynamic(g);
        }

        // Set this flag to true so our PlayScript/ViewLine components update bonds per frame
        ThisFile.GetComponent<ThisFileInfo>().dynamicBonds = true;

        // Determine the size and center of the molecule and ensure it is in the center of a box
        CenterMolecule();

        // Save information about this molecule in its FileObject
        ThisFile.GetComponent<ThisFileInfo>().FileNumber = FileNumber;
        ThisFile.GetComponent<ThisFileInfo>().molecule = molecule;
        ThisFile.GetComponent<ThisFileInfo>().top = top;
        ThisFile.GetComponent<ThisFileInfo>().TotalFrames = framenum;
        ThisFile.GetComponent<ThisFileInfo>().FileName = $"Gaussian {FileNumber}";

        //  Set representation mode to line
        ThisFile.GetComponent<ViewLine>().Line();

        // Create the lines to represent bonds
        ThisFile.GetComponent<ViewLine>().CreateLines(molecule);

        // Tell the PlayScript how many frames were loaded in
        ThisFile.transform.GetChild(4).GetComponent<PlayScript>().framenum = framenum;

        // If there are less than 800 atoms (spheres to render), set the representation mode to Ball & Stick
        if (top.NATOM < 200)
        {
            ThisFile.GetComponent<BallAndStick>().SpherePrefabs = SpherePrefabs;
            ThisFile.GetComponent<BallAndStick>().BnS();
        }
        else if (top.NATOM >= 200 && top.NATOM < 1600)
        {
            ThisFile.GetComponent<BallAndStick>().SpherePrefabs = LowResSpherePrefabs;
        }
        else
        {
            // create billboard circles ; 
            // maybe just give a warning in console about performance up to like 1500 atoms and then give an error "too many atoms to be drawn" if >1500
        }

        // Set the Play object
        Play = ThisFile.transform.GetChild(4).gameObject;

        // If multiple frames were loaded in, set Play as active so we can see the play button and associated sliders
        if (framenum > 1)
        {
            Play.SetActive(true);

            // Set it to play automatically
            ThisFile.transform.GetChild(4).gameObject.GetComponent<PlayScript>().PlayTraj();
        }
    }

    void LoadMOL2(GameObject ThisFile)
    {
        // Parse file to load in molecular data
        Manager.GetComponent<LoadOrca>().LoadMolecule();

        // Create an array (per frame) of a list of bonds
        ThisFile.GetComponent<ThisFileInfo>().Bonds = new List<Bonds>[framenum];

        // Get the bonds based on atom distances
        // Have to create lists for each element of array
        for (int g = 0; g < framenum; g++)
        {
            ThisFile.GetComponent<ThisFileInfo>().Bonds[g] = new List<Bonds>();
            GetBondsFromMOL2(Manager.GetComponent<OpenFileButton>().filestring, g);
        }

        // Set this flag to true so our PlayScript/ViewLine components update bonds per frame
        ThisFile.GetComponent<ThisFileInfo>().dynamicBonds = true;

        // Determine the size and center of the molecule and ensure it is in the center of a box
        CenterMolecule();

        // Save information about this molecule in its FileObject
        ThisFile.GetComponent<ThisFileInfo>().FileNumber = FileNumber;
        ThisFile.GetComponent<ThisFileInfo>().molecule = molecule;
        ThisFile.GetComponent<ThisFileInfo>().top = top;
        ThisFile.GetComponent<ThisFileInfo>().TotalFrames = framenum;
        ThisFile.GetComponent<ThisFileInfo>().FileName = $"MOL2 {FileNumber}";

        //  Set representation mode to line
        ThisFile.GetComponent<ViewLine>().Line();

        // Create the lines to represent bonds
        ThisFile.GetComponent<ViewLine>().CreateLines(molecule);

        // Tell the PlayScript how many frames were loaded in
        ThisFile.transform.GetChild(4).GetComponent<PlayScript>().framenum = framenum;

        // If there are less than 800 atoms (spheres to render), set the representation mode to Ball & Stick
        if (top.NATOM < 200)
        {
            ThisFile.GetComponent<BallAndStick>().SpherePrefabs = SpherePrefabs;
            ThisFile.GetComponent<BallAndStick>().BnS();
        }
        else if (top.NATOM >= 200 && top.NATOM < 1600)
        {
            ThisFile.GetComponent<BallAndStick>().SpherePrefabs = LowResSpherePrefabs;
        }
        else
        {
            // create billboard circles ; 
            // maybe just give a warning in console about performance up to like 1500 atoms and then give an error "too many atoms to be drawn" if >1500
        }

        // Set the Play object
        Play = ThisFile.transform.GetChild(4).gameObject;

        // If multiple frames were loaded in, set Play as active so we can see the play button and associated sliders
        if (framenum > 1)
        {
            Play.SetActive(true);

            // Set it to play automatically
            ThisFile.transform.GetChild(4).gameObject.GetComponent<PlayScript>().PlayTraj();
        }
    }

    void LoadOrca(GameObject ThisFile)
    {
        // Parse file to load in molecular data
        Manager.GetComponent<LoadOrca>().LoadMolecule();

        // Create an array (per frame) of a list of bonds
        ThisFile.GetComponent<ThisFileInfo>().Bonds = new List<Bonds>[framenum];

        // Get the bonds based on atom distances
        // Have to create lists for each element of array
        for (int g = 0; g < framenum; g++)
        {
            ThisFile.GetComponent<ThisFileInfo>().Bonds[g] = new List<Bonds>();
            GetBondsDynamic(g);
        }

        // Set this flag to true so our PlayScript/ViewLine components update bonds per frame
        ThisFile.GetComponent<ThisFileInfo>().dynamicBonds = true;

        // Determine the size and center of the molecule and ensure it is in the center of a box
        CenterMolecule();

        // Save information about this molecule in its FileObject
        ThisFile.GetComponent<ThisFileInfo>().FileNumber = FileNumber;
        ThisFile.GetComponent<ThisFileInfo>().molecule = molecule;
        ThisFile.GetComponent<ThisFileInfo>().top = top;
        ThisFile.GetComponent<ThisFileInfo>().TotalFrames = framenum;
        ThisFile.GetComponent<ThisFileInfo>().FileName = $"Orca {FileNumber}";

        //  Set representation mode to line
        ThisFile.GetComponent<ViewLine>().Line();

        // Create the lines to represent bonds
        ThisFile.GetComponent<ViewLine>().CreateLines(molecule);

        // Tell the PlayScript how many frames were loaded in
        ThisFile.transform.GetChild(4).GetComponent<PlayScript>().framenum = framenum;

        // If there are less than 800 atoms (spheres to render), set the representation mode to Ball & Stick
        if (top.NATOM < 200)
        {
            ThisFile.GetComponent<BallAndStick>().SpherePrefabs = SpherePrefabs;
            ThisFile.GetComponent<BallAndStick>().BnS();
        }
        else if (top.NATOM >= 200 && top.NATOM < 1600)
        {
            ThisFile.GetComponent<BallAndStick>().SpherePrefabs = LowResSpherePrefabs;
        }
        else
        {
            // create billboard circles ; 
            // maybe just give a warning in console about performance up to like 1500 atoms and then give an error "too many atoms to be drawn" if >1500
        }

        // Set the Play object
        Play = ThisFile.transform.GetChild(4).gameObject;

        // If multiple frames were loaded in, set Play as active so we can see the play button and associated sliders
        if (framenum > 1)
        {
            Play.SetActive(true);

            // Set it to play automatically
            ThisFile.transform.GetChild(4).gameObject.GetComponent<PlayScript>().PlayTraj();
        }
    }

    public BoundingBox BoundingBoxBasic;
    public Material BoundingBoxHandle, BoundingBoxHandleGrabbed;
    public AppBar appbar;

    float leftmost = 999999, rightmost = -999999, bottommost = 999999, topmost = -999999, backmost = 999999, frontmost = -999999;
    float toptobottom, lefttoright;

    void CenterMolecule()
    {
        //double starttime = Time.realtimeSinceStartup;
        Vector3 midpoint = Vector3.zero;

        // Determine where the center of the molecule is for the first frame
        for (int atom = 0; atom < top.NATOM; atom++)
        {
            midpoint.x += molecule[0, atom].x;
            midpoint.y += molecule[0, atom].y;
            midpoint.z += molecule[0, atom].z;
        }
        midpoint = midpoint / top.NATOM;

        // Determine how the first frame's molecule center differs from Vector3.zero
        Vector3 centerDiff = MoleculeCollection.transform.position - midpoint;

        // Determine for all frames and all atoms where the outermost atoms are to ensure we can fit everything inside a box
        for (int atom = 0; atom < top.NATOM; atom++)
        {
            for (int frame = 0; frame < framenum; frame++)
            {
                molecule[frame, atom].x -= midpoint.x;
                molecule[frame, atom].y -= midpoint.y;
                molecule[frame, atom].z -= midpoint.z;
            }

            if (molecule[0, atom].x < leftmost)
                leftmost = molecule[0, atom].x;
            if (molecule[0, atom].x > rightmost)
                rightmost = molecule[0, atom].x;
            if (molecule[0, atom].y > topmost)
                topmost = molecule[0, atom].y;
            if (molecule[0, atom].y < bottommost)
                bottommost = molecule[0, atom].y;
            if (molecule[0, atom].z > frontmost)
                frontmost = molecule[0, atom].z;
            if (molecule[0, atom].z < backmost)
                backmost = molecule[0, atom].z;
        }

        // Find the 3D space the simulation occupies and add 0.2f padding
        toptobottom = Math.Abs(topmost) + Math.Abs(bottommost) + 0.2f;
        lefttoright = Math.Abs(rightmost) + Math.Abs(leftmost) + 0.2f;
        fronttoback = Math.Abs(backmost) + Math.Abs(frontmost) + 0.2f;

        // Rescale factors
        rescaleY = 0.5f / toptobottom;
        rescaleX = 0.5f / lefttoright;
        rescaleZ = 0.5f / fronttoback;

        max = Math.Max(rescaleY, Math.Max(rescaleX, rescaleZ));

//*************************  TODO: Rather than changing these, the AtomCollectionNew box should be sized around the whole molecule and then resized to be smaller
        // that way the units in the atominfo[,] molecule object are still the same as they are for that filetype and can be used later
        // for calculating bond distances and AEVs for ANI
        // Look at all of the Load*.cs scripts because they often have a resize factor
        // then check GetBondsDynamic function for resize factors that are arbitrary (rather than legitimate unit conversion)
        for (int atom = 0; atom < top.NATOM; atom++)
        {
            for (int frame = 0; frame < framenum; frame++)
            {
                molecule[frame, atom].x *= max * 2;
                molecule[frame, atom].y *= max * 2;
                molecule[frame, atom].z *= max * 2;
            }
        }

        // Reset these values for the next molecule loaded in
        leftmost = 999999; rightmost = -999999; bottommost = 999999; topmost = -999999; backmost = 999999; frontmost = -999999;

        //Debug.Log($"Centering time: {Time.realtimeSinceStartup - starttime}");
    }
    public float max;
    public GameObject AtomCollection;
    float rescaleY, rescaleX, rescaleZ, fronttoback;

//******************* test this
    void GetBondsFromMOL2(string filestring, int frame)
    {
        //Bonds[0].Clear();

        int i, j;
        float distance = 0;

        string[] substrings = Regex.Split(filestring, "<TRIPOS>BOND");

        string[] substrings2 = Regex.Split(substrings[1], "@<TRIPOS>SUBSTRUCTURE");

        string pattern = @"\r?\n?\s+(\S+)\s+(\S+)\s+(\S+)\s+(\S+)";

        foreach (Match m in Regex.Matches(substrings2[0], pattern))
        {
            i = Convert.ToInt32(m.Groups[2].ToString()) - 1;
            j = Convert.ToInt32(m.Groups[3].ToString()) - 1;

            Bonds temp = new Bonds();

            temp.Constructor(i, j);
            distance = Vector3.Distance(new Vector3(molecule[frame, i].x, 
                                                    molecule[frame, i].y, 
                                                    molecule[frame, i].z), 
                                        new Vector3(molecule[frame, j].x, 
                                                    molecule[frame, j].y, 
                                                    molecule[frame, j].z)) * 2500;
            temp.distance = distance;
            ThisFile.GetComponent<ThisFileInfo>().Bonds[frame].Add(temp);
        }
    }

    void GetBondsDynamic(int frame)
    {
        //ThisFile.GetComponent<ThisFileInfo>().Bonds[frame].Clear();

        for (int i = 0; i < top.NATOM; i++)
        {
            for (int j = 0; j < i; j++)
            {
                float distance = Vector3.Distance(new Vector3(molecule[frame, i].x, molecule[frame, i].y, molecule[frame, i].z),
                                                  new Vector3(molecule[frame, j].x, molecule[frame, j].y, molecule[frame, j].z)) * 100; // unit conversion

                bool bondExists = CheckBondDyn(molecule[frame, i].element,
                                               molecule[frame, j].element,
                                               distance);
                if (bondExists)
                {
                    Bonds temp = new Bonds();
                    temp.Constructor(i, j);
                    temp.distance = distance;
                    ThisFile.GetComponent<ThisFileInfo>().Bonds[frame].Add(temp);
                }

                /*
                else
                {
                    Bonds temp = new Bonds();
                    temp.Constructor(i, j);
                    temp.distance = distance;
                    if (ThisFile.GetComponent<ThisFileInfo>().Bonds[frame].Contains(temp))
                    {
                        ThisFile.GetComponent<ThisFileInfo>().Bonds[frame].Remove(temp);
                    }
                }
                */
            }
        }
    }


    bool CheckBondDyn(string element1, string element2, double distance)
    {
        bool contained = false;
        string[] Bondz = new string[] {
        element1 + element2 + "s",
        element1 + element2 + "d",
        element1 + element2 + "t",
        element2 + element1 + "s",
        element2 + element1 + "d",
        element2 + element1 + "t",
        element1 + element2 + "b",  //benzene carbons
        element1 + element2,
        element2 + element1
    };

        for (int i = 0; i < Bondz.Length; i++)
        {
            if (BondLengths.ContainsKey(Bondz[i]))
            {
                if (BondLengths[Bondz[i]] + 15 > distance && BondLengths[Bondz[i]] - 15 < distance)
                {
                    contained = true;
                }
            }
        }

        if (contained)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // in picometers
    // https://chem.libretexts.org/Textbook_Maps/Organic_Chemistry_Textbook_Maps/Map%3A_Organic_Chemistry_(Smith)/Chapter_01%3A_Structure_and_Bonding/1.6%3A_Determining_Molecular_Shape
    // https://pubs.acs.org/doi/abs/10.1021/ja00740a045
    // http://www.wiredchemist.com/chemistry/data/bond_energies_lengths.html

    Dictionary<string, float> BondLengths = new Dictionary<string, float>()
    {
        {"CCs", 154},
        {"CCb", 140},
        {"CCd", 134},
        {"CCt", 120},
        {"CSi", 185},
        {"CNs", 147},
        {"CNd", 129},
        {"CNt", 116},
        {"COs", 143},
        {"COd", 120},
        {"COt", 113},
        {"CF", 135},
        {"CCl", 177},
        {"CBr", 194},
        {"CI", 214},
        {"NNs", 145},
        {"NNd", 125},
        {"NNt", 110},
        {"NOs", 140},
        {"NOd", 121},
        {"PP", 221},
        {"POs", 163},
        {"POd", 150},
        {"HH", 74},
        {"HB", 119},
        {"HC", 109},
        {"HO", 96},
        {"OOs", 148},
        {"OOd", 121},
        {"NH", 99},
        {"CS", 187},
        {"HS", 134 }
    };

    public void GetBondsPDB(int frame)
    {
        Bonds[0].Clear();

        double starttime = Time.realtimeSinceStartup;
        Bonds temp = new Bonds();
        //adjustPdbUnits = rescale * 100;cent
        for (int k = 0; k < top.NATOM - 1; k++)
        {
            for (int j = k + 1; j < top.NATOM; j++)
            {
                bool bondexists = false;


                if (j != k && molecule[frame, k].resnum == molecule[frame, j].resnum)
                {

                    bondexists = CheckBond(
                        string.Format("{0}-{1}", (molecule[frame, k].atomsym), (molecule[frame, j].atomsym)),
                        string.Format("{0}-{1}", (molecule[frame, j].atomsym), (molecule[frame, k].atomsym)),
                        molecule[frame, k].resid
                        );

                }
                else
                {
                    if (molecule[frame, k].resnum == molecule[frame, j].resnum + 1 || molecule[frame, k].resnum == molecule[frame, j].resnum - 1)
                    {
                        if (string.Format("{0}-{1}", (molecule[frame, k].atomsym), (molecule[frame, j].atomsym)) == "C-N")
                        {
                            bondexists = true;
                        }
                    }
                }



                if (bondexists == true)
                {

                    temp.Constructor(k, j);

                    Bonds[0].Add(temp);
                }

            }
        }
        Debug.Log($"PDB Bonds time: {Time.realtimeSinceStartup - starttime}");
    }

    //string[] aalist = {"ALA", "VAL", "ILE", "LEU", "MET", "PHE", "TYR", "TRP", "SER", "THR", "ASN", "GLN", "CYS", "GLY", "PRO", "ASP", "GLU", "ARG",
     //                       "HID", "HIE", "LYS", "AS4", "HIP", "GL4"};

    List<string>[] aabondslists ;

    bool CheckBond(string atomsyms1, string atomsyms2, string aminoacid)
    {
        bool contained = false;

        if (aabondlist[aminoacid].Contains(atomsyms1) || aabondlist[aminoacid].Contains(atomsyms2))
            contained = true;
                    
        if (contained)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void GetAngles(int frame)
    {
        Angles temp = new Angles();

        // Loop through list.
        foreach (Bonds k in Bonds[0])
        {

            //foreach (string l in list)

            for (p = Bonds[0].IndexOf(k); p < Bonds[0].Count; p++)
            {
                Bonds q = Bonds[0][p];


                // TODO: Double check Angle calculation (vectors part)

                if (q.a1 == k.a2 && k.a1 != q.a2)
                {

                    temp.Constructor(k.a1, k.a2, q.a2);
                    Angles.Add(temp);

                }


                if (k.a2 == q.a2 && k.a1 != q.a1)
                {
                    temp.Constructor(k.a1, k.a2, q.a1);
                    Angles.Add(temp);

                }

                if (k.a1 == q.a1 && k.a2 != q.a2)
                {

                    temp.Constructor(k.a1, k.a2, q.a1);
                    Angles.Add(temp);

                }


                if (k.a1 == q.a2 && k.a2 != q.a1)
                {
                    temp.Constructor(q.a1, k.a1, k.a2);
                    Angles.Add(temp);

                }



            }
        }
    }


    string element1, element2, element3, element4, element5, element6;
    Vector3 first, second, third, fourth;
    int p;
    //public Dictionary<string, double> Dihedrals = new Dictionary<string, double>();
    //public List<string> Dihedrals = new List<string>();
    public int totdihs = 0;

    // TODO: Do getangles and getdihedrals right in the script where you need them so you don't have to keep referencing this one

    public void GetDihedrals(int frame)
    {
        totdihs = 0;
        Dihedrals temp = new Dihedrals();


        foreach (Angles k in Angles)
        {

            for (p = Angles.IndexOf(k); p < Angles.Count; p++)
            {

                Angles q = Angles[p];


                if (k.a2 == q.a1 && k.a3 == q.a2 && k.a1 != q.a3)
                {

                    temp.Constructor(k.a1, k.a2, k.a3, q.a3);
                    Dihedrals.Add(temp);

                }


                if (k.a2 == q.a3 && k.a3 == q.a2 && k.a1 != q.a1)
                {

                    temp.Constructor(k.a1, k.a2, k.a3, q.a1);
                    Dihedrals.Add(temp);
                }


                if (k.a1 == q.a2 && k.a2 == q.a3 && q.a1 != k.a3)
                {
                    temp.Constructor(q.a1, k.a1, k.a2, k.a3);
                    Dihedrals.Add(temp);
                }


                if (k.a1 == q.a2 && k.a2 == q.a1 && q.a3 != k.a3)
                {

                    temp.Constructor(q.a3, k.a1, k.a2, k.a3);
                    Dihedrals.Add(temp);
                }
            }
        }

    }

    List<string> Ala = new List<string>()
    {
        {"C-O" },
        {"C-CA" },
        {"CA-N" },
        {"N-H" },
        {"N-H1" },
        {"N-H2" },
        {"N-H3" },
        {"CA-CB" },
        {"CB-HB1" },
        {"CB-HB2" },
        {"CB-HB3" },
        {"CA-HA" }
    };

    List<string> Val = new List<string>()
    {
        {"C-O" },
        {"C-CA" },
        {"CA-N" },
        {"N-H" },
        {"N-H1" },
        {"N-H2" },
        {"N-H3" },
        {"CA-HA" },
        {"CA-CB" },
        {"CB-HB" },
        {"CB-CG1" },
        {"CG1-HG11" },
        {"CG1-HG12" },
        {"CB-CG2" },
        {"CG1-HG13" },
        {"CG2-HG21" },
        {"CG2-HG22" },
        {"CG2-HG23" },
        {"CB-HB" }
    };

    List<string> Ile = new List<string>()
    {
        {"C-O" },
        {"C-CA" },
        {"CA-N" },
        {"N-H" },
        {"N-H1" },
        {"N-H2" },
        {"N-H3" },
        {"CA-HA" },
        {"CA-CB" },
        {"CB-HB" },
        {"CB-CG2" },
        {"CB-CG1" },
        {"CG2-HG21" },
        {"CG2-HG22" },
        {"CG2-HG23" },
        {"CG1-HG11" },
        {"CG1-HG12" },
        {"CG1-HG13" },
        {"CG1-CD1" },
        {"CD1-HD11" },
        {"CD1-HD12" },
        {"CD1-HD13" }
    };

    List<string> Leu = new List<string>()
    {
        {"C-O" },
        {"C-CA" },
        {"CA-N" },
        {"N-H" },
        {"N-H1" },
        {"N-H2" },
        {"N-H3" },
        {"CA-HA" },
        {"CA-CB" },
        {"CB-HB2" },
        {"CB-HB3" },
        {"CB-CG" },
        {"CG-HG" },
        {"CG-CD1" },
        {"CD1-HD11" },
        {"CD1-HD12" },
        {"CD1-HD13" },
        {"CG-CD2" },
        {"CD2-HD21" },
        {"CD2-HD22" },
        {"CD2-HD23" }
    };

    List<string> Met = new List<string>()
    {
        {"C-O" },
        {"C-CA" },
        {"CA-N" },
        {"N-H" },
        {"N-H1" },
        {"N-H2" },
        {"N-H3" },
        {"CA-HA" },
        {"CA-CB" },
        {"CB-HB2" },
        {"CB-HB3" },
        {"CB-CG" },
        {"CG-HD2" },
        {"CG-HD3" },
        {"CG-SD" },
        {"SD-CE" },
        {"CE-HE1" },
        {"CE-HE2" },
        {"CE-HE3" }
    };

    List<string> Phe = new List<string>()
    {
        {"C-O" },
        {"C-CA" },
        {"CA-N" },
        {"N-H" },
        {"N-H1" },
        {"N-H2" },
        {"N-H3" },
        {"CA-HA" },
        {"CA-CB" },
        {"CB-HB2" },
        {"CB-HB3" },
        {"CB-CG" },
        {"CG-CD1" },
        {"CD1-HD1" },
        {"CD1-CE1" },
        {"CE1-HE1" },
        {"CE1-CZ" },
        {"CZ-HZ" },
        {"CZ-CE2" },
        {"CE2-HE2" },
        {"CE2-CD2" },
        {"CD2-HD2" },
        {"CD2-CG" }
    };

    List<string> Tyr = new List<string>()
    {
        {"C-O" },
        {"C-CA" },
        {"CA-N" },
        {"N-H" },
        {"N-H1" },
        {"N-H2" },
        {"N-H3" },
        {"CA-HA" },
        {"CA-CB" },
        {"CB-HB2" },
        {"CB-HB3" },
        {"CB-CG" },
        {"CG-CD1" },
        {"CD1-HD1" },
        {"CD1-CE1" },
        {"CE1-HE1" },
        {"CE1-CZ" },
        {"CZ-OH" },
        {"OH-HH" },
        {"CZ-CE2" },
        {"CE2-HE2" },
        {"CE2-CD2" },
        {"CD2-HD2" },
        {"CD2-CG" }
    };

    List<string> Trp = new List<string>()
    {
        {"C-O" },
        {"C-CA" },
        {"CA-N" },
        {"N-H" },
        {"N-H1" },
        {"N-H2" },
        {"N-H3" },
        {"CA-HA" },
        {"CA-CB" },
        {"CB-HB2" },
        {"CB-HB3" },
        {"CB-CG" },
        {"CG-CD1" },
        {"CD1-HD1" },
        {"CD1-NE1" },
        {"NE1-HE1" },
        {"NE1-CE2" },
        {"CE2-CD2" },
        {"CD2-CG" },
        {"CD2-CE3" },
        {"CE3-HE3" },
        {"CE3-CZ3" },
        {"CZ3-HZ3" },
        {"CZ3-CH2" },
        {"CH2-HH2" },
        {"CH2-CZ2" },
        {"CZ2-HZ2" },
        {"CZ2-CE2" }
    };

    List<string> Ser = new List<string>()
    {
        {"C-O" },
        {"C-CA" },
        {"CA-N" },
        {"N-H" },
        {"N-H1" },
        {"N-H2" },
        {"N-H3" },
        {"CA-HA" },
        {"CA-CB" },
        {"CB-HB2" },
        {"CB-HB3" },
        {"CB-OG" },
        {"OG-HG" }
    };

    List<string> Thr = new List<string>()
    {
        {"C-O" },
        {"C-CA" },
        {"CA-N" },
        {"N-H" },
        {"N-H1" },
        {"N-H2" },
        {"N-H3" },
        {"CA-HA" },
        {"CA-CB" },
        {"CB-HB" },
        {"CB-OG1" },
        {"OG-HG1" },
        {"OG1-HG1" },
        {"CB-CG2" },
        {"CG2-HG21" },
        {"CG2-HG22" },
        {"CG2-HG23" }
    };

    List<string> Asn = new List<string>()
    {
        {"C-O" },
        {"C-CA" },
        {"CA-N" },
        {"N-H" },
        {"N-H1" },
        {"N-H2" },
        {"N-H3" },
        {"CA-HA" },
        {"CA-CB" },
        {"CB-HB2" },
        {"CB-HB3" },
        {"CB-CG" },
        {"CG-OD1" },
        {"CG-ND2" },
        {"ND2-HD21" },
        {"ND2-HD22" }
    };

    List<string> Gln = new List<string>()
    {
        {"C-O" },
        {"C-CA" },
        {"CA-N" },
        {"N-H" },
        {"N-H1" },
        {"N-H2" },
        {"N-H3" },
        {"CA-HA" },
        {"CA-CB" },
        {"CB-HB2" },
        {"CB-HB3" },
        {"CB-CG" },
        {"CG-HG2" },
        {"CG-HG3" },
        {"CG-CD" },
        {"CD-OE1" },
        {"CD-NE2" },
        {"NE2-HE21" },
        {"NE2-HE22" }
    };

    List<string> Cys = new List<string>()
    {
        {"C-O" },
        {"C-CA" },
        {"CA-N" },
        {"N-H" },
        {"N-H1" },
        {"N-H2" },
        {"N-H3" },
        {"CA-HA" },
        {"CA-CB" },
        {"CB-HB2" },
        {"CB-HB3" },
        {"CB-SG" },
        {"SG-HG" }
    };

    List<string> Gly = new List<string>()
    {
        {"C-O" },
        {"C-CA" },
        {"CA-N" },
        {"N-H" },
        {"N-H1" },
        {"N-H2" },
        {"N-H3" },
        {"CA-HA2" },
        {"CA-HA3" }
    };

    /* What happens if Pro is terminal res */

    List<string> Pro = new List<string>()
    {
        {"C-O" },
        {"C-CA" },
        {"CA-N" },
        {"CA-HA" },
        {"N-CD" },
        {"CD-HD2" },
        {"CD-HD3" },
        {"CD-CG" },
        {"CG-HG2" },
        {"CG-HG3" },
        {"CG-CB" },
        {"CB-CA" },
        {"CB-HB2" },
        {"CB-HB3" }
    };

    List<string> Asp = new List<string>()
    {
        {"C-O" },
        {"C-CA" },
        {"CA-N" },
        {"CA-HA" },
        {"N-H" },
        {"N-H1" },
        {"N-H2" },
        {"N-H3" },
        {"CA-CB" },
        {"CB-HB2" },
        {"CB-HB3" },
        {"CB-CG" },
        {"CG-OD1" },
        {"CG-OD2" },
    };

    List<string> Glu = new List<string>()
    {
        {"C-O" },
        {"C-CA" },
        {"CA-N" },
        {"CA-HA" },
        {"N-H" },
        {"N-H1" },
        {"N-H2" },
        {"N-H3" },
        {"CA-CB" },
        {"CB-HB2" },
        {"CB-HB3" },
        {"CB-CG" },
        {"CG-HG2" },
        {"CG-HG3" },
        {"CG-CD" },
        {"CD-OE1" },
        {"CD-OE2" },
    };

    List<string> Arg = new List<string>()
    {
        {"C-O" },
        {"C-CA" },
        {"CA-N" },
        {"CA-HA" },
        {"N-H" },
        {"N-H1" },
        {"N-H2" },
        {"N-H3" },
        {"CA-CB" },
        {"CB-HB2" },
        {"CB-HB3" },
        {"CB-CG" },
        {"CG-HG2" },
        {"CG-HG3" },
        {"CG-CD" },
        {"CD-HD2" },
        {"CD-HD3" },
        {"CD-NE" },
        {"NE-HE" },
        {"NE-CZ" },
        {"CZ-NH1" },
        {"NH1-HH11" },
        {"NH1-HH12" },
        {"CZ-NH2" },
        {"NH2-HH21" },
        {"NH2-HH22"}
    };

    List<string> Hid = new List<string>()
    {
        {"C-O" },
        {"C-CA" },
        {"CA-N" },
        {"CA-HA" },
        {"N-H" },
        {"N-H1" },
        {"N-H2" },
        {"N-H3" },
        {"CA-CB" },
        {"CB-HB2" },
        {"CB-HB3" },
        {"CB-CG" },
        {"CG-CD2" },
        {"CD2-HD2" },
        {"CD2-NE2" },
        {"NE2-CE1" },
        {"CE1-HE1" },
        {"CE1-ND1" },
        {"ND1-CG" },
        {"ND1-HD1" }
    };

    List<string> Hie = new List<string>()
    {
        {"C-O" },
        {"C-CA" },
        {"CA-N" },
        {"CA-HA" },
        {"N-H" },
        {"N-H1" },
        {"N-H2" },
        {"N-H3" },
        {"CA-CB" },
        {"CB-HB2" },
        {"CB-HB3" },
        {"CB-CG" },
        {"CG-CD2" },
        {"CD2-HD2" },
        {"CD2-NE2" },
        {"NE2-CE1" },
        {"NE2-HE2" },
        {"CE1-HE1" },
        {"CE1-ND1" },
        {"ND1-CG" }
    };

    List<string> Lys = new List<string>()
    {
        {"C-O" },
        {"C-CA" },
        {"CA-N" },
        {"CA-HA" },
        {"N-H" },
        {"N-H1" },
        {"N-H2" },
        {"N-H3" },
        {"CA-CB" },
        {"CB-HB2" },
        {"CB-HB3" },
        {"CB-CG" },
        {"CG-HG2" },
        {"CG-HG3" },
        {"CG-CD" },
        {"CD-HD3" },
        {"CD-HD2" },
        {"CD-CE" },
        {"CE-HE3" },
        {"CE-HE2" },
        {"CE-NZ" },
        {"NZ-HZ1" },
        {"NZ-HZ2" },
        {"NZ-HZ3" }
    };

    List<string> As4 = new List<string>()
    {
        {"C-O" },
        {"C-CA" },
        {"CA-N" },
        {"CA-HA" },
        {"N-H" },
        {"N-H1" },
        {"N-H2" },
        {"N-H3" },
        {"CA-CB" },
        {"CB-HB2" },
        {"CB-HB3" },
        {"CB-CG" },
        {"CG-OD1" },
        {"CG-OD2" },
        {"OD1-HD11" },
        {"OD1-HD12" },
        {"OD2-HD21" },
        {"OD2-HD22" }
    };

    List<string> Hip = new List<string>()
    {
        {"C-O" },
        {"C-CA" },
        {"CA-N" },
        {"CA-HA" },
        {"N-H" },
        {"N-H1" },
        {"N-H2" },
        {"N-H3" },
        {"CA-CB" },
        {"CB-HB2" },
        {"CB-HB3" },
        {"CB-CG" },
        {"CG-CD2" },
        {"CD2-HD2" },
        {"CD2-NE2" },
        {"NE2-CE1" },
        {"NE2-HE2" },
        {"CE1-HE1" },
        {"CE1-ND1" },
        {"ND1-CG" },
        {"ND1-HD1" }
    };

    List<string> Gl4 = new List<string>()
    {
        {"C-O" },
        {"C-CA" },
        {"CA-N" },
        {"CA-HA" },
        {"N-H" },
        {"N-H1" },
        {"N-H2" },
        {"N-H3" },
        {"CA-CB" },
        {"CB-HB2" },
        {"CB-HB3" },
        {"CB-CG" },
        {"CG-HG2" },
        {"CG-HG3" },
        {"CG-CD" },
        {"CD-OE1" },
        {"CD-OE2" },
        {"OE2-HE21" },
        {"OE2-HE22" },
        {"OE1-HE11" },
        {"OE1-HE12" }
    };



}
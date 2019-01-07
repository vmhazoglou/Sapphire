using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallAndStick : MonoBehaviour {

    topinfo top;
    public GameObject parent1, Manager;
    public double distance;
    private int frame;
    GameObject sphere,spheree;
    public GameObject HologramCollection, MoleculeCollection;
    public Shader shaderr;
    public Material spheremat;
    MeshRenderer renderr;
    MaterialPropertyBlock props;
    public Material carbonmat, nitrogenmat, hydrogenmat, oxygenmat, sulfurmat;

    public void changetobns()
    {
        frame = transform.GetChild(4).gameObject.GetComponent<PlayScript>().i;
        transform.GetChild(4).gameObject.GetComponent<PlayScript>().representation = "BnS";
        transform.GetChild(0).gameObject.SetActive(true);
    }
    public void BnS()
    {
        frame = transform.GetChild(4).gameObject.GetComponent<PlayScript>().i;
        //transform.GetChild(4).gameObject.GetComponent<PlayScript>().representation = "BnS";
        //transform.GetChild(0).gameObject.SetActive(true);
        if (parent1.transform.childCount != Manager.GetComponent<Load>().top.NATOM)
        {
            CreatePrimitive(parent1.transform.childCount);
            MoveSpheres(frame);
        }

        sph = 0;
        StartCoroutine(EnableSpheres());
    }

    int sph = 0;
    private IEnumerator EnableSpheres()
    {
        while (sph < top.NATOM)
        {
            transform.GetChild(0).transform.GetChild(sph).gameObject.SetActive(true);
            sph++;
            yield return new WaitForSeconds(0.01f);
        }

    }

    public GameObject SpherePrefabs;
    // make this int currentatoms thing just a check to see that we're rendering under 800 atoms
    public void CreatePrimitive(int currentatoms)
    {
        top.NATOM = GetComponent<ThisFileInfo>().top.NATOM;

        double starttime = Time.realtimeSinceStartup;
        for (int a = currentatoms; a < top.NATOM; a++)
        {
            // this can be sped up by not using a switch case
            // use arrays

            switch (Manager.GetComponent<Load>().molecule[0, a].element)
            {
                case "O":
                    Transform oxygen = Instantiate(SpherePrefabs.transform.GetChild(4));
                    oxygen.name = "Atom" + a;
                    oxygen.tag = "Atoms";
                    oxygen.transform.parent = parent1.transform;
                    oxygen.transform.localScale = new Vector3(0.0168f, 0.0168f, 0.0168f) *VdWRadii[Manager.GetComponent<Load>().molecule[0, a].element];
                    break;
                case "C":
                    Transform carbon = Instantiate(SpherePrefabs.transform.GetChild(2));
                    carbon.name = "Atom" + a;
                    carbon.tag = "Atoms";
                    carbon.transform.parent = parent1.transform;
                    carbon.transform.localScale = new Vector3(0.0168f, 0.0168f, 0.0168f) * VdWRadii[Manager.GetComponent<Load>().molecule[0, a].element];
                    break;
                case "N":
                    Transform nitrogen = Instantiate(SpherePrefabs.transform.GetChild(3));
                    nitrogen.name = "Atom" + a;
                    nitrogen.tag = "Atoms";
                    nitrogen.transform.parent = parent1.transform;
                    nitrogen.transform.localScale = new Vector3(0.0168f, 0.0168f, 0.0168f) * VdWRadii[Manager.GetComponent<Load>().molecule[0, a].element];
                    break;
                case "H":
                    Transform hydrogen = Instantiate(SpherePrefabs.transform.GetChild(1));
                    hydrogen.name = "Atom" + a;
                    hydrogen.tag = "Atoms";
                    hydrogen.transform.parent = parent1.transform;
                    hydrogen.transform.localScale = new Vector3(0.0168f, 0.0168f, 0.0168f) * VdWRadii[Manager.GetComponent<Load>().molecule[0, a].element];
                    break;
                case "S":
                    Transform sulfur = Instantiate(SpherePrefabs.transform.GetChild(5));
                    sulfur.name = "Atom" + a;
                    sulfur.tag = "Atoms";
                    sulfur.transform.parent = parent1.transform;
                    sulfur.transform.localScale = new Vector3(0.0168f, 0.0168f, 0.0168f) * VdWRadii[Manager.GetComponent<Load>().molecule[0, a].element];
                    break;
                default:
                    Transform def = Instantiate(SpherePrefabs.transform.GetChild(0));
                    def.name = "Atom" + a;
                    def.tag = "Atoms";
                    def.transform.parent = parent1.transform;
                    def.transform.localScale = new Vector3(0.0168f, 0.0168f, 0.0168f) * 1.70f;
                    break;
            }
            
        }
        Debug.Log($"Time draw spheres: {Time.realtimeSinceStartup - starttime}");
    }

    public Transform nitrogenpf, carbonpf, oxygenpf, hydrogenpf, sulfurpf, defaultpf;

    public void MoveSpheres(int frame)
    {
        for (int a = 0; a < top.NATOM; a++)
        {
            // Get first child of File Object (AtomCollection)
            // Get child a of that (each atom)
            // Change the position based on the frame
            transform.GetChild(0).transform.GetChild(a).gameObject.transform.localPosition = new Vector3(GetComponent<ThisFileInfo>().molecule[frame, a].x,
                                                                                                         GetComponent<ThisFileInfo>().molecule[frame, a].y,
                                                                                                         GetComponent<ThisFileInfo>().molecule[frame, a].z);
        }

        // Update the position of the bonds
        GetComponent<ViewLine>().MoveLine(frame, GetComponent<ThisFileInfo>().molecule);
    }

    public Color Sulfur, Phosphorus, Potassium, Magnesium, Aluminium; 
    void ColorAtoms(string element, Transform sphere)
    {
        switch (element)
        {
            case "O":
                props.SetColor("red", Color.red);
                break;
            case "C":
                props.SetColor("black", Color.black);
                break;
            case "N":
                props.SetColor("blue", Color.blue);
                break;
            case "H":
                props.SetColor("white", Color.white);
                break;
            case "S":
                props.SetColor("yellow", Sulfur);
                break;
            default:
                props.SetColor("cyan", Color.cyan);
                break;
        }
        renderr = sphere.GetComponent<MeshRenderer>();
        renderr.SetPropertyBlock(props);
    }

	// Citation: http://www.crystalmaker.com/support/tutorials/crystalmaker/atomic-radii/index.html
	// Missing VdW radii used those of nearby elements on periodic table

        // Why am I using VdW radii instead of covalent here?? Use the covalent radii in AtomicInfo
	Dictionary<string, float> VdWRadii = new Dictionary<string, float>()
	{
		{"H", 1.20f},
		{"He", 1.40f},
		{"Li", 1.82f},
		{"Be", 1.70f },
		{"B",1.70f },
		{"C", 1.70f },
		{"N",1.55f },
		{"O",1.52f },
		{"F",1.47f },
		{"Ne",1.54f },
		{"Na",2.21f },
		{"Mg",1.73f },
		{"Al",2.10f },
		{"Si",2.10f },
		{"P",1.80f },
		{"S", 1.80f },
		{"Cl",1.75f },
		{"Ar",1.88f },
		{"K",2.75f }
		 // can add more later or perhaps use Atomic Radii. 
		 // default : Carbon is drawn always as Vector3(0.015f, 0.015f, 0.015f) as a reference point
		 // elements with no data here will have the same, so 1.70 VdW radius will be used
		 // others will be resized accordingly

		// 0.0088 * 1.7 ~= 0.015
	};
}

using UnityEngine;
using System.IO;
using System.Text;
using CI.WSANative.FilePickers;
using HoloToolkit.Unity;
using MixedRealityToolkit.Utilities;
using UnityEngine.UI;
using System;
using CI.HttpClient;

public class OpenFileButton : MonoBehaviour
{
#pragma warning disable 0219
#pragma warning disable 0414
#pragma warning disable 0618

    public string filestring, topfilestring, xfilestring;
    public GameObject LoadButton, HologramCollection;
    public Text ConsoleTex;

	public void ShowFileOpenPicker()
    {
		// File picker to be used on mixed reality device
#if !UNITY_EDITOR
        // StopAllCoroutines();
		HologramCollection.SetActive(false);
        WSANativeFilePicker.PickSingleFile("Select", WSAPickerViewMode.Thumbnail, WSAPickerLocationId.PicturesLibrary, new[] { ".txt", ".pdb", ".top", ".crd", ".nc", ".x", ".out", ".log", ".xyz", ".prmtop" }, (result) =>
        {
             if (result != null)
             {
		filestring = result.ReadText();
        ConsoleTex.text += "\n File loaded in successfully. Click \"Load\" button.";

		switch (result.FileType)
		{
			case ".pdb":
				GameObject.Find("Manager").GetComponent<Dropdowns>().filetypeSelected = "PDB";
                //GameObject.Find("Manager").GetComponent<Dropdowns>().filetypeDropdown.value = GameObject.Find("Manager").GetComponent<Dropdowns>().filetypes.IndexOf("PDB");
				break;
			case ".out":
				GameObject.Find("Manager").GetComponent<Dropdowns>().filetypeSelected = "ACES"; 
				break;
			case ".xyz":
				GameObject.Find("Manager").GetComponent<Dropdowns>().filetypeSelected = "XYZ"; 
				break;
			default:
				break;
		}

                ConsoleTex.text += String.Format("\n Filetype extension: {0}, automatically loaded in with filetype {1}", result.FileType, GameObject.Find("Manager").GetComponent<Dropdowns>().filetypeSelected);

            }
        });

        HologramCollection.SetActive(true);

        //GameObject.Find("Manager").GetComponent<Load>().LoadMolecule();

#endif

		// Opening files from online sources for Unity testing

#if UNITY_EDITOR

        GameObject Manager = GameObject.Find("Manager");

        Manager.GetComponent<Dropdowns>().filetypeSelected = "XYZ";

		HttpClient httpClient = new HttpClient();

		httpClient.GetString(new Uri("https://s3.us-east-2.amazonaws.com/sapphirefiles/IRC001+(2).xyz"), (r) =>
		{
			filestring = r.Data.ToString();
			gameObject.GetComponent<LoadXYZ>().filestring = r.Data.ToString();
			gameObject.GetComponent<Load>().LoadMolecule();
		});

		// Below is left as reference in case one is using two files, i.e. AMBER parm and crd

        /*

        var fileStream2 = new FileStream(@"C:\Users\Roitberg Lab\OneDrive\Test Files\AMBER\1uao.top", FileMode.Open, FileAccess.Read);
		using (var streamReader = new StreamReader(fileStream2, Encoding.UTF8))
		{
			topfilestring = streamReader.ReadToEnd();

			//filestring = GameObject.Find("Manager").GetComponent<TheFile>().oneuaofilestring;
		}

		var fileStream3 = new FileStream(@"C:\Users\Roitberg Lab\OneDrive\Test Files\AMBER\md2.x", FileMode.Open, FileAccess.Read);
		using (var streamReader = new StreamReader(fileStream3, Encoding.UTF8))
		{
			xfilestring = streamReader.ReadToEnd();

			//filestring = GameObject.Find("Manager").GetComponent<TheFile>().oneuaofilestring;
		}

		*/
		
#endif

    }
}

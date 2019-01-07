using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;

public class Dropdowns : MonoBehaviour {

	public Text dropdownLabel;
	public GameObject Manager;
	public Dropdown filetypeDropdown, representationDropdown;
	public string filetypeSelected, representationSelected;

	// List of filetypes is here. Must be in the same order as in the dropdown menu.
	public List<string> filetypes = new List<string>() { "Select Filetype", "PDB", "XYZ", "ACES", "AMBER Parm", "AMBER Coord", "Gaussian", "MOL2", "Orca"};
	public List<string> representations = new List<string>() { "Select Rep", "Ball and Stick", "Line", "Cartoon" };

	public void FiletypeDropdown(int index)
	{
		index = filetypeDropdown.value;
		dropdownLabel.text = filetypes[index];
		filetypeSelected = filetypes[index];
	}

	public void RepresentationDropdown(int index)
	{
		switch(representations[representationDropdown.value])
		{
			case "Ball and Stick":
				Manager.GetComponent<BallAndStick>().BnS();
				break;
			case "Line":
				Manager.GetComponent<ViewLine>().Line();
				break;
			case "Cartoon":
				Manager.GetComponent<ParseDSSP>().ParseFile();
				break;
			default:
				break;
		}
	}
}

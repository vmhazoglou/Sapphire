using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CI.HttpClient;
using System;
using System.Text.RegularExpressions;
using UnityEngine.UI;


public class PDBHTTPGet : MonoBehaviour {

	HttpClient httpClient = new HttpClient();
	String id = "";
	public string PDBfile;
	public string PDBID;
	public Text txt, consoleText;
	public string outputfile;

	// Use this for initialization
	void Start () {
	}

	private void OnEnable()
	{
		/*
		 * 
		 * 
		 * 
		Debug.Log("https://files.rcsb.org/download/" + id + ".pdb");
		httpClient.GetString(new Uri("https://files.rcsb.org/download/1UAO.pdb"), (r) =>
		{
		Debug.Log(r.StatusCode.ToString());
		Debug.Log(r.Data.ToString());
		consoleText.text += $"\n PDB file successfully loaded.\n\n";
			//consoleText.text += "final Result:" + response.Data.ToString(); // print results to console 
			//if you want to see more text when scrolling you need to increase height of "content"
			// console can be enabled/disabled in unity in settings > console

			//outputfile = response.Data.ToString();
			//gameObject.GetComponent<ParseDSSP>().DSSPfile = response.Data.ToString();
			//gameObject.GetComponent<ParseDSSP>().ParseFile(response.Data.ToString());

			//GameObject.Find("Manager").GetComponent<ParseDSSP>().ParseFile();

			//gameObject.GetComponent<HttpTest>().enabled = false;
		});

	*/
	}
	public GameObject Keyboard;
	public void wakeup()
	{
		id = txt.text;

		GameObject.Find("Manager").GetComponent<PDBHTTPGet>().enabled = true;
		Keyboard.SetActive(false);
	}

	// Update is called once per frame
	void Update () {
		
	}

}



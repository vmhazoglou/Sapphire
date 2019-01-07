using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CI.HttpClient;
using System;
using System.Text.RegularExpressions;
using UnityEngine.UI;

public class HttpTest : MonoBehaviour
{
	HttpClient httpClient = new HttpClient();
	String id = "";
	int counter = 20;
	bool status = false;
	float timer;
	public float waitTime = 1f;
	public string DSSPfile;
	public string PDBID;
	public Text txt, consoleText;
	public string outputfile;

	public void wakeup()
	{
        GameObject.Find("Manager").GetComponent<HttpTest>().PDBID = txt.text;

		GameObject.Find("Manager").GetComponent<HttpTest>().enabled = true;
        GameObject.Find("Keyboard").SetActive(false);
	}

	private void OnEnable()
	{
		counter = 100;
		status = false;
		//txt.text = null;

		consoleText.text += "\n Initializing the HTTP call for " + PDBID;
		Dictionary <string, string> data = new Dictionary<string, string>()
		{
			{ "data", PDBID },
		};
		FormUrlEncodedContent formUrlEncodedContent = new FormUrlEncodedContent(data);

		httpClient.Post(new Uri("http://www.cmbi.ru.nl/xssp/api/create/pdb_id/dssp/"), formUrlEncodedContent, (r) =>
		{
			consoleText.text += string.Format("\n Making the HTTP call... ");
			//+  "Orginal Response:" + r.OriginalResponse.ToString()+"\\n \n" + "Status Code: " + r.StatusCode.ToString() + "\\n \n";
			string result = r.Data.ToString();
			id = Regex.Split(Regex.Split(result, ": \"")[1], "\"")[0];
		});
		
	}
	private void Update()
	{
		timer += Time.deltaTime;
		if (timer > waitTime && (!status))
		{
			consoleText.text += string.Format("\n Waiting for HTTP call... Time: {0} seconds", (waitTime - counter + 100));
			{
				if (counter < 0){
					status = true;
					consoleText.text += string.Format("\n The API is not returning correct request!");
				}

				httpClient.GetString(new Uri("http://www.cmbi.ru.nl/xssp/api/status/pdb_id/dssp/" + id + "/"), (response) =>
				{
					String resulttemp = response.Data.ToString();
					String output = Regex.Split(Regex.Split(resulttemp, "status\": \"")[1], "\"")[0];
					if (output == "SUCCESS")
					{
						resulttemp = null;
						output = null;
						status = true;
					}
					else if (output == "FAILURE")
					{
						resulttemp = null;
						output = null;
						status = false;
						gameObject.GetComponent<HttpTest>().enabled = false;
						consoleText.text += string.Format("\n Four character code not recognized. Enter again.\n\n");
					}
					else
						status = false;
				});
				counter--;
				timer = 0f;
			}
		}

		if (status && counter != -2) //runs only once the status is true
		{
			consoleText.text += string.Format("\n DSSP database returned request in: {0} seconds.", (waitTime - counter + 100));
			counter = -2; // run only once
			httpClient.GetString(new Uri("http://www.cmbi.ru.nl/xssp/api/result/pdb_id/dssp/" + id + "/"), (response) =>
			{
				//Debug.Log(response.Data.ToString());

				consoleText.text += string.Format("\n DSSP file successfully loaded.\n\n");
				//consoleText.text += "final Result:" + response.Data.ToString(); // print results to console 
				//if you want to see more text when scrolling you need to increase height of "content"
				// console can be enabled/disabled in unity in settings > console

				outputfile = response.Data.ToString();
				gameObject.GetComponent<ParseDSSP>().DSSPfile = response.Data.ToString();
				//gameObject.GetComponent<ParseDSSP>().ParseFile(response.Data.ToString());

				GameObject.Find("Manager").GetComponent<ParseDSSP>().ParseFile();

				gameObject.GetComponent<HttpTest>().enabled = false;
			});
		}

	}
	
}




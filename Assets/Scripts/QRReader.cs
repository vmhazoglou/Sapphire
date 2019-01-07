using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class QRReader : MonoBehaviour
{
    // Test URL:
    // https://s3.us-east-2.amazonaws.com/sapphirefiles/IRC001+(2).xyz
    // https://files.rcsb.org/download/1UAO.pdb
    public Text tex;
    public string url;
    public UnityWebRequest www;

    void Start()
    {
        tex.text += string.Format("\n QR script initialized.");
    }

    public Transform textMeshObject;

    private IEnumerator WaitAndPrint(float waitTime, UnityWebRequest www)
    {
        yield return www.SendWebRequest();
        while (!www.isDone)
        {
            tex.text += String.Format("\n Download progress: {0}", www.downloadProgress * 100);
            yield return new WaitForSeconds(waitTime);
        }
        if (www.isNetworkError || www.isHttpError)
        {
            tex.text += www.error;
        }
        if (www.isDone)
        {
            GameObject.Find("Manager").GetComponent<OpenFileButton>().filestring = www.downloadHandler.text;
            tex.text += "\n File loaded in successfully. Click \"Load\" button.";

            switch (www.url.Split('.')[www.url.Split('.').Length - 1])
            {
                case "pdb":
                    GameObject.Find("Manager").GetComponent<Dropdowns>().filetypeSelected = "PDB";
                    //GameObject.Find("Manager").GetComponent<Dropdowns>().filetypeDropdown.value = GameObject.Find("Manager").GetComponent<Dropdowns>().filetypes.IndexOf("PDB");
                    break;
                case "out":
                    GameObject.Find("Manager").GetComponent<Dropdowns>().filetypeSelected = "ACES";
                    break;
                case "xyz":
                    GameObject.Find("Manager").GetComponent<Dropdowns>().filetypeSelected = "XYZ";
                    break;
                default:
                    break;
            }

            tex.text += String.Format("\n Filetype extension: {0}, automatically loaded in with filetype {1}", www.url.Split('.')[www.url.Split('.').Length - 1], GameObject.Find("Manager").GetComponent<Dropdowns>().filetypeSelected);

            www = null;
            yield break;
        }
    }

    
    public void OnRun()
    {    
        tex.text += string.Format("\n Scanning for 30 seconds...");
#if !UNITY_EDITOR
        MediaFrameQrProcessing.Wrappers.ZXingQrCodeScanner.ScanFirstCameraForQrCode(
            result =>
            {
                UnityEngine.WSA.Application.InvokeOnAppThread(() =>
            {
                tex.text += "\n QR code scanned.";
                tex.text += String.Format("\n URL: {0}", result);

                www = UnityWebRequest.Get(result);
                StartCoroutine(WaitAndPrint(1.0f, www));

            },
            false);
            },
            TimeSpan.FromSeconds(30));
#endif
    }
}
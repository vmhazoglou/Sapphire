using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeShader : MonoBehaviour {

	public GameObject selectedObject;
	public Text consoleText;
	public Shader Default, Metallic, Transparent;

	public void Shader()
	{
		if (selectedObject != null)
		{

		}
		else
		{
			consoleText.text += "\n No object is selected.";
		}
	}
}

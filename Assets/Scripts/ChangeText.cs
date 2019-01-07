using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChangeText : MonoBehaviour
{
	public GameObject Manager;
    Text txt;
    public int currentFrame;

    // Use this for initialization
    public void ChangeIt()
    {
        txt = gameObject.GetComponent<Text>();
        txt.text = string.Format("({0}/{1})", currentFrame, (Manager.GetComponent<Load>().framenum - 1));
    }

    /*
    // Update is called once per frame
    void Update()
    {
        txt.text = "Score : " + currentscore;
        currentscore = PlayerPrefs.GetInt("TOTALSCORE");
        PlayerPrefs.SetInt("SHOWSTARTSCORE", currentscore);
    }
    */
}
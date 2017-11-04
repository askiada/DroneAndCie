using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUDManager : MonoBehaviour {

    public Text prefabText;
	// Use this for initialization
	void Start () {

    }


    public void AddTextLayout(string name, string value)
    {
        GameObject go = new GameObject();
        LayoutElement le = go.AddComponent<LayoutElement>();
        le.name = name;
        Text text = go.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        text.fontSize = 12;
        text.color = new Color(0, 0, 0);
        go.transform.SetParent(GameObject.Find("DroneInfo").transform, false);
    }
	


    public void UpdateTextLayout(string name, string text)
    {
        GameObject.Find(name).GetComponent<Text>().text = text;
    }

    // Update is called once per frame
    void Update()
    {

    }



}

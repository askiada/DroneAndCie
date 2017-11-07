using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HUDManager : MonoBehaviour {

    public Text prefabText;


    /*public void AddGraph(string name, List<int> list)
    {
        GameObject go = new GameObject();
        LayoutElement le = go.AddComponent<LayoutElement>();
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.name = "LR";
        //go.AddComponent<RectTransform>();
        le.name = name;
        Graph graph = go.AddComponent<Graph>();
        //LineRenderer lr = graph.gameObject.AddComponent<LineRenderer>();
        
        graph.Draw(list);
        go.transform.SetParent(GameObject.Find("DroneInfo").transform, false);
    }*/

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
}

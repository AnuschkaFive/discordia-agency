using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class CreateGame : MonoBehaviour {

    // Use this for initialization
    void Start() {

    }

    void loadGame(string lvlName) {
        XmlReader reader = XmlReader.Create("Level1.xml");

        while (reader.Read()) {

        }

        reader.ReadEndElement();
        reader.Close();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class DataController : MonoBehaviour {
    public static GameData activeData;
    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        
    }
    public void Save() {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/game.dat");

        GameData dataToSave = new GameData(activeData.saveNum);

        bf.Serialize(file, dataToSave);
        file.Close();
    }

    public void Load() {
        if (File.Exists(Application.persistentDataPath + "/game.dat")) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/game.dat", FileMode.Open);
            activeData = (GameData)bf.Deserialize(file);
            file.Close();
      

        }
    }

    [Serializable]
    public class GameData {
        public int saveNum = 0;

        public GameData(int saveNum) {
            this.saveNum = saveNum+1;
        }
    }
}

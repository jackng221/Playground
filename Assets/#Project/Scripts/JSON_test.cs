using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JSON_test : MonoBehaviour
{
    [SerializeField] Data data = new Data();

    [ContextMenu("Write JSON")]
    void Write()
    {
        string json = JsonUtility.ToJson(data, true);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/JSON.json", json);
    }
    
    [ContextMenu("Read JSON")]
    void Read()
    {
        string json = System.IO.File.ReadAllText(Application.persistentDataPath + "/JSON.json");
        data = JsonUtility.FromJson<Data>(json);
    }
}

[System.Serializable]
public class Data
{
    public List<Character> characters = new List<Character>();
    public List<Item> items = new List<Item>();
}

[System.Serializable]
public class Item
{
    public string _name;
    public int _stat;
}

[System.Serializable]
public class Character
{
    public string _name;
    public ChrClass _class;
    public enum ChrClass
    {
        Braver,
        Warden,
        Mage
    }
}
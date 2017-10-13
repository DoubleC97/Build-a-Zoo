using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LevelEditor;

public class LevelManager : MonoBehaviour {


    GridBase gridBase;

    public List<GameObject> inSceneGameObjects = new List<GameObject>();
    public List<GameObject> inSceneWalls = new List<GameObject>();
    public List<GameObject> inSceneStackObjects = new List<GameObject>();

    private static LevelManager instance = null;
    public static LevelManager GetInstance()
    {
        return instance;
    }

    void Awake()
    {
        instance = this;   
    }

     void Start()
    {
        gridBase = GridBase.GetInstance();

        //InitLevelObjects();
    }

    void InitLevelObjects()
    {
        if(inSceneGameObjects.Count > 0)
        {
            for (int i =0; i < inSceneGameObjects.Count; i++)
            {
                Level_Object obj = inSceneGameObjects[i].GetComponent<Level_Object>();
                obj.UpdateNode(gridBase.grid);
            }
        }
    }

}

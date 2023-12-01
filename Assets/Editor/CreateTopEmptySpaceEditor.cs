using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreateTopEmptySpaceEditor : EditorWindow
{
    private string PATH_FOLDER = "Assets/Prefab/AfterMasterData/";

    private HashSet<GameObject> m_dropList = new HashSet<GameObject>();

    //ÉÅÉjÉÖÅ[Ç…çÄñ⁄í«â¡
    [MenuItem("ägí£/TopEmptySpaceEditor")]
    static void open()
    {
        EditorWindow.GetWindow<CreateTopEmptySpaceEditor>("TopEmptySpaceEditor");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("TopEmptySpaceEditor");
        var dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drop objects on which players can put things");

        var evt = Event.current;
        switch (evt.type)
        {
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                {
                    break;
                }
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                DragAndDrop.AcceptDrag();
                break;

            case EventType.DragExited:
                foreach (GameObject go in DragAndDrop.objectReferences)
                {
                    Debug.Log(go);
                    m_dropList.Add(go);
                }

                CreatePrefabs();
                Event.current.Use();
                break;
        }
    }

    void CreatePrefabs()
    {
        foreach (GameObject go in m_dropList)
        {
            if (null == m_dropList)
            {
                return;
            }
            GameObject newGo = CreateTopEmptySpace(go);
            if(newGo != null)
            {
                string newGoPath = PATH_FOLDER + go.name + ".prefab";
                PrefabUtility.SaveAsPrefabAsset(newGo, newGoPath);
                Object.DestroyImmediate(newGo);
            }

        }
        m_dropList.Clear();
    }

    GameObject CreateTopEmptySpace(GameObject prefab)
    {
        if (!prefab.name.ContainsUpperOrLower("desk") && !prefab.name.ContainsUpperOrLower("shelf") && !prefab.name.ContainsUpperOrLower("table"))
        {
            return null;
        }

        GameObject go = Object.Instantiate(prefab);
        RoomObject goRoomObject = go.GetComponent<RoomObject>();

        if (goRoomObject == null)
        {
            return null;
        }
        else
        {
            if (goRoomObject.EmptySpaceBehaviourList.Count > 0)
            {
                return null;
            }
        }

        if (go.transform.childCount == 0)
        {
            return null;
        }

        GameObject childObject = go.transform.GetChild(0).gameObject;
        MeshFilter meshFilter = childObject.GetComponent<MeshFilter>();
        if(meshFilter == null || meshFilter.sharedMesh == null)
        {
            return null;
        }

        string assetPath = AssetDatabase.GetAssetPath(meshFilter.sharedMesh);
        ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
        if(importer != null)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }

        Outline floorOutline = childObject.AddComponent<Outline>();

        GameObject topEmptySpace = new GameObject();
        topEmptySpace.name = "Top";
        topEmptySpace.transform.SetParent(go.transform);

        BoxCollider goCollider = go.GetComponent<BoxCollider>();
        if(goCollider != null)
        {
            topEmptySpace.transform.localPosition = new Vector3(0f, goCollider.size.y * go.transform.localScale.y, 0f);
            topEmptySpace.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            BoxCollider topCollider = topEmptySpace.AddComponent<BoxCollider>();
            topCollider.size = Vector3.Scale(goCollider.size, go.transform.localScale);
            topCollider.size = Vector3.Scale(topCollider.size, new Vector3(1f, 1f, 1f));
            topCollider.center = Vector3.Scale(goCollider.center, go.transform.localScale);

            EmptySpaceBehaviour esBehaviour = topEmptySpace.AddComponent<EmptySpaceBehaviour>();
            esBehaviour.Id = -1;
            esBehaviour.FloorOutline = floorOutline;
            topEmptySpace.layer = LayerMask.NameToLayer("EmptySpace");
            goRoomObject.EmptySpaceBehaviourList.Add(esBehaviour);
        }

        return go;
    }
}

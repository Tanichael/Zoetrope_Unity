using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AddRoomObjectTappableEditor : EditorWindow
{
    private HashSet<GameObject> m_dropList = new HashSet<GameObject>();
    private Camera m_CaptureCamera;

    [MenuItem("MasterData/AddRoomObjectTappable")]
    static void open()
    {
        EditorWindow.GetWindow<AddRoomObjectTappableEditor>("AddRoomObjectTappableEditor");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("AddRoomObjectTappableEditor");
        var dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drop objects to get icons");

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

                HandleDropList();
                Event.current.Use();
                break;
        }
    }

    void HandleDropList()
    {
        foreach (GameObject go in m_dropList)
        {
            if (null == m_dropList)
            {
                return;
            }
            AddRoomObjectSelectable(go);
        }
        m_dropList.Clear();
        AssetDatabase.SaveAssets();
    }

    void AddRoomObjectSelectable(GameObject prefab)
    {
        int cnt = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(prefab);

        GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

        // Create a new camera and setup its position
        if (go == null)
        {
            Debug.Log("go is null");
            return;
        }

        go.transform.SetParent(null);

        if (go.GetComponent<RoomObject>() == null)
        {
            go.AddComponent<RoomObjectMovable>();
        }

        string assetPath = AssetDatabase.GetAssetPath(prefab);
        PrefabUtility.SaveAsPrefabAsset(go, assetPath);

        // Clean up
        Object.DestroyImmediate(go);
    }
}

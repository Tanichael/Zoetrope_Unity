using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateObjectIconEditor : EditorWindow
{
    private string PATH_FOLDER = "Assets/Texture/RoomObjectIcons/";

    private HashSet<GameObject> m_dropList = new HashSet<GameObject>();
    private Camera m_CaptureCamera;

    [MenuItem("Icon/Create Icons")]
    static void open()
    {
        EditorWindow.GetWindow<CreateObjectIconEditor>("CreateObjectIconEditor");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("CreateObjectIconEditor");
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

                CreateObjectIcons();
                Event.current.Use();
                break;
        }
    }

    void CreateObjectIcons()
    {
        m_CaptureCamera = new GameObject("Camera").AddComponent<Camera>();
        m_CaptureCamera.cullingMask = (1 << 0);
        m_CaptureCamera.clearFlags = CameraClearFlags.SolidColor;

        foreach (GameObject go in m_dropList)
        {
            if (null == m_dropList)
            {
                return;
            }

            SaveTextureToPNG(TakeScreenShot(go));
        }
        Object.DestroyImmediate(m_CaptureCamera.gameObject);
        m_dropList.Clear();
    }

    Texture2D TakeScreenShot(GameObject prefab)
    {
        GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

        // Create a new camera and setup its position
        if(go == null)
        {
            Debug.Log("go is null");
        }
        m_CaptureCamera.transform.position = go.transform.position;
        m_CaptureCamera.transform.position += Vector3.back * 2;

        go.transform.position -= Vector3.up * 0.3f;
        go.transform.localRotation = Quaternion.Euler(new Vector3(4.48f, 116.2f, -10.277f));

        // Set up the RenderTexture
        RenderTexture renderTexture = new RenderTexture(512, 512, 24);
        m_CaptureCamera.targetTexture = renderTexture;

        // Render the object
        m_CaptureCamera.Render();

        // Get the RenderTexture data
        RenderTexture.active = renderTexture;
        Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height);
        texture2D.name = prefab.name;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        // Clean up
        Object.DestroyImmediate(go);
        //RenderTexture.ReleaseTemporary(renderTexture);

        return texture2D;
    }

    void SaveTextureToPNG(Texture2D texture)
    {
        if (texture == null)
        {
            Debug.Log("texture is null");
            return;
        }

        byte[] pngData = texture.EncodeToPNG();

        if (pngData != null)
        {
            File.WriteAllBytes(PATH_FOLDER + texture.name + ".png", pngData);
            AssetDatabase.Refresh();
        }
    }
}

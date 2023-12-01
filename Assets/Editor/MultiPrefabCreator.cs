using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
/// <summary>
/// �����̃I�u�W�F�N�g���܂Ƃ߂�prefab������G�f�B�^�g��
/// ref: https://qiita.com/twt_paul/items/7394b82e475366f89224
/// </summary>
public class MultiPrefabCreator : EditorWindow
{

    // �o�͐�
    private string PATH_FOLDER = "Assets/Prefab/AfterMasterData/";

    private HashSet<GameObject> m_dropList = new HashSet<GameObject>();

    //���j���[�ɍ��ڒǉ�
    [MenuItem("�g��/MultiPrefabCreator")]
    static void open()
    {
        EditorWindow.GetWindow<MultiPrefabCreator>("MultiPrefabCreator");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("MultiPrefabCreator");
        var dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "������Prefab�����������̂��܂Ƃ߂Ă��h���b�O���h���b�v");

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


    /// <summary>
    /// Prefab��
    /// </summary>
    void CreatePrefabs()
    {
        foreach (GameObject go in m_dropList)
        {
            if (null == m_dropList)
            {
                return;
            }
            string prefab = PATH_FOLDER + go.name + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(go, prefab);

        }
        m_dropList.Clear();
    }
}
#endif

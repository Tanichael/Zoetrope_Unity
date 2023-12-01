using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class MakeRoomObjectEditor : AssetPostprocessor
{
    private static string ms_BeforeMasterDataPath = "Assets/Prefab/BeforeMasterData";
    private static string ms_AfterMasterDataPath = "Assets/Prefab/AfterMasterData";

    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromPath)
    {
        foreach (var asset in importedAssets)
        {
            if (!asset.Contains(ms_BeforeMasterDataPath))
            {
                continue;
            }

            var loadAsset = AssetDatabase.LoadAssetAtPath<GameObject>(asset);
            if(loadAsset == null)
            {
                continue;
            }
            loadAsset = Object.Instantiate(loadAsset);
            string assetFullName = asset.ToString().Replace(ms_BeforeMasterDataPath + "/", "");
            loadAsset.name = assetFullName.Replace(".prefab", "") + "_mod";
            GameObject childObject = new GameObject(loadAsset.gameObject.name + "_child");
            childObject.transform.SetParent(loadAsset.transform);
            childObject.transform.localPosition = Vector3.zero;

            if (loadAsset.GetComponent<BoxCollider>() == null)
            {
                loadAsset.AddComponent<RoomObject>();
                BoxCollider boxCollider = loadAsset.gameObject.AddComponent<BoxCollider>();
                /*BoxCollider parentCollider = childObject.gameObject.AddComponent<BoxCollider>();
                parentCollider.size = boxCollider.size;
                parentCollider.center = Vector3.Scale(new Vector3(-1f, 1f, -1f), boxCollider.center);
                Object.DestroyImmediate(boxCollider);*/

                MeshFilter meshFilter = loadAsset.GetComponent<MeshFilter>();
                Mesh mesh = meshFilter.sharedMesh;
                MeshFilter childFilter = childObject.AddComponent<MeshFilter>();
                childFilter.sharedMesh = mesh;

                MeshRenderer meshRenderer = loadAsset.GetComponent<MeshRenderer>();
                List<Material> m = new List<Material>();
                Material sharedMaterial = meshRenderer.sharedMaterial;
                MeshRenderer childRenderer = childObject.AddComponent<MeshRenderer>();
                childRenderer.sharedMaterial = sharedMaterial;

                boxCollider.size = new Vector3(boxCollider.size.z, boxCollider.size.y, boxCollider.size.x);
                boxCollider.center = new Vector3(boxCollider.center.z, boxCollider.center.y, -boxCollider.center.x);
                childObject.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

                if(loadAsset.name.Contains("window") || loadAsset.name.Contains("Window"))
                {
                    childObject.gameObject.transform.localScale /= 2f;
                    boxCollider.size /= 2f;
                    boxCollider.center /= 2f;
                }
                
                Object.DestroyImmediate(loadAsset.GetComponent<MeshCollider>());
                Object.DestroyImmediate(meshRenderer);
                Object.DestroyImmediate(meshFilter);
            }

           /* var destFilePath = asset.Replace(ms_BeforeMasterDataPath, ms_AfterMasterDataPath);
            AssetDatabase.CopyAsset(asset, destFilePath);*/

            AssetDatabase.Refresh();
        }
    }
}
#endif
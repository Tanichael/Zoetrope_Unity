using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// ref: https://qiita.com/amam0102/items/469adb2b8fce1e46ad80
/// </summary>
public class RoomObjectMasterDataNameEditor : AssetPostprocessor
{
    private static string ms_RoomObjectMasterDataPath = "Assets/Prefab/MasterData/";
    private static string ms_Suffix = ".prefab";
    private static string ms_CsvPath = "Assets/MasterData/assetnames.csv";

    static void OnPostprocessAllAssets(
         string[] importedAssets,
         string[] deletedAssets,
         string[] movedAssets,
         string[] movedFromPath)
    {
        List<string> assetNames = new List<string>();

        foreach (var asset in importedAssets)
        {
            if (asset.Contains(ms_RoomObjectMasterDataPath) && asset.Contains(ms_Suffix))
            {
                int startIndex = ms_RoomObjectMasterDataPath.Length;
                int endIndex = asset.IndexOf(ms_Suffix);
                string assetName = asset.Substring(startIndex, endIndex - startIndex);
                assetNames.Add(assetName);
            }
        }

        try
        {
            StreamWriter file = new StreamWriter(ms_CsvPath, false, Encoding.Unicode);
            int id = 0;
            for (int i = 0; i < assetNames.Count; i++)
            {
                string assetName = assetNames[i];
                string typeName = GetTypeName(assetName);
                string posTypeName = GetPosTypeName(assetName);
                string isPlaneStr = GetIsPlaneStr(assetName);
                string levelStr = GetLevelStr(assetName);
                string tagsStr = GetTagsStr(assetName);
                string uiCategoryStr = GetUICategoryStr(assetName);

                if (uiCategoryStr != "")
                {
                    file.WriteLine(id.ToString() + "," + assetName + "," + typeName + "," + posTypeName + "," + isPlaneStr + "," + levelStr + "," + tagsStr + "," + uiCategoryStr);
                    id++;
                }
            }

            file.Close();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message); // 例外検出時にエラーメッセージを表示
        }

        AssetDatabase.Refresh();
    }

    private static string GetTypeName(string assetName)
    {
        string typeName = "FURNITURE";

        if (assetName.ContainsUpperOrLower("poster"))
        {
            typeName = "ITEM";
        }
        else if (assetName.ContainsUpperOrLower("curtain"))
        {
            typeName = "ITEM";
        }
        else if (assetName.ContainsUpperOrLower("art"))
        {
            typeName = "ITEM";
        }
        else if (assetName.ContainsUpperOrLower("photo"))
        {
            typeName = "ITEM";
        }
        return typeName;
    }

    private static string GetPosTypeName(string assetName)
    {
        string typeName = "FLOOR";

        if (assetName.ContainsUpperOrLower("poster"))
        {
            typeName = "WALL";
        }
        else if (assetName.ContainsUpperOrLower("window"))
        {
            typeName = "WALL";
        }
        else if (assetName.ContainsUpperOrLower("curtain"))
        {
            typeName = "WALL";
        }
        else if (assetName.ContainsUpperOrLower("art"))
        {
            typeName = "WALL";
        }
        else if (!assetName.ContainsUpperOrLower("PhotoStand") && assetName.ContainsUpperOrLower("photo"))
        {
            typeName = "WALL";
        }
        else if (assetName.ContainsUpperOrLower("altar"))
        {
            typeName = "WALL";
        }
        return typeName;
    }

    private static string GetIsPlaneStr(string assetName)
    {
        if (assetName.ContainsUpperOrLower("desk") || assetName.ContainsUpperOrLower("shelf") || assetName.ContainsUpperOrLower("table"))
        {
            return "TRUE";
        }
        else
        {
            return "FALSE";
        }
    }

    private static string GetLevelStr(string assetName)
    {
        string levelStr = "1";
        return levelStr;
    }

    private static string GetTagsStr(string assetName)
    {
        string tagsStr = "";
        if (assetName.ContainsUpperOrLower("book") || assetName.ContainsUpperOrLower("magazine") || assetName.ContainsUpperOrLower("lp"))
        {
            tagsStr += "Book";
        }
        return tagsStr;
    }

    private static string GetUICategoryStr(string assetName)
    {
        string category = "";

        if (assetName.ContainsUpperOrLower("desk") || assetName.ContainsUpperOrLower("table"))
        {
            category = "Desk";
        }
        else if (assetName.ContainsUpperOrLower("poster"))
        {
            category = "Poster";
        }
        else if (assetName.ContainsUpperOrLower("prop"))
        {
            category = "Prop";
        }
        else if (assetName.ContainsUpperOrLower("electronics"))
        {
            category = "Electronics";
        }
        else if (assetName.ContainsUpperOrLower("window"))
        {
            category = "Window";
        }
        else if (assetName.ContainsUpperOrLower("curtain"))
        {
            category = "Curtains";
        }
        else if (assetName.ContainsUpperOrLower("partition"))
        {
            category = "Partitions";
        }
        else if (assetName.ContainsUpperOrLower("plant") || assetName.ContainsUpperOrLower("flower"))
        {
            category = "Plant";
        }
        else if (assetName.ContainsUpperOrLower("toy") || assetName.ContainsUpperOrLower("kids"))
        {
            category = "Toy";
        }
        else if (assetName.ContainsUpperOrLower("shelf"))
        {
            category = "Shelf";
        }
        else if (assetName.ContainsUpperOrLower("carpet"))
        {
            category = "Carpet";
        }
        else if (assetName.ContainsUpperOrLower("chair") || assetName.ContainsUpperOrLower("sofa"))
        {
            category = "Chair";
        }
        else if (assetName.ContainsUpperOrLower("warehouse"))
        {
            category = "Warehouse";
        }
        else if (assetName.ContainsUpperOrLower("bed"))
        {
            category = "Bed";
        }
        else if (assetName.ContainsUpperOrLower("music"))
        {
            category = "Music";
        }
        else if (assetName.ContainsUpperOrLower("art"))
        {
            category = "Art";
        }
        else if (assetName.ContainsUpperOrLower("lamp"))
        {
            category = "Lamp";
        }
        else if (assetName.ContainsUpperOrLower("book") || assetName.ContainsUpperOrLower("magazine") || assetName.ContainsUpperOrLower("lp"))
        {
            category = "Book";
        }
        else if (assetName.ContainsUpperOrLower("acryl"))
        {
            category = "Acryl";
        }
        else if (assetName.ContainsUpperOrLower("avatar"))
        {
            category = "Avatar";
        }
        else if (assetName.ContainsUpperOrLower("photo"))
        {
            category = "Photo";
        }
        else if (assetName.ContainsUpperOrLower("altar"))
        {
            category = "WallFurniture";
        }

        return category;
    }
}

public static class StringExtensions
{
    public static bool ContainsUpperOrLower(this string name, string str)
    {
        if (name.Contains(str) || name.Contains(str.FirstLetterToUpperCase()))
        {
            return true;
        }
        return false;
    }

    public static string FirstLetterToUpperCase(this string str)
    {
        string originalString = str;
        string resultString;

        if (!string.IsNullOrEmpty(originalString))
        {
            resultString = char.ToUpper(originalString[0]) + originalString.Substring(1);
        }
        else
        {
            resultString = "";
        }

        return resultString;
    }
}


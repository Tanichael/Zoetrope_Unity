using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

/// <summary>
/// ToDo: ディレクトリ構成を検討する
/// </summary>
public class RoomSaveManager : IRoomSaveManager
{
    private readonly string m_FilePath = Application.persistentDataPath + "/" + "./savedata.json";
    private readonly string m_TemplatePath = Application.persistentDataPath + "/" + "./template0.json";

    public void Save(MockRoomManager mockRoomManager)
    {
        //現在の状態を保存したいとき
        Debug.Log("Save: " + m_FilePath);
        SaveData saveData = new SaveData();
        saveData.Name = "savedata";
        saveData.Data = new List<SaveDataUnit>();
        int[] visited = new int[mockRoomManager.RoomStates.Length];

        for(int i = 0; i < mockRoomManager.RoomStates.Length; i++)
        {
            visited[i] = 0;
        }

        for (int i = 0; i < mockRoomManager.RoomStates.Length; i++)
        {
            RoomObject roomObject = mockRoomManager.RoomStates[i];
            if (roomObject is RoomObjectFixed) continue;
            if(roomObject != null)
            {
                int roomIndex = roomObject.RoomIndex;
                if(visited[roomIndex] == 0)
                {
                    visited[roomIndex] = 1;
                    saveData.Data.Add(new SaveDataUnit(roomObject));
                }
            }
        }

        string jsonData = JsonUtility.ToJson(saveData);
        StreamWriter streamWriter = new StreamWriter(m_FilePath);
        streamWriter.Write(jsonData);
        streamWriter.Flush();
        streamWriter.Close();
    }

    public SaveData Load()
    {
        SaveData saveData = null;

        if(File.Exists(m_FilePath))
        {
            StreamReader streamReader = new StreamReader(m_FilePath);
            string data = streamReader.ReadToEnd();
            streamReader.Close();
            saveData = JsonUtility.FromJson<SaveData>(data);
        }

        return saveData;
    }

    public List<SaveData> LoadAll()
    {
        List<SaveData> saveDataList = new List<SaveData>();

        string[] files = Directory.GetFiles(Application.persistentDataPath);
        foreach(var file in files)
        {
            string extension = Path.GetExtension(file);
            if(extension != ".json")
            {
                continue;
            }

            SaveData saveData = null;
            StreamReader streamReader = new StreamReader(file);
            string data = streamReader.ReadToEnd();
            streamReader.Close();
            saveData = JsonUtility.FromJson<SaveData>(data);
            int fileNameStartIndex = Application.persistentDataPath.Length + 1; //スラッシュ分文字数を1増やしている
            int extensionIndex = file.IndexOf(extension);
            int extensionLength = extensionIndex - fileNameStartIndex;
            string name = file.Substring(fileNameStartIndex, extensionLength);
            saveData.Name = name;
            saveDataList.Add(saveData);
        }

        return saveDataList;
    }

    //本来は最初の起動時(またはテンプレート追加後)に一度だけ読み込めばいい処理
    public void InitializeTemplates()
    {
        TextAsset[] templates = Resources.LoadAll<TextAsset>("Templates");
        string[] files = Directory.GetFiles(Application.persistentDataPath);
        List<string> deleteFiles = new List<string>();
        
        foreach(var file in files)
        {
            string extension = Path.GetExtension(file);
            if (extension != ".json")
            {
                continue;
            }

            int fileNameStartIndex = Application.persistentDataPath.Length + 1; //スラッシュ分文字数を1増やしている
            int extensionIndex = file.IndexOf(extension);
            int extensionLength = extensionIndex - fileNameStartIndex;
            string name = file.Substring(fileNameStartIndex, extensionLength);

            if(name == "savedata")
            {
                continue;
            }

            bool isRemain = false;
            foreach(var template in templates)
            {
                if(template.name == name)
                {
                    isRemain = true;
                }
            }

            if(!isRemain)
            {
                //使用しないテンプレートフォルダの削除
                deleteFiles.Add(file);
            }
        }

        foreach(var deleteFile in deleteFiles)
        {
            File.Delete(deleteFile);
        }

        foreach(var template in templates)
        {
            string fileName = template.name;
            string destinationPath = Path.Combine(Application.persistentDataPath, fileName + ".json");
            /*if (File.Exists(destinationPath))
            {
                continue;
            }*/
            File.WriteAllText(destinationPath, template.text);
        }
    }

    public void Delete()
    {
        try
        {
            File.Delete(m_FilePath);
            Debug.Log("セーブデータを削除しました");
        }
        catch(Exception e)
        {
            Debug.Log("Error: " + e.Message);
        }
    }

    //RoomCommandRemoveAllから簡単に呼べるようにstaticな関数にした
    //本当はinjectしたい
    public SaveData GetRoomState(MockRoomManager mockRoomManager)
    {
        SaveData saveData = new SaveData();
        saveData.Data = new List<SaveDataUnit>();
        int[] visited = new int[mockRoomManager.RoomStates.Length];

        for (int i = 0; i < mockRoomManager.RoomStates.Length; i++)
        {
            visited[i] = 0;
        }

        for (int i = 0; i < mockRoomManager.RoomStates.Length; i++)
        {
            RoomObject roomObject = mockRoomManager.RoomStates[i];
            if (roomObject is RoomObjectFixed) continue;
            if (roomObject != null)
            {
                int roomIndex = roomObject.RoomIndex;
                if (visited[roomIndex] == 0)
                {
                    visited[roomIndex] = 1;
                    saveData.Data.Add(new SaveDataUnit(roomObject));
                }
            }
        }

        return saveData;
    }

    public void SetTemplate(MockRoomManager mockRoomManager)
    {
        //現在の状態をテンプレートとして保存したいとき
        Debug.Log("Set Template: " + m_TemplatePath);
        SaveData saveData = new SaveData();
        saveData.Name = "savedata";
        saveData.Data = new List<SaveDataUnit>();
        int[] visited = new int[mockRoomManager.RoomStates.Length];

        for (int i = 0; i < mockRoomManager.RoomStates.Length; i++)
        {
            visited[i] = 0;
        }

        for (int i = 0; i < mockRoomManager.RoomStates.Length; i++)
        {
            RoomObject roomObject = mockRoomManager.RoomStates[i];
            if (roomObject is RoomObjectFixed) continue;
            if (roomObject != null)
            {
                int roomIndex = roomObject.RoomIndex;
                if (visited[roomIndex] == 0)
                {
                    visited[roomIndex] = 1;
                    saveData.Data.Add(new SaveDataUnit(roomObject));
                }
            }
        }

        string jsonData = JsonUtility.ToJson(saveData);
        StreamWriter streamWriter = new StreamWriter(m_TemplatePath);
        streamWriter.Write(jsonData);
        streamWriter.Flush();
        streamWriter.Close();
    }
}

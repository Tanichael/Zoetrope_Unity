using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

/// <summary>
/// ToDo: �f�B���N�g���\������������
/// </summary>
public class RoomSaveManager : IRoomSaveManager
{
    private readonly string m_FilePath = Application.persistentDataPath + "/" + "./savedata.json";
    private readonly string m_TemplatePath = Application.persistentDataPath + "/" + "./template0.json";

    public void Save(MockRoomManager mockRoomManager)
    {
        //���݂̏�Ԃ�ۑ��������Ƃ�
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
            int fileNameStartIndex = Application.persistentDataPath.Length + 1; //�X���b�V������������1���₵�Ă���
            int extensionIndex = file.IndexOf(extension);
            int extensionLength = extensionIndex - fileNameStartIndex;
            string name = file.Substring(fileNameStartIndex, extensionLength);
            saveData.Name = name;
            saveDataList.Add(saveData);
        }

        return saveDataList;
    }

    //�{���͍ŏ��̋N����(�܂��̓e���v���[�g�ǉ���)�Ɉ�x�����ǂݍ��߂΂�������
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

            int fileNameStartIndex = Application.persistentDataPath.Length + 1; //�X���b�V������������1���₵�Ă���
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
                //�g�p���Ȃ��e���v���[�g�t�H���_�̍폜
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
            Debug.Log("�Z�[�u�f�[�^���폜���܂���");
        }
        catch(Exception e)
        {
            Debug.Log("Error: " + e.Message);
        }
    }

    //RoomCommandRemoveAll����ȒP�ɌĂׂ�悤��static�Ȋ֐��ɂ���
    //�{����inject������
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
        //���݂̏�Ԃ��e���v���[�g�Ƃ��ĕۑ��������Ƃ�
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

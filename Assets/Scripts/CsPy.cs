using System.Diagnostics;
using System.IO;
using System;
using UnityEngine;

public class CsPy : MonoBehaviour
{
    private string pyExePath = @"C:\Users\MEIP-users\Documents\Zoetrope_Unity\Assets\StreamingAssets\python-3.11.3-embed-amd64\python.exe";
    //private string pyExePath = System.IO.Path.Combine(Environment.SystemDirectory, "python.exe");
    private string pyCodePath = @"C:\Users\MEIP-users\Documents\Zoetrope_Unity\Assets/StreamingAssets/myproject/test_cs.py";

    // Start is called before the first frame update
    void Start()
    {
        ProcessStartInfo processStartInfo = new ProcessStartInfo()
        {
            FileName = pyExePath,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            Arguments = pyCodePath + " " + "Hello,ptyhon",
        };

        Process process = Process.Start(processStartInfo);

        StreamReader streamReader = process.StandardOutput;
        string str = streamReader.ReadLine();

        process.WaitForExit();
        process.Close();

        print(str);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

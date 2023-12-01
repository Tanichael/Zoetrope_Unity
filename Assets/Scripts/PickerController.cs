using System.Collections;
using Kakera;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PickerController : MonoBehaviour
{
    [SerializeField]
    private Unimgpicker imagePicker;

    public event Action OnPickComplete;

    /*[SerializeField]
    private MeshRenderer imageRenderer;

    [SerializeField]
    private Image image;*/

    private RawImage m_RawImage;

    void Awake()
    {
        // Unimgpicker returns the image file path.
        imagePicker.Completed += (string path) =>
        {
            /*StartCoroutine(LoadImage(path, imageRenderer));
            StartCoroutine(LoadImage(path, image));*/
            StartCoroutine(LoadImage(path, m_RawImage));
            OnPickComplete.Invoke();
        };
    }

    private void Update()
    {
       /* if(Input.GetMouseButtonDown(2))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log("this one image");
                OnPressShowPicker();
            }
        }*/
    }

    public void OnPressShowPicker()
    {
        // With v1.1 or greater, you can set the maximum size of the image
        // to save the memory usage.
        imagePicker.Show("Select Image", "unimgpicker", 1024);
        //imagePicker.Show("Select Image", "unimgpicker");
    }

    public void OnPressShowPicker(RawImage rawImage)
    {
        // With v1.1 or greater, you can set the maximum size of the image
        // to save the memory usage.
        m_RawImage = rawImage;
        imagePicker.Show("Select Image", "unimgpicker", 1024);
        //imagePicker.Show("Select Image", "unimgpicker");
    }

    private IEnumerator LoadImage(string path, RawImage image)
    {
        var url = "file://" + path;
        var www = new WWW(url);
        yield return www;

        var texture = www.texture;
        if (texture == null)
        {
            Debug.LogError("Failed to load texture url:" + url);
        }

        //スプライトの生成, imageの作成
        image.texture = www.texture;

        image.SetNativeSize();
    }

    private IEnumerator LoadImage(string path, MeshRenderer output)
    {
        var url = "file://" + path;
        var www = new WWW(url);
        yield return www;

        var texture = www.texture;
        if (texture == null)
        {
            Debug.LogError("Failed to load texture url:" + url);
        }

        output.material.mainTexture = texture;
    }

    private IEnumerator LoadImage(string path, Image image)
    {
        var url = "file://" + path;
        var www = new WWW(url);
        yield return www;

        var texture = www.texture;
        if (texture == null)
        {
            Debug.LogError("Failed to load texture url:" + url);
        }

        //スプライトの生成, imageの作成
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0f, 0f), 100f);
        image.sprite = sprite;
        
        image.SetNativeSize();
    }
}

using UnityEngine;
using UnityEngine.UI;
using Coffee.UISoftMask;
using System.Collections;

public class RoomUITrim : RoomUIBase
{
    [SerializeField] private Canvas m_TrimCanvas;
    [SerializeField] private GameObject m_TrimCover;
    [SerializeField] private RawImage m_BackgroundImage;
    [SerializeField] private RawImage m_MaskImage;
    [SerializeField] private RawImage m_TestImage;
    [SerializeField] private Image m_CoverageImage;

    Vector2 m_BefPos;
    private MeshRenderer m_Renderer;
    private float m_BefDegree;
    private bool m_IsListenerAdded;
    private Color m_BackColor;
    private Color m_CoverageColor;
    private Texture2D m_DummyTex;
    private float m_RotateOffset;
    private Vector3 m_InitAngles;

    public override RoomPhase GetRoomPhase()
    {
        return RoomPhase.Trim;
    }

    public override void Init(RoomUIMachine machine, MockRoomManager roomManager, IInputProvider inputProvider)
    {
        base.Init(machine, roomManager, inputProvider);
        m_Machine.CompleteTrimButton.onClick.AddListener(() =>
        {
            Debug.Log("Trim complete");
            m_BackgroundImage.transform.SetParent(m_MaskImage.transform);

            //‚±‚±‚Å‚Ç‚¤‚É‚©‚µ‚Ä‰æ‘œ‚ð”½‰f‚³‚¹‚½‚¢
            StartCoroutine(CompleteTrimCoroutine());
        });
        m_BackColor = new Color(255f, 255f, 255f, 255f);
        m_CoverageColor = new Color(204f / 255f, 204f / 255f, 204f / 255f, 1f);
        m_CoverageImage.color = m_CoverageColor;
        m_RotateOffset = 0f;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();
        m_Machine.ItemPanel.SetActive(true);
        m_TrimCanvas.gameObject.SetActive(true);
        m_TrimCover.SetActive(true);
        m_BackgroundImage.gameObject.SetActive(true);
        m_MaskImage.gameObject.SetActive(true);
        m_Machine.CompleteTrimButton.gameObject.SetActive(true);
        m_CoverageImage.gameObject.SetActive(true);

        if(m_RoomManager.SelectedPictureObject == null)
        {
            Debug.LogError("selected picture object is null");
        }
        m_Renderer = m_RoomManager.SelectedPictureObject.MeshRenderer;
        
        //Set MaskImage
        Texture2D texture = SetMask(300, 200);

        //‘I‘ð‚µ‚½‰æ‘œ—Ìˆæ‚ð”½‰f‚·‚é
        MeshFilter meshFilter = m_RoomManager.SelectedPictureObject.MeshFilter;
        if (meshFilter != null && meshFilter.mesh != null)
        {
            Vector3 meshSize = meshFilter.mesh.bounds.size;
            //aspectRatio‚ÌŽæ“¾
            float aspectRatio = meshSize.x / meshSize.z;
            if (m_RoomManager.SelectedObject.Data.Tags.Contains(Tag.Book) || m_RoomManager.SelectedObject.Data.Tags.Contains(Tag.AcrylStand))
            {
                aspectRatio = meshSize.x / meshSize.y;
            }
            else if (m_RoomManager.SelectedObject.Data.UICategory == UICategory.Poster || m_RoomManager.SelectedObject.Data.UICategory == UICategory.Art || m_RoomManager.SelectedObject.Data.UICategory == UICategory.Photo)
            {
                aspectRatio = meshSize.x / meshSize.z;
            }
            else
            {
                aspectRatio = meshSize.x / meshSize.y;
            }

            if(Screen.width / aspectRatio <= Screen.height * 0.8f)
            {
                texture = SetMask(Screen.width, (int)(Screen.width / aspectRatio));
            }
            else
            {
                texture = SetMask((int)(Screen.height * 0.8f * aspectRatio), (int)(Screen.height * 0.8f));
            }
        }
        // -> ‚±‚±‚ÍŒ»Ý‚ÌƒeƒNƒXƒ`ƒƒ‚ðŽæ“¾‚·‚é•ûŽ®‚É‚Å‚«‚È‚¢‚©H
        /* m_MaskImage.texture = m_Renderer.material.mainTexture;
        float textureAspectRatio = (float)m_MaskImage.texture.width / m_MaskImage.texture.height;
        if(textureAspectRatio > Screen.width / Screen.height)
        {
            float scaleFactor = Screen.width / m_Renderer.material.mainTexture.width;
            m_MaskImage.rectTransform.sizeDelta = new Vector2(m_Renderer.material.mainTexture.width * scaleFactor, m_Renderer.material.mainTexture.height * scaleFactor);
        }
        else
        {
            float scaleFactor = Screen.height / m_Renderer.material.mainTexture.height;
            m_MaskImage.rectTransform.sizeDelta = new Vector2(m_Renderer.material.mainTexture.width * scaleFactor, m_Renderer.material.mainTexture.height * scaleFactor);
        }*/

        m_MaskImage.texture = texture;
        m_MaskImage.SetNativeSize();

        RectTransform trimRectTransform = m_TrimCover.transform as RectTransform;
        if(trimRectTransform != null)
        {
            trimRectTransform.anchoredPosition = new Vector2(0f, 0f);
        }

        m_TrimCover.transform.SetParent(m_MaskImage.transform);

        RectTransform maskTransform = m_MaskImage.transform as RectTransform;
        if(maskTransform != null)
        {
            maskTransform.anchoredPosition = new Vector2(0f, Screen.height * 0.1f);
        }

        RectTransform backRectTransform = m_BackgroundImage.transform as RectTransform;
        if (backRectTransform != null)
        {
            backRectTransform.anchoredPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
        }
    }

    private Texture2D SetMask(int w, int h)
    {
        Texture2D texture = new Texture2D(w, h, TextureFormat.ARGB32, false);
        //Color[] pixels = texture.GetPixels();
        //Color[] buffer = new Color[pixels.Length];
        Color[] buffer = new Color[w * h];

        for (int i = 0; i < w * h; i++)
        {
            buffer.SetValue(m_BackColor, i);
        }

        texture.SetPixels(buffer);
        texture.Apply();

        return texture;
    }

    IEnumerator CompleteTrimCoroutine()
    {
        // ‚±‚±‚Å‚Ç‚¤‚É‚©‚µ‚Ä‰æ‘œ‚ð”½‰f‚³‚¹‚½‚¢
        SoftMask softMask = m_MaskImage.gameObject.GetComponent<SoftMask>();
        Texture2D trimmedTexture = null;
        if (softMask != null)
        {
            yield return new WaitForEndOfFrame();

            //texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            RectTransform maskRectTransform = m_MaskImage.transform as RectTransform;
            if(maskRectTransform != null)
            {
                float maskX = maskRectTransform.anchoredPosition.x;
                float maskY = maskRectTransform.anchoredPosition.y;
                float maskWidth = maskRectTransform.sizeDelta.x;
                float maskHeight = maskRectTransform.sizeDelta.y;
                Texture2D texture = new Texture2D((int)maskWidth, (int)maskHeight, TextureFormat.ARGB32, false);
                Texture2D dummyTex = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
                texture.ReadPixels(new Rect(maskX + Screen.width *0.5f - maskWidth / 2, maskY + Screen.height * 0.5f - maskHeight / 2, maskWidth, maskHeight), 0, 0);
                dummyTex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                texture.Apply();

                yield return new WaitForEndOfFrame();

                Color[] pixels = texture.GetPixels();
                Color[] dummyPixels = dummyTex.GetPixels();
                //Color[] dummyPixels = m_DummyTex.GetPixels();
                Color[] buffer = new Color[pixels.Length];

                for(int i = 0; i < pixels.Length; i++)
                {
                    if(pixels[i] == dummyPixels[0])
                    {
                        buffer.SetValue(new Color(0f, 0f, 0f, 0f), i);
                    }
                    else if(Vector4.Distance(pixels[i], m_CoverageColor) < 0.01f)
                    {
                        buffer.SetValue(new Color(0f, 0f, 0f, 0f), i);
                    }
                    else
                    {
                        buffer[i] = pixels[i];
                    }
                }

                texture.SetPixels(buffer);
                texture.Apply();
                m_TestImage.texture = texture;

                m_TestImage.SetNativeSize();

                Texture2D newTex = texture;
               
                m_RoomManager.SelectedObject.IsPictureSet = true;
                trimmedTexture = newTex;
            }
        }

        PublishUIEvent(new CompleteTrimButtonClickEvent(trimmedTexture, m_RoomManager.SelectedPictureObject));
    }

    #region LEGACY
    /*    private Texture2D RotateTexture90Degrees(Texture2D originalTexture)
        {
            Texture2D newTexture = new Texture2D(originalTexture.height, originalTexture.width);
            for (int x = 0; x < originalTexture.width; x++)
            {
                for (int y = 0; y < originalTexture.height; y++)
                {
                    newTexture.SetPixel(y, originalTexture.width - 1 - x, originalTexture.GetPixel(x, y));
                }
            }
            newTexture.Apply();
            return newTexture;
        }

        private Texture2D RotateTexture180Degrees(Texture2D originalTexture)
        {
            Texture2D newTexture = new Texture2D(originalTexture.width, originalTexture.height);
            for (int x = 0; x < originalTexture.width; x++)
            {
                for (int y = 0; y < originalTexture.height; y++)
                {
                    newTexture.SetPixel(originalTexture.width - 1 - x, originalTexture.height - 1 - y, originalTexture.GetPixel(x, y));
                }
            }
            newTexture.Apply();
            return newTexture;
        }

        private Texture2D RotateTexture270Degrees(Texture2D originalTexture)
        {
            Texture2D newTexture = new Texture2D(originalTexture.height, originalTexture.width);
            for (int x = 0; x < originalTexture.width; x++)
            {
                for (int y = 0; y < originalTexture.height; y++)
                {
                    newTexture.SetPixel(originalTexture.height - 1 - y, x, originalTexture.GetPixel(x, y));
                }
            }
            newTexture.Apply();
            return newTexture;
        }

        private Texture2D ReverseTextureX(Texture2D originalTexture)
        {
            Texture2D newTexture = new Texture2D(originalTexture.width, originalTexture.height);
            for (int x = 0; x < originalTexture.width; x++)
            {
                for (int y = 0; y < originalTexture.height; y++)
                {
                    newTexture.SetPixel(originalTexture.width - 1 - x, y, originalTexture.GetPixel(x, y));
                }
            }
            newTexture.Apply();
            return newTexture;
        }

        private Texture2D ReverseTextureY(Texture2D originalTexture)
        {
            Texture2D newTexture = new Texture2D(originalTexture.width, originalTexture.height);
            for (int x = 0; x < originalTexture.width; x++)
            {
                for (int y = 0; y < originalTexture.height; y++)
                {
                    newTexture.SetPixel(x, originalTexture.height - 1 - y, originalTexture.GetPixel(x, y));
                }
            }
            newTexture.Apply();
            return newTexture;
        }
    */
    #endregion

    public override void OnExitState()
    {
        m_TrimCanvas.gameObject.SetActive(false);
        m_TrimCover.transform.SetParent(m_TrimCanvas.transform);
        m_BackgroundImage.transform.SetParent(m_TrimCanvas.transform);
        m_BackgroundImage.transform.SetSiblingIndex(1);
        RectTransform rectTransform = m_BackgroundImage.transform as RectTransform;
        if (rectTransform != null)
        {
            rectTransform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
        }
        base.OnExitState();
    }

    private void Update()
    {
        //BackgroundImage‚ÌŠg‘å
        if (m_InputProvider.GetIsZooming())
        {
            float deltaDistance = m_InputProvider.GetZoomDeltaDistance();
            RectTransform rectTransform = m_BackgroundImage.transform as RectTransform;
            if (rectTransform != null)
            {
                deltaDistance *= -1f * 0.3f;
                rectTransform.localScale += new Vector3(deltaDistance, deltaDistance, 0f);
                rectTransform.localScale = new Vector3(Mathf.Clamp(rectTransform.localScale.x, 0.3f, 4f), Mathf.Clamp(rectTransform.localScale.y, 0.3f, 4f), 1f);
            }

        }

#if UNITY_EDITOR
        //BackgroundImage‚ÌˆÚ“®
        if (Input.GetMouseButtonDown(1))
        {
            m_BefPos = Input.mousePosition;
        }

        if (Input.GetMouseButton(1))
        {
            RectTransform backRectTransform = m_BackgroundImage.transform as RectTransform;
            if (backRectTransform != null)
            {
                backRectTransform.anchoredPosition += new Vector2(Input.mousePosition.x, Input.mousePosition.y) - m_BefPos;
                backRectTransform.anchoredPosition = new Vector2(Mathf.Clamp(backRectTransform.anchoredPosition.x, 0f, Screen.width), Mathf.Clamp(backRectTransform.anchoredPosition.y, 0f, Screen.height));
                m_BefPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            }
        }

#else
        if (Input.touchCount == 2) //‰ñ“]
        {
            foreach(var touch in Input.touches)
            {
                if(touch.phase == TouchPhase.Ended)
                {
                    /*if(m_RotateOffset <= 5f) //‰ñ“]‚ª¬‚³‚¯‚ê‚Î–³Œø‚É‚·‚é
                    {
                        RectTransform rectTransform = m_BackgroundImage.transform as RectTransform;
                        if (rectTransform != null)
                        {
                            rectTransform.localRotation = Quaternion.Euler(m_InitAngles);
                        }
                    }*/

                    //uŠÔˆÚ“®‚ð–h‚®‚½‚ß‚ÉAˆÚ“®‚ÌŠî€‚Æ‚È‚éˆÊ’u‚ð•ÏX‚·‚é
                    m_BefPos = Input.touches[(1 ^ touch.fingerId)].position;
                }
                if(touch.phase == TouchPhase.Began)
                {
                    Vector3 vec = Input.touches[1].position - Input.touches[0].position;
                    m_BefDegree = Mathf.Atan2(vec.y, vec.x) * Mathf.Rad2Deg;
                    m_RotateOffset = 0f;

                    RectTransform rectTransform = m_BackgroundImage.transform as RectTransform;
                    if (rectTransform != null)
                    {
                        m_InitAngles = rectTransform.localRotation.eulerAngles;
                    }
                }
            }
            
            Vector3 zeroPos = Input.GetTouch(0).position;
            Vector3 onePos = Input.GetTouch(1).position;
            Vector3 angleVec = onePos - zeroPos;
            float degree = Mathf.Atan2(angleVec.y, angleVec.x) * Mathf.Rad2Deg;

            float offset = degree - m_BefDegree;
            m_RotateOffset += offset;

            //ˆê’è‚Ìè‡’l‚ð’´‚¦‚Ä‚¢‚½‚ç‰ñ“]
            if (Mathf.Abs(m_RotateOffset) > 15f)
            {
                RectTransform rectTransform = m_BackgroundImage.transform as RectTransform;
                if (rectTransform != null)
                {
                    Vector3 eulerAngles = rectTransform.localRotation.eulerAngles;
                    rectTransform.localRotation = Quaternion.Euler(eulerAngles + new Vector3(0f, 0f, offset));
                }
            }

            m_BefDegree = degree;
        }
        else
        {
            //BackgroundImage‚ÌˆÚ“®
            if (Input.touchCount == 1)
            {
                if(Input.touches[0].phase == TouchPhase.Began)
                {
                    m_BefPos = Input.touches[0].position;
                }

                RectTransform backRectTransform = m_BackgroundImage.transform as RectTransform;
                if (backRectTransform != null)
                {
                    Vector2 pos = Input.touches[0].position;
                    backRectTransform.anchoredPosition += new Vector2(pos.x, pos.y) - m_BefPos;
                    backRectTransform.anchoredPosition = new Vector2(Mathf.Clamp(backRectTransform.anchoredPosition.x, 0f, Screen.width), Mathf.Clamp(backRectTransform.anchoredPosition.y, 0f, Screen.height));
                    m_BefPos = Input.touches[0].position;
                }
            }
        }
#endif
    }

    protected override void PublishUIEvent(RoomUIEvent roomUIEvent)
    {
        base.PublishUIEvent(roomUIEvent);
    }
}

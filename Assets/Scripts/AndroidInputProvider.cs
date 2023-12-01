using UnityEngine;

public class AndroidInputProvider : MonoBehaviour, IInputProvider
{
    public Vector2 GetBefDragPosition()
    {
        if (Input.touchCount == 1)
        {
            return GetDragPosition() - GetDragDeltaPosition();
        }
        return new Vector2(0f, 0f);
    }

    public Vector2 GetDragDeltaPosition()
    {
        if (Input.touchCount == 1 || Input.touchCount == 2)
        {
            //return new Vector2(Input.touches[0].deltaPosition.x / Screen.width, Input.touches[0].deltaPosition.y / Screen.height);
            return new Vector2(Input.touches[0].deltaPosition.x, Input.touches[0].deltaPosition.y) / 2.5f;

        }
        return new Vector2(0f, 0f);
    }

    public Vector2 GetDragDeltaWithID(int id)
    {
        foreach(var touch in Input.touches)
        {
            if(touch.fingerId == id)
            {
                return new Vector2(touch.deltaPosition.x, touch.deltaPosition.y) / 2.5f;
            }
        }
            //return new Vector2(Input.touches[0].deltaPosition.x / Screen.width, Input.touches[0].deltaPosition.y / Screen.height);
        return new Vector2(0f, 0f);
    }

    public Vector2 GetDragPosition()
    {
        if (Input.touchCount == 1 || Input.touchCount == 2)
        {
            // return new Vector2(Input.touches[0].position.x / Screen.width, Input.touches[0].position.y / Screen.height); ;
            return new Vector2(Input.touches[0].position.x, Input.touches[0].position.y);
        }
        return new Vector2(0f, 0f);
    }

    public Vector2 GetDragSpeed()
    {
        return GetDragDeltaPosition() / Time.deltaTime;
    }

    public bool GetIsDragEnd()
    {
        if(Input.touchCount == 1)
        {
            if(Input.touches[0].phase == TouchPhase.Ended)
            {
                return true;
            }
        }
        return false;
    }

    public bool GetIsDragging()
    {
        if (Input.touchCount == 1)
        {
            if (Input.touches[0].phase == TouchPhase.Moved)
            {
                return true;
            }
        }
        return false;
    }

    public bool GetIsMultiDragEnd()
    {
        if(Input.touchCount == 2)
        {
            if(Input.touches[0].phase == TouchPhase.Ended || Input.touches[1].phase == TouchPhase.Ended)
            {
                return true;
            }
        }
        return false;
    }

    public bool GetIsMultiDragging()
    {
        if (Input.touchCount == 2)
        {
            if(Input.touches[0].phase != TouchPhase.Ended && Input.touches[1].phase != TouchPhase.Ended)
            {
                return true;
            }
        }
        return false;
    }

    public bool GetIsZooming()
    {
        if (Input.touchCount == 2)
        {
            return true;
        }
        return false;
    }

    public Vector2 GetMultiDragDeltaPosition()
    {
        return new Vector2((Input.touches[0].deltaPosition.x + Input.touches[1].deltaPosition.x) / 5f, (Input.touches[0].deltaPosition.y + Input.touches[1].deltaPosition.y) / 5f); ;
    }

    public Vector2 GetMultiDragPosition()
    {
        return new Vector2((Input.touches[0].position.x + Input.touches[1].position.x) / 2f, (Input.touches[0].position.y + Input.touches[1].position.y) / 2f);
    }

    public float GetZoomDeltaDistance()
    {
        if(Input.touchCount == 2)
        {
            Touch tZero = Input.GetTouch(0);
            Touch tOne = Input.GetTouch(1);

            Vector2 tZeroPrevious = tZero.position - tZero.deltaPosition;
            Vector2 tOnePrevious = tOne.position - tOne.deltaPosition;

            /*tZeroPrevious.x /= Screen.width;
            tZeroPrevious.y /= Screen.height;
            tOnePrevious.x /= Screen.width;
            tOnePrevious.y /= Screen.height;*/

            float oldTouchDistance = Vector2.Distance(tZeroPrevious, tOnePrevious);
            float currentTouchDistance = Vector2.Distance(tZero.position, tOne.position);

            return (oldTouchDistance - currentTouchDistance) * 0.01f;
        }
        return 0f;
    }

    public float GetZoomSpeed()
    {
        return 0.12f;
    }
}

using UnityEngine;

//detailにも対応できるように拡張したい
//SetFloorなどしてFloorObjectをセットできるようにする
public class MockRoomSelector : ITapSelector
{
    private readonly float ms_HoldTime = 0.2f;
    private readonly float ms_DoubleTapCoolTime = 0.2f;

    private ITappable m_FloorObject;
    private ITappable m_SuspectObject;
    private ITappable m_SelectedObject;
    private float m_SelectingTime;
    private float m_DoubleTapTime;
    private bool m_IsWhileSelect;
    private Vector3 m_SelectStartPos;

    public MockRoomSelector()
    {
        m_FloorObject = null;
        m_SuspectObject = null;
        m_SelectedObject = null;
        m_SelectingTime = 0f;
        m_IsWhileSelect = false;
        m_SelectStartPos = new Vector3(0f, 0f, 0f);
        m_DoubleTapTime = ms_DoubleTapCoolTime;
    }

    public TapSelectValues TapSelect(Ray ray, int layerMask)
    {
        m_DoubleTapTime += Time.deltaTime;

        TapSelectState selectState = TapSelectState.None;
        ITappable tappable = null;

        if(!m_IsWhileSelect && Input.GetMouseButtonDown(0))
        {
            #region START_SELECT
            //EmptySpaceに当たらないようにする
            selectState = TapSelectState.TapStart;

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                ITappable selectable = hit.transform.gameObject.GetComponent<ITappable>();

                if (selectable != null)
                {
                    m_IsWhileSelect = true;
                    m_SelectingTime = 0f;
                    m_SelectStartPos = Input.mousePosition;
                    m_SuspectObject = selectable;
                    selectState = TapSelectState.Suspect;
                    tappable = m_SuspectObject;
                    return new TapSelectValues(TapSelectState.Suspect, m_SuspectObject);
                }
            }
            #endregion
        }

        #region WHILE_SELECT
        if (m_IsWhileSelect)
        {
            selectState = TapSelectState.Suspect;
            tappable = m_SuspectObject;

            //指を動かしすぎたら選択終了
            if (Vector2.Distance(m_SelectStartPos, Input.mousePosition) > 30f)
            {
                m_IsWhileSelect = false;
                selectState = TapSelectState.None;
                tappable = null;
            }
            else
            {
                m_SelectingTime += Time.deltaTime;
                if (m_SelectingTime > ms_HoldTime)
                {
                    //選択完了 + 操作開始
                    m_IsWhileSelect = false;
                    /*if(m_SelectedObject != null && m_SelectedObject != m_SuspectObject)
                    {
                        m_SelectedObject.OnDeselect();
                    }*/
                    m_SelectedObject = m_SuspectObject;
                    m_SelectedObject.OnHold();
                    selectState = TapSelectState.Tap;
                    tappable = m_SelectedObject;
                }

                if (Input.GetMouseButtonUp(0))
                {
                    m_IsWhileSelect = false;

                    m_SelectedObject = m_SuspectObject;

                    if(m_DoubleTapTime < ms_DoubleTapCoolTime)
                    {
                        m_SelectedObject.OnDoubleTap();
                    }
                    else
                    {
                        m_SelectedObject.OnTap();
                        m_DoubleTapTime = 0f;
                    }

                    //選択完了
                    /*if(m_SelectedObject == null || m_SuspectObject != m_SelectedObject)
                    {
                        if(m_SelectedObject != null)
                        {
                            m_SelectedObject.OnDeselect();
                        }
                        m_SelectedObject = m_SuspectObject;
                        m_SelectedObject.OnTap();
                    }
                    else if(m_SuspectObject == m_SelectedObject)
                    {
                        m_SelectedObject.OnDeselect();
                        m_SelectedObject = m_FloorObject;
                    }*/

                    m_SuspectObject = null;
                    selectState = TapSelectState.Tap;
                    tappable = m_SelectedObject;
                }
            }
        }
        #endregion

        return new TapSelectValues(selectState, tappable);
    }
}

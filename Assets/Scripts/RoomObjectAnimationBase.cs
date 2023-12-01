using Cysharp.Threading.Tasks;
using System.Threading;
using DG.Tweening;
using UnityEngine;

public abstract class RoomObjectAnimationBase
{
    protected Camera m_Camera;
    protected RoomObjectAnimationController m_AnimationController;
    protected CancellationTokenSource m_Cts;
    protected Transform m_Transform;
    protected RoomObject m_RoomObject;
    protected Vector3 m_OriginPos;
    protected Quaternion m_OriginRotation;
    protected Transform m_BefParent;
    protected bool m_IsLoop;
    protected bool m_IsPlaying;

    public bool IsLoop => m_IsLoop;

    public abstract RoomObjectPhase GetPhase();

    public virtual async UniTask PlayAsync()
    {
        await UniTask.Delay(1);
        m_IsPlaying = false;
    }

    protected async UniTask SetCursorAsync()
    {
        await UniTask.WaitUntil(() => !m_RoomObject.CursorManager.IsLock, cancellationToken: m_Cts.Token);
        m_RoomObject.CursorManager.gameObject.SetActive(true);
        m_RoomObject.CursorManager.IsLock = true;
    }

    public virtual bool GetIsPlaying()
    {
        return m_IsPlaying;
    }

    public virtual void ForceStop()
    {
        m_Cts.Cancel();
    }

    public RoomObjectAnimationBase(RoomObject roomObject, RoomObjectAnimationController animationController)
    {
        m_Cts = new CancellationTokenSource();
        m_RoomObject = roomObject;
        m_Transform = roomObject.transform;

        m_OriginPos = roomObject.transform.position;
        m_OriginRotation = roomObject.transform.rotation;

        m_AnimationController = animationController;
    }

    public virtual void OnEnterState()
    {
        //Debug.Log("Animation: " + GetPhase().ToString());
        m_Camera = Camera.main;
        m_RoomObject.transform.localScale = m_RoomObject.OriginScale;
        m_IsLoop = false;
        m_IsPlaying = true;
        PlayAsync().Forget();
    }

    public virtual void OnUpdate()
    {

    }

    public virtual void OnExitState()
    {
        //Debug.Log("cancel");
        if(m_Transform != null)
        {
            m_Transform.localScale = m_RoomObject.OriginScale;
        }
        m_Cts.Cancel();
    }

}

public class RoomObjectAnimationNone : RoomObjectAnimationBase
{
    public override RoomObjectPhase GetPhase()
    {
        return RoomObjectPhase.None;
    }

    public override async UniTask PlayAsync()
    {
        m_RoomObject.transform.localScale = m_RoomObject.OriginScale;
        await UniTask.DelayFrame(1, cancellationToken: m_Cts.Token);
        m_IsPlaying = false;
    }

    public RoomObjectAnimationNone(RoomObject roomObject, RoomObjectAnimationController animationController) : base(roomObject, animationController)
    {
        
    }
}

public class RoomObjectAnimationPut : RoomObjectAnimationBase
{
    public override RoomObjectPhase GetPhase()
    {
        return RoomObjectPhase.Put;
    }
    
    public override async UniTask PlayAsync()
    {
        float targetScaleY = m_RoomObject.OriginScale.y;
        await m_Transform.DOScaleY(targetScaleY * 1.2f, 0.08f).SetLink(m_Transform.gameObject).ToUniTask(cancellationToken: m_Cts.Token);
        await m_Transform.DOScaleY(targetScaleY * 0.9f, 0.08f).SetLink(m_Transform.gameObject).ToUniTask(cancellationToken: m_Cts.Token);
        await m_Transform.DOScaleY(targetScaleY * 1.05f, 0.08f).SetLink(m_Transform.gameObject).ToUniTask(cancellationToken: m_Cts.Token);
        await m_Transform.DOScaleY(targetScaleY * 0.98f, 0.08f).SetLink(m_Transform.gameObject).ToUniTask(cancellationToken: m_Cts.Token);
        await m_Transform.DOScaleY(targetScaleY * 1f, 0.08f).SetLink(m_Transform.gameObject).ToUniTask(cancellationToken: m_Cts.Token);
        m_IsPlaying = false;
    }

    public RoomObjectAnimationPut(RoomObject roomObject, RoomObjectAnimationController animationController) : base(roomObject, animationController)
    {

    }
}

public class RoomObjectAnimationSelected : RoomObjectAnimationBase
{
    private RectTransform m_CursorRectTransform;
    private RoomObject m_BefObject;

    public override RoomObjectPhase GetPhase()
    {
        return RoomObjectPhase.Selected;
    }

    public RoomObjectAnimationSelected(RoomObject roomObject, RoomObjectAnimationController animationController) : base(roomObject, animationController)
    {

    }

    public override async UniTask PlayAsync()
    {
        /*if(m_CursorRectTransform != null)
        {
            await m_CursorRectTransform.DOScale(new Vector3(0.06f, 0.06f, 0f), 1f).SetLoops(-1, LoopType.Yoyo).SetLink(m_Transform.gameObject).ToUniTask(cancellationToken: m_Cts.Token);
        }*/
        //await m_RoomObject.transform.DOScale(m_RoomObject.OriginScale * 1.1f, 0.7f).SetLoops(-1, LoopType.Yoyo).SetLink(m_Transform.gameObject).ToUniTask(cancellationToken: m_Cts.Token);
    }

    public override void OnEnterState()
    {
        m_CursorRectTransform = m_RoomObject.CursorManager.transform as RectTransform;
        m_RoomObject.CursorManager.SetCursorType(true);
        m_BefObject = m_RoomObject;
        base.OnEnterState();
        m_IsLoop = true;
        SetCursorAsync().Forget();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if(m_RoomObject != m_BefObject)
        {
            m_CursorRectTransform = m_RoomObject.CursorManager.transform as RectTransform;
            m_BefObject = m_RoomObject;
        }

        if(m_CursorRectTransform != null)
        {
            Vector3 worldCursorPos = new Vector3(0f, 0f, 0f);
            if (m_RoomObject.Data.PosType == PositionType.FLOOR)
            {
                worldCursorPos = m_RoomObject.transform.position + Vector3.up * (0.4f + m_RoomObject.GetFamilySize(m_RoomObject.PutType).y * m_RoomObject.RoomUnit);
            }
            else if(m_RoomObject.Data.PosType == PositionType.WALL)
            {
                worldCursorPos = m_RoomObject.transform.position + Vector3.up * (0.4f + m_RoomObject.GetFamilySize(m_RoomObject.PutType).y / 2f * m_RoomObject.RoomUnit);
            }
            
            Vector3 screenCursorPos = m_Camera.WorldToScreenPoint(worldCursorPos);
            m_CursorRectTransform.anchoredPosition = new Vector2(screenCursorPos.x, screenCursorPos.y);
            /*float fov = Camera.main
             * .fieldOfView;
            float originScale = 0.06f;
            float nextScale = originScale * Mathf.Tan(80f / 2f * Mathf.Deg2Rad) / Mathf.Tan(fov / 2f * Mathf.Deg2Rad);
            m_CursorRectTransform.localScale = new Vector2(nextScale, nextScale);*/
        }
    }

    public override void OnExitState()
    {
        base.OnExitState();
        if(m_CursorRectTransform != null)
        {
            //m_CursorRectTransform.localScale = new Vector3(0.06f, 0.06f, 0f);
        }
        if(m_RoomObject.CursorManager != null)
        {
            m_RoomObject.CursorManager.gameObject.SetActive(false);
            m_RoomObject.CursorManager.IsLock = false;
        }
    }
}

public class RoomObjectAnimationControl : RoomObjectAnimationBase
{
    private RectTransform m_CursorRectTransform;

    public override RoomObjectPhase GetPhase()
    {
        return RoomObjectPhase.Control;
    }

    public RoomObjectAnimationControl(RoomObject roomObject, RoomObjectAnimationController animationController) : base(roomObject, animationController)
    {

    }

    public override async UniTask PlayAsync()
    {
        await m_Transform.DOMoveY(m_Transform.position.y + m_RoomObject.ControlPosition.FloatDistance, 0.03f).SetLink(m_Transform.gameObject).ToUniTask(cancellationToken: m_Cts.Token);
        m_IsPlaying = false;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        float heightPos = m_RoomObject.ControlPosition.HeightPos;
        float depthPos = m_RoomObject.ControlPosition.DepthPos;
        float widthPos = m_RoomObject.ControlPosition.WidthPos;

        int objHeight = m_RoomObject.Data.GetObjHeight(m_RoomObject.PutType);
        int objDepth = m_RoomObject.Data.GetObjDepth(m_RoomObject.PutType);
        int objWidth = m_RoomObject.Data.GetObjWidth(m_RoomObject.PutType);

        float roomUnit = m_RoomObject.RoomUnit;

        if (m_RoomObject.Data.PosType == PositionType.FLOOR)
        {
            m_RoomObject.transform.position = roomUnit * new Vector3((float)objWidth / 2f, 0f, -(float)objDepth / 2f) + new Vector3(widthPos, heightPos, depthPos);
            m_RoomObject.transform.position -= Vector3.Scale(m_RoomObject.Data.ColliderCenter.RotateAngleXZPlane(m_RoomObject.transform.localRotation.eulerAngles.y), new Vector3(1f, 0f, 1f));
        }
        else if (m_RoomObject.Data.PosType == PositionType.WALL)
        {
            if (m_RoomObject.PutType == PutType.NORMAL)
            {
                m_RoomObject.transform.position = roomUnit * new Vector3(0f, (float)objHeight / 2f, -(float)objDepth / 2f) + new Vector3(widthPos, heightPos, depthPos);
                m_RoomObject.transform.position -= Vector3.Dot(m_RoomObject.Data.ColliderCenter, Vector3.forward) * Vector3.forward + Vector3.Dot(m_RoomObject.Data.ColliderCenter, Vector3.up) * Vector3.up;
            }
            else if (m_RoomObject.PutType == PutType.REVERSE)
            {
                m_RoomObject.transform.position = roomUnit * new Vector3((float)objWidth / 2f, (float)objHeight / 2f, 0f) + new Vector3(widthPos, heightPos, depthPos);
                m_RoomObject.transform.position -= Vector3.Dot(m_RoomObject.Data.ColliderCenter, Vector3.up) * Vector3.up + Vector3.Dot(m_RoomObject.Data.ColliderCenter, Vector3.forward) * Vector3.right;
            }
        }

        if(m_CursorRectTransform != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(m_RoomObject.transform.position + Vector3.up * (0.4f + m_RoomObject.GetFamilySize(m_RoomObject.PutType).y * m_RoomObject.RoomUnit));
            m_CursorRectTransform.anchoredPosition = new Vector2(screenPos.x, screenPos.y);
           /* float fov = Camera.main.fieldOfView;
            float originScale = 0.06f;
            float nextScale = originScale * Mathf.Tan(80f / 2f * Mathf.Deg2Rad) / Mathf.Tan(fov / 2f * Mathf.Deg2Rad);
            m_CursorRectTransform.localScale = new Vector2(nextScale, nextScale);*/
            m_RoomObject.CursorManager.SetCursorType(m_RoomObject.ControlPosition.CanPut);
        }
    }

    public override void OnEnterState()
    {
        base.OnEnterState();
        m_CursorRectTransform = m_RoomObject.CursorManager.transform as RectTransform;
        SetCursorAsync().Forget();
    }

    public override void OnExitState()
    {
        base.OnExitState();
        m_RoomObject.SetRoomObjectPosition(m_RoomObject.RoomIndex, m_RoomObject.Data, m_RoomObject.PutType);
        if (m_CursorRectTransform != null)
        {
            m_CursorRectTransform.localScale = new Vector3(0.06f, 0.06f, 0f);
        }
        m_RoomObject.CursorManager.gameObject.SetActive(false);
        m_RoomObject.CursorManager.IsLock = false;
    }
}

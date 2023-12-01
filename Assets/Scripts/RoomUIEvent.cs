public abstract class RoomUIEvent
{
    public abstract IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander);
}

public class UndoButtonClickEvent : RoomUIEvent
{
    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return null;
    }
}

public class DeleteButtonClickEvent : RoomUIEvent
{
    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return new RoomCommandDelete(roomManager, this, machine, roomCommander);
    }
}
public class RotateButtonClickEvent : RoomUIEvent
{
    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return new RoomCommandRotate(roomManager, this, machine, roomCommander);
    }
}

public class RoomObjectDoubleTapEvent : RoomUIEvent
{
    public RoomObject TargetObject;

    public RoomObjectDoubleTapEvent(RoomObject targetObject)
    {
        TargetObject = targetObject;
    }

    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return null;
    }
}

public class DetailButtonClickEvent : RoomUIEvent
{
    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return null;

    }
}

public class BackButtonClickEvent : RoomUIEvent
{
    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return null;

    }
}

public class RemoveAllButtonClickEvent : RoomUIEvent
{
    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return new RoomCommandRemoveAll(roomManager, this, machine, roomCommander);
    }
}

public class PutItemEvent : RoomUIEvent
{
    public RoomObjectData Data { get; }

    public PutItemEvent(RoomObjectData data)
    {
        Data = data;
    }

    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return new RoomCommandPut(roomManager, this, machine, roomCommander);
    }
}

public class DragPutItemEvent : RoomUIEvent
{
    public RoomObjectData Data { get; }

    public DragPutItemEvent(RoomObjectData data)
    {
        Data = data;
    }

    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return new RoomCommandDragPut(roomManager, this, machine, roomCommander);
    }
}

public class CompleteTrimButtonClickEvent : RoomUIEvent
{
    public UnityEngine.Texture2D TrimmedTexture { get; }
    /* public UnityEngine.MeshRenderer MeshRenderer { get; }
    public UnityEngine.Material Material { get; }*/

    public SetMaterialEvent SetMaterialEvent { get; }

    public CompleteTrimButtonClickEvent(UnityEngine.Texture2D trimmedTexture, SetMaterialEvent setMaterialEvent)
    {
        TrimmedTexture = trimmedTexture;
        /*MeshRenderer = meshRenderer;
        Material = material;*/
        SetMaterialEvent = setMaterialEvent;
    }

    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return null;

    }
}

public class EditButtonClickEvent : RoomUIEvent
{
    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return null;

    }
}

public class PickCompleteEvent : RoomUIEvent
{
    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return null;

    }
}

public class EditCompleteButtonClickEvent : RoomUIEvent
{
    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return null;

    }
}

public class RoomEditButtonClickEvent : RoomUIEvent
{
    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return null;

    }
}

public class VirtualCamButtonClickEvent : RoomUIEvent
{
    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return null;

    }
}

public class VirtualCamReturnViewButtonClickEvent : RoomUIEvent
{
    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return null;

    }
}
public class VirtualCamAvatarAnimButtonClickEvent : RoomUIEvent
{
    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return null;

    }
}

public class VirtualCamAvatarHeadIKButtonClickEvent : RoomUIEvent
{
    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return null;

    }
}

public class VirtualCamARToggleButtonClickEvent : RoomUIEvent
{
    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return null;

    }
}

public class VirtualCamStartAvatarEditButtonClickEvent : RoomUIEvent
{
    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return null;

    }
}

public class VirtualCamEndAvatarEditButtonClickEvent : RoomUIEvent
{
    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return null;

    }
}

public class VirtualCamPlacementModeButtonClickEvent : RoomUIEvent
{
    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return null;

    }
}

public class VirtualCamFocusModeButtonClickEvent : RoomUIEvent
{
    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return null;

    }
}


public class EndEditButtonClickEvent : RoomUIEvent
{
    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return null;

    }
}

public class UnselectButtonClickEvent : RoomUIEvent
{
    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return null;
    }
}

public class RemoveAllYesButtonClickEvent : RoomUIEvent
{
    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return new RoomCommandRemoveAllYes(roomManager, this, machine, roomCommander);
    }
}

public class RemoveAllNoButtonClickEvent : RoomUIEvent
{
    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return new RoomCommandRemoveAllNo(roomManager, this, machine, roomCommander);
    }
}

public class DeltaRotateStartEvent : RoomUIEvent
{
    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return null;
    }
}

public class DetaRoteteValueChangeEvent : RoomUIEvent
{
    public float DeltaAngle { get; set; }

    public DetaRoteteValueChangeEvent(float deltaAngle)
    {
        DeltaAngle = deltaAngle;
    }

    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        //?R?}???h?????`?????????????]??Undo????????????????
        return null;
    }
}

public class CompleteDeltaRotateButtoClickEvent : RoomUIEvent
{
    public float DeltaAngle { get; set; }

    public CompleteDeltaRotateButtoClickEvent(float deltaAngle)
    {
        DeltaAngle = deltaAngle;
    }

    public override IRoomCommand GetCommand(RoomPhaseBase roomPhase, MockRoomManager roomManager, RoomPhaseMachine machine, IRoomCommander roomCommander)
    {
        return null;
    }
}
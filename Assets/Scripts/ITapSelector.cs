using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITapSelector
{
    public TapSelectValues TapSelect(Ray ray, int layerMask);
}

public class TapSelectValues
{
    public TapSelectState SelectState { get; }
    public ITappable TappableObject { get; }

    public TapSelectValues(TapSelectState selectState, ITappable tappableObject)
    {
        SelectState = selectState;
        TappableObject = tappableObject;
    }
}

public enum TapSelectState
{
    None = 0,
    TapStart = 1,
    Suspect = 2,
    Tap = 3,
    Hold = 4,
}

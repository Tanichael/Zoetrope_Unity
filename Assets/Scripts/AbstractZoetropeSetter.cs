using UnityEngine;

public abstract class AbstractZoetropeSetter : IZoetropeSetter
{
    public Zoetrope Set()
    {
        Zoetrope zoetrope = new Zoetrope();

        CreateZoetropeObject(zoetrope);
        CreateMasks(zoetrope);
        CreateAvatars(zoetrope);

        return zoetrope;
    }

    protected abstract void CreateZoetropeObject(Zoetrope zoetrope);
    protected abstract void CreateMasks(Zoetrope zoetrope);
    protected abstract void CreateAvatars(Zoetrope zoetrope);
}
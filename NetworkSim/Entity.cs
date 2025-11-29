namespace NetworkSim;

public abstract class Entity : ICloneable
{
    public World? CurrentWorld { get; set; }

    public virtual void Initialize()
    {

    }

    public abstract void Update(float delta);

    public virtual object Clone()
    {
        return this.MemberwiseClone();
    }

    public Entity CloneEntity()
    {
        return (Entity)Clone();
    }
}

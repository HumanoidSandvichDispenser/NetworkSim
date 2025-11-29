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

    /// <summary>
    /// Called when the entity is added to a world. Override to perform setup
    /// such as adding child entities.
    /// </summary>
    public virtual void AddToWorld(World world)
    {

    }
}

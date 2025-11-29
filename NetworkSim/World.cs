namespace NetworkSim;

public class World
{
    private static World? _instance;

    public static World Instance
    {
        get
        {
            if (_instance is null)
            {
                _instance = new();
            }
            return _instance;
        }
    }

    private HashSet<Entity> _entities { get; set; } = new();

    private List<IDrawable> _drawables { get; set; } = new();

    private Queue<Entity> _entitiesToAdd { get; set; } = new();

    private Queue<Entity> _entitiesToRemove { get; set; } = new();

    public IEnumerable<Entity> Entities => _entities.ToList();

    public float TimeScale { get; set; } = 1;

    public Entity AddEntity(Entity entity)
    {
        if (_entities.Contains(entity))
        {
            return entity;
        }

        _entitiesToAdd.Enqueue(entity);
        entity.CurrentWorld = this;

        return entity;
    }

    public Entity RemoveEntity(Entity entity)
    {
        if (_entities.Contains(entity))
        {
            _entitiesToRemove.Enqueue(entity);
        }

        return entity;
    }

    private void ProcessEntityQueues()
    {
        while (_entitiesToAdd.Count > 0)
        {
            var entity = _entitiesToAdd.Dequeue();
            _entities.Add(entity);
            entity.Initialize();
            if (entity is IDrawable drawable)
            {
                _drawables.Add(drawable);
            }
        }

        while (_entitiesToRemove.Count > 0)
        {
            var entity = _entitiesToRemove.Dequeue();
            _entities.Remove(entity);
            if (entity is IDrawable drawable)
            {
                _drawables.Remove(drawable);
            }
            entity.CurrentWorld = null;
        }
    }

    public void Update(float delta, int steps = 1)
    {
        ProcessEntityQueues();

        for (int i = 0; i < steps; i++)
        {
            float scaledDelta = delta * TimeScale;
            foreach (var entity in _entities)
            {
                entity.Update(scaledDelta);
            }
        }
    }

    public void Draw()
    {
        foreach (var drawable in _drawables)
        {
            if (drawable.Visible)
            {
                drawable.Draw();
            }
        }
    }
}

using System.Numerics;

/// <summary>
/// Represents an entity that can be drawn on the screen.
/// </summary>
public interface IDrawable
{
    Vector2 Position { get; }

    bool Visible { get; }

    abstract void Draw();
}

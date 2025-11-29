using NSubstitute;

namespace NetworkSim.Tests;

public class WorldTests
{
    public class TestEntity : Entity
    {
        public override void Update(float delta)
        {

        }
    }

    [Fact]
    public void AddEntity_AddsEntityToWorld()
    {
        var world = new World();
        var entity = new TestEntity();

        world.AddEntity(entity);

        world.Entities.ShouldContain(entity);
    }

    [Fact]
    public void RemoveEntity_RemovesEntityFromWorld()
    {
        var world = new World();
        var entity = new TestEntity();
        world.AddEntity(entity);
        world.RemoveEntity(entity);
        world.Entities.ShouldNotContain(entity);
    }

    [Fact]
    public void Update_UpdatesAllEntities()
    {
        var world = new World();
        var entity1 = Substitute.ForPartsOf<TestEntity>();
        var entity2 = Substitute.ForPartsOf<TestEntity>();

        world.AddEntity(entity1);
        world.AddEntity(entity2);
        world.Update(1f);

        entity1.Received(1).Update(1f);
        entity2.Received(1).Update(1f);
    }

    [Theory]
    [InlineData(0.5f)]
    [InlineData(1f)]
    [InlineData(2f)]
    public void Update_ShouldUpdateWithTimeScale(float timeScale)
    {
        var world = new World();
        world.TimeScale = timeScale;
        var entity = Substitute.ForPartsOf<TestEntity>();

        world.AddEntity(entity);
        world.Update(1f);

        entity.Received(1).Update(1f * timeScale);
    }

    [Fact]
    public void AddEntity_ShouldSetCurrentWorld()
    {
        var world = new World();
        var entity = new TestEntity();
        world.AddEntity(entity);
        entity.CurrentWorld.ShouldBe(world);
    }
}

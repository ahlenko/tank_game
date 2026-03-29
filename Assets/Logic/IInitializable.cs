using UnityEngine;
public interface IInitializable
{
    public void Initialize();

    public void InitializeWithPosition(Vector3 position, float rotation = 0f);
}
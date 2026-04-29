using UnityEngine;

namespace Utilities
{
    public abstract class Flyweight : MonoBehaviour
    {
        public FlyweightSettings Settings; // Intrinsic state
    }

    public enum FlyweightType
    {
        Projectile,
        Effect,
        Entity,
        Item
    }
}
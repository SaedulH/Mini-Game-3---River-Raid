using UnityEngine;

namespace Utilities
{
    [CreateAssetMenu(fileName = "flyweightSetting", menuName = "ObjectPool/Flyweight" )]
    public class FlyweightSettings : ScriptableObject
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public FlyweightType Type { get; private set; }
        [field: SerializeField] public GameObject Prefab { get; private set; }

        public Flyweight Initialise(GameObject flyweightObject)
        {
            if (flyweightObject.TryGetComponent(out Flyweight flyweight))
            {
                flyweight.Settings = this;
                return flyweight;
            }

            return null;
        }
    }
}
using SOEventSystem.Shared;
using UnityEngine;

namespace SOEventSystem.Tests {
    [CreateAssetMenu(menuName = "TestShared/Singleton")]
    public class TestSingleton : SharedSingleton<TestSingleton> { }
}
using UnityEngine;

namespace Game.Scripts.Helper.Pooling
{
    public static class Pooling
    {
        private static bool _isQuitting = false;
        public static bool IsQuitting => _isQuitting;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Application.quitting += () => _isQuitting = true;
        }
    }
}
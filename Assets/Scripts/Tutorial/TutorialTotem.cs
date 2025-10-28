using System.Reflection;
using UnityEngine;

namespace Tutorial
{
    [CreateAssetMenu(fileName = "TutorialTotem", menuName = "Tutorial/TutorialTotem", order = 10)]
    public class TutorialTotem : TutorialSO
    {
        // No SerializedFields: follow other TutorialSO patterns and locate the ritual at runtime
        private PowerUpRitualGranFuego _ritual;

        // Reflection cache
        private FieldInfo _leftField;
        private FieldInfo _rightField;

        public override void Initialize()
        {
            base.Initialize();
            FindAndCacheRitual();
        }

        public override void ResetTutorial()
        {
            base.ResetTutorial();
            _ritual = null;
            _leftField = null;
            _rightField = null;
        }

        private void FindAndCacheRitual()
        {
            if (_ritual != null) return;
            _ritual = Object.FindFirstObjectByType<PowerUpRitualGranFuego>();
            if (_ritual == null) return;

            var t = _ritual.GetType();
            _leftField = t.GetField("leftTorchLit", BindingFlags.NonPublic | BindingFlags.Instance);
            _rightField = t.GetField("rightTorchLit", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public override void UpdateTutorial()
        {
            base.UpdateTutorial();
            if (TutorialFinished) return;

            if (_ritual == null) FindAndCacheRitual();
            if (_ritual == null) return; // nothing to check

            // Ensure fields are cached
            if (_leftField == null || _rightField == null)
            {
                var t = _ritual.GetType();
                _leftField = t.GetField("leftTorchLit", BindingFlags.NonPublic | BindingFlags.Instance);
                _rightField = t.GetField("rightTorchLit", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            bool left = false;
            bool right = false;

            if (_leftField != null) left = (bool)_leftField.GetValue(_ritual);
            if (_rightField != null) right = (bool)_rightField.GetValue(_ritual);

            if (left && right)
            {
                CompleteTutorial();
            }
        }
    }
}

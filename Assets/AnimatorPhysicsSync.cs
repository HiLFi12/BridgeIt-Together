using UnityEngine;

namespace BridgeItTogether.Gameplay.AutoControllers
{

    [RequireComponent(typeof(Animator))]
    public class AnimatorPhysicsSync : MonoBehaviour
    {
        private void Awake()
        {
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.Fixed;
            }
        }
    }
}

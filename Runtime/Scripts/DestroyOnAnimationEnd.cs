using UnityEngine;

namespace GGLabo.Games.Runtime
{
    public class DestroyOnAnimationEnd : MonoBehaviour
    {
        public Animator animator;

        private void Start()
        {
            Debug.Assert(animator != null, "No animator.");

            Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
        }
    }
}
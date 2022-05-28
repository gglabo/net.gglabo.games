using UnityEngine;

namespace GGLabo.Games.Runtime
{
    public class PeriodicGenerator : MonoBehaviour
    {
        public GameObject prefab;

        public float delay = 0.2f;

        private ActionState _actionState;

        private void Start()
        {
            Debug.Assert(prefab != null, "No prefab.");

            _actionState = new ActionState();

            _actionState
                .Define(0, EffectCollide)
                .Define(1, Interval)
                .Next(0);
        }

        private void OnDestroy()
        {
            _actionState.Destroy();
        }

        private void Update()
        {
            _actionState.Update();
        }

        private void EffectCollide(ActionState actionState)
        {
            if (actionState.Time > delay)
            {
                var currentTransform = transform;
                Instantiate(prefab, currentTransform.position, currentTransform.rotation);
                actionState.Next(1);
            }
        }

        private void Interval(ActionState actionState)
        {
            _actionState.Next(0);
        }
    }
}
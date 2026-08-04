using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>Subtle idle head/body look variation so NPCs do not animate in perfect sync.</summary>
    [DisallowMultipleComponent]
    public class NPCIdleLookVariation : MonoBehaviour
    {
        [SerializeField] private float lookAmount = 14f;
        [SerializeField] private float lookSpeed = 0.28f;
        [SerializeField] private float phaseOffset;
        [SerializeField] private Transform lookPivot;

        private float baseYaw;

        private void Awake()
        {
            if (lookPivot == null)
            {
                lookPivot = transform;
            }

            baseYaw = lookPivot.localEulerAngles.y;
            if (phaseOffset <= 0.001f)
            {
                phaseOffset = Random.Range(0f, Mathf.PI * 2f);
            }
        }

        private void Update()
        {
            if (lookPivot == null)
            {
                return;
            }

            float sway = Mathf.Sin((Time.time * lookSpeed) + phaseOffset) * lookAmount;
            Vector3 euler = lookPivot.localEulerAngles;
            euler.y = baseYaw + sway;
            lookPivot.localEulerAngles = euler;
        }

        public void Configure(float amount, float speed, float phase)
        {
            lookAmount = amount;
            lookSpeed = speed;
            phaseOffset = phase;
        }
    }
}

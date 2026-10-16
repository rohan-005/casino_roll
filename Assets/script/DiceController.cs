using System.Collections;
using UnityEngine;

public class DiceController : MonoBehaviour
{
    [System.Serializable]
    public class DiceFace
    {
        public int number;
        public Transform faceTransform;
    }

    [Header("Components")]
    [SerializeField] private Rigidbody rb;

    [Header("Face Mapping")]
    [SerializeField] private DiceFace[] faces = new DiceFace[6];

    [Header("Rolling Settings")]
    [SerializeField] private float torqueStrength = 4f;
    [SerializeField] private float forceStrength = 0.3f;
    [SerializeField] private float rollDuration = 1.5f;
    [SerializeField] private float settleTimeout = 3f;

    [Header("Pity Assist")]
    [Range(0.2f, 0.9f)]
    [SerializeField] private float assistStartPercent = 0.6f;
    [SerializeField] private float assistStrength = 12f;

    private const float SettleLinearSpeed = 0.08f;
    private const float SettleAngularSpeed = 0.1f;

    public int Result { get; private set; }
    public bool IsRolling { get; private set; }

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
    }

    // forcedFace = 0 means a fully random roll.
    public IEnumerator Roll(int forcedFace = 0)
    {
        if (rb == null) yield break;

        IsRolling = true;

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddTorque(Random.onUnitSphere * torqueStrength, ForceMode.Impulse);

        Vector3 push = new Vector3(
            Random.Range(-0.3f, 0.3f),
            0.2f,
            Random.Range(-0.3f, 0.3f)
        ).normalized;

        rb.AddForce(push * forceStrength, ForceMode.Impulse);

        bool forced = forcedFace > 0 && TryGetFaceCorrection(forcedFace, out _);

        // Let the die tumble freely first, then start steering it while it's still moving.
        float assistDelay = forced ? rollDuration * assistStartPercent : rollDuration;
        yield return new WaitForSeconds(assistDelay);

        float minTime = rollDuration - assistDelay;
        float timer = 0f;

        while (timer < minTime + settleTimeout)
        {
            if (forced)
            {
                SteerTowardFace(forcedFace, assistStrength * Time.deltaTime);
            }

            bool settled =
                rb.linearVelocity.magnitude < SettleLinearSpeed &&
                rb.angularVelocity.magnitude < SettleAngularSpeed;

            if (timer >= minTime && settled) break;

            timer += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        // Remove any tiny leftover tilt so the result is exact.
        if (forced && TryGetFaceCorrection(forcedFace, out Quaternion correction))
        {
            transform.rotation = correction * transform.rotation;
            Physics.SyncTransforms();
        }

        Result = DetectTopFace();
        IsRolling = false;
    }

    public int DetectTopFace()
    {
        float bestDot = float.NegativeInfinity;
        int bestNumber = 0;

        foreach (DiceFace face in faces)
        {
            if (face == null || face.faceTransform == null) continue;

            Vector3 direction = face.faceTransform.position - transform.position;
            if (direction.sqrMagnitude < 0.0001f) continue;

            float dot = Vector3.Dot(direction.normalized, Vector3.up);
            if (dot > bestDot)
            {
                bestDot = dot;
                bestNumber = face.number;
            }
        }

        return bestNumber;
    }

    // Gradually rotates the die so the chosen face points up, while physics keeps running.
    private void SteerTowardFace(int number, float weight)
    {
        if (!TryGetFaceCorrection(number, out Quaternion correction)) return;

        weight = Mathf.Clamp01(weight);

        rb.rotation = Quaternion.Slerp(Quaternion.identity, correction, weight) * rb.rotation;
        rb.angularVelocity *= 1f - weight * 0.5f;
    }

    private bool TryGetFaceCorrection(int number, out Quaternion correction)
    {
        correction = Quaternion.identity;

        foreach (DiceFace face in faces)
        {
            if (face == null || face.faceTransform == null || face.number != number) continue;

            Vector3 direction = face.faceTransform.position - transform.position;
            if (direction.sqrMagnitude < 0.0001f) return false;

            correction = Quaternion.FromToRotation(direction.normalized, Vector3.up);
            return true;
        }

        return false;
    }
}
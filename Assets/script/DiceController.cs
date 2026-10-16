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
    [SerializeField] private float torqueStrength = 8f;
    [SerializeField] private float forceStrength = 1.5f;
    [SerializeField] private float rollDuration = 1.5f;
    [SerializeField] private float settleTimeout = 3f;

    private bool isRolling;

    public int Result { get; private set; }

    public bool IsRolling => isRolling;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    public IEnumerator Roll()
    {
        isRolling = true;

        rb.isKinematic = false;
        rb.useGravity = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 torque = Random.onUnitSphere * torqueStrength;

        rb.AddTorque(torque, ForceMode.Impulse);

        rb.AddForce(
            Random.insideUnitSphere * forceStrength,
            ForceMode.Impulse
        );

        yield return new WaitForSeconds(rollDuration);

        float timer = 0f;

        while (timer < settleTimeout)
        {
            if (rb.linearVelocity.magnitude < 0.08f &&
                rb.angularVelocity.magnitude < 0.1f)
            {
                break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Result = DetectFrontFace();

        isRolling = false;

        Debug.Log($"{gameObject.name} result: {Result}");
    }

    public int DetectFrontFace()
{
    float bestDot = -Mathf.Infinity;
    int bestNumber = 0;

    // The die's front faces toward world forward (+Z).
    Vector3 frontDirection = Vector3.forward;

    foreach (DiceFace face in faces)
    {
        if (face == null || face.faceTransform == null)
            continue;

        Vector3 direction =
            face.faceTransform.position - transform.position;

        if (direction.sqrMagnitude < 0.0001f)
            continue;

        direction.Normalize();

        float dot = Vector3.Dot(direction, frontDirection);

        if (dot > bestDot)
        {
            bestDot = dot;
            bestNumber = face.number;
        }
    }

    return bestNumber;
}

    public void ForceTopFace(int targetNumber)
    {
        DiceFace targetFace = null;

        foreach (DiceFace face in faces)
        {
            if (face != null &&
                face.faceTransform != null &&
                face.number == targetNumber)
            {
                targetFace = face;
                break;
            }
        }

        if (targetFace == null)
        {
            Debug.LogError(
                $"Face {targetNumber} is not mapped on {gameObject.name}."
            );
            return;
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 direction =
            targetFace.faceTransform.position - transform.position;

        if (direction.sqrMagnitude < 0.0001f)
        {
            Debug.LogError(
                $"Face marker {targetNumber} is at the die's center."
            );
            return;
        }

        Quaternion correction = Quaternion.FromToRotation(
            direction.normalized,
            Vector3.up
        );

        rb.rotation = correction * rb.rotation;

        rb.position = transform.position;

        Physics.SyncTransforms();

        Result = DetectFrontFace();
    }
}
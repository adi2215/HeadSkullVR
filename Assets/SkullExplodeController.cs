using UnityEngine;

public class SkullExplodeController : MonoBehaviour
{
    [Header("Bones To Spread")]
    [SerializeField] private Transform[] skullBones;

    [Header("Explosion Settings")]
    [SerializeField] private Transform centerPoint;
    [SerializeField] private float maxSpreadDistance = 1.5f;
    [SerializeField] private float changeSpeed = 1.5f;
    [SerializeField] private float moveSmooth = 8f;

    private Vector3[] startPositions;
    private Quaternion[] startRotations;
    private Vector3[] directions;

    private float spreadAmount;

    private void Start()
    {
        if (centerPoint == null)
            centerPoint = transform;

        startPositions = new Vector3[skullBones.Length];
        startRotations = new Quaternion[skullBones.Length];
        directions = new Vector3[skullBones.Length];

        for (int i = 0; i < skullBones.Length; i++)
        {
            if (skullBones[i] == null)
                continue;

            startPositions[i] = skullBones[i].position;
            startRotations[i] = skullBones[i].rotation;

            Vector3 dir = skullBones[i].position - centerPoint.position;

            if (dir.magnitude < 0.01f)
                dir = Random.onUnitSphere;

            directions[i] = dir.normalized;
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            spreadAmount += changeSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            spreadAmount -= changeSpeed * Time.deltaTime;
        }

        spreadAmount = Mathf.Clamp01(spreadAmount);

        UpdateBones();
    }

    private void UpdateBones()
    {
        for (int i = 0; i < skullBones.Length; i++)
        {
            if (skullBones[i] == null)
                continue;

            Vector3 targetPosition = startPositions[i] + directions[i] * maxSpreadDistance * spreadAmount;

            skullBones[i].position = Vector3.Lerp(
                skullBones[i].position,
                targetPosition,
                Time.deltaTime * moveSmooth
            );

            skullBones[i].rotation = Quaternion.Slerp(
                skullBones[i].rotation,
                startRotations[i],
                Time.deltaTime * moveSmooth
            );
        }
    }
}
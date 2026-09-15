using UnityEngine;

/// <summary>
/// Quản lý di chuyển của người chơi (Player Controller)
/// Tích hợp:
/// 1. Động tác chân bước nhịp nhàng (Left/Right Leg cycle)
/// 2. Tay đung đưa nhịp nhàng đối xứng (Left/Right Arm swing)
/// 3. Cơ chế Chạy Nước Rút (Sprint / Dash) bằng Shift hoặc nút Sprint trên màn hình
/// 4. Hiệu ứng vệt gió tốc độ (Speed Trail) và Camera Dynamic FOV
/// 5. Bụi bước chân (Dust Puff VFX) và Cartoon Lean
/// </summary>
public class PlayerMove : MonoBehaviour
{
    public static PlayerMove Instance { get; private set; }

    [Header("Movement Settings")]
    public float walkSpeed = 6.0f;
    public float sprintSpeed = 11.5f;
    public float rotationSpeed = 14f;

    [Header("Sprint Settings")]
    public bool isSprinting = false;
    public static bool isMobileSprintHeld = false; // Nhận từ nút UI trên Mobile
    public float sprintFOVBonus = 6.0f;

    [Header("Movement VFX & Feel")]
    public bool enableMovementVFX = true;
    public float leanAngle = 9f;          // Góc nghiêng người khi ôm cua
    public float bounceHeight = 0.08f;    // Nhún nhảy chibi
    public float armSwingAngle = 32f;     // Góc vung tay đung đưa khi đi bộ
    public float sprintArmSwingAngle = 55f; // Góc vung tay mạnh mẽ khi chạy nước rút
    public float legSwingAngle = 30f;     // Góc sải chân khi đi bộ
    public float sprintLegSwingAngle = 50f; // Góc sải chân mạnh khi chạy nước rút

    [Header("Procedural Limbs")]
    public Transform leftArmTransform;
    public Transform rightArmTransform;
    public Transform leftLegTransform;
    public Transform rightLegTransform;

    private Animator animator;
    private ParticleSystem dustPuffVFX;
    private TrailRenderer sprintTrailLeft;
    private TrailRenderer sprintTrailRight;
    private Transform visualModel;
    private Vector3 initialModelLocalPos;
    private Quaternion initialModelLocalRot;

    private float stepCycleTimer = 0f;
    private float defaultCameraFOV = 60f;
    private Camera mainCam;

    private static readonly int isMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int speedHash = Animator.StringToHash("Speed");
    private static readonly int isSprintingHash = Animator.StringToHash("IsSprinting");

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            visualModel = animator.transform;
        }
        else if (transform.childCount > 0)
        {
            visualModel = transform.GetChild(0);
        }
        else
        {
            visualModel = transform;
        }

        initialModelLocalPos = visualModel.localPosition;
        initialModelLocalRot = visualModel.localRotation;

        mainCam = Camera.main;
        if (mainCam != null)
        {
            defaultCameraFOV = mainCam.fieldOfView;
        }

        SetupMovementDustVFX();
        SetupSprintTrails();
        DetectOrBuildProceduralLimbs();
    }

    private void SetupMovementDustVFX()
    {
        GameObject vfxObj = new GameObject("Player_Movement_DustPuff");
        vfxObj.transform.SetParent(transform, false);
        vfxObj.transform.localPosition = new Vector3(0f, 0.05f, -0.2f);

        dustPuffVFX = vfxObj.AddComponent<ParticleSystem>();
        var main = dustPuffVFX.main;
        main.startLifetime = 0.35f;
        main.startSpeed = 0.9f;
        main.startSize = 0.35f;
        main.startColor = new Color(0.95f, 0.92f, 0.85f, 0.65f);
        main.gravityModifier = -0.15f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;

        var emission = dustPuffVFX.emission;
        emission.rateOverTime = 18f;
        emission.enabled = false;

        var shape = dustPuffVFX.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.25f;

        var sizeOverLifetime = dustPuffVFX.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0f, 0.4f);
        curve.AddKey(0.4f, 1f);
        curve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);

        var renderer = dustPuffVFX.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        Material particleMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        if (particleMat.shader == null) particleMat = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.sharedMaterial = particleMat;
    }

    private void SetupSprintTrails()
    {
        // Tạo 2 dải vệt gió lướt sau lưng khi chạy nước rút (Speed Trail)
        GameObject trailObjL = new GameObject("SprintTrail_L");
        trailObjL.transform.SetParent(transform, false);
        trailObjL.transform.localPosition = new Vector3(-0.35f, 0.8f, -0.1f);
        sprintTrailLeft = trailObjL.AddComponent<TrailRenderer>();
        ConfigureTrail(sprintTrailLeft);

        GameObject trailObjR = new GameObject("SprintTrail_R");
        trailObjR.transform.SetParent(transform, false);
        trailObjR.transform.localPosition = new Vector3(0.35f, 0.8f, -0.1f);
        sprintTrailRight = trailObjR.AddComponent<TrailRenderer>();
        ConfigureTrail(sprintTrailRight);
    }

    private void ConfigureTrail(TrailRenderer tr)
    {
        tr.time = 0.22f;
        tr.startWidth = 0.18f;
        tr.endWidth = 0.02f;
        tr.emitting = false;

        Material trailMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        if (trailMat.shader == null) trailMat = new Material(Shader.Find("Particles/Standard Unlit"));
        tr.sharedMaterial = trailMat;

        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.85f, 0.4f), 0f), new GradientColorKey(new Color(0.4f, 0.8f, 1f), 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.7f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        tr.colorGradient = grad;
    }

    private void DetectOrBuildProceduralLimbs()
    {
        // 1. Thử tìm kiếm trong các khớp xương nếu mô hình có sẵn xương tay/chân
        if (visualModel != null)
        {
            leftArmTransform = FindLimbRecursive(visualModel, "left", "arm", "hand", "shoulder");
            rightArmTransform = FindLimbRecursive(visualModel, "right", "arm", "hand", "shoulder");
            leftLegTransform = FindLimbRecursive(visualModel, "left", "leg", "foot", "thigh");
            rightLegTransform = FindLimbRecursive(visualModel, "right", "leg", "foot", "thigh");
        }

        // 2. Nếu nhân vật là khối mesh chưa có tay chân rời, tự động tạo bộ tay chân hoạt hình chibi sinh động
        if (leftArmTransform == null || rightArmTransform == null)
        {
            CreateProceduralArm(out leftArmTransform, "Procedural_LeftArm", new Vector3(-0.48f, 0.95f, 0.05f));
            CreateProceduralArm(out rightArmTransform, "Procedural_RightArm", new Vector3(0.48f, 0.95f, 0.05f));
        }

        if (leftLegTransform == null || rightLegTransform == null)
        {
            CreateProceduralLeg(out leftLegTransform, "Procedural_LeftLeg", new Vector3(-0.24f, 0.48f, 0f));
            CreateProceduralLeg(out rightLegTransform, "Procedural_RightLeg", new Vector3(0.24f, 0.48f, 0f));
        }
    }

    private Transform FindLimbRecursive(Transform parent, string sideKeyword, params string[] limbKeywords)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>())
        {
            string name = child.name.ToLower();
            if (name.Contains(sideKeyword))
            {
                foreach (var kw in limbKeywords)
                {
                    if (name.Contains(kw)) return child;
                }
            }
        }
        return null;
    }

    private void CreateProceduralArm(out Transform armTrans, string name, Vector3 localPos)
    {
        GameObject armPivot = new GameObject(name);
        armPivot.transform.SetParent(visualModel != null ? visualModel : transform, false);
        armPivot.transform.localPosition = localPos;

        GameObject armMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        armMesh.name = "ArmMesh";
        armMesh.transform.SetParent(armPivot.transform, false);
        armMesh.transform.localPosition = new Vector3(0f, -0.22f, 0f);
        armMesh.transform.localScale = new Vector3(0.18f, 0.24f, 0.18f);

        Collider col = armMesh.GetComponent<Collider>();
        if (col != null) Destroy(col);

        Renderer r = armMesh.GetComponent<Renderer>();
        if (r != null)
        {
            r.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (r.material.shader == null) r.material = new Material(Shader.Find("Standard"));
            r.material.color = new Color(0.92f, 0.52f, 0.38f); // Màu áo barista ấm áp
        }

        armTrans = armPivot.transform;
    }

    private void CreateProceduralLeg(out Transform legTrans, string name, Vector3 localPos)
    {
        GameObject legPivot = new GameObject(name);
        legPivot.transform.SetParent(visualModel != null ? visualModel : transform, false);
        legPivot.transform.localPosition = localPos;

        GameObject legMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        legMesh.name = "LegMesh";
        legMesh.transform.SetParent(legPivot.transform, false);
        legMesh.transform.localPosition = new Vector3(0f, -0.24f, 0f);
        legMesh.transform.localScale = new Vector3(0.20f, 0.26f, 0.20f);

        Collider col = legMesh.GetComponent<Collider>();
        if (col != null) Destroy(col);

        Renderer r = legMesh.GetComponent<Renderer>();
        if (r != null)
        {
            r.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (r.material.shader == null) r.material = new Material(Shader.Find("Standard"));
            r.material.color = new Color(0.18f, 0.22f, 0.30f); // Quần tối màu sang trọng
        }

        legTrans = legPivot.transform;
    }

    private void Update()
    {
        // 1. Nhận input phím (PC) & Cần gạt ảo (Mobile)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 keyInput = new Vector2(h, v);

        Vector2 touchInput = Vector2.zero;
        if (VirtualJoystick.Instance != null && VirtualJoystick.Instance.Direction.sqrMagnitude > 0.0025f)
        {
            touchInput = VirtualJoystick.Instance.Direction;
        }

        Vector2 finalInput = (touchInput.sqrMagnitude > keyInput.sqrMagnitude) ? touchInput : keyInput;
        Vector3 moveDirection = new Vector3(finalInput.x, 0, finalInput.y);
        float inputStrength = Mathf.Clamp01(finalInput.magnitude);
        bool isMoving = inputStrength > 0.05f;

        // 2. Nhận lệnh Chạy Nước Rút (Sprint): Giữ phím Shift (PC) hoặc nút Sprint (Mobile)
        bool sprintKeyHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || isMobileSprintHeld;
        isSprinting = isMoving && sprintKeyHeld;

        float targetSpeed = isSprinting ? sprintSpeed : walkSpeed;
        float currentSpeed = targetSpeed * inputStrength;

        // 3. Xử lý di chuyển & xoay
        if (isMoving)
        {
            transform.Translate(moveDirection.normalized * (currentSpeed * Time.deltaTime), Space.World);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Tần số bước chân & vung tay tăng gấp bội khi chạy nước rút
            float stepSpeed = isSprinting ? 22f : 13f;
            stepCycleTimer += Time.deltaTime * stepSpeed;

            // Góc vung tay & sải chân
            float curArmSwing = isSprinting ? sprintArmSwingAngle : armSwingAngle;
            float curLegSwing = isSprinting ? sprintLegSwingAngle : legSwingAngle;

            // Động tác tay đung đưa nhịp nhàng: Tay trái vung trước thì tay phải vung sau
            float armAngle = Mathf.Sin(stepCycleTimer) * curArmSwing;
            if (leftArmTransform != null) leftArmTransform.localRotation = Quaternion.Euler(armAngle, 0f, 0f);
            if (rightArmTransform != null) rightArmTransform.localRotation = Quaternion.Euler(-armAngle, 0f, 0f);

            // Động tác chân bước nhịp nhàng: Chân bước đối xứng với tay
            float legAngle = Mathf.Sin(stepCycleTimer) * curLegSwing;
            if (leftLegTransform != null) leftLegTransform.localRotation = Quaternion.Euler(-legAngle, 0f, 0f);
            if (rightLegTransform != null) rightLegTransform.localRotation = Quaternion.Euler(legAngle, 0f, 0f);

            // Nhún nhảy cơ thể theo bước chạy (Bounce & Lean)
            if (visualModel != null)
            {
                float bounceY = Mathf.Abs(Mathf.Sin(stepCycleTimer)) * (isSprinting ? bounceHeight * 1.5f : bounceHeight);
                visualModel.localPosition = initialModelLocalPos + new Vector3(0f, bounceY, 0f);

                // Nghiêng nhẹ vào cua khi rẽ hướng
                float turnAngle = Vector3.SignedAngle(transform.forward, moveDirection.normalized, Vector3.up);
                float currentLean = Mathf.Clamp(turnAngle / 90f, -1f, 1f) * (isSprinting ? leanAngle * 1.4f : leanAngle);
                visualModel.localRotation = Quaternion.Euler(isSprinting ? 8f : 0f, 0f, -currentLean); // Khi chạy hơi chúi người về trước
            }

            // Kích hoạt hạt bụi bước chân
            if (dustPuffVFX != null)
            {
                var em = dustPuffVFX.emission;
                em.rateOverTime = isSprinting ? 35f : 16f;
                if (!em.enabled) em.enabled = true;
                if (!dustPuffVFX.isPlaying) dustPuffVFX.Play();
            }

            // Hiệu ứng vệt gió khi chạy nước rút (Sprint Trail)
            if (sprintTrailLeft != null) sprintTrailLeft.emitting = isSprinting;
            if (sprintTrailRight != null) sprintTrailRight.emitting = isSprinting;

            // Animator params
            if (animator != null)
            {
                animator.SetBool(isMovingHash, true);
                animator.SetFloat(speedHash, currentSpeed);
                animator.SetBool(isSprintingHash, isSprinting);
            }
        }
        else
        {
            // Trả về tư thế đứng yên thư giãn (Idle Rest)
            stepCycleTimer = 0f;

            if (leftArmTransform != null) leftArmTransform.localRotation = Quaternion.Slerp(leftArmTransform.localRotation, Quaternion.identity, Time.deltaTime * 10f);
            if (rightArmTransform != null) rightArmTransform.localRotation = Quaternion.Slerp(rightArmTransform.localRotation, Quaternion.identity, Time.deltaTime * 10f);
            if (leftLegTransform != null) leftLegTransform.localRotation = Quaternion.Slerp(leftLegTransform.localRotation, Quaternion.identity, Time.deltaTime * 10f);
            if (rightLegTransform != null) rightLegTransform.localRotation = Quaternion.Slerp(rightLegTransform.localRotation, Quaternion.identity, Time.deltaTime * 10f);

            if (visualModel != null)
            {
                // Thở nhẹ nhàng khi đứng yên
                float idleBreathing = Mathf.Sin(Time.time * 2.5f) * 0.015f;
                visualModel.localPosition = Vector3.Lerp(visualModel.localPosition, initialModelLocalPos + new Vector3(0, idleBreathing, 0), Time.deltaTime * 8f);
                visualModel.localRotation = Quaternion.Slerp(visualModel.localRotation, initialModelLocalRot, Time.deltaTime * 10f);
            }

            if (dustPuffVFX != null)
            {
                var em = dustPuffVFX.emission;
                if (em.enabled) em.enabled = false;
            }

            if (sprintTrailLeft != null) sprintTrailLeft.emitting = false;
            if (sprintTrailRight != null) sprintTrailRight.emitting = false;

            if (animator != null)
            {
                animator.SetBool(isMovingHash, false);
                animator.SetFloat(speedHash, 0f);
                animator.SetBool(isSprintingHash, false);
            }
        }

        // 4. Hiệu ứng Camera Dynamic FOV khi Chạy Nước Rút (Tạo cảm giác tốc độ điện ảnh)
        if (mainCam != null)
        {
            float targetFOV = isSprinting ? defaultCameraFOV + sprintFOVBonus : defaultCameraFOV;
            mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, targetFOV, Time.deltaTime * 6f);
        }
    }
}
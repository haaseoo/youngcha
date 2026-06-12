using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReceiptStackGameManager : MonoBehaviour
{
    [Header("Stack Object")]
    public GameObject receiptBlockPrefab;
    public Transform stackBase;

    [Header("Receipt Block Materials")]
    public Material[] receiptBlockMaterials;

    [Header("Move Setting")]
    public float moveRange = 1.8f;
    public float moveSpeed = 2.2f;

    [Header("Drop Setting")]
    public float dropHeight = 2.2f;
    public float dropDuration = 0.18f;

    [Header("Stack Setting")]
    public float blockHeight = 0.18f;
    public float minOverlapSize = 0.15f;

    [Header("Camera Follow")]
    public Transform cameraFollowTarget;
    public float cameraTargetYOffset = 1.2f;

    [Header("Managers")]
    public StackUIManager uiManager;
    public StackRecordManager recordManager;

    private GameObject currentBlock;
    private Transform lastBlock;

    private bool isMoving;
    private bool isDropping;
    private bool isGameStarted;
    private bool isGameOver;

    private bool moveOnXAxis = true;

    private int score;
    private int stackCount;

    private float currentY;

    private void Start()
    {
        if (stackBase == null)
        {
            Debug.LogError("Stack Base가 연결되지 않았습니다.");
            return;
        }

        lastBlock = stackBase;

        currentY = stackBase.position.y + stackBase.localScale.y / 2f + blockHeight / 2f;

        UpdateCameraFollowTarget();

        if (uiManager != null)
        {
            uiManager.UpdateGameUI(score, stackCount);
            uiManager.HideGameOverPanel();
            uiManager.ShowStartGuide();
        }
    }

    private void Update()
    {
        if (isGameOver) return;

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (!isGameStarted)
            {
                StartGame();
                return;
            }

            if (currentBlock != null && !isDropping)
            {
                StartCoroutine(DropAndPlaceBlock());
            }
        }

        if (isGameStarted)
        {
            MoveCurrentBlock();
        }
    }

    private void StartGame()
    {
        isGameStarted = true;

        if (uiManager != null)
        {
            uiManager.HideStartGuide();
        }

        SpawnBlock();
    }

    private void SpawnBlock()
    {
        if (isGameOver) return;

        if (receiptBlockPrefab == null)
        {
            Debug.LogError("Receipt Block Prefab이 연결되지 않았습니다.");
            return;
        }

        Vector3 spawnPos = new Vector3(
            lastBlock.position.x,
            currentY + dropHeight,
            lastBlock.position.z
        );

        currentBlock = Instantiate(receiptBlockPrefab, spawnPos, Quaternion.identity);

        ApplyRandomMaterial(currentBlock);
        MatchBlockSizeToLastBlock(currentBlock);

        Rigidbody rb = currentBlock.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        isMoving = true;
        isDropping = false;
    }

    private void MoveCurrentBlock()
    {
        if (currentBlock == null || !isMoving || isDropping) return;

        float moveValue = Mathf.Sin(Time.time * moveSpeed) * moveRange;

        Vector3 pos = currentBlock.transform.position;

        if (moveOnXAxis)
        {
            pos.x = lastBlock.position.x + moveValue;
            pos.z = lastBlock.position.z;
        }
        else
        {
            pos.x = lastBlock.position.x;
            pos.z = lastBlock.position.z + moveValue;
        }

        pos.y = currentY + dropHeight;
        currentBlock.transform.position = pos;
    }

    private IEnumerator DropAndPlaceBlock()
    {
        isMoving = false;
        isDropping = true;

        Vector3 startPos = currentBlock.transform.position;
        Vector3 targetPos = new Vector3(startPos.x, currentY, startPos.z);

        float time = 0f;

        while (time < dropDuration)
        {
            time += Time.deltaTime;
            float t = time / dropDuration;

            currentBlock.transform.position = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        currentBlock.transform.position = targetPos;

        CutBlock();
    }

    private void CutBlock()
    {
        if (currentBlock == null || lastBlock == null) return;

        int axis = moveOnXAxis ? 0 : 2;

        float currentCenter = currentBlock.transform.position[axis];
        float lastCenter = lastBlock.position[axis];

        float currentSize = currentBlock.transform.localScale[axis];
        float lastSize = lastBlock.localScale[axis];

        float currentMin = currentCenter - currentSize / 2f;
        float currentMax = currentCenter + currentSize / 2f;

        float lastMin = lastCenter - lastSize / 2f;
        float lastMax = lastCenter + lastSize / 2f;

        float overlapMin = Mathf.Max(currentMin, lastMin);
        float overlapMax = Mathf.Min(currentMax, lastMax);

        float overlapSize = overlapMax - overlapMin;

        if (overlapSize <= minOverlapSize)
        {
            DropBlock(currentBlock);

            currentBlock = null;
            isDropping = false;

            GameOver();

            return;
        }

        float newCenter = (overlapMin + overlapMax) / 2f;
        float cutSize = currentSize - overlapSize;

        Vector3 blockPos = currentBlock.transform.position;
        blockPos[axis] = newCenter;
        blockPos.y = currentY;
        currentBlock.transform.position = blockPos;

        Vector3 blockScale = currentBlock.transform.localScale;
        blockScale[axis] = overlapSize;
        currentBlock.transform.localScale = blockScale;

        CreateCutPiece(axis, currentCenter, currentSize, overlapMin, overlapMax, cutSize);

        CompleteStack(overlapSize, lastSize);
    }

    private void CreateCutPiece(
        int axis,
        float originalCenter,
        float originalSize,
        float overlapMin,
        float overlapMax,
        float cutSize
    )
    {
        if (cutSize <= 0.01f) return;

        float originalMin = originalCenter - originalSize / 2f;
        float originalMax = originalCenter + originalSize / 2f;

        float cutCenter;

        if (originalMin < overlapMin)
        {
            cutCenter = originalMin + cutSize / 2f;
        }
        else
        {
            cutCenter = originalMax - cutSize / 2f;
        }

        GameObject cutPiece = Instantiate(currentBlock);
        cutPiece.name = "CutPiece";

        Vector3 cutPos = currentBlock.transform.position;
        cutPos[axis] = cutCenter;
        cutPos.y = currentY;
        cutPiece.transform.position = cutPos;

        Vector3 cutScale = currentBlock.transform.localScale;
        cutScale[axis] = cutSize;
        cutPiece.transform.localScale = cutScale;

        DropBlock(cutPiece);
    }

    private void CompleteStack(float overlapSize, float previousSize)
    {
        lastBlock = currentBlock.transform;

        stackCount++;
        score += CalculateScore(overlapSize, previousSize);

        if (uiManager != null)
        {
            uiManager.UpdateGameUI(score, stackCount);
        }

        currentBlock = null;
        currentY += blockHeight;

        UpdateCameraFollowTarget();

        isDropping = false;
        moveOnXAxis = !moveOnXAxis;

        Invoke(nameof(SpawnBlock), 0.35f);
    }

    private void DropBlock(GameObject block)
    {
        if (block == null) return;

        Collider[] colliders = block.GetComponents<Collider>();

        foreach (Collider col in colliders)
        {
            Destroy(col);
        }

        Rigidbody rb = block.GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = block.AddComponent<Rigidbody>();
        }

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.mass = 0.2f;
        rb.linearDamping = 0.2f;
        rb.angularDamping = 1.5f;

        rb.AddTorque(
            Random.Range(-3f, 3f),
            Random.Range(-2f, 2f),
            Random.Range(-3f, 3f),
            ForceMode.Impulse
        );

        Destroy(block, 1.2f);
    }

    private void ApplyRandomMaterial(GameObject block)
    {
        if (receiptBlockMaterials == null || receiptBlockMaterials.Length == 0) return;

        MeshRenderer meshRenderer = block.GetComponent<MeshRenderer>();

        if (meshRenderer == null) return;

        int randomIndex = Random.Range(0, receiptBlockMaterials.Length);
        meshRenderer.material = receiptBlockMaterials[randomIndex];
    }

    private void MatchBlockSizeToLastBlock(GameObject block)
    {
        Vector3 scale = block.transform.localScale;

        scale.x = lastBlock.localScale.x;
        scale.y = blockHeight;
        scale.z = lastBlock.localScale.z;

        block.transform.localScale = scale;
    }

    private int CalculateScore(float overlapSize, float previousSize)
    {
        float ratio = overlapSize / previousSize;

        if (ratio > 0.95f) return 100;
        if (ratio > 0.75f) return 70;
        if (ratio > 0.5f) return 40;

        return 20;
    }

    private void GameOver()
    {
        isGameOver = true;
        isMoving = false;
        isDropping = false;

        CancelInvoke(nameof(SpawnBlock));

        if (uiManager != null)
        {
            uiManager.ShowGameOverPanel(score, stackCount);
        }

        if (recordManager != null)
        {
            recordManager.SaveRecord(score, stackCount);
        }

        Debug.Log("게임 오버!");
    }

    private void UpdateCameraFollowTarget()
    {
        if (cameraFollowTarget == null) return;

        cameraFollowTarget.position = new Vector3(
            lastBlock.position.x,
            currentY + cameraTargetYOffset,
            lastBlock.position.z
        );
    }
}
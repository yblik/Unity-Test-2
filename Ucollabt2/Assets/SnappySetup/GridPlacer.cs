using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridPlacer : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject[] brushPrefabs;
    public float cellSize = 1f;
    public LayerMask placementMask;

    public int gridWidth = 20;
    public int gridHeight = 20;

    private int selectedBrushIndex = 0;
    private float currentRotation = 0f;
    private bool eraserMode = false;
    private int elevation = 0;
    private int brushSize = 1;

    private HashSet<Vector3Int> filledPositions = new HashSet<Vector3Int>();
    private Dictionary<Vector3Int, GameObject> placedObjects = new Dictionary<Vector3Int, GameObject>();

    private GameObject selectedObject = null;
    private Vector3Int selectedGridPos;

    void Update()
    {
        HandleCameraMovement();

        if (EventSystem.current.IsPointerOverGameObject())
            return;

        HandlePlacement();
        HandleSelection();
        HandleSelectedMovement();
    }

    void HandleCameraMovement()
    {
        if (selectedObject != null) return;

        float speed = 10f;
        Vector3 move = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) move += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) move += Vector3.back;
        if (Input.GetKey(KeyCode.A)) move += Vector3.left;
        if (Input.GetKey(KeyCode.D)) move += Vector3.right;

        mainCamera.transform.position += move * speed * Time.deltaTime;
    }

    void HandlePlacement()
    {
        if (Input.GetMouseButton(0) && selectedObject == null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, placementMask))
            {
                Vector3Int baseGridPos = WorldToGrid(hit.point);

                for (int x = 0; x < brushSize; x++)
                {
                    for (int z = 0; z < brushSize; z++)
                    {
                        Vector3Int gridPos = new Vector3Int(baseGridPos.x + x, elevation, baseGridPos.z + z);

                        if (eraserMode)
                        {
                            if (placedObjects.ContainsKey(gridPos))
                            {
                                Destroy(placedObjects[gridPos]);
                                placedObjects.Remove(gridPos);
                                filledPositions.Remove(gridPos);
                                if (selectedGridPos == gridPos) selectedObject = null;
                            }
                        }
                        else
                        {
                            if (!filledPositions.Contains(gridPos))
                            {
                                Vector3 spawnPos = GridToWorld(gridPos);
                                GameObject obj = Instantiate(brushPrefabs[selectedBrushIndex], spawnPos, Quaternion.Euler(0, currentRotation, 0));
                                obj.transform.parent = this.transform;

                                filledPositions.Add(gridPos);
                                placedObjects[gridPos] = obj;
                            }
                        }
                    }
                }
            }
        }
    }

    void HandleSelection()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, placementMask))
            {
                Vector3Int gridPos = WorldToGrid(hit.point);
                if (placedObjects.ContainsKey(gridPos))
                {
                    selectedObject = placedObjects[gridPos];
                    selectedGridPos = gridPos;
                }
            }
        }
    }

    void HandleSelectedMovement()
    {
        if (selectedObject == null) return;

        Vector3Int direction = Vector3Int.zero;

        if (Input.GetKeyDown(KeyCode.W)) direction = new Vector3Int(0, 0, 1);
        if (Input.GetKeyDown(KeyCode.S)) direction = new Vector3Int(0, 0, -1);
        if (Input.GetKeyDown(KeyCode.A)) direction = new Vector3Int(-1, 0, 0);
        if (Input.GetKeyDown(KeyCode.D)) direction = new Vector3Int(1, 0, 0);

        if (direction != Vector3Int.zero)
        {
            Vector3Int newPos = selectedGridPos + direction;

            if (!filledPositions.Contains(newPos) &&
                newPos.x >= 0 && newPos.x < gridWidth &&
                newPos.z >= 0 && newPos.z < gridHeight)
            {
                filledPositions.Remove(selectedGridPos);
                placedObjects.Remove(selectedGridPos);

                selectedObject.transform.position = GridToWorld(newPos);

                selectedGridPos = newPos;
                filledPositions.Add(newPos);
                placedObjects[newPos] = selectedObject;
            }
        }

        if (Input.GetKeyDown(KeyCode.Comma))
        {
            selectedObject.transform.Rotate(0f, -45f, 0f);
        }
        else if (Input.GetKeyDown(KeyCode.Period))
        {
            selectedObject.transform.Rotate(0f, 45f, 0f);
        }
    }

    Vector3Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector3Int(
            Mathf.FloorToInt(worldPos.x / cellSize),
            elevation,
            Mathf.FloorToInt(worldPos.z / cellSize)
        );
    }

    Vector3 GridToWorld(Vector3Int gridPos)
    {
        return new Vector3(
            gridPos.x * cellSize + cellSize / 2f,
            gridPos.y * cellSize,
            gridPos.z * cellSize + cellSize / 2f
        );
    }

    public void SelectBrush(int brushIndex)
    {
        selectedBrushIndex = Mathf.Clamp(brushIndex, 0, brushPrefabs.Length - 1);
        eraserMode = false;
        selectedObject = null;
    }

    public void ToggleEraser()
    {
        eraserMode = !eraserMode;
        selectedObject = null;
    }

    public void SetElevation(int newElevation)
    {
        elevation = newElevation;
    }

    public void IncreaseBrushSize() => brushSize++;
    public void DecreaseBrushSize() => brushSize = Mathf.Max(1, brushSize - 1);
    public void IncreaseGridCellSize() => cellSize += 0.5f;
    public void DecreaseGridCellSize() => cellSize = Mathf.Max(0.1f, cellSize - 0.5f);

    public void ToggleCameraMode()
    {
        mainCamera.orthographic = !mainCamera.orthographic;
        if (mainCamera.orthographic)
            mainCamera.orthographicSize = 10f;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        float y = elevation * cellSize;
        Gizmos.color = eraserMode ? Color.red : Color.gray;

        for (int x = 0; x <= gridWidth; x++)
        {
            Gizmos.DrawLine(new Vector3(x * cellSize, y, 0), new Vector3(x * cellSize, y, gridHeight * cellSize));
        }

        for (int z = 0; z <= gridHeight; z++)
        {
            Gizmos.DrawLine(new Vector3(0, y, z * cellSize), new Vector3(gridWidth * cellSize, y, z * cellSize));
        }
    }
}

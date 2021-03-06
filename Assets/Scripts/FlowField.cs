﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowField : MonoBehaviour
{
    public LayerMask nonTraverseableMask;
    public LayerMask difficultTerrainMask;

    public Vector3 flowFieldWorldSize;
    public float cellRadius;
    float cellDiameter;
    Cell[,,] cells;
    public Cell destinationCell;

    Vector3Int flowFieldGridSize;
    Vector3 flowFieldBottomLeft;

    bool showBestDirectionVectors = false;
    bool showCollisionBlocks = false;

    void Start()
    {
        cellDiameter = cellRadius * 2;
        flowFieldGridSize.x = Mathf.CeilToInt(flowFieldWorldSize.x / cellDiameter);
        flowFieldGridSize.y = Mathf.CeilToInt(flowFieldWorldSize.y / cellDiameter);
        flowFieldGridSize.z = Mathf.CeilToInt(flowFieldWorldSize.z / cellDiameter);

        CreateCellGrid();
        CreateCostField();
    }

    // Flowfield Creation & Path Calculation //
    void CreateCellGrid()
    {
        cells = new Cell[flowFieldGridSize.x, flowFieldGridSize.y, flowFieldGridSize.z];
        flowFieldBottomLeft = transform.position;
        flowFieldBottomLeft -= Vector3.right * flowFieldWorldSize.x / 2;
        flowFieldBottomLeft -= Vector3.up * flowFieldWorldSize.y / 2;
        flowFieldBottomLeft -= Vector3.forward * flowFieldWorldSize.z / 2;

        for (int x = 0; x < flowFieldGridSize.x; x++)
        {
            for (int y = 0; y < flowFieldGridSize.y; y++)
            {
                for (int z = 0; z < flowFieldGridSize.z; z++)
                {
                    Vector3 cellWorldPos = flowFieldBottomLeft;
                    cellWorldPos += Vector3.right * (x * cellDiameter + cellRadius);
                    cellWorldPos += Vector3.up * (y * cellDiameter + cellRadius);
                    cellWorldPos += Vector3.forward * (z * cellDiameter + cellRadius);

                    Vector3Int cellsPos = new Vector3Int(x, y, z);
                    cells[x, y, z] = new Cell(cellWorldPos, cellsPos);
                }
            }
        }

    }
    void CreateCostField()
    {
        byte difficultTerrainCost = 3;

        foreach(Cell cell in cells)
        {
            if (Physics.CheckSphere(cell.worldPos, cellRadius, nonTraverseableMask))
            {
                cell.IncreaseCost(byte.MaxValue);
            }
            else if (Physics.CheckSphere(cell.worldPos, cellRadius, difficultTerrainMask))
            {
                cell.IncreaseCost(difficultTerrainCost);
            }
        }
    }
    void CreateIntegrationField(Cell _destinationCell)
    {
        destinationCell = _destinationCell;
        destinationCell.bestCost = 0;

        Queue<Cell> openQueue = new Queue<Cell>();
        openQueue.Enqueue(destinationCell);

        while (openQueue.Count > 0)
        {
            Cell currentCell = openQueue.Dequeue();
            List<Cell> curNeighbors = GetAdjacentCells(currentCell);
            foreach(Cell neighbor in curNeighbors)
            {
                if (!IsCellTraverseable(neighbor))
                {
                    continue;
                }

                uint newBestCost = (uint)(neighbor.cost + currentCell.bestCost);
                if (newBestCost < neighbor.bestCost)
                {
                    neighbor.bestCost = newBestCost;
                    openQueue.Enqueue(neighbor);
                }
            }
        }
    }
    void CreateFlowField(Vector3 targetPos)
    {
        foreach(Cell cell in cells)
        {
            if (!IsCellTraverseable(cell))
            {
                continue;
            }

            uint lowestBestCost = uint.MaxValue;
            float sqrDistanceToTarget = float.MaxValue;
            Cell bestNeighbor = null;

            List<Cell> neighbors = GetAdjacentCells(cell);
            foreach(Cell neighbor in neighbors)
            {
                if (neighbor.bestCost < lowestBestCost)
                {
                    lowestBestCost = neighbor.bestCost;
                    bestNeighbor = neighbor;
                    sqrDistanceToTarget = (targetPos - neighbor.worldPos).sqrMagnitude;
                }
                else if (neighbor.bestCost == lowestBestCost) 
                {
                    float newSqrDistance = (targetPos - neighbor.worldPos).sqrMagnitude;
                    if (newSqrDistance < sqrDistanceToTarget)
                    {
                        lowestBestCost = neighbor.bestCost;
                        bestNeighbor = neighbor;
                        sqrDistanceToTarget = (targetPos - neighbor.worldPos).sqrMagnitude;
                    }
                }
            }

            if (bestNeighbor != null)
            {
                cell.bestDirection = (bestNeighbor.worldPos - cell.worldPos).normalized;
            }
        }
    }
    public void CalculateFlowField(Vector3 _worldPos)
    {
        CreateIntegrationField(CellFromWorldPos(_worldPos));
        CreateFlowField(_worldPos);
    }
    public void ResetFlowField()
    {
        foreach(Cell cell in cells)
        {
            cell.bestCost = uint.MaxValue;
            cell.bestDirection = Vector3.zero;
        }
    }

    // Helpers // 
    public Cell CellFromWorldPos(Vector3 _worldPos)
    {
        Vector3 bottomLeftCellPos = flowFieldBottomLeft + Vector3.one * cellRadius;
        Vector3 bottomLeftToWorldPos = _worldPos - bottomLeftCellPos;

        int x, y, z;
        x = Mathf.Clamp(Mathf.RoundToInt(bottomLeftToWorldPos.x / cellDiameter), 0, flowFieldGridSize.x - 1);
        y = Mathf.Clamp(Mathf.RoundToInt(bottomLeftToWorldPos.y / cellDiameter), 0, flowFieldGridSize.y - 1);
        z = Mathf.Clamp(Mathf.RoundToInt(bottomLeftToWorldPos.z / cellDiameter), 0, flowFieldGridSize.z - 1);
        return cells[x, y, z];
    }
    public List<Cell> GetAdjacentCells(Cell _cellToGetNeighborsOf)
    {
        List<Cell> neighbours = new List<Cell>();

        Vector3Int oldCoords = _cellToGetNeighborsOf.coordinates;

        Vector3Int bottomLeftBack = new Vector3Int(oldCoords.x - 1, oldCoords.y - 1, oldCoords.z - 1);
        Vector3Int topRightFront = new Vector3Int(oldCoords.x + 1, oldCoords.y + 1, oldCoords.z + 1);

        for (int x = bottomLeftBack.x; x <= topRightFront.x; x++)
        {
            for (int y = bottomLeftBack.y; y <= topRightFront.y; y++)
            {
                for (int z = bottomLeftBack.z; z <= topRightFront.z; z++)
                {
                    Vector3Int neighborCoords = new Vector3Int(x, y, z);
                    if (IsCellPosWithinBounds(neighborCoords) && cells[x,y,z] != _cellToGetNeighborsOf)
                    {
                        neighbours.Add(cells[x, y, z]);
                    }
                }
            }
        }

        return neighbours;
    }
    bool IsCellPosWithinBounds(Vector3Int _cellPos)
    {
        if (_cellPos.x < 0 || _cellPos.x >= flowFieldGridSize.x)
        {
            return false;
        }
        if (_cellPos.y < 0 || _cellPos.y >= flowFieldGridSize.y)
        {
            return false;
        }
        if (_cellPos.z < 0 || _cellPos.z >= flowFieldGridSize.z)
        {
            return false;
        }
        return true;
    }
    bool IsCellTraverseable(Cell _cellToCheck)
    {
        return _cellToCheck.cost != byte.MaxValue;
    }
    public Cell GetRandomTraverseableCell(int maxAttempts = 10)
    {
        Cell cellToReturn = null;
        Vector3Int randomCellIndex = new Vector3Int();
        for(int attempt = 0; attempt < maxAttempts; attempt++)
        {
            randomCellIndex.x = Random.Range(0, flowFieldGridSize.x - 1);
            randomCellIndex.y = Random.Range(0, flowFieldGridSize.y - 1);
            randomCellIndex.z = Random.Range(0, flowFieldGridSize.z - 1);

            if (IsCellTraverseable(cells[randomCellIndex.x, randomCellIndex.y, randomCellIndex.z]))
            {
                return cells[randomCellIndex.x, randomCellIndex.y, randomCellIndex.z];
            }
        }
        return cellToReturn;
    }

    void Update()
    {
        HandleInput();
    }
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            ToggleShowBestDirectionVectors();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleShowCollisionBlocks();
        }
    }
    void ToggleShowBestDirectionVectors()
    {
        showBestDirectionVectors = !showBestDirectionVectors;
    }
    void ToggleShowCollisionBlocks()
    {
        showCollisionBlocks = !showCollisionBlocks;
    }


    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, flowFieldWorldSize);

        if(cells != null)
        {
            foreach (Cell cell in cells)
            {
                Color cellColor = Color.white;
                cellColor.a = 0.01f;

                if (showCollisionBlocks)
                {
                    if (!IsCellTraverseable(cell))
                    {
                        cellColor = Color.red;
                    }
                    else if (cell.cost > 1) // Rough Terrain
                    {
                        cellColor = Color.yellow;
                        cellColor.a = 0.5f;
                    }
                }

                if (cell == destinationCell)
                {
                    cellColor = Color.cyan;
                }

                Gizmos.color = cellColor;
                Gizmos.DrawCube(cell.worldPos, Vector3.one * (cellDiameter - 0.1f));

                if (showBestDirectionVectors)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(cell.worldPos, cell.worldPos + (cell.bestDirection * 0.1f));
                }
            }
        }
    }
}

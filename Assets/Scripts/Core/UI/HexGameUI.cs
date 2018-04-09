using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexGameUI : MonoBehaviour
{

    public HexGrid grid;
    HexCell currentCell;
    HexUnit selectedUnit;

    List<HexCell> fixedpath = new List<HexCell>();
    
    void ok()
    {       
        fixedpath.Add(grid.cells[22]);
        fixedpath.Add(grid.cells[24]);
        fixedpath.Add(grid.cells[26]);
        fixedpath.Add(grid.cells[28]);
        fixedpath.Add(grid.cells[30]);
    }

    private void Start()
    {
        ok();
    }

    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0))
            {
                DoSelection();
                if (selectedUnit)
                {
                    DoRun(fixedpath, selectedUnit);
                }          
            }
            else if (selectedUnit)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    DoMove();
                }
                else
                {
                    DoPathfinding();
                }
            }
        }
    }

    public void SetEditMode(bool toggle)
    {
        enabled = !toggle;
        grid.ShowUI(!toggle);
        grid.ClearPath();
        if (toggle)
        {
            Shader.EnableKeyword("HEX_MAP_EDIT_MODE");
        }
        else
        {
            Shader.DisableKeyword("HEX_MAP_EDIT_MODE");
        }
    }

    bool UpdateCurrentCell()
    {
        HexCell cell =
            grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
        if (cell != currentCell)
        {
            currentCell = cell;
            return true;
        }
        return false;
    }
    //选择单位
    void DoSelection()
    {
        grid.ClearPath();
        UpdateCurrentCell();
        if (currentCell)
        {
            selectedUnit = currentCell.Unit;
        }
    }

    //寻路
    void DoPathfinding()
    {
        if (UpdateCurrentCell())
        {
            if (currentCell && selectedUnit.IsValidDestination(currentCell))
            {
                grid.FindPath(selectedUnit.Location, currentCell, selectedUnit);//selectUnit决定单位速度
            }
            else
            {
                grid.ClearPath();
            }
        }
    }

    //移动
    void DoMove()
    {
        if (grid.HasPath)
        {
            selectedUnit.Travel(grid.GetPath());
            grid.ClearPath();
        }
    }

    //既定道路移动
    void DoRun(List<HexCell> fixedpath,HexUnit unit)
    {

            for (int i = 0; i < fixedpath.Count - 1; i++)
            {
                grid.FindPath(fixedpath[i], fixedpath[i+1], unit);
                DoMove();          
            }
    }

    //既定道路可视化
    public bool showGizmo = true;
    public Color gizmoColor = Color.blue;
    private void OnDrawGizmos()
    {
        if (showGizmo)
        {
            Gizmos.color = gizmoColor;

            if (fixedpath!=null)
            {
                for (int i = 0; i < fixedpath.Count-1; i++)
                {
                    Vector3 pathNodeFront = fixedpath[i].Position;
                    Vector3 pathNodeBehind = fixedpath[i+1].Position;

                    Gizmos.DrawLine(pathNodeFront, pathNodeBehind);

                }
            }
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexGameUI : MonoBehaviour
{

    public HexGrid grid;
    HexCell currentCell;
    HexUnit selectedUnit;

    public HexGridChunk aaaa;
    List<HexCell> fixedpath = new List<HexCell>();
    
    void ok()
    {
        fixedpath.Add(grid.GetCell(2));
        fixedpath.Add(grid.GetCell(10));
        fixedpath.Add(grid.GetCell(22));
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
                DoRun(fixedpath, selectedUnit);
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
        if (unit==selectedUnit)
        {
            for (int i = 0; i < fixedpath.Count; i++)
            {
                grid.FindPath(fixedpath[i], fixedpath[i+1], unit);
                DoMove();
            }
        }
    }
}
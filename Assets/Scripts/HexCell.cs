using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class HexCell : MonoBehaviour {


    int terrainTypeIndex;

    public HexCoordinates coordinates;

    public RectTransform uiRect;

    int elevation = int.MinValue;

    public HexGridChunk chunk;

    bool hasIncomingRiver, hasOutgoingRiver;//是否拥有出入口河流
    HexDirection incomingRiver, outgoingRiver;//出入口河流方向

    int waterLevel;//水位
    bool walled;//围墙

    [SerializeField]
    HexCell[] neighbors;
    [SerializeField]
    bool[] roads;
    int urbanLevel, farmLevel, plantLevel;//城市等级、农田等级、植物等级
    int specialIndex;//特殊地标

    int distance;//距离

    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(HexDirection direction,HexCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    public int Elevation
    {
        get
        {
            return elevation;
        }
        set
        {
            if (elevation == value)
            {
                return;
            }
            elevation = value;
            RefreshPosition();//刷新高度海拔
            //清除非法河流(即河流上坡)
            ValidateRivers();

            //判断高程差是否超过1，超过就取消当前道路
            for (int i = 0; i < roads.Length; i++)
            {
                if (roads[i] && GetElevationDifference((HexDirection)i) > 1)
                {
                    SetRoad(i, false);
                }
            }

            Refresh();
        }
    }

    //地形
    public int TerrainTypeIndex
    {
        get
        {
            return terrainTypeIndex;
        }
        set
        {
            if (terrainTypeIndex != value)
            {
                terrainTypeIndex = value;
                ShaderData.RefreshTerrain(this);
            }
        }
    }

    //获取边界类型
    public HexEdgeType GetEdgeType(HexDirection direction)
    {
        return HexMetrics.GetEdgeType(
            elevation, neighbors[(int)direction].elevation
        );
    }

    public HexEdgeType GetEdgeType(HexCell otherCell)
    {
        return HexMetrics.GetEdgeType(
            elevation, otherCell.elevation
        );
    }

    public Vector3 Position
    {
        get
        {
            return transform.localPosition;
        }
    }

    //刷新方法的私有化
    void Refresh()
    {
        if (chunk)
        {
            chunk.Refresh();
            for (int i = 0; i < neighbors.Length; i++)
            {
                HexCell neighbor = neighbors[i];
                if (neighbor != null && neighbor.chunk != chunk)
                {
                    neighbor.chunk.Refresh();
                }
            }
            if(Unit){
                Unit.ValidateLocation();
            }
        }
    }
    //刷新高度海拔
    void RefreshPosition()
    {
        Vector3 position = transform.localPosition;
        position.y = elevation * HexMetrics.elevationStep;
        position.y +=
            (HexMetrics.SampleNoise(position).y * 2f - 1f) *
            HexMetrics.elevationPerturbStrength;
        transform.localPosition = position;

        //坐标轴展示
        Vector3 uiPosition = uiRect.localPosition;
        uiPosition.z = -position.y;
        uiRect.localPosition = uiPosition;
    }
    #region 河流
    public bool HasIncomingRiver//是否有入口河流
    {
        get
        {
            return hasIncomingRiver;
        }
    }

    public bool HasOutgoingRiver//是否有出口河流
    {
        get
        {
            return hasOutgoingRiver;
        }
    }

    public HexDirection IncomingRiver//入口河流方向
    {
        get
        {
            return incomingRiver;
        }
    }

    public HexDirection OutgoingRiver//出口河流方向
    {
        get
        {
            return outgoingRiver;
        }
    }
    public bool HasRiver//是否有河流
    {
        get
        {
            return hasIncomingRiver || hasOutgoingRiver;
        }
    }
    public bool HasRiverBeginOrEnd//是否有河流的终点或起点
    {
        get
        {
            return hasIncomingRiver != hasOutgoingRiver;
        }
    }
    public bool HasRiverThroughEdge(HexDirection direction)//河流经过的边缘
    {
        return
            hasIncomingRiver && incomingRiver == direction ||
            hasOutgoingRiver && outgoingRiver == direction;
    }

    #region 移除河流
    public void RemoveRiver()//移除河流
    {
        RemoveOutgoingRiver();
        RemoveIncomingRiver();
    }
    //移除出口河流
    public void RemoveOutgoingRiver()
    {
        if (!hasOutgoingRiver)
        {
            return;
        }
        hasOutgoingRiver = false;
        RefreshSelfOnly();

        //移除邻居的入口河流(当前单元出口河流可能关联其某个邻居的入口河流)
        HexCell neighbor = GetNeighbor(outgoingRiver);
        neighbor.hasIncomingRiver = false;
        neighbor.RefreshSelfOnly();
    }
    //移除入口河流
    public void RemoveIncomingRiver()
    {
        if (!hasIncomingRiver)
        {
            return;
        }
        hasIncomingRiver = false;
        RefreshSelfOnly();

        HexCell neighbor = GetNeighbor(incomingRiver);
        neighbor.hasOutgoingRiver = false;
        neighbor.RefreshSelfOnly();
    }
    #endregion

    #region 添加河流
    //添加出口河流
    public void SetOutgoingRiver(HexDirection direction)
    {
        if (hasOutgoingRiver && outgoingRiver == direction)
        {
            return;
        }
        //确保有邻居且河流不能上坡
        HexCell neighbor = GetNeighbor(direction);
        if (!IsValidRiverDestination(neighbor))
        {
            return;
        }
        //移除原有出口河流，如果和入口河流重合也同样移除
        RemoveOutgoingRiver();
        if (hasIncomingRiver && incomingRiver == direction)
        {
            RemoveIncomingRiver();
        }
        //设置出口河流参数
        hasOutgoingRiver = true;
        outgoingRiver = direction;
        specialIndex = 0;

        //设置邻居的入口河流
        neighbor.RemoveIncomingRiver();
        neighbor.hasIncomingRiver = true;
        neighbor.incomingRiver = direction.Opposite();
        specialIndex = 0;
        SetRoad((int)direction, false);

    }

    //检索河床垂直位置
    public float StreamBedY
    {
        get
        {
            return
                (elevation + HexMetrics.streamBedElevationOffset) *
                HexMetrics.elevationStep;
        }
    }

    //河流表面高度
    public float RiverSurfaceY
    {
        get
        {
            return
                (elevation + HexMetrics.waterElevationOffset) *
                HexMetrics.elevationStep;
        }
    }

    void RefreshSelfOnly()//只刷新自己不影响邻居
    {
        chunk.Refresh();
        if (Unit)
        {
            Unit.ValidateLocation();
        }
    }

    //检查河流方向是否正确
    bool IsValidRiverDestination(HexCell neighbor)
    {
        return neighbor && (
            elevation >= neighbor.elevation || waterLevel == neighbor.elevation
        );
    }

    //改变时检查河流是否合法
    void ValidateRivers()
    {
        if (
            hasOutgoingRiver &&
            !IsValidRiverDestination(GetNeighbor(outgoingRiver))
        )
        {
            RemoveOutgoingRiver();
        }
        if (
            hasIncomingRiver &&
            !GetNeighbor(incomingRiver).IsValidRiverDestination(this)
        )
        {
            RemoveIncomingRiver();
        }
    }
    #endregion
    #endregion
    #region 道路
    //相应方向上是否有道路
    public bool HasRoadThroughEdge(HexDirection direction)
    {
        return roads[(int)direction];
    }
    //单元是否有道路
    public bool HasRoads
    {
        get
        {
            for (int i = 0; i < roads.Length; i++)
            {
                if (roads[i])
                {
                    return true;
                }
            }
            return false;
        }
    }

    //添加道路
    public void AddRoad(HexDirection direction)
    {
        if (!roads[(int)direction] && !HasRiverThroughEdge(direction) && 
            !IsSpecial && !GetNeighbor(direction).IsSpecial &&
            GetElevationDifference(direction) <= 1)//高度差不能超过1
        {
            SetRoad((int)direction, true);
        }
    }

    //移除道路
    public void RemoveRoads()
    {
        for (int i = 0; i < neighbors.Length; i++)
        {
            if (roads[i])
            {
                SetRoad(i, false);
            }
        }
    }

    //设置道路
    void SetRoad(int index, bool state)
    {
        roads[index] = state;
        neighbors[index].roads[(int)((HexDirection)index).Opposite()] = state;
        neighbors[index].RefreshSelfOnly();
        RefreshSelfOnly();
    }

    //判断高度差，高度差太多应该不能连接道路
    public int GetElevationDifference(HexDirection direction)
    {
        int difference = elevation - GetNeighbor(direction).elevation;
        return difference >= 0 ? difference : -difference;
    }

    public HexDirection RiverBeginOrEndDirection
    {
        get
        {
            return hasIncomingRiver ? incomingRiver : outgoingRiver;
        }
    }
    #endregion
    #region 水位

    public int WaterLevel
    {
        get
        {
            return waterLevel;
        }
        set
        {
            if (waterLevel == value)
            {
                return;
            }
            waterLevel = value;
            ValidateRivers();
            Refresh();
        }
    }

    //判断是否在水下
    public bool IsUnderwater
    {
        get
        {
            return waterLevel > elevation;
        }
    }

    //水位表面高度
    public float WaterSurfaceY
    {
        get
        {
            return
                (waterLevel + HexMetrics.waterElevationOffset) *
                HexMetrics.elevationStep;
        }
    }

    #endregion
    #region 地面特征

    //城市等级
    public int UrbanLevel
    {
        get
        {
            return urbanLevel;
        }
        set
        {
            if (urbanLevel != value)
            {
                urbanLevel = value;
                RefreshSelfOnly();
            }
        }
    }
    //农田等级
    public int FarmLevel
    {
        get
        {
            return farmLevel;
        }
        set
        {
            if (farmLevel != value)
            {
                farmLevel = value;
                RefreshSelfOnly();
            }
        }
    }
    //植物等级
    public int PlantLevel
    {
        get
        {
            return plantLevel;
        }
        set
        {
            if (plantLevel != value)
            {
                plantLevel = value;
                RefreshSelfOnly();
            }
        }
    }

    //特殊地标
    public int SpecialIndex
    {
        get
        {
            return specialIndex;
        }
        set
        {
            if (specialIndex != value && !HasRiver)
            {
                specialIndex = value;
                RemoveRoads();//不一定要与道路分离
                RefreshSelfOnly();
            }
        }
    }

    //是否有特殊地标
    public bool IsSpecial
    {
        get
        {
            return specialIndex > 0;
        }
    }
    #endregion
    #region 围墙

    public bool Walled
    {
        get { return walled; }
        set
        {
            if (walled!=value)
            {
                walled = value;
                Refresh();
            }
        }
    }

    #endregion
    #region 保存加载

    //保存所需要的内容
    public void Save(BinaryWriter writer)
    {
        writer.Write((byte)terrainTypeIndex);
        writer.Write((byte)elevation);
        writer.Write((byte)waterLevel);
        writer.Write((byte)urbanLevel);
        writer.Write((byte)farmLevel);
        writer.Write((byte)plantLevel);
        writer.Write((byte)specialIndex);
        writer.Write(walled);

        if (hasIncomingRiver)
        {
            writer.Write((byte)(incomingRiver + 128));
        }
        else
        {
            writer.Write((byte)0);
        }

        if (hasOutgoingRiver)
        {
            writer.Write((byte)(outgoingRiver + 128));
        }
        else
        {
            writer.Write((byte)0);
        }

        int roadFlags = 0;
        for (int i = 0; i < roads.Length; i++)
        {
            if (roads[i])
            {
                roadFlags |= 1 << i;
            }
        }
        writer.Write((byte)roadFlags);
    }

    //加载顺序一定要和保存顺序一致
    public void Load(BinaryReader reader)
    {
        terrainTypeIndex = reader.ReadByte();
        ShaderData.RefreshTerrain(this);
        elevation = reader.ReadByte();
        RefreshPosition();
        waterLevel = reader.ReadByte();
        urbanLevel = reader.ReadByte();
        farmLevel = reader.ReadByte();
        plantLevel = reader.ReadByte();
        specialIndex = reader.ReadByte();
        walled = reader.ReadBoolean();

        byte riverData = reader.ReadByte();
        if (riverData >= 128)
        {
            hasIncomingRiver = true;
            incomingRiver = (HexDirection)(riverData - 128);
        }
        else
        {
            hasIncomingRiver = false;
        }

        riverData = reader.ReadByte();
        if (riverData >= 128)
        {
            hasOutgoingRiver = true;
            outgoingRiver = (HexDirection)(riverData - 128);
        }
        else
        {
            hasOutgoingRiver = false;
        }

        int roadFlags = reader.ReadByte();
        for (int i = 0; i < roads.Length; i++)
        {
            roads[i] = (roadFlags & (1 << i)) != 0;
        }
    }
    #endregion
    #region 距离
    //显示距离
    public void SetLabel(string text)
    {
        UnityEngine.UI.Text label = uiRect.GetComponent<Text>();
        label.text = text;
    }
    //void UpdateDistanceLabel()
    //{
    //    Text label = uiRect.GetComponent<Text>();
    //    label.text = distance == int.MaxValue ? "" : distance.ToString();
    //}
    public int Distance
    {
        get
        {
            return distance;
        }
        set
        {
            distance = value;
           //UpdateDistanceLabel();
        }
    }
    #endregion
    #region 高亮
    public void DisableHighlight()
    {
        Image highlight = uiRect.GetChild(0).GetComponent<Image>();
        highlight.enabled = false;
    }

    public void EnableHighlight(Color color)
    {
        Image highlight = uiRect.GetChild(0).GetComponent<Image>();
        highlight.color = color;
        highlight.enabled = true;
    }

    //寻路
    public HexCell PathFrom { get; set; }
    #endregion
    #region A*寻路
    public int SearchHeuristic { get; set; }
    public int SearchPriority//搜索优先级
    {
        get
        {
            return distance + SearchHeuristic;
        }
    }
    public HexCell NextWithSamePriority { get; set; }
    public int SearchPhase { get; set; }
    #endregion
    #region 单位
    public HexUnit Unit { get; set; }
    #endregion
    #region 战争迷雾
    public HexCellShaderData ShaderData { get; set; }//渲染
    #endregion
}

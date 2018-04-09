using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexUnit : MonoBehaviour
{
    HexCell location, currentTravelLocation;//位置,移动中的当前位置
    float orientation;//转向
    public static HexUnit unitPrefab;
    const float travelSpeed = 4f;

    List<HexCell> pathToTravel;

    const float rotationSpeed = 180f;

    //视野范围
    public int VisionRange
    {
        get
        {
            return 3;
        }
    }

    void OnEnable()
    {
        if (location)
        {
            transform.localPosition = location.Position;
            if (currentTravelLocation)
            {
                Grid.IncreaseVisibility(location, VisionRange);
                Grid.DecreaseVisibility(currentTravelLocation, VisionRange);
                currentTravelLocation = null;
            }
        }
    }

    public HexCell Location
    {
        get
        {
            return location;
        }
        set
        {
            if (location)
            {
                Grid.DecreaseVisibility(location, VisionRange);
                location.Unit = null;
            }
            location = value;
            value.Unit = this;
            Grid.IncreaseVisibility(value, VisionRange);
            transform.localPosition = value.Position;
        }
    }

    public float Orientation//转向
    {
        get
        {
            return orientation;
        }
        set
        {
            orientation = value;
            transform.localRotation = Quaternion.Euler(0f, value, 0f);
        }
    }

    public void ValidateLocation()//验证位置
    {
        Grid.DecreaseVisibility(location, VisionRange);
    }

    public void Die()
    {
        location.Unit = null;
        Destroy(gameObject);
    }
    //保存
    public void Save(BinaryWriter writer)
    {
        if (location)
        {
            location.DecreaseVisibility();
        }
        location.coordinates.Save(writer);
        writer.Write(orientation);
    }
    //加载
    public static void Load(BinaryReader reader, HexGrid grid)
    {
        HexCoordinates coordinates = HexCoordinates.Load(reader);
        float orientation = reader.ReadSingle();
        grid.AddUnit(
            Instantiate(unitPrefab), grid.GetCell(coordinates), orientation
        );
    }

    //能有效到达目的地
    public bool IsValidDestination(HexCell cell)
    {
        //     禁止为探索状态单元寻路
        return cell.IsExplored && !cell.IsUnderwater && !cell.Unit;
    }

    //移动
    public void Travel(List<HexCell> path)
    {
        location.Unit = null;
        location = path[path.Count - 1];
        location.Unit = this;
        pathToTravel = path;
        StopAllCoroutines();
        StartCoroutine(TravelPath());
    }
    ///移动路径可视化
    //void OnDrawGizmos()
    //{
    //    if (pathToTravel == null || pathToTravel.Count == 0)
    //    {
    //        return;
    //    }
    //    Vector3 a, b, c = pathToTravel[0].Position;

    //    for (int i = 1; i < pathToTravel.Count; i++)
    //    {
    //        a = c;
    //        b = pathToTravel[i - 1].Position;
    //        c = (b + pathToTravel[i].Position) * 0.5f;
    //        for (float t = 0f; t < 1f; t += Time.deltaTime * travelSpeed)
    //        {
    //            Gizmos.DrawSphere(Bezier.GetPoint(a, b, c, t), 2f);
    //        }
    //    }

    //    a = c;
    //    b = pathToTravel[pathToTravel.Count - 1].Position;
    //    c = b;
    //    for (float t = 0f; t < 1f; t += 0.1f)
    //    {
    //        Gizmos.DrawSphere(Bezier.GetPoint(a, b, c, t), 2f);
    //    }
    //}

    IEnumerator TravelPath()
    {
        Vector3 a, b, c = pathToTravel[0].Position;    
        yield return LookAt(pathToTravel[1].Position);
        Grid.DecreaseVisibility(
             currentTravelLocation ? currentTravelLocation : pathToTravel[0],
             VisionRange
         );

        float t = Time.deltaTime * travelSpeed;
        for (int i = 1; i < pathToTravel.Count; i++)
        {
            currentTravelLocation = pathToTravel[i];
            a = c;
            b = pathToTravel[i - 1].Position;
            c = (b + currentTravelLocation.Position) * 0.5f;
            Grid.IncreaseVisibility(pathToTravel[i], VisionRange);
            for (; t < 1f; t += Time.deltaTime * travelSpeed)
            {
                transform.localPosition = Bezier.GetPoint(a, b, c, t);
                Vector3 d = Bezier.GetDerivative(a, b, c, t);
                d.y = 0f;
                transform.localRotation = Quaternion.LookRotation(d);
                yield return null;
            }
            Grid.DecreaseVisibility(pathToTravel[i], VisionRange);
            t -= 1f;
        }
        currentTravelLocation = null;

        a = c;
        b = location.Position;//pathToTravel[pathToTravel.Count - 1].Position;
        c = b;
        Grid.IncreaseVisibility(location, VisionRange);
        for (; t < 1f; t += Time.deltaTime * travelSpeed)
        {
            transform.localPosition = Bezier.GetPoint(a, b, c, t);
            Vector3 d = Bezier.GetDerivative(a, b, c, t);
            d.y = 0f;
            transform.localRotation = Quaternion.LookRotation(d);
            yield return null;
        }
        transform.localPosition = location.Position;
        orientation = transform.localRotation.eulerAngles.y;

        ListPool<HexCell>.Add(pathToTravel);
        pathToTravel = null;
    }

    //朝向
    IEnumerator LookAt(Vector3 point)
    {
        point.y = transform.localPosition.y;
        transform.LookAt(point);
        Quaternion fromRotation = transform.localRotation;
        Quaternion toRotation =
            Quaternion.LookRotation(point - transform.localPosition);
        float angle = Quaternion.Angle(fromRotation, toRotation);
        if (angle > 0f)
        {
            float speed = rotationSpeed / angle;

            for (
                float t = Time.deltaTime * speed;
                t < 1f;
                t += Time.deltaTime * speed
            )
            {
                transform.localRotation =
                    Quaternion.Slerp(fromRotation, toRotation, t);
                yield return null;
            }
        }
    }

    public HexGrid Grid { get; set; }

    //移动成本
    public int GetMoveCost(
    HexCell fromCell, HexCell toCell, HexDirection direction)
    {
        HexEdgeType edgeType = fromCell.GetEdgeType(toCell);
        if (edgeType == HexEdgeType.Cliff)
        {
            return -1;
        }
        int moveCost;
        if (fromCell.HasRoadThroughEdge(direction))
        {
            moveCost = 1;
        }
        else if (fromCell.Walled != toCell.Walled)
        {
            return -1;
        }
        else
        {
            moveCost = edgeType == HexEdgeType.Flat ? 5 : 10;
            moveCost +=
                toCell.UrbanLevel + toCell.FarmLevel + toCell.PlantLevel;
        }
        return moveCost;
    }

    //单位速度
    public int Speed
    {
        get
        {
            return 24;
        }
    }


}

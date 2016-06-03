/*     INFINITY CODE 2013-2015      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component that controls the buildings.
/// </summary>
[AddComponentMenu("Infinity Code/Online Maps/Plugins/Buildings")]
[Serializable]
public class OnlineMapsBuildings : MonoBehaviour
{
    /// <summary>
    /// The event, which occurs when creating of the building.
    /// </summary>
    public Action<OnlineMapsBuildingBase> OnBuildingCreated;

    /// <summary>
    /// The event, which occurs when disposing of the building.
    /// </summary>
    public Action<OnlineMapsBuildingBase> OnBuildingDispose;

    /// <summary>
    /// The event, which occurs when the new building was received.
    /// </summary>
    public Action OnNewBuildingsReceived;

    /// <summary>
    /// The event, which occurs when the request for a building sent.
    /// </summary>
    public Action OnRequestSent;

    /// <summary>
    /// The event, which occurs after the response has been received.
    /// </summary>
    public Action OnRequestComplete;

    /// <summary>
    /// GameObject, which will create the building.
    /// </summary>
    public static GameObject buildingContainer;

    private static OnlineMapsBuildings _instance;

    /// <summary>
    /// Range of zoom, in which the building will be created.
    /// </summary>
    public OnlineMapsRange zoomRange = new OnlineMapsRange(19, 20);

    /// <summary>
    /// Range levels of buildings, if the description of the building is not specified.
    /// </summary>
    public OnlineMapsRange levelsRange = new OnlineMapsRange(3, 7);

    /// <summary>
    /// Height of the building level.
    /// </summary>
    public float levelHeight = 4.5f;

    /// <summary>
    /// Minimal height of the building.
    /// </summary>
    public float minHeight = 4.5f;

    /// <summary>
    /// Scale height of the building.
    /// </summary>
    public float heightScale = 1;

    /// <summary>
    /// Materials of buildings.
    /// </summary>
    public OnlineMapsBuildingMaterial[] materials;
    
    private OnlineMapsVector2i topLeft;
    private OnlineMapsVector2i bottomRight;

    private Dictionary<string, OnlineMapsBuildingBase> buildings = new Dictionary<string, OnlineMapsBuildingBase>();
    private Dictionary<string, OnlineMapsBuildingBase> unusedBuildings = new Dictionary<string, OnlineMapsBuildingBase>();
    private List<OnlineMapsBuildingsNodeData> newBuildingsData = new List<OnlineMapsBuildingsNodeData>();

    private bool sendBuildingsReceived = false;

    private static OnlineMaps api
    {
        get { return OnlineMaps.instance; }
    }

    /// <summary>
    /// Instance of OnlineMapsBuildings.
    /// </summary>
    public static OnlineMapsBuildings instance
    {
        get { return _instance; }
    }

    private void GenerateBuildings()
    {
        long startTicks = DateTime.Now.Ticks;
        const int maxTicks = 500000;

        lock (newBuildingsData)
        {
            while (newBuildingsData.Count > 0)
            {
                OnlineMapsBuildingsNodeData data = newBuildingsData[0];
                newBuildingsData.RemoveAt(0);

                if (buildings.ContainsKey(data.way.id) || unusedBuildings.ContainsKey(data.way.id)) continue;

                OnlineMapsBuildingBase building = null;
                building = OnlineMapsBuildingBuiltIn.Create(this, data.way, data.nodes);
                if (building != null)
                {
                    building.LoadMeta(data.way);
                    if (OnBuildingCreated != null) OnBuildingCreated(building);
                    unusedBuildings.Add(data.way.id, building);
                }

                data.Dispose();

                if (DateTime.Now.Ticks - startTicks > maxTicks) break;
            }
        }

        UpdateBuildings();
    }

    private void LoadNewBuildings()
    {
        Vector2 tl = OnlineMapsUtils.TileToLatLong(topLeft, api.zoom);
        Vector2 br = OnlineMapsUtils.TileToLatLong(bottomRight, api.zoom);

        string requestData = String.Format("node({0},{1},{2},{3});way(bn)[{4}];(._;>;);out;", br.y, tl.x, tl.y, br.x, "'building'");
        OnlineMapsOSMAPIQuery.Find(requestData).OnComplete += OnBuildingRequestComplete;
        if (OnRequestSent != null) OnRequestSent();
    }

    private void OnEnable()
    {
        _instance = this;
    }

    private void OnBuildingRequestComplete(string response)
    {
        Action action = () =>
        {
            Dictionary<string, OnlineMapsOSMNode> nodes;
            List<OnlineMapsOSMWay> ways;
            List<OnlineMapsOSMRelation> relations;

            OnlineMapsOSMAPIQuery.ParseOSMResponse(response, out nodes, out ways, out relations);

            lock (newBuildingsData)
            {
                foreach (OnlineMapsOSMWay way in ways)
                {
                    newBuildingsData.Add(new OnlineMapsBuildingsNodeData(way, nodes));
                }
            }

            sendBuildingsReceived = true;
        };

#if !UNITY_WEBGL
        OnlineMapsThreadManager.AddThreadAction(action);
#else
        action();
#endif

        if (OnRequestComplete != null) OnRequestComplete();
    }

    private void Start()
    {
        buildingContainer = new GameObject("Buildings");
        buildingContainer.transform.parent = transform;
        buildingContainer.transform.localPosition = Vector3.zero;
        buildingContainer.transform.localRotation = Quaternion.Euler(Vector3.zero);

        api.OnChangePosition += UpdateBuildings;
        api.OnChangeZoom += UpdateBuildingsScale;

        UpdateBuildings();
    }

    private void RemoveAllBuildings()
    {
        foreach (KeyValuePair<string, OnlineMapsBuildingBase> building in buildings)
        {
            if (OnBuildingDispose != null) OnBuildingDispose(building.Value);
            DestroyImmediate(building.Value.gameObject);
        }

        foreach (KeyValuePair<string, OnlineMapsBuildingBase> building in unusedBuildings)
        {
            if (OnBuildingDispose != null) OnBuildingDispose(building.Value);
            DestroyImmediate(building.Value.gameObject);
        }

        buildings.Clear();
        unusedBuildings.Clear();
    }

    private void Update()
    {
        if (sendBuildingsReceived)
        {
            if (OnNewBuildingsReceived != null) OnNewBuildingsReceived();
            sendBuildingsReceived = false;
        }

        GenerateBuildings();
    }

    private void UpdateBuildings()
    {
        if (!zoomRange.InRange(api.zoom))
        {
            RemoveAllBuildings();
            return;
        }

        OnlineMapsVector2i newTopLeft = OnlineMapsUtils.LatLongToTile(api.topLeftPosition, api.zoom) - new OnlineMapsVector2i(1, 1);
        OnlineMapsVector2i newBottomRight = OnlineMapsUtils.LatLongToTile(api.bottomRightPosition, api.zoom) + new OnlineMapsVector2i(1, 1);

        if (newTopLeft != topLeft || newBottomRight != bottomRight)
        {
            topLeft = newTopLeft;
            bottomRight = newBottomRight;
            LoadNewBuildings();
        }

        UpdateBuildingsPosition();
    }

    private void UpdateBuildingsPosition()
    {
        Bounds bounds = new Bounds();
        Vector2 topLeftPosition = OnlineMaps.instance.topLeftPosition;
        Vector2 bottomRightPosition = OnlineMaps.instance.bottomRightPosition;

        bounds.min = new Vector3(topLeftPosition.x, bottomRightPosition.y);
        bounds.max = new Vector3(bottomRightPosition.x, topLeftPosition.y);

        List<string> unusedKeys = new List<string>();

        bool useElevation = OnlineMapsTileSetControl.instance.useElevation;

        foreach (KeyValuePair<string, OnlineMapsBuildingBase> building in buildings)
        {
            if (!bounds.Intersects(building.Value.boundsCoords)) unusedKeys.Add(building.Key);
            else
            {
                if (useElevation)
                {
                    Vector3 newPosition = OnlineMapsTileSetControl.instance.GetWorldPositionWithElevation(building.Value.centerCoordinates, topLeftPosition, bottomRightPosition);
                    building.Value.transform.position = newPosition;
                }
                else
                {
                    Vector3 newPosition = OnlineMapsTileSetControl.instance.GetWorldPosition(building.Value.centerCoordinates);
                    building.Value.transform.position = newPosition;
                }
            }
        }

        List<string> usedKeys = new List<string>();
        List<string> destroyKeys = new List<string>();

        double px, py;
        api.GetPosition(out px, out py);
        OnlineMapsUtils.LatLongToTiled(px, py, api.zoom, out px, out py);

        float maxDistance = Mathf.Sqrt(Mathf.Pow(api.width / 2 / OnlineMapsUtils.tileSize, 2) + Mathf.Pow(api.height / 2 / OnlineMapsUtils.tileSize, 2)) * 2;

        foreach (KeyValuePair<string, OnlineMapsBuildingBase> building in unusedBuildings)
        {
            OnlineMapsBuildingBase value = building.Value;
            if (bounds.Intersects(value.boundsCoords))
            {
                usedKeys.Add(building.Key);
                Vector3 newPosition =
                    OnlineMapsTileSetControl.instance.GetWorldPosition(value.centerCoordinates);
                value.transform.position = newPosition;
            }
            else
            {
                Vector2 buildingTilePos = OnlineMapsUtils.LatLongToTilef(value.centerCoordinates, api.zoom);
                if ((buildingTilePos - new Vector2((float)px, (float)py)).magnitude > maxDistance) destroyKeys.Add(building.Key);
            }
        }

        foreach (string key in unusedKeys)
        {
            OnlineMapsBuildingBase value = buildings[key];
            value.gameObject.SetActive(false);
            unusedBuildings.Add(key, value);
            buildings.Remove(key);
        }

        foreach (string key in usedKeys)
        {
            OnlineMapsBuildingBase value = unusedBuildings[key];
            value.gameObject.SetActive(true);
            buildings.Add(key, value);
            unusedBuildings.Remove(key);
        }

        foreach (string key in destroyKeys)
        {
            OnlineMapsBuildingBase value = unusedBuildings[key];
            if (OnBuildingDispose != null) OnBuildingDispose(value);
            DestroyImmediate(value.gameObject);
            unusedBuildings.Remove(key);
        }

        if (destroyKeys.Count > 0) OnlineMaps.instance.needGC = true;
    }

    private void UpdateBuildingsScale()
    {
        UpdateBuildings();
        foreach (KeyValuePair<string, OnlineMapsBuildingBase> building in buildings)
        {
            OnlineMapsBuildingBase value = building.Value;
            if (value.initialZoom == api.zoom) value.transform.localScale = Vector3.one;
            else if (value.initialZoom < api.zoom) value.transform.localScale = Vector3.one * (1 << api.zoom - value.initialZoom);
            else if (value.initialZoom > api.zoom) value.transform.localScale = Vector3.one / (1 << value.initialZoom - api.zoom);
        }

        foreach (KeyValuePair<string, OnlineMapsBuildingBase> building in unusedBuildings)
        {
            OnlineMapsBuildingBase value = building.Value;
            if (value.initialZoom == api.zoom) value.transform.localScale = Vector3.one;
            else if (value.initialZoom < api.zoom) value.transform.localScale = Vector3.one * (1 << api.zoom - value.initialZoom);
            else if (value.initialZoom > api.zoom) value.transform.localScale = Vector3.one / (1 << value.initialZoom - api.zoom);
        }
    }
}

public class OnlineMapsBuildingsNodeData
{
    public OnlineMapsOSMWay way;
    public Dictionary<string, OnlineMapsOSMNode> nodes;

    public OnlineMapsBuildingsNodeData(OnlineMapsOSMWay way, Dictionary<string, OnlineMapsOSMNode> nodes)
    {
        this.way = way;
        this.nodes = nodes;
    }

    public void Dispose()
    {
        way = null;
        nodes = null;
    }
}
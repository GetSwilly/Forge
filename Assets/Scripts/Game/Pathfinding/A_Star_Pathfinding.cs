using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class A_Star_Pathfinding : MonoBehaviour {

    static readonly float DEBUG_DRAW_DURATION = 1f;
    static readonly float YIELD_VALUE = 7500;


    static readonly Color BASIC_COLOR = Color.HSVToRGB(0.5f,0.5f,0.5f);
    static readonly Color WATER_COLOR = Color.blue; // Color.HSVToRGB(0.85f, 1f, 1f);
    static readonly Color ICE_COLOR = Color.cyan;// Color.HSVToRGB(0.77f,0.55f,1f);
    static readonly Color LAVA_COLOR = Color.HSVToRGB(0f, 0f, .59f);
    static readonly Color QUICKSAND_COLOR = Color.HSVToRGB(0.176f,1f,0.78f);
    static readonly Color BOG_COLOR = Color.HSVToRGB(0.26f,0.99f,0.51f);






    static readonly float HIGHEST_POINT = 50f;

    public static readonly float NODE_BUFFER_PERCENTAGE = 0.3f;
    static readonly int MAX_GIZMO_NODES = 4000000;
    static readonly int MAX_PATHFINDING_ITERATIONS = 200;
    static readonly int MAX_WALKABLE_ITERATIONS = 25;

    static readonly float MIN_VALID_HEIGHT_CLEARANCE = 3f;

    static readonly int MAX_PATHFINDING_JUMP_ITERATIONS = 500;
    int jumpCounter;

    static readonly float GRADIENT_MAX = 7f;


    [SerializeField]
    GameObject m_GroundObject;

    #region GRID variables

    [Tooltip("Time interval between adding and removing weight from node.")]
    [SerializeField]
    float nodeWeightCooldown = 5f;

    [Tooltip("Ratio of falloff of added weight on surrounding nodes")]
    [SerializeField]
    [Range(0f, 1f)]
    float nodeWeightFallOff = 0.5f;

    [Tooltip("Minimum amount of weight to add to a node")]
    [SerializeField]
    float nodeWeightMinimum = 0.2f;

   [Tooltip("LayerMask for ground layer")]
    [SerializeField]
    LayerMask groundMask;

    [Tooltip("LayerMask for environment region layer")]
    [SerializeField]
    LayerMask environmentRegionMask;

    [Tooltip("LayerMask for environment layer")]
    [SerializeField]
    LayerMask environmentMask;

    [Tooltip("Tags that make node not walkable when checking environment for obstacles")]
    [SerializeField]
    List<string> DisallowedTags;

    [Tooltip("Tags that are ignored when checking environment for obstacles")]
    [SerializeField]
    List<string> IgnoreTags;

    [Tooltip("Size of A* grid")]
    [SerializeField]
    Vector3 gridWorldSize;

    [Tooltip("Radius of each node")]
    [SerializeField]
    float nodeRadius;


  


    Node[][] nodeGrid;
    int[][] landMap;

    int gridSizeX;
    int gridSizeY;

    int totalNodes;
    float nodeDiameter;
    Vector3 gridOrigin;

    #endregion

    #region PATHFINDING variables

    public enum DISTANCE_HEURISTIC { MANHATTAN, DIAGONAL, EUCLIDEAN, DEFAULT }
    public DISTANCE_HEURISTIC distanceHeuristic = DISTANCE_HEURISTIC.DIAGONAL;

    //FPS
    private float updateinterval = 1F;
    private int frames = 0;
    private float timeleft = 1F;
    private int FPS = 60;
    private int times = 0;
    private int averageFPS = 0;

    //Queue path finding to not bottleneck it
    private List<QueuePath> queue = new List<QueuePath>();
    #endregion
    

    [Tooltip("Should detect A* grid on Start?")]
    [SerializeField]
    bool detectOnStart = false;

    /*
    [Space(15)]
    [Header("Pathfinding")]
    [Space(5)]
    */

    [Tooltip("Should check for straight path during A*?")]
    [SerializeField]
    bool shouldCheckStraightPath = true;

    [Tooltip("Shouled use Jump Point Search durign A*?")]
    [SerializeField]
    bool shouldJPS = true;

    [Tooltip("Should skip nodes with NodeType.Empty during grid detection?")]
    [SerializeField]
    bool shouldSkipEmptyChecks = false;

    [Tooltip("Should show debug information?")]
    [SerializeField]
    bool showDebug = false;

    public enum GizmoStyle { NONE, NODETYPE, NODETYPE_ALTERNATE, NODEHEIGHT, HEIGHTMAP };
    public GizmoStyle gizmoToShow;

    [SerializeField]
    Gradient elevationGradient = new Gradient();


    float overalltimer = 0;
    int iterations = 0;



    //Dictionary<UnitController, List<Node>> occupiedNodeTracker = new Dictionary<UnitController, List<Node>>();  



    //basic singleton implementation
    [HideInInspector]
    public static A_Star_Pathfinding Instance { get; private set; }
    void Awake() {
        Instance = this;
    }
    void Start()
    {
        if (detectOnStart)
            GridFromDetection();
    }

   


    #region GRID

    void GridBasics() {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.z / nodeDiameter);

        nodeGrid = new Node[gridSizeX][];
        for (int i = 0; i < nodeGrid.Length; i++) {
            nodeGrid[i] = new Node[gridSizeY];
        }

        gridOrigin = transform.position - (Vector3.right * gridWorldSize.x / 2) - (Vector3.forward * gridWorldSize.z / 2);

        totalNodes = gridSizeX * gridSizeY;
    }



    public IEnumerator GridFromIntMap(int[][] intMap) {//, int[][] objMap){

        landMap = intMap;

        gridWorldSize = new Vector3(landMap.Length, 0, landMap[0].Length);

        GridBasics();

        //float loadingProg = 0f;

        if (UIManager.Instance != null)
            UIManager.Instance.InflateLoadingScreen("Constructing A* from IntMap");


        yield return null;

        for (int x = 0; x < nodeGrid.Length; x++) {

            for (int y = 0; y < nodeGrid[0].Length; y++) {

                Vector3 worldPosition = gridOrigin + Vector3.right * ((x * nodeDiameter) + nodeRadius) + Vector3.forward * ((y * nodeDiameter) + nodeRadius);
                int ID = (x * nodeGrid.Length) + y;  //(y * gridSizeY) + x;

                int mapX = (int)((x / (float)nodeGrid.Length) * landMap.Length);
                int mapY = (int)((y / (float)nodeGrid[0].Length) * landMap[0].Length);

                switch (landMap[mapX][mapY]) {
                    case 0:
                        nodeGrid[x][y] = new Node(NodeType.Empty, worldPosition, HIGHEST_POINT, x, y, ID, 0, true);
                        break;
                    default:


                        nodeGrid[x][y] = new Node(NodeType.BasicGround, worldPosition, HIGHEST_POINT, x, y, ID, 1, true);

                        CheckNode(x, y);


                        break;

                }
            }
        }


    }

    public void GridFromDetection() {
        StartCoroutine(GridFromDetection(new Vector2(gridWorldSize.x, gridWorldSize.z)));
    }

    public IEnumerator GridFromDetection(Vector2 worldSize)
    {
        gridWorldSize = new Vector3(worldSize.x, 0, worldSize.y);

        // GridBasics();
        //
        //  yield return StartCoroutine(CheckAllNodes());


        yield return StartCoroutine(GroundCheck());
        yield return StartCoroutine(EnvironmentAreaCheck());
        yield return StartCoroutine(EnvironmentCheck());
    }

    /*
    IEnumerator CheckNodes() {
        //Fill up Map
        for (int y = 0; y < gridSizeY; y++)
        {
            if (y % 1000 == 0)
                yield return null;


            for (int x = 0; x < gridSizeX; x++)
            {
                CheckNode(x, y);
            }
        }
    }
    */
    void CheckNode(int x, int y)
    {

        if (!IsInRange(x, y))
            return;

        Vector3 worldPosition = gridOrigin + Vector3.right * ((x * nodeDiameter) + nodeRadius) + Vector3.forward * ((y * nodeDiameter) + nodeRadius);
        worldPosition.y = 0;

        int ID = (y * gridSizeY) + x;

        NodeType type = NodeType.BasicGround;
        float nodeWeight = 1;
        bool walkable = true;


        RaycastHit[] hits = Physics.SphereCastAll(new Vector3(worldPosition.x, HIGHEST_POINT, worldPosition.z), nodeRadius, Vector3.down, HIGHEST_POINT * 1.1f, environmentMask);  // Physics.RaycastAll(new Ray( new Vector3(worldPosition.x, CHECK_START_HEIGHT, worldPosition.z), -Vector3.up), CHECK_DISTANCE, movementMask);

        if (hits.Length == 0) {
            type = NodeType.Empty;
            nodeWeight = 1;
        }

        for (int i = 0; i < hits.Length; i++) {

            if (hits[i].collider.gameObject.layer == LayerMask.NameToLayer("Ground")) {
                worldPosition.y = hits[i].point.y;
            }
            else if (DisallowedTags.Contains(hits[i].transform.tag))
            {
                //type = NodeType.BasicGround;
                //nodeWeight = 1;
                walkable = false;

            }
            else if (IgnoreTags.Contains(hits[i].transform.tag))
            {
                //Do nothing we ignore these tags
            }
            else
            {

                nodeWeight = 1;

                EnvironmentArea _area = hits[i].collider.GetComponent<EnvironmentArea>();

                if (_area != null) {
                    type = _area.EnvironmentType;

                    nodeWeight = _area.MovementWeight;
                }

                if (!hits[i].collider.isTrigger) {
                    walkable = false;
                }
            }
        }


        nodeGrid[x][y] = new Node(type, worldPosition, HIGHEST_POINT, x, y, ID, nodeWeight, walkable);
    }

    IEnumerator CheckAllNodes()
    {
        Vector3 worldPosition;
        int nodeID;
        NodeType type;
        float nodeWeight;
        bool walkable;
        RaycastHit[] environmentHits;
        EnvironmentArea _area;
        
        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                if (!IsInRange(x, y))
                    break;
                
                if(FrameRateTracker.Instance != null && FrameRateTracker.Instance.IsFrameDue())
                {
                    FrameRateTracker.Instance.Reset();
                    yield return null;
                }

                worldPosition = gridOrigin + (Vector3.right * ((x * nodeDiameter) + nodeRadius)) + (Vector3.forward * ((y * nodeDiameter) + nodeRadius));
                worldPosition.y = 0;

                nodeID = (y * gridSizeY) + x;

                type = NodeType.BasicGround;
                nodeWeight = 1;
                walkable = true;




                /*
                RaycastHit groundHit;
                Physics.SphereCast(new Vector3(worldPosition.x, HIGHEST_POINT, worldPosition.z), nodeRadius, Vector3.down, out groundHit, HIGHEST_POINT * 1.1f, groundMask); 
                

                if (groundHit.collider == null)
                {
                    type = NodeType.Empty;
                    nodeWeight = 1;
                }
                else
                {
                    worldPosition.y = groundHit.point.y;
                }
                */



                //TODO - Raycast in 4 corners of node. Only make walkable if all raycasts hit
                Vector3 highPoint = new Vector3(worldPosition.x, HIGHEST_POINT, worldPosition.z);

                RaycastHit[] groundHits = new RaycastHit[4];
                bool didHitGround = true;

                didHitGround &= Physics.Raycast(highPoint + (Vector3.right * nodeRadius), Vector3.down, out groundHits[0], HIGHEST_POINT * 1.1f, groundMask);
                didHitGround &= Physics.Raycast(highPoint + (Vector3.right * -nodeRadius), Vector3.down, out groundHits[1], HIGHEST_POINT * 1.1f, groundMask);
                didHitGround &= Physics.Raycast(highPoint + (Vector3.forward * nodeRadius), Vector3.down, out groundHits[2], HIGHEST_POINT * 1.1f, groundMask);
                didHitGround &= Physics.Raycast(highPoint + (Vector3.forward * -nodeRadius), Vector3.down, out groundHits[3], HIGHEST_POINT * 1.1f, groundMask);


                if (didHitGround)
                {
                    float yVal = 0;
                    _area = null;

                    for (int i = 0; i < groundHits.Length; i++)
                    {
                        yVal += groundHits[i].point.y;

                        if (_area == null)
                        {
                            _area = groundHits[i].collider.GetComponent<EnvironmentArea>();

                            if (_area != null)
                            {
                                type = type | _area.EnvironmentType;

                                nodeWeight = _area.MovementWeight;
                            }
                        }
                    }

                    walkable = true;
                    worldPosition.y = groundHits.Length == 0 ? 0 : yVal / groundHits.Length;
                }
                else
                {
                    walkable = false;
                    type = NodeType.Empty;
                    nodeWeight = 1;
                }

                /*
                environmentHits = Physics.SphereCastAll(new Vector3(worldPosition.x, HIGHEST_POINT, worldPosition.z), nodeRadius, Vector3.down, HIGHEST_POINT * 1.1f, groundMask); 


                if(environmentHits.Length == 0)
                {
                    type = NodeType.Empty;
                    nodeWeight = 1;
                }


                for(int i = 0; i < environmentHits.Length; i++)
                {
                    if (environmentHits[i].collider.gameObject == groundObj)
                    {
                        worldPosition.y = environmentHits[i].point.y;
                    }


                    _area = environmentHits[i].collider.GetComponent<EnvironmentArea>();

                    if (_area != null)
                    {
                        type = _area.EnvironmentType;

                        nodeWeight = _area.MovementWeight;
                    }
                }

                */




                float lowestHeight = HIGHEST_POINT;
                environmentHits = Physics.SphereCastAll(worldPosition, nodeRadius, Vector3.up, HIGHEST_POINT * 1.1f, environmentMask);  // Physics.RaycastAll(new Ray( new Vector3(worldPosition.x, CHECK_START_HEIGHT, worldPosition.z), -Vector3.up), CHECK_DISTANCE, movementMask);



                for (int i = 0; i < environmentHits.Length; i++)
                {
                    _area = environmentHits[i].collider.GetComponent<EnvironmentArea>();

                    if (_area != null)
                        type |= _area.EnvironmentType;


                    if (DisallowedTags.Contains(environmentHits[i].transform.tag))
                    {
                        //type = NodeType.BasicGround;
                        //nodeWeight = 1;
                        walkable = false;

                    }
                    else if (IgnoreTags.Contains(environmentHits[i].transform.tag))
                    {
                        //Do nothing we ignore these tags
                    }
                    else
                    {

                        lowestHeight = Mathf.Min(lowestHeight, environmentHits[i].point.y);

                        nodeWeight = 1;

                       

                        if (!environmentHits[i].collider.isTrigger)
                        {
                            walkable = false;
                        }
                    }
                }

























                float heightDiff = lowestHeight - worldPosition.y;

                if (!walkable && lowestHeight > worldPosition.y && heightDiff > MIN_VALID_HEIGHT_CLEARANCE)
                    walkable = true;

                /*
                if (showDebug && lowestHeight > worldPosition.y && !lowestHeight.Equals(HIGHEST_POINT))
                {
                   // UnityEngine.Debug.Log(string.Format("Lowest height: {0}. Difference: {1}", lowestHeight, lowestHeight - groundHeight));
                    UnityEngine.Debug.DrawLine(worldPosition, new Vector3(worldPosition.x, lowestHeight, worldPosition.z), Color.magenta, 10f);
                }
                */

                nodeGrid[x][y] = new Node(type, worldPosition, lowestHeight, x, y, nodeID, nodeWeight, walkable);
            }
        }
    }


    public IEnumerator GroundCheck()
    {
        GridBasics();


        Vector3 worldPosition;
        int nodeID;
        NodeType type;
        float nodeWeight;
        bool walkable;

        int counter = 0;

        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                if (!IsInRange(x, y))
                    break;
                
                if (FrameRateTracker.Instance != null && FrameRateTracker.Instance.IsFrameDue())
                {
                    FrameRateTracker.Instance.Reset();
                    yield return null;
                }



                worldPosition = gridOrigin + (Vector3.right * ((x * nodeDiameter) + nodeRadius)) + (Vector3.forward * ((y * nodeDiameter) + nodeRadius));
                worldPosition.y = 0;

                nodeID = (y * gridSizeY) + x;


                nodeWeight = 1;
                walkable = true;



                //Raycast in 4 corners of node. Only make walkable if all raycasts hit
                Vector3 highPoint = new Vector3(worldPosition.x, HIGHEST_POINT, worldPosition.z);

                RaycastHit[] groundHits = new RaycastHit[4];
                bool didHitGround = true;

                didHitGround &= Physics.Raycast(highPoint + (Vector3.right * nodeRadius), Vector3.down, out groundHits[0], HIGHEST_POINT * 1.1f, groundMask);
                didHitGround &= Physics.Raycast(highPoint + (Vector3.right * -nodeRadius), Vector3.down, out groundHits[1], HIGHEST_POINT * 1.1f, groundMask);
                didHitGround &= Physics.Raycast(highPoint + (Vector3.forward * nodeRadius), Vector3.down, out groundHits[2], HIGHEST_POINT * 1.1f, groundMask);
                didHitGround &= Physics.Raycast(highPoint + (Vector3.forward * -nodeRadius), Vector3.down, out groundHits[3], HIGHEST_POINT * 1.1f, groundMask);


                if (didHitGround)
                {
                    float yVal = 0;

                    for (int i = 0; i < groundHits.Length; i++)
                    {
                        yVal += groundHits[i].point.y;
                    }

                    type = NodeType.BasicGround;
                    walkable = true;
                    worldPosition.y = groundHits.Length == 0 ? 0 : yVal / groundHits.Length;

                    counter++;
                }
                else
                {
                    walkable = false;
                    type = NodeType.Empty;
                    nodeWeight = 1;
                }

                nodeGrid[x][y] = new Node(type, worldPosition, HIGHEST_POINT, x, y, nodeID, nodeWeight, walkable);
            }

            float pctg = ((y + 1) * gridSizeX) / (float)(gridSizeX * gridSizeY);

            if(UIManager.Instance.IsLoadingScreenActive)
                UIManager.Instance.InflateLoadingScreen(pctg);
        }

        if (showDebug)
            UnityEngine.Debug.Log(string.Format("Ground hits : {0} / {1}. ({2} %)", counter, (gridSizeX * gridSizeY), ((float)counter / (gridSizeX * gridSizeY))));
    }

    public IEnumerator EnvironmentAreaCheck()
    {
        Vector3 worldPosition;
        RaycastHit[] environmentHits;
        EnvironmentArea area;
        NodeType type;
        float weight;

        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                if (!IsInRange(x, y))
                    break;

                if (FrameRateTracker.Instance != null && FrameRateTracker.Instance.IsFrameDue())
                {
                    FrameRateTracker.Instance.Reset();
                    yield return null;
                }


                if (shouldSkipEmptyChecks && Utilities.HasFlag(nodeGrid[x][y].Type, NodeType.Empty))
                    continue;

                worldPosition = gridOrigin + (Vector3.right * ((x * nodeDiameter) + nodeRadius)) + (Vector3.forward * ((y * nodeDiameter) + nodeRadius));
                worldPosition.y = HIGHEST_POINT;



                environmentHits = Physics.SphereCastAll(worldPosition, NodeRadius, Vector3.down, HIGHEST_POINT * 1.1f, environmentRegionMask);


                if (environmentHits.Length == 0)
                    continue;


                weight = 1;
                type = NodeType.BasicGround;

                for(int i = 0; i < environmentHits.Length; i++)
                {
                    area = environmentHits[i].collider.GetComponent<EnvironmentArea>();

                    if (area == null)
                        continue;


                    type |= area.EnvironmentType;
                    weight += area.MovementWeight;
                }


                nodeGrid[x][y].Type = type;
                nodeGrid[x][y].StaticWeight = weight;
            }

            float pctg = ((y + 1) * gridSizeX) / (float)(gridSizeX * gridSizeY);

            if (UIManager.Instance.IsLoadingScreenActive)
                UIManager.Instance.InflateLoadingScreen(pctg);
        }
    }

    public IEnumerator EnvironmentCheck()
    {
        Vector3 worldPosition;
        bool walkable;

        RaycastHit environmentHit;


        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                if (!IsInRange(x, y))
                    break;

                //if (((y * gridSizeX) + x) % YIELD_VALUE == 0)
                //    yield return null;

                if (FrameRateTracker.Instance != null && FrameRateTracker.Instance.IsFrameDue())
                {
                    FrameRateTracker.Instance.Reset();
                    yield return null;
                }



                if (shouldSkipEmptyChecks && Utilities.HasFlag(nodeGrid[x][y].Type, NodeType.Empty))
                    continue;


                worldPosition = gridOrigin + (Vector3.right * ((x * nodeDiameter) + nodeRadius)) + (Vector3.forward * ((y * nodeDiameter) + nodeRadius));
                worldPosition.y = -HIGHEST_POINT;


                walkable = true;
                float lowestHeight = HIGHEST_POINT;


                if (Physics.SphereCast(worldPosition, nodeRadius, Vector3.up, out environmentHit, HIGHEST_POINT * 2.1f, environmentMask))
                {
                    if (DisallowedTags.Contains(environmentHit.transform.tag))
                    {
                        //type = NodeType.BasicGround;
                        //nodeWeight = 1;
                        walkable = false;

                    }
                    else if (IgnoreTags.Contains(environmentHit.transform.tag))
                    {
                        //Do nothing we ignore these tags
                    }
                    else
                    {

                        lowestHeight = Mathf.Min(lowestHeight, environmentHit.point.y);

                        if (!environmentHit.collider.isTrigger)
                        {
                            walkable = false;
                        }
                    }
                }



               

              //  float heightDiff = lowestHeight - worldPosition.y;

              //  if (!walkable && lowestHeight > worldPosition.y && heightDiff > MIN_VALID_HEIGHT_CLEARANCE)
                //    walkable = true;


               // if (!Utilities.HasFlag(nodeGrid[x][y].Type, NodeType.Empty))
                    nodeGrid[x][y].Walkable = walkable;

                nodeGrid[x][y].LowestHeight = lowestHeight;
            }

            float pctg = ((y + 1) * gridSizeX) / (float)(gridSizeX * gridSizeY);

            if (UIManager.Instance.IsLoadingScreenActive)
                UIManager.Instance.InflateLoadingScreen(pctg);
        }
    }





    public void UpdateArea(Vector3 centerPoint, float radius) {
        Node centerNode = NodeFromWorldPoint(centerPoint);

        if (centerNode == null)
            return;

        int nodesToCheck = Mathf.CeilToInt(radius / nodeDiameter);

        for (int x = centerNode.X - nodesToCheck; x <= centerNode.X + nodesToCheck; x++) {
            for (int y = centerNode.Y - nodesToCheck; y <= centerNode.Y + nodesToCheck; y++) {
                CheckNode(x, y);
            }
        }
    }

    public void AddWeightToNode(Vector3 worldPos, float delta)
    {
        AddWeightToNode(worldPos, delta, false);
    }
    public void AddWeightToNode(Vector3 worldPos, float delta, bool spread)
    {
        Node centerNode = this.NodeFromWorldPoint(worldPos);
        centerNode.WeightChange(delta);

        //Spread delta to nearby nodes
        if (spread) {
            int count = 0;
            float addWeight = delta;

            //Stop when delta is too small
            while (Mathf.Abs(addWeight) >= nodeWeightMinimum) {

                addWeight = delta * Mathf.Pow(nodeWeightFallOff, count);

                for (int x = centerNode.X - count; x <= centerNode.X + count; x++) {
                    for (int y = centerNode.Y - count; y <= centerNode.Y + count; y++) {

                        //Stick to the edges
                        if ((x > centerNode.X - count && x < centerNode.X + count) && (y > centerNode.Y - count && y < centerNode.Y + count))
                            continue;

                        //Is outside of grid bounds?
                        if (IsInRange(x, y)) {
                            nodeGrid[x][y].WeightChange(addWeight);
                            RemoveWeightAfterTime(x, y, -addWeight, nodeWeightCooldown);
                        }
                    }
                }

                count++;
            }
        }
    }

    public void RemoveWeightAfterTime(int nodeX, int nodeY, float removeAmount, float delayTime) {
        StartCoroutine(DelayedWeightRemoval(nodeX, nodeY, removeAmount, delayTime));
    }
    IEnumerator DelayedWeightRemoval(int nodeX, int nodeY, float removeAmount, float delayTime) {

        yield return new WaitForSeconds(delayTime);

        if (removeAmount < 0)
            nodeGrid[nodeX][nodeY].WeightChange(removeAmount);
    }

    public Vector3 WorldPointFromIndex(int x, int y)
    {
        if (IsInRange(x, y))
            return nodeGrid[x][y].WorldPosition;


        if (nodeGrid.Length == 0 || nodeGrid[0].Length == 0)
            return Vector3.zero;



    
        int newX = Math.Max(0, x);
        newX = Math.Min(nodeGrid.Length - 1, x);

        int newY = Math.Max(0, y);
        newY = Math.Min(nodeGrid[0].Length - 1, y);

        return nodeGrid[newX][newY].WorldPosition;

    }

    public Node WalkableNodeFromWorldPoint(Vector3 worldPosition, Vector3 bounds,  NodeType _walkableNodes)
    {

        Node centerNode = NodeFromWorldPoint(worldPosition);
        if (centerNode == null)
            return null;


        Node chosenNode = centerNode;
        List<Node> neighborNodes = new List<Node>();


        int count = 0;
        bool isValid = false;



       
        while (!isValid && count < MAX_WALKABLE_ITERATIONS)
        {

            neighborNodes.Clear();

            for (int x = centerNode.X - count; x <= centerNode.X + count; x++)
            {
                for (int y = centerNode.Y - count; y <= centerNode.Y + count; y++)
                {

                    //Stick to the edges
                    if ((x > centerNode.X - count && x < centerNode.X + count) && (y > centerNode.Y - count && y < centerNode.Y + count))
                        continue;
                   

                    if (IsValidNode(x,y, bounds, _walkableNodes))
                    {
                        neighborNodes.Add(nodeGrid[x][y]);
                    }
                }
            }

            isValid = false;

            if (neighborNodes.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, neighborNodes.Count);
                chosenNode = neighborNodes[index];
                isValid = true;
            }

            count++;
        }

        if (count >= MAX_PATHFINDING_ITERATIONS)
            return centerNode;

        return chosenNode;
    }


    public List<Node> GetNodes(Vector3 worldPosition, Vector3 bounds)
    {
        Node centerNode = NodeFromWorldPoint(worldPosition);

        if (centerNode == null)
            return new List<Node>();


        Node chosenNode = centerNode;
        List<Node> neighborNodes = new List<Node>();


        int spread = Mathf.CeilToInt(Mathf.Max(bounds.x, bounds.z) / nodeDiameter) - 1;

        if (spread < 0)
            spread = 0;


        for (int x = centerNode.X - spread; x <= centerNode.X + spread; x++)
        {
            for (int y = centerNode.Y - spread; y <= centerNode.Y + spread; y++)
            {

                if (IsInRange(x, y))
                {
                    neighborNodes.Add(nodeGrid[x][y]);
                }
            }
        }


        return neighborNodes;
    }



    bool IsValidNode(Node n, Vector3 bounds, NodeType _walkableNodes)
    {
        return IsValidNode(n.X, n.Y, bounds, _walkableNodes);
    }
    bool IsValidNode(int x, int y, Vector3 bounds, NodeType _walkableNodes)
    {

        if (!IsInRange(x, y))
            return false;

        if (nodeGrid == null || nodeGrid[x][y] == null)
            return false;


        int spread = Mathf.CeilToInt(Mathf.Max(bounds.x, bounds.z) / nodeDiameter) - 1;

        if (spread < 0)
            spread = 0;



        for (int i = nodeGrid[x][y].X - spread; i <= nodeGrid[x][y].X + spread; i++)
        {
            for (int k = nodeGrid[x][y].Y - spread; k <= nodeGrid[x][y].Y + spread; k++)
            {
                if (!IsInRange(i, k) || nodeGrid[i][k] == null || !nodeGrid[i][k].IsWalkable(_walkableNodes) ||nodeGrid[i][k].HeightClearance <= bounds.y)
                { 
                    return false;
                }
            }
        }

        return true;
    }








    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x - gridOrigin.x) / gridWorldSize.x;
        float percentY = (worldPosition.z - gridOrigin.z) / gridWorldSize.z;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        //UnityEngine.Debug.Log(string.Format("Node percentage. X: {0}%. Y: {1} %", percentX, percentY));


        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        
        if(IsInRange(x, y))
            return nodeGrid[x][y];


        return null;
    }
    public Vector3 WorldPointFromWorldPoint(Vector3 worldPosition)
    {
        Node _node = NodeFromWorldPoint(worldPosition);

        return _node == null ? worldPosition : _node.WorldPosition;
    }


    //Get neighbors within grid bounds
    public List<Node> GetNeighbors(Node centerNode)
    {
        List<Node> neighbors = new List<Node>();

        for (int x = centerNode.X - 1; x <= centerNode.X + 1; x++) {
            for (int y = centerNode.Y - 1; y <= centerNode.Y + 1; y++) {

                if (x == centerNode.X && y == centerNode.Y)
                    continue;


                if (IsInRange(x, y)) {
                    neighbors.Add(nodeGrid[x][y]);
                }
            }
        }

        return neighbors;
    }

    public void ResetAllCosts() {
        for (int x = 0; x < nodeGrid.Length; x++) {
            for (int y = 0; y < nodeGrid[0].Length; y++) {
                nodeGrid[x][y].GCost = int.MaxValue;
                nodeGrid[x][y].HCost = int.MaxValue;
            }
        }

    }
    public void ResetCosts(List<Node> resetNodes) {
        for (int i = 0; i < resetNodes.Count; i++) {
            ResetCost(resetNodes[i]);
        }
    }
    public void ResetCost(Node n) {
        n.GCost = int.MaxValue;
        n.HCost = int.MaxValue;
    }

    public bool IsInRange(int x, int y) {

        if (nodeGrid == null || nodeGrid.Length == 0 || nodeGrid[0] == null)
            return false;

        if (x >= 0 && x < nodeGrid.Length && y >= 0 && y < nodeGrid[0].Length)
            return true;

        return false;
    }

    #endregion


    #region PATHFINDING

    void UpdatePathfinding()
    {

        timeleft -= Time.deltaTime;
        frames++;

        if (timeleft <= 0F)
        {
            FPS = frames;
            averageFPS += frames;
            times++;
            timeleft = updateinterval;
            frames = 0;
        }

        float timer = 0F;
        float maxtime = 1000 / FPS;
        //Bottleneck prevention
        while (queue.Count > 0 && timer < maxtime)
        {
            //UnityEngine.Debug.Log(queue.Count.ToString());
            Stopwatch sw = new Stopwatch();
            sw.Start();
            StartCoroutine(PathHandler(queue[0].startPos, queue[0].endPos, queue[0].bounds, queue[0].types, queue[0].storeRef));
            //queue[0].storeRef.Invoke(FindPath(queue[0].startPos, queue[0].endPos));
            queue.RemoveAt(0);
            sw.Stop();
            //print("Timer: " + sw.ElapsedMilliseconds);
            timer += sw.ElapsedMilliseconds;
            overalltimer += sw.ElapsedMilliseconds;
            iterations++;
        }
    }

    IEnumerator PathHandler(Vector3 startPos, Vector3 endPos, Vector3 bounds, NodeType walkableTypes, Action<List<Vector3>> listMethod)
    {
        yield return StartCoroutine(SinglePath(startPos, endPos, bounds, walkableTypes, listMethod));
    }

    IEnumerator SinglePath(Vector3 startPos, Vector3 endPos, Vector3 bounds, NodeType walkableTypes, Action<List<Vector3>> listMethod)
    {
        FindPath(startPos, endPos, bounds, walkableTypes, listMethod);
        yield return null;
    }


    public void InsertInQueue(Vector3 startPos, Vector3 endPos, Vector3 bounds, NodeType walkableTypes, Action<List<Vector3>> listMethod) {
        QueuePath q = new QueuePath(startPos, endPos, bounds, walkableTypes, listMethod);
        queue.Add(q);
    }


    public float EstimatePathDistance(Vector3 startPos, Vector3 endPos, Vector3 bounds, NodeType walkableTypes)
    {
        Node _node = TraceNodes(startPos, endPos, bounds, walkableTypes);

        return _node == null ? float.MaxValue : _node.GCost;
    }



    public void FindPath(Vector3 startPos, Vector3 endPos, Vector3 bounds, NodeType walkableTypes, Action<List<Vector3>> listMethod, Node[] bannedNodes, CustomTuple2<Node, float>[] weigthOverrideNodes, CustomTuple2<Node,float>[] weightMultipliedNodes)
    {
        throw new NotImplementedException();
    }
    public void FindPath(Vector3 startPos, Vector3 endPos, Vector3 bounds, NodeType walkableTypes, Action<List<Vector3>> listMethod)
    {

        //UnityEngine.Profiling.Profiler.BeginSample("A* -- FindPath()");


        Node startNode = WalkableNodeFromWorldPoint(startPos, bounds, walkableTypes);
        Node currentNode = TraceNodes(startPos, endPos, bounds, walkableTypes);
        List<Vector3> returnPath = new List<Vector3>();
        

        if (currentNode != null)
        {
            int count = 0;

            //Retrace path from end node to start node
            while (currentNode != null && currentNode != startNode && count < MAX_PATHFINDING_ITERATIONS)
            {
                returnPath.Add(currentNode.WorldPosition);
                currentNode = currentNode.ParentNode;

                count++;
            }

            if (count >= MAX_PATHFINDING_ITERATIONS)
            {
                listMethod.Invoke(new List<Vector3>());
                //UnityEngine.Profiling.Profiler.EndSample();
                return;
            }


            returnPath.Reverse();
        }


        if (showDebug)
        {
            for(int i = 0; i < returnPath.Count-1; i++)
            {
                UnityEngine.Debug.DrawLine(returnPath[i], returnPath[i + 1], Color.yellow, DEBUG_DRAW_DURATION);
            }
        }


        listMethod.Invoke(returnPath);

       // UnityEngine.Profiling.Profiler.EndSample();
    }

  


    Node TraceNodes(Vector3 startPos, Vector3 endPos, Vector3 bounds, NodeType walkableTypes) {

		MinHeap openHeap = new MinHeap();

        HashSet<Node> openSet = new HashSet<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        


		Node startNode = WalkableNodeFromWorldPoint(startPos, bounds, walkableTypes);
		Node endNode = WalkableNodeFromWorldPoint(endPos, bounds, walkableTypes);

		if(startNode == null || endNode == null || (endNode == startNode))
			return null;


        startNode.GCost = 0;
        startNode.HCost = GetDistance(startNode, endNode);
        startNode.ParentNode = null;
        endNode.HCost = 0;
        endNode.ParentNode = null;

      

        int spread = Mathf.CeilToInt(Mathf.Max(bounds.x, bounds.z) / nodeDiameter) - 1;

		if(spread < 0)
			spread = 0;


        

		

		


		Node currentNode = null;
		bool stillSearching = true;
        
		
		int count = 0;
		openHeap.Insert(startNode);
		openSet.Add(startNode);


		//Search for end node
		while (count <= MAX_PATHFINDING_ITERATIONS && openHeap.Size() > 0 && stillSearching) {
			
			//Choose node in openSet with smallest fCost
			Node n = openHeap.Pop();
            if (endNode.ID == n.ID)
            {
                n.ParentNode = currentNode;
                currentNode = n;
                stillSearching = false;
                break;
            }

            currentNode = n;




            if (shouldCheckStraightPath && CheckStraightPath(currentNode, endNode, bounds, walkableTypes))
            {
                endNode.GCost = GetDistance(currentNode, endNode);
                endNode.ParentNode = currentNode;

                if (showDebug)
                {
                    UnityEngine.Debug.DrawLine(currentNode.WorldPosition, endNode.WorldPosition, Color.green, DEBUG_DRAW_DURATION);
                }


                return endNode;
            }



          


            openSet.Remove(currentNode);

			if(!closedSet.Contains(currentNode))
			   closedSet.Add(currentNode);
			
			List<Node> neighbors = GetNeighbors(currentNode);





            //TODO -- Add all Nodes in neighbors and update fCosts and update minHeap

            //TODO -- Prune neighbors somehow to avoid unneccessary jumps in reverse direction. Need to continue moving on promisng path somehow.



            for(int i = 0; i < neighbors.Count; i++)
            {
                float _cost = currentNode.GCost + GetDistance(currentNode, neighbors[i]);

                neighbors[i].GCost = _cost;
                neighbors[i].HCost = GetDistance(neighbors[i], endNode);
            }



            MinHeap neighborHeap = new MinHeap();
            neighborHeap.Insert(neighbors);

			while(neighborHeap.Size() > 0)
            {
                Node curNeighbor = neighborHeap.Pop();

                if (endNode.ID == curNeighbor.ID)
                {
                    curNeighbor.ParentNode = currentNode;
                    currentNode = curNeighbor;
                    stillSearching = false;
                    break;
                }
                else if (!curNeighbor.IsWalkable(walkableTypes))
                {
                    continue;
                }
                else
                {
                    jumpCounter = 0;

                   // UnityEngine.Profiling.Profiler.BeginSample("A* ---Jump()");

                    Node jumpNode = shouldJPS ? Jump(currentNode.X, currentNode.Y, curNeighbor.X - currentNode.X, curNeighbor.Y - currentNode.Y, bounds, spread, endNode, walkableTypes) : null;

                   // UnityEngine.Profiling.Profiler.EndSample();


                    Node newNode = jumpNode; //(jumpNode == null) ? neighbors[i] : jumpNode;

                    if (newNode == null)
                    {
                        newNode = curNeighbor;


                        if (!IsValidNode(newNode, bounds, walkableTypes))
                            continue;
                    }




                    if (endNode.ID == newNode.ID)
                    {
                        newNode.ParentNode = currentNode;
                        currentNode = newNode;
                        stillSearching = false;
                        break;
                    }




                    float newMovementCostToNeighbor = currentNode.GCost + GetDistance(currentNode, newNode);
                    
                    if (newMovementCostToNeighbor < newNode.GCost || (!closedSet.Contains(newNode) && !openSet.Contains(newNode)))
                    {
                        newNode.GCost = newMovementCostToNeighbor;
                        newNode.HCost = GetDistance(newNode, endNode);
                        newNode.ParentNode = currentNode;


                        if (!openSet.Contains(newNode))
                        {
                            openSet.Add(newNode);
                            openHeap.Insert(newNode);
                        }
                    }
                }
            }

            

			count++;
			
		}
		



		if(count >= MAX_PATHFINDING_ITERATIONS)
            return null;


        return currentNode;
	}

    public bool CheckStraightPath(Node startNode, Node endNode, Vector3 bounds, NodeType _walkableNodes)
    {
        return CheckStraightPath(startNode.WorldPosition, endNode.WorldPosition, bounds, _walkableNodes);
    }
    public bool CheckStraightPath(Vector3 startPos, Vector3 endPos, Vector3 bounds, NodeType _walkableNodes)
    {
        Vector3 toVector = endPos - startPos;
        Ray castRay = new Ray(startPos, toVector);
        float currentStep = nodeDiameter;

        Node n;


        while (currentStep < toVector.magnitude)
        {
           // UnityEngine.Profiling.Profiler.BeginSample("A* --- CheckStraightPath() -- NodeFromWorldPoint()");

            n = NodeFromWorldPoint(castRay.GetPoint(currentStep));

           // UnityEngine.Profiling.Profiler.EndSample();



            if (n == null || !n.IsWalkable(_walkableNodes))
            {
                return false;
            }

            currentStep += nodeRadius;
        }
        
        return true;
    }


    Node Jump(int x, int y, int deltaX, int deltaY, Vector3 bounds, int necessarySpread, Node endNode, NodeType _walkableNodes)
    {
        if (deltaX == 0 & deltaY == 0)
            return null;


        
        jumpCounter++;

        if (jumpCounter >= MAX_PATHFINDING_JUMP_ITERATIONS)
        {
            //UnityEngine.Debug.Log("MAx jumps reached");
            return null;
        }
        

        // Position of new node we are going to consider
        int nextX = x + deltaX;
        int nextY = y + deltaY;

        if (!IsInRange(nextX, nextY) || !nodeGrid[nextX][nextY].IsWalkable(_walkableNodes))
            return null;




        // If the node is the goal return it
        if (nodeGrid[nextX][nextY].ID == endNode.ID)
        {
            if (showDebug)
            {
                UnityEngine.Debug.DrawLine(nodeGrid[x][y].WorldPosition, nodeGrid[nextX][nextY].WorldPosition, Color.green, DEBUG_DRAW_DURATION);
            }


            return nodeGrid[nextX][nextY];
        }


        if (showDebug)
        {
            //UnityEngine.Debug.Log(string.Format("A* --- Jump -- From {0} to {1}", nodeGrid[x][y].worldPosition, nodeGrid[nextX][nextY].worldPosition));
            UnityEngine.Debug.DrawLine(nodeGrid[x][y].WorldPosition, nodeGrid[nextX][nextY].WorldPosition, Color.red, DEBUG_DRAW_DURATION);
        }


        if (!IsValidNode(nextX, nextY, bounds, _walkableNodes))
            return null;


        for (int a = nextX - necessarySpread; a <= nextX + necessarySpread; a++)
        {
            for (int b = nextY - necessarySpread; b <= nextY + necessarySpread; b++)
            {
                if (!IsInRange(a, b) || !nodeGrid[a][b].IsWalkable(_walkableNodes))
                {
                    return null;
                }
            }
        }


       // UnityEngine.Profiling.Profiler.BeginSample("A* --- Jump() -- CheckStraightPath()");

        bool isStraightPath = CheckStraightPath(nodeGrid[nextX][nextY], endNode, bounds, _walkableNodes);

      //  UnityEngine.Profiling.Profiler.EndSample();




        if (isStraightPath)
            return nodeGrid[nextX][nextY];



        Node n;

        // Diagonal Movement   
        if (deltaX != 0 && deltaY != 0)
        {

            int dirX = deltaX > 0 ? 1 : -1;
            int dirY = deltaY > 0 ? 1 : -1;

            int newNextX = nextX - dirX;
            int newNextY = nextY - dirY;

            if ((IsInRange(nextX, newNextY) && !nodeGrid[nextX][newNextY].IsWalkable(_walkableNodes)) || (IsInRange(newNextX, nextY) && !nodeGrid[newNextX][nextY].IsWalkable(_walkableNodes)))
            {
                return nodeGrid[nextX][nextY];
            }

            // Check in horizontal and vertical directions for forced neighbors
            // This is a special case for diagonal direction
            //if (Jump(nextX, nextY, deltaX, 0, bounds, necessarySpread, endNode, _walkableNodes) != null || Jump(nextX, nextY, 0, deltaY, bounds, necessarySpread, endNode, _walkableNodes) != null)

           // UnityEngine.Profiling.Profiler.BeginSample("A* --- Jump() -- Jump(Diagonal)");

            n = Jump(nextX, nextY, deltaX, 0, bounds, necessarySpread, endNode, _walkableNodes) ?? Jump(nextX, nextY, 0, deltaY, bounds, necessarySpread, endNode, _walkableNodes);

           // UnityEngine.Profiling.Profiler.EndSample();



            if (n != null)
            {
                return nodeGrid[nextX][nextY];
            }
        }
        // Horizontal case
        else if (deltaX != 0)
        {
            int a = nextY + 1;
            int b = nextY - 1;

            //Obstacle above or below?
            if ((IsInRange(nextX, a) && !nodeGrid[nextX][a].IsWalkable(_walkableNodes)) || (IsInRange(nextX, b) && !nodeGrid[nextX][b].IsWalkable(_walkableNodes)))
            {
                return nodeGrid[nextX][nextY];
            }
        }
        // Vertical case
        else
        {
            int a = nextX + 1;
            int b = nextX - 1;

            //Obstacle left or right?
            if ((IsInRange(a, nextY) && !nodeGrid[a][nextY].IsWalkable(_walkableNodes)) || (IsInRange(b, nextY) && !nodeGrid[b][nextY].IsWalkable(_walkableNodes)))
            {
                return nodeGrid[nextX][nextY];
            }
        }


        // If forced neighbor was not found try next jump point

       // UnityEngine.Profiling.Profiler.BeginSample("A* --- Jump() -- Jump(Normal)");

        n =  Jump(nextX, nextY, deltaX, deltaY, bounds, necessarySpread, endNode, _walkableNodes);

       // UnityEngine.Profiling.Profiler.BeginSample("A* --- Jump() -- Jump(Normal)");

        return n;
    }

    /*
    Node FindNearestNodeType(Vector3 startPos, NodeType desiredType, Vector3 bounds, NodeType _walkableNodes)
    {

        MinHeap openHeap = new MinHeap();

        HashSet<Node> openSet = new HashSet<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();


        Node startNode = WalkableNodeFromWorldPoint(startPos, bounds, _walkableNodes);

        if (startNode == null || startNode.Type == desiredType)
            return null;


        startNode.GCost = 0;
        startNode.HCost = 0;// int.MaxValue;
        startNode.ParentNode = null;



        int spread = Mathf.CeilToInt(Mathf.Max(bounds.x, bounds.z) / nodeDiameter) - 1;
        if (spread < 0)
            spread = 0;



        Node currentNode = null;
        bool stillSearching = true;


        int count = 0;
        openHeap.Insert(startNode);
        openSet.Add(startNode);


        //Search for desired NodeType
        while (count <= MAX_PATHFINDING_ITERATIONS && openHeap.Size() > 0 && stillSearching)
        {

            //Choose node in openSet with smallest fCost
            currentNode = openHeap.Pop();
            if(currentNode.Type == desiredType)
            {
                stillSearching = false;
                break;
            }


            openSet.Remove(currentNode);

            if (!closedSet.Contains(currentNode))
                closedSet.Add(currentNode);

            List<Node> neighbors = GetNeighbors(currentNode);


            for (int i = 0; i < neighbors.Count; i++)
            {
                if (!neighbors[i].IsWalkable(_walkableNodes))
                {
                    continue;
                }
                else
                {
                    if (!IsValidNode(neighbors[i], bounds, _walkableNodes))
                        continue;
                    


                    float newMovementCostToNeighbor = currentNode.GCost + GetDistance(currentNode, neighbors[i]);


                    if (newMovementCostToNeighbor < neighbors[i].GCost || (!closedSet.Contains(neighbors[i]) && !openSet.Contains(neighbors[i])))
                    {
                        neighbors[i].GCost = newMovementCostToNeighbor;
                        neighbors[i].HCost = 0;// GetDistance(newNode, endNode);
                        neighbors[i].ParentNode = currentNode;


                        if (!openSet.Contains(neighbors[i]))
                        {
                            openSet.Add(neighbors[i]);
                            openHeap.Insert(neighbors[i]);
                        }
                    }
                }
            }

            count++;

        }




        if (count >= MAX_PATHFINDING_ITERATIONS)
            return null;


        return currentNode;
    }
    */

    int GetDistance(Node nodeA, Node nodeB) {

        if(nodeA == null || nodeB == null)
        {
            return int.MaxValue;
        }



		int dstX,dstY;

		switch(distanceHeuristic)
        {
		case DISTANCE_HEURISTIC.MANHATTAN:
			dstX = Mathf.Abs(nodeA.X - nodeB.X);
			dstY = Mathf.Abs(nodeA.Y - nodeB.Y);

			return (int)(1 * (dstX + dstY));
		case DISTANCE_HEURISTIC.DIAGONAL:
			dstX = Mathf.Abs(nodeA.X - nodeB.X);
			dstY = Mathf.Abs(nodeA.Y - nodeB.Y);

			return (int)(1 * (dstX + dstY) + (Mathf.Sqrt(2f) - 2 * 1) * Math.Min(dstX,dstY));
		case DISTANCE_HEURISTIC.EUCLIDEAN:
			dstX = Mathf.Abs(nodeA.X - nodeB.X);
			dstY = Mathf.Abs(nodeA.Y - nodeB.Y);
			
			return (int)(1 * Math.Sqrt(dstX * dstX + dstY * dstY));
		default:
			dstX = Mathf.Abs(nodeA.X - nodeB.X);
			dstY = Mathf.Abs(nodeA.Y - nodeB.Y);

			if (dstX > dstY)
				return 14*dstY + 10* (dstX-dstY);

			return 14*dstX + 10 * (dstY-dstX);
		}
		
		//return (int)(nodeA.worldPosition - nodeB.worldPosition).magnitude;
	}

    #endregion


    void Update()
    {
        UpdatePathfinding();
       // UpdateTracker();
    }


    //void UpdateTracker()
    //{
    //    Dictionary<UnitController, List<Node>>.Enumerator trackerEnumerator = occupiedNodeTracker.GetEnumerator();
    //    while (trackerEnumerator.MoveNext())
    //    {
    //        List<Node> nodeList = trackerEnumerator.Current.Value;
    //        trackerEnumerator.Current.Value.ForEach(n => n.RemoveOccupier(trackerEnumerator.Current.Key));

    //        AddOccupy(trackerEnumerator.Current.Key);
    //    }
    //}

    //public void BeginTracking(UnitController _controller)
    //{
    //    if (_controller == null)
    //        return;

    //    if (occupiedNodeTracker.ContainsKey(_controller))
    //    {
    //        StopTracking(_controller);
    //        BeginTracking(_controller);
    //    }
    //    else
    //    {
    //        occupiedNodeTracker.Add(_controller, new List<Node>());
    //    }
    //}
    //public void StopTracking(UnitController _controller)
    //{
    //    if (_controller == null)
    //        return;


    //    throw new NotImplementedException();
    //}

  
    private List<Node> GetOccupyingNodes(UnitController _controller)
    {
        if (_controller == null)
            return new List<Node>();


        Vector3 bounds = Utilities.GetMaxBounds(_controller.transform);

        return GetNodes(_controller.transform.position, bounds);
    }

    #region Getters / Setters

    public float NodeRadius
    {
        get { return nodeRadius; }
    }
    public float NodeDiameter
    {
        get { return NodeRadius * 2f; }
    }
   /* public float NodeBuffer
    {
        get { return nodeBuffer; }
    }*/
    public Vector3 WorldSize
    {
        get { return gridWorldSize; }
        set { gridWorldSize = value; }
    }


    public bool[][] LandMap
    {
        get
        {
            bool[][] _map = new bool[nodeGrid.Length][];
            for(int i = 0; i < _map.Length; i++)
            {
                _map[i] = new bool[nodeGrid[i].Length];

                for(int k = 0; k < _map[i].Length; k++)
                {
                    _map[i][k] = Utilities.HasFlag(nodeGrid[i][k].Type, NodeType.BasicGround);
                }
            }

            return _map;
        }
    }
    public float[][] HeightMap
    {
        get
        {
            float[][] _map = new float[nodeGrid.Length][];
            for (int i = 0; i < _map.Length; i++)
            {
                _map[i] = new float[nodeGrid[i].Length];

                for (int k = 0; k < _map[i].Length; k++)
                {
                    _map[i][k] = nodeGrid[i][k].WorldPosition.y;
                }
            }

            return _map;
        }
    }
    public NodeType[][] RegionMap
    {
        get
        {
            NodeType[][] _map = new NodeType[nodeGrid.Length][];
            for (int i = 0; i < _map.Length; i++)
            {
                _map[i] = new NodeType[nodeGrid[i].Length];

                for (int k = 0; k < _map[i].Length; k++)
                {
                    _map[i][k] = nodeGrid[i][k].Type;
                }
            }

            return _map;
        }
    }
    #endregion






    void OnDrawGizmos(){

		Gizmos.color = Color.white;
		Gizmos.DrawWireCube(transform.position, gridWorldSize);

        if (gizmoToShow == GizmoStyle.NONE)
            return;


		if(nodeGrid != null && totalNodes <= MAX_GIZMO_NODES){

			//int hueDelta = 360 / Enum.GetNames(typeof(NodeType)).Length;
			Vector3 hsvColor = Vector3.one;

            float thisY = transform.position.y;



			for(int i = 0; i < nodeGrid.Length; i++){
                for (int j = 0; j < nodeGrid[i].Length; j++) {

                    if (nodeGrid[i][j] == null)
                        continue;

                    switch (gizmoToShow)
                    {
                        case GizmoStyle.NODEHEIGHT:
                        case GizmoStyle.NODETYPE_ALTERNATE:
                        case GizmoStyle.NODETYPE:

                            if (nodeGrid[i][j].Type == NodeType.Empty)
                            {
                                continue;
                            }
                            else// if (nodeGrid[i][j].Walkable)
                            {

                                NodeType _type = nodeGrid[i][j].Type;

                                if (Utilities.HasFlag(_type, NodeType.BasicGround))
                                {
                                    Gizmos.color = nodeGrid[i][j].Walkable ? BASIC_COLOR : Color.magenta;
                                }

                                if (Utilities.HasFlag(_type, NodeType.Bog))
                                {
                                    Gizmos.color = BOG_COLOR;
                                }

                                if (Utilities.HasFlag(_type, NodeType.Ice))
                                {
                                    Gizmos.color = ICE_COLOR;
                                }

                                if (Utilities.HasFlag(_type, NodeType.Lava))
                                {
                                    Gizmos.color = LAVA_COLOR;
                                }

                                if (Utilities.HasFlag(_type, NodeType.Quicksand))
                                {
                                    Gizmos.color = QUICKSAND_COLOR;
                                }

                                if (Utilities.HasFlag(_type, NodeType.Water))
                                {
                                    Gizmos.color = WATER_COLOR;
                                }



                               // hsvColor.x = hueDelta * (int)nodeGrid[i][j].Type;
                               // Gizmos.color = Utilities.HSVtoRGB(hsvColor);
                            }
                            /*
                            else
                            {
                                Gizmos.color = Color.magenta;
                            }*/
                            break;
                        case GizmoStyle.HEIGHTMAP:

                            if (Utilities.HasFlag(nodeGrid[i][j].Type, NodeType.Empty))
                                continue;


                            float _key = (nodeGrid[i][j].WorldPosition.y - thisY) / GRADIENT_MAX;

                            if (_key > 1)
                                _key = 1;

                            Gizmos.color = elevationGradient.Evaluate(_key);
                            break;
                    }

                    if (gizmoToShow == GizmoStyle.NODEHEIGHT)
                    {
                        if (nodeGrid[i][j].LowestHeight.Equals(HIGHEST_POINT))
                            continue;

                        /*
                        Vector3 drawPosition = nodeGrid[i][j].WorldPosition;
                        drawPosition.y += nodeGrid[i][j].HeightClearance / 2f;

                        Vector3 drawSize = Vector3.one * (nodeDiameter - nodeBuffer);
                        drawSize.y = nodeGrid[i][j].HeightClearance;
                        */
                        Gizmos.DrawLine(nodeGrid[i][j].WorldPosition, nodeGrid[i][j].WorldPosition + (Vector3.up * nodeGrid[i][j].HeightClearance));
                        //Gizmos.DrawCube(drawPosition, drawSize);
                    }
                    else if (gizmoToShow == GizmoStyle.NODETYPE_ALTERNATE && nodeGrid[i][j].Type == NodeType.BasicGround && nodeGrid[i][j].Walkable)
                    {

                    }
                    else
                    {
                        Gizmos.DrawCube(nodeGrid[i][j].WorldPosition, Vector3.one * (nodeDiameter * (1 - NODE_BUFFER_PERCENTAGE)));
                    }
                }
			}
		}
	}  
}


public class MinHeap{
	
	List<Node> heapNodes = new List<Node>();

	/*
	public void createHeap(T[] elements) {
		if (elements.Length > 0) {
			for (int i = 0; i < elements.Length; i++) {
				Insert(elements[i]);
			}
		}
	}*/

	public void Clear(){
		heapNodes.Clear();
	}

	public void Insert(Node newVal) {

        bool shouldResetHeap = false;

        for(int i = 0; i < heapNodes.Count; i++)
        {
            if (heapNodes[i].Equals(newVal))
            {
                heapNodes[i] = newVal;
                shouldResetHeap = true;
                break;
            }
        }

        if (shouldResetHeap)
        {
            List<Node> holderHeap = heapNodes;
            heapNodes = new List<Node>();
            
            for(int i = 0; i < holderHeap.Count; i++)
            {
                Insert(holderHeap[i]);
            }

            return;
        }
		
		heapNodes.Add(newVal);
		
		int curIndex = heapNodes.Count - 1;
		int parentIndex = 0;
		
		while((parentIndex = Mathf.FloorToInt((curIndex - 1) / 2f)) >= 0 && heapNodes[curIndex].CompareTo(heapNodes[parentIndex]) <= 0){
			Swap(curIndex, parentIndex);
			
			curIndex = parentIndex;
		}
	}
    public void Insert(List<Node> newVals)
    {
        for(int i = 0; i < newVals.Count; i++)
        {
            Insert(newVals[i]);
        }
    }
	
	public Node Pop() {
	
	
		if(heapNodes.Count == 0)
			return null;

		/*
		int bestIndex = 0;

		for(int i = 1; i < heapNodes.Count; i++){
			if(heapNodes[i].fCost < heapNodes[bestIndex].fCost){
				bestIndex = i;
			}
		}

		Node bestNode = heapNodes[bestIndex];

		heapNodes.RemoveAt(bestIndex);

		return bestNode; */

		Node returnVal = Peek();
		
		heapNodes[0] = heapNodes[heapNodes.Count-1];
		heapNodes.RemoveAt(heapNodes.Count-1);
		
		int curIndex = 0;
		int leftChildIndex = (2 * curIndex) + 1;
		int rightChildIndex = (2 * curIndex) + 2;
		
		bool shouldBreak = false;
		while(!shouldBreak && (leftChildIndex = (2 * curIndex) + 1) < heapNodes.Count){
			rightChildIndex = (2 * curIndex) + 2;
			shouldBreak = true;

			//Check both left and right children
			if(rightChildIndex < heapNodes.Count){
				//Is parent greater than left and right child?
				if(heapNodes[curIndex].CompareTo(heapNodes[leftChildIndex]) >= 0 && heapNodes[curIndex].CompareTo(heapNodes[rightChildIndex]) >= 0){
					//Which child is smaller
					if(heapNodes[leftChildIndex].CompareTo(heapNodes[rightChildIndex]) <= 0){
						Swap(curIndex, leftChildIndex);
						curIndex = leftChildIndex;
						shouldBreak = false;
					}else{
						Swap(curIndex, rightChildIndex);
						curIndex = rightChildIndex;
						shouldBreak = false;
					}
				//Is parent greater than left child
				}else if(heapNodes[curIndex].CompareTo(heapNodes[leftChildIndex]) >= 0){
					Swap(curIndex, leftChildIndex);
					curIndex = leftChildIndex;
					shouldBreak = false;
				//Is parent greater than right child
				}else if(heapNodes[curIndex].CompareTo(heapNodes[rightChildIndex]) >= 0){
					Swap(curIndex, rightChildIndex);
					curIndex = rightChildIndex;
					shouldBreak = false;
				}
			//Is parent greater than left child?
			}else if(heapNodes[curIndex].CompareTo(heapNodes[leftChildIndex]) >= 0){
				Swap(curIndex, leftChildIndex);
				curIndex = leftChildIndex;
				shouldBreak = false;
			}
		}
		
		return returnVal;
	}
	
	public Node Peek(){
		
		if(heapNodes.Count == 0)
			return null;
		
		return heapNodes[0];
	}
	
	
	public void Swap(int indexA, int indexB) {
		if(indexA < 0 || indexA >= heapNodes.Count || indexB < 0 || indexB >= heapNodes.Count)
			return;
		
		Node tempNode = heapNodes[indexA];
		heapNodes[indexA] = heapNodes[indexB];
		heapNodes[indexB] = tempNode;
	}
	
	public int Size(){
		return heapNodes.Count;
	}
}
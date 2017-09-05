using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshGenerator : MonoBehaviour {

	static readonly float WALL_HEIGHT = 100f;

    public enum ColliderAdditionType { None, PolygonEdges, Polygon, MeshCollider, MeshColliderAndWall};
    /*
    [SerializeField]
    [Range(0f, 1f)]
    float meshVariationStrength = 0f;
    */

	public SquareGrid squareGrid;
	
	List<Vector3> vertices;
	List<int> triangles;
	
	Dictionary<int,List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>> ();
	List<List<int>> outlines = new List<List<int>> ();
	HashSet<int> checkedVertices = new HashSet<int>();

	[HideInInspector]
	public static MeshGenerator Instance { get; private set; }
	void Awake(){
		Instance = this;
	}
	

	public void GenerateMesh(GameObject targetObject, Material meshMaterial, int[][] map, int scaleFactor, ColliderAdditionType collAddition){

		float[][] nullFloat = new float[0][];

		GenerateMesh(targetObject, meshMaterial, ref map, scaleFactor, 0f, ref nullFloat, collAddition);
	}
    public void GenerateMesh(GameObject targetObject, Material meshMaterial, bool[][] map, int scaleFactor, ColliderAdditionType collAddition)
    {

        float[][] nullFloat = new float[0][];

        GenerateMesh(targetObject, meshMaterial, ref map, scaleFactor, 0f, ref nullFloat, collAddition);
    }
    public void GenerateMesh(GameObject targetObject, Material meshMaterial, ref int[][] intMap, int scaleFactor, float meshVariationStrength, ref float[][] heightMap, ColliderAdditionType collAddition)
    {
        bool[][] boolMap = new bool[intMap.Length][];


        for(int i = 0; i < boolMap.Length; i++)
        {
            boolMap[i] = new bool[intMap[0].Length];


            for(int k = 0; k < boolMap[i].Length; k++)
            {
                boolMap[i][k] = intMap[i][k] == 1;
            }
        }

        GenerateMesh(targetObject, meshMaterial, ref boolMap, scaleFactor, meshVariationStrength, ref heightMap, collAddition);
    }


    public void GenerateMesh(GameObject targetObject, Material meshMaterial, ref bool[][] boolMap, int scaleFactor, float meshVariationStrength, ref float[][] heightMap, ColliderAdditionType collAddition)
    {
        if (targetObject == null)
			return;



		MeshFilter _filter = targetObject.GetComponent<MeshFilter>();
		if(_filter == null)
			_filter = targetObject.AddComponent<MeshFilter>();

		MeshRenderer _renderer = targetObject.GetComponent<MeshRenderer>();
		if(_renderer == null)
			_renderer = targetObject.AddComponent<MeshRenderer>();



		triangleDictionary.Clear ();
		outlines.Clear ();
		checkedVertices.Clear ();

		if(heightMap == null || heightMap.Length == 0){
			squareGrid = new SquareGrid(ref boolMap);
		}else{
			squareGrid = new SquareGrid(ref heightMap);
		}

		vertices = new List<Vector3>();
		triangles = new List<int>();
		
		for (int x = 0; x < squareGrid.squares.Length; x ++) {
			for (int y = 0; y < squareGrid.squares[0].Length; y ++) {
				TriangulateSquare(squareGrid.squares[x][y]);
			}
		}

		ScaleMesh(scaleFactor);
        ApplyMeshVariation(scaleFactor, meshVariationStrength);
		
		Mesh mesh = new Mesh();
		_filter.mesh = mesh;
		
		mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals();
		

		Vector2[] uvs = new Vector2[vertices.Count];
		for (int i = 0; i < vertices.Count; i++) {
            float percentX = vertices[i].x / (float)boolMap.Length;   // Mathf.InverseLerp(-boolMap.GetLength(0)/2,boolMap.GetLength(0)/2,vertices[i].x) * tileAmount;
			float percentY = vertices[i].y / (float)boolMap[0].Length;  //Mathf.InverseLerp(-boolMap.GetLength(0)/2, boolMap.GetLength(0)/2,vertices[i].y) * tileAmount;
			uvs[i] = new Vector2(percentX,percentY);
		}
		mesh.uv = uvs;

		if(meshMaterial != null)
			_renderer.material = meshMaterial;


		if(collAddition == ColliderAdditionType.PolygonEdges){
			GenerateEdges(targetObject);
		}else if(collAddition == ColliderAdditionType.Polygon){
			GeneratePolygon(targetObject);
		}else if(collAddition == ColliderAdditionType.MeshCollider)
        {
			MeshCollider _collider = targetObject.GetComponent<MeshCollider> ();
			
			if(_collider == null)
				_collider = targetObject.AddComponent<MeshCollider>();

           // _collider.convex = true;
			_collider.sharedMesh = mesh;
		
		}else if (collAddition == ColliderAdditionType.MeshColliderAndWall){
			MeshCollider _collider = targetObject.GetComponent<MeshCollider> ();
			
			if(_collider == null)
				_collider = targetObject.AddComponent<MeshCollider>();

            //_collider.convex = true;
			_collider.sharedMesh = mesh;

            GenerateWallMesh(targetObject);
        }

	}


	
	void GenerateWallMesh(GameObject parentObj) {

		GameObject wallObj = new GameObject("Wall");
		wallObj.transform.parent = parentObj.transform;

		MeshFilter _filter = wallObj.GetComponent<MeshFilter>();
		if(_filter == null)
			_filter = wallObj.AddComponent<MeshFilter>();
		
		CalculateMeshOutlines ();
		
		List<Vector3> wallVertices = new List<Vector3> ();
		List<int> wallTriangles = new List<int> ();
		Mesh wallMesh = new Mesh ();

		foreach (List<int> outline in outlines) {
			for (int i = 0; i < outline.Count -1; i ++) {
				int startIndex = wallVertices.Count;
				wallVertices.Add(vertices[outline[i]]); // left
				wallVertices.Add(vertices[outline[i+1]]); // right
				wallVertices.Add(vertices[outline[i]] + Vector3.up * WALL_HEIGHT); // bottom left
				wallVertices.Add(vertices[outline[i+1]] + Vector3.up * WALL_HEIGHT); // bottom right
				
				wallTriangles.Add(startIndex + 0);
				wallTriangles.Add(startIndex + 2);
				wallTriangles.Add(startIndex + 3);
				
				wallTriangles.Add(startIndex + 3);
				wallTriangles.Add(startIndex + 1);
				wallTriangles.Add(startIndex + 0);
			}
		}
		wallMesh.vertices = wallVertices.ToArray ();
		wallMesh.triangles = wallTriangles.ToArray ();
		_filter.mesh = wallMesh;
		
		MeshCollider wallCollider = wallObj.AddComponent<MeshCollider> ();
		wallCollider.sharedMesh = wallMesh;
	} 

	void ScaleMesh(int scaleFactor){
		Vector3 myPos = transform.position;

		for(int i = 0; i < vertices.Count; i++){
			Vector3 disparityVector = vertices[i] - myPos;

            float height = vertices[i].y;   // disparityVector.y;

            disparityVector *= scaleFactor;
			disparityVector.y = height;


            //Vector2 variationVector = UnityEngine.Random.insideUnitCircle * scaleFactor * meshVariationStrength;
            //Vector3 vertexVariation = new Vector3(variationVector.x, 0, variationVector.y);

            vertices[i] = myPos + disparityVector;  // + vertexVariation;
		}
	}
    void ApplyMeshVariation(int scaleFactor, float variationStrength)
    {
        variationStrength = Mathf.Clamp01(variationStrength) * 0.5f;


        for (int i = 0; i < vertices.Count; i++)
        {
            Vector2 variationVector = Random.insideUnitCircle * scaleFactor * variationStrength;
            vertices[i] += new Vector3(variationVector.x, 0, variationVector.y);
        }
        
    }





    void DestroyEdges(GameObject _obj){

		EdgeCollider2D[] currentColliders = _obj.GetComponents<EdgeCollider2D> ();
		for (int i = 0; i < currentColliders.Length; i++) {
			Destroy(currentColliders[i]);
		}

		MeshFilter _filter = _obj.GetComponent<MeshFilter>();
		if(_filter == null)
			_filter = _obj.AddComponent<MeshFilter>();

		_filter.mesh = null;
	}
	
	void GenerateEdges(GameObject _obj) {

		DestroyEdges(_obj);
		
		CalculateMeshOutlines ();
		
		foreach (List<int> outline in outlines) {
			EdgeCollider2D edgeCollider = _obj.AddComponent<EdgeCollider2D>();
			Vector2[] edgePoints = new Vector2[outline.Count];
			
			for (int i = 0; i < outline.Count; i ++) {
				edgePoints[i] = new Vector2(vertices[outline[i]].x,vertices[outline[i]].y);
			}
			edgeCollider.points = edgePoints;
		}
		
	}


	
	void GeneratePolygon(GameObject _obj) {
		
		CalculateMeshOutlines ();

		PolygonCollider2D polyColl = _obj.AddComponent<PolygonCollider2D>();
		polyColl.pathCount = outlines.Count;


		for(int a = 0; a < outlines.Count; a++){
			Vector2[] edgePoints = new Vector2[outlines[a].Count];
			
			for (int i = 0; i < outlines[a].Count; i ++) {
				edgePoints[i] = new Vector2(vertices[outlines[a][i]].x,vertices[outlines[a][i]].y);
			}

			polyColl.SetPath(a, edgePoints);
		}
		
	}


	/*
	void VertexDecimation(int vertexIndex){
		List<Triangle> connectedTris = triangleDictionary[vertexIndex];

	}


	void TriangulateMesh(){
		bool[][] squareFlags = new bool[squareGrid.squares.Length][];
		for(int i = 0; i < squareFlags.Length; i++){
			squareFlags[i] = new bool[squareGrid.squares[0].Length];
		}

		Node nodeA,nodeB,nodeC,nodeD;
		int indexA,indexB;

		for(int p = 0; p < squareGrid.squares.Length; p++){
			for(int q = 0; q < squareGrid.squares[0].Length; q++){

				if(!squareFlags[p][q])
					continue;

				Square square = squareGrid.squares[p][q];
				switch (square.configuration) {
				case 0:
					break;
					
					// 1 points:
				case 1:

					squareFlags[p][q] = true;

					nodeA = square.centreLeft;
					nodeB = square.centreBottom;
					nodeC = square.bottomLeft;

					indexA = p;
					indexB = q;

					while((indexA-1) >= 0 && (indexB+1) < squareGrid.squares[0].Length && squareGrid.squares[indexA+1][indexB-1].configuration == square.configuration && !squareFlags[indexA-1][indexB+1] &&){
						indexA = indexA - 1;
						indexB = indexB + 1;

						nodeA = squareGrid.squares[indexA][indexB].centreLeft;
						nodeB = squareGrid.squares[indexA][indexB].centreBottom;
						squareFlags[indexA][indexB] = true;
					}

					indexA = p;
					indexB = q;

					while((indexA+1) < squareGrid.squares.Length && (indexB-1) >= 0 && squareGrid.squares[indexA+1][indexB-1].configuration == square.configuration && !squareFlags[indexA+1][indexB-1]){
						indexA = indexA + 1;
						indexB = indexB - 1;
						nodeC = squareGrid.squares[indexA][indexB].bottomRight;
						squareFlags[indexA][indexB] = true;
					}

					MeshFromPoints(nodeA, nodeB, nodeC);
					break;
				case 2:
					squareFlags[p][q] = true;
					
					nodeA = square.bottomRight;
					nodeB = square.centreBottom;
					nodeC = square.centreRight;
					
					indexA = p;
					indexB = q;
					
					while((indexA-1) >= 0 && (indexB-1) >= 0 && squareGrid.squares[indexA-1][indexB-1].configuration == square.configuration && !squareFlags[indexA-1][indexB-1]){
						indexA = indexA - 1;
						indexB = indexB - 1;
						
						nodeA = squareGrid.squares[indexA][indexB].bottomRight;
						nodeB = squareGrid.squares[indexA][indexB].centreBottom;
						squareFlags[indexA][indexB] = true;
					}
					
					indexA = p;
					indexB = q;
					
					while((indexA+1) < squareGrid.squares.Length && (indexB+1) < squareGrid.squares[0].Length && squareGrid.squares[indexA+1][indexB+1].configuration == square.configuration && !squareFlags[indexA+1][indexB+1]){
						indexA = indexA + 1;
						indexB = indexB + 1;
						nodeC = squareGrid.squares[indexA][indexB].centreRight;
						squareFlags[indexA][indexB] = true;
					}
					
					MeshFromPoints(nodeA, nodeB, nodeC);

					//MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
					break;
				case 4:

					squareFlags[p][q] = true;
					
					nodeA = square.topRight;
					nodeB = square.centreRight;
					nodeC = square.centreTop;
					
					indexA = p;
					indexB = q;
					
					while((indexA-1) >= 0 && (indexB+1) < squareGrid.squares[0].Length && squareGrid.squares[indexA+1][indexB-1].configuration == square.configuration && !squareFlags[indexA-1][indexB+1]){
						indexA = indexA - 1;
						indexB = indexB + 1;
						
						nodeA = squareGrid.squares[indexA][indexB].centreLeft;
						nodeB = squareGrid.squares[indexA][indexB].centreBottom;
						squareFlags[indexA][indexB] = true;
					}
					
					indexA = p;
					indexB = q;
					
					while((indexA+1) < squareGrid.squares.Length && (indexB-1) >= 0 && squareGrid.squares[indexA+1][indexB-1].configuration == square.configuration && !squareFlags[indexA+1][indexB-1]){
						indexA = indexA + 1;
						indexB = indexB - 1;
						nodeC = squareGrid.squares[indexA][indexB].bottomRight;
						squareFlags[indexA][indexB] = true;
					}
					
					MeshFromPoints(nodeA, nodeB, nodeC);
					//MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
					break;
				case 8:
					MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
					break;
					
					// 2 points:
				case 3:
					MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
					break;
				case 6:
					MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
					break;
				case 9:
					MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
					break;
				case 12:
					MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
					break;
				case 5:
					MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
					break;
				case 10:
					MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
					break;
					
					// 3 point:
				case 7:
					MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
					break;
				case 11:
					MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
					break;
				case 13:
					MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
					break;
				case 14:
					MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
					break;
					
					// 4 point:
				case 15:
					MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
					checkedVertices.Add(square.topLeft.vertexIndex);
					checkedVertices.Add(square.topRight.vertexIndex);
					checkedVertices.Add(square.bottomRight.vertexIndex);
					checkedVertices.Add(square.bottomLeft.vertexIndex);
					break;
				}
			}
		}
	}
	*/


	void TriangulateSquare(Square square) {
		switch (square.configuration) {
		case 0:
			break;
			
			// 1 points:
		case 1:
			MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft);
			break;
		case 2:
			MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
			break;
		case 4:
			MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
			break;
		case 8:
			MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
			break;
			
			// 2 points:
		case 3:
			MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
			break;
		case 6:
			MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
			break;
		case 9:
			MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
			break;
		case 12:
			MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
			break;
		case 5:
			MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
			break;
		case 10:
			MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
			break;
			
			// 3 point:
		case 7:
			MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
			break;
		case 11:
			MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
			break;
		case 13:
			MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
			break;
		case 14:
			MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
			break;
			
			// 4 point:
		case 15:
			MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
			//checkedVertices.Add(square.topLeft.vertexIndex);
			//checkedVertices.Add(square.topRight.vertexIndex);
			//checkedVertices.Add(square.bottomRight.vertexIndex);
			//checkedVertices.Add(square.bottomLeft.vertexIndex);
			break;
		}
		
	}
	
    /*
	void MeshFromPoints(params Node[] points) {
		AssignVertices(points);
		
		if (points.Length >= 3)
			CreateTriangle(points[2], points[1], points[0]);
		if (points.Length >= 4)
			CreateTriangle(points[2], points[2], points[0]);
		if (points.Length >= 5) 
			CreateTriangle(points[4], points[3], points[0]);
		if (points.Length >= 6)
			CreateTriangle(points[5], points[4], points[0]);
		
	}
    */
    void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3)
            CreateTriangle(points[0], points[1], points[2]);
        if (points.Length >= 4)
            CreateTriangle(points[0], points[2], points[3]);
        if (points.Length >= 5)
            CreateTriangle(points[0], points[3], points[4]);
        if (points.Length >= 6)
            CreateTriangle(points[0], points[4], points[5]);

    }


    void AssignVertices(Node[] points) {
		for (int i = 0; i < points.Length; i ++) {
			if (points[i].vertexIndex == -1) {
				points[i].vertexIndex = vertices.Count;
				vertices.Add(points[i].position);
			}
		}
	}
	
	void CreateTriangle(Node a, Node b, Node c) {
		triangles.Add(a.vertexIndex);
		triangles.Add(b.vertexIndex);
		triangles.Add(c.vertexIndex);

        Triangle triangle = new Triangle (a.vertexIndex, b.vertexIndex, c.vertexIndex);
        AddTriangleToDictionary (triangle.vertexIndexA, triangle);
		AddTriangleToDictionary (triangle.vertexIndexB, triangle);
		AddTriangleToDictionary (triangle.vertexIndexC, triangle);
	}
	
	void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle) {
		if (triangleDictionary.ContainsKey (vertexIndexKey)) {
			triangleDictionary [vertexIndexKey].Add (triangle);
		} else {
			List<Triangle> triangleList = new List<Triangle>();
			triangleList.Add(triangle);
			triangleDictionary.Add(vertexIndexKey, triangleList);
		}
	}
	
	void CalculateMeshOutlines() {
		
		for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex ++) {
			if (!checkedVertices.Contains(vertexIndex)) {
				int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
				if (newOutlineVertex != -1) {
					checkedVertices.Add(vertexIndex);
					
					List<int> newOutline = new List<int>();
					newOutline.Add(vertexIndex);
					outlines.Add(newOutline);
					FollowOutline(newOutlineVertex, outlines.Count-1);
					outlines[outlines.Count-1].Add(vertexIndex);
				}
			}
		}
	}
	
	void FollowOutline(int vertexIndex, int outlineIndex) {
		outlines [outlineIndex].Add (vertexIndex);
		checkedVertices.Add (vertexIndex);
		int nextVertexIndex = GetConnectedOutlineVertex (vertexIndex);
		
		if (nextVertexIndex != -1) {
			FollowOutline(nextVertexIndex, outlineIndex);
		}
	}
	
	int GetConnectedOutlineVertex(int vertexIndex) {
		List<Triangle> trianglesContainingVertex = triangleDictionary [vertexIndex];
		
		for (int i = 0; i < trianglesContainingVertex.Count; i ++) {
			Triangle triangle = trianglesContainingVertex[i];
			
			for (int j = 0; j < 3; j ++) {
				int vertexB = triangle[j];
				if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB)) {
					if (IsOutlineEdge(vertexIndex, vertexB)) {
						return vertexB;
					}
				}
			}
		}
		
		return -1;
	}
	
	bool IsOutlineEdge(int vertexA, int vertexB) {
		List<Triangle> trianglesContainingVertexA = triangleDictionary [vertexA];
		int sharedTriangleCount = 0;
		
		for (int i = 0; i < trianglesContainingVertexA.Count; i ++) {
			if (trianglesContainingVertexA[i].Contains(vertexB)) {
				sharedTriangleCount ++;
				if (sharedTriangleCount > 1) {
					break;
				}
			}
		}
		return sharedTriangleCount == 1;
	}
	
	struct Triangle {
		public int vertexIndexA;
		public int vertexIndexB;
		public int vertexIndexC;
		int[] vertices;
		
		public Triangle (int a, int b, int c) {
			vertexIndexA = a;
			vertexIndexB = b;
			vertexIndexC = c;
			
			vertices = new int[3];
			vertices[0] = a;
			vertices[1] = b;
			vertices[2] = c;
		}
		
		public int this[int i] {
			get {
				return vertices[i];
			}
		}
		
		
		public bool Contains(int vertexIndex) {
			return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
		}
	}
	
	public class SquareGrid {
		public Square[][] squares;

        public SquareGrid(ref int[][] intMap) {
            bool[][] boolMap = new bool[intMap.Length][];


            for (int i = 0; i < boolMap.Length; i++)
            {
                boolMap[i] = new bool[intMap[0].Length];


                for (int k = 0; k < boolMap[i].Length; k++)
                {
                    boolMap[i][k] = intMap[i][k] == 1;
                }
            }

            Initialize(ref boolMap);
        }

        public SquareGrid(ref bool[][] boolMap) {
            Initialize(ref boolMap);
        }

        void Initialize(ref bool[][] boolMap) { 
			int nodeCountX = boolMap.Length;
			int nodeCountY = boolMap[0].Length;
			float mapWidth = nodeCountX ;
			float mapHeight = nodeCountY;
			
			ControlNode[][] controlNodes = new ControlNode[nodeCountX][];
			for(int i = 0; i < controlNodes.Length; i++){
				controlNodes[i] = new ControlNode[nodeCountY];
			}
			
			for (int x = 0; x < nodeCountX; x ++) {
				for (int y = 0; y < nodeCountY; y ++) {
					Vector3 pos = new Vector3(-mapWidth/2 + x + 0.5f, 0, -mapHeight/2 + y + 0.5f);
					controlNodes[x][y] = new ControlNode(pos, boolMap[x][y], 1);
				}
			}
			
			squares = new Square[nodeCountX -1][];
			for(int i = 0; i < squares.Length; i++){
				squares[i] = new Square[nodeCountY-1];
			}
			
			for (int x = 0; x < nodeCountX-1; x ++) {
				for (int y = 0; y < nodeCountY-1; y ++) {
					squares[x][y] = new Square(controlNodes[x][y+1], controlNodes[x+1][y+1], controlNodes[x+1][y], controlNodes[x][y]);
				}
			}
			
		}

		public SquareGrid(ref float[][] heightMap) {
			int nodeCountX = heightMap.Length;
			int nodeCountY = heightMap[0].Length;
			float mapWidth = nodeCountX ;
			float mapHeight = nodeCountY;
			
			ControlNode[][] controlNodes = new ControlNode[nodeCountX][];
			for(int i = 0; i < controlNodes.Length; i++){
				controlNodes[i] = new ControlNode[nodeCountY];
			}
			
			for (int x = 0; x < nodeCountX; x ++) {
				for (int y = 0; y < nodeCountY; y ++) {
					Vector3 pos = new Vector3(-mapWidth/2 + x + 0.5f, 0, -mapHeight/2 + y + 0.5f);
                    pos.y = heightMap[x][y];// (heightMap[x][y] - threshold) * heightMultiplier;
					controlNodes[x][y] = new ControlNode(pos,heightMap[x][y] > 0, 1);
				}
			}
			
			squares = new Square[nodeCountX -1][];
			for(int i = 0; i < squares.Length; i++){
				squares[i] = new Square[nodeCountY-1];
			}

			for (int x = 0; x < nodeCountX-1; x ++) {
				for (int y = 0; y < nodeCountY-1; y ++) {
					squares[x][y] = new Square(controlNodes[x][y+1], controlNodes[x+1][y+1], controlNodes[x+1][y], controlNodes[x][y]);
				}
			}
			
		}
	}
	
	public class Square {
		
		public ControlNode topLeft, topRight, bottomRight, bottomLeft;
		public Node centreTop, centreRight, centreBottom, centreLeft;
		public int configuration;
		
		public Square (ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft) {
			topLeft = _topLeft;
			topRight = _topRight;
			bottomRight = _bottomRight;
			bottomLeft = _bottomLeft;
			
			centreTop = topLeft.right;
			centreRight = bottomRight.above;
			centreBottom = bottomLeft.right;
			centreLeft = bottomLeft.above;
			
			if (topLeft.active)
				configuration += 8;
			if (topRight.active)
				configuration += 4;
			if (bottomRight.active)
				configuration += 2;
			if (bottomLeft.active)
				configuration += 1;
		}
		
	}
	
	public class Node {
		public Vector3 position;
		public int vertexIndex = -1;
		
		public Node(Vector3 _pos) {
			position = _pos;
		}
	}
	
	public class ControlNode : Node {
		
		public bool active;
		public Node above, right;
		
		public ControlNode(Vector3 _pos, bool _active, float squareSize) : base(_pos) {
			active = _active;
			above = new Node(position + Vector3.forward * squareSize/2f);
			right = new Node(position + Vector3.right * squareSize/2f);
		}
		
	}
}
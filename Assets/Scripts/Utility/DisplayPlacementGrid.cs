using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayPlacementGrid : MonoBehaviour {

    LevelController.PlacementGrid _grid;


    void Start()
    {
        //_grid = LevelController.Instance.CalculatePlacementGrid(this.gameObject);
    }


    void OnDrawGizmos()
    {
        _grid = LevelController.Instance.CalculatePlacementGrid(this.gameObject);


        Gizmos.color = Color.cyan;

        float nodeSize = (A_Star_Pathfinding.Instance.NodeRadius * 2f) * (1 - A_Star_Pathfinding.NODE_BUFFER_PERCENTAGE);

        for(int i = 0; i < _grid.occupyMask.Length; i++)
        {
            for (int k = 0; k < _grid.occupyMask[i].Length; k++)
            {
                Gizmos.color = _grid.occupyMask[i][k] ? Color.cyan : Color.magenta;

                Vector3 horizontalOffset = Vector3.right * ((-_grid.occupyMask.Length * A_Star_Pathfinding.Instance.NodeRadius * .5f) + (i * A_Star_Pathfinding.Instance.NodeRadius));// (A_Star_Pathfinding.Instance.NodeRadius / 2f)));
                Vector3 forwardOffset = Vector3.forward * ((-_grid.occupyMask[i].Length * A_Star_Pathfinding.Instance.NodeRadius * .5f) + (k * A_Star_Pathfinding.Instance.NodeRadius));// + (A_Star_Pathfinding.Instance.NodeRadius / 2f)));
                //Vector3 checkOrigin = transform.position + horizontalOffset + forwardOffset;


                Gizmos.DrawCube(transform.position + horizontalOffset + forwardOffset, Vector3.one * nodeSize);

            }
        }
    }
}

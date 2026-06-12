using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using Tetrex.DataStructures;
using UnityEngine;

namespace Tetrex.Blocks
{
    public class BlockSpawnDrop : NormalBlock
    {
        override public BlockAction[] Activate(NormalBlock[,] blockGrid)
        {
            Vector2Int myPos = GetPosInGrid(blockGrid);

            // Positions that are valid, default to Vector2Int.left (-1, 0)
            Vector2Int[] emptySpots = new Vector2Int[3] { Vector2Int.left, Vector2Int.left, Vector2Int.left };

            // For every neighboring column including this one
            for (int x = myPos.x - 1; x <= myPos.x + 1; x++)
            {
                // If column is in the grid
                if (IsPosInsideGrid(blockGrid, new Vector2Int(x, myPos.y)))
                {
                    bool isInsideGround = false;
                    // Go through even block from this block's Y to bottom
                    for (int y = myPos.y; y >= 0; y--)
                    {
                        // If block is empty
                        if (blockGrid[x, y] == null)
                        {
                            // If encountered non-empty block before
                            if (isInsideGround)
                            {
                                // Add this spot to empty spots
                                emptySpots[x - myPos.x + 1] = new Vector2Int(x, y);
                                break;
                            }
                        }
                        // If block is not empty
                        else isInsideGround = true;
                    }
                }
            }

            // Create the actions list
            int emptySpotsAmount = 0;
            int[] esIndices = new int[3];
            // For every spot in emptySpots, if spot is empty, increase amount of empty spots variable
            for (int i = 0; i < 3; i++)
            {
                if (emptySpots[i].x != -1)
                {
                    esIndices[emptySpotsAmount] = i;
                    emptySpotsAmount++;
                }
            }

            BlockAction[] actions = new BlockAction[emptySpotsAmount + 2];

            actions[0] = PackageBlockRemoval(this, true);
            actions[1] = PackageBlockSpawn(0, myPos);
            for (int i = 0; i < emptySpotsAmount; i++) actions[i + 2] = PackageBlockSpawn(0, emptySpots[esIndices[i]]);

            return actions;
        }

        private bool IsPosInsideGrid(object[,] grid, Vector2Int position)
        {
            return position.x >= 0 && position.y >= 0 && position.x < grid.GetLength(0) && position.y < grid.GetLength(1);
        }
    }
}

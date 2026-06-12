using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using Tetrex.DataStructures;
using UnityEngine;

namespace Tetrex.Blocks
{
    public class BlockSpawnFill : NormalBlock
    {
        override public BlockAction[] Activate(NormalBlock[,] blockGrid)
        {
            Vector2Int myPos = GetPosInGrid(blockGrid);
            BlockAction[] actions = new BlockAction[7];

            int blocksToFill = 5;
            int curRow = 0;
            int blocksFilled = 0;

            do
            {
                //int[] emptySpotsOnRow = new int[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                //int esAmount = 0;
                List<int> emptySpotsOnRow = new();

                for (int x = 0; x < 10; x++)
                {
                    if (blockGrid[x, curRow] == null)
                    {
                        emptySpotsOnRow.Add(x);
                    }
                }

                if (emptySpotsOnRow.Count >= blocksToFill)
                {
                    for (int i = 0; i < blocksToFill; i++)
                    {
                        int randInd = Random.Range(0, emptySpotsOnRow.Count);
                        Vector2Int ps = new Vector2Int(emptySpotsOnRow[randInd], curRow);
                        actions[blocksFilled] = PackageBlockSpawn(0, ps);
                        emptySpotsOnRow.RemoveAt(randInd);
                        blocksFilled++;
                    }

                    blocksToFill = 0;
                }
                else
                {
                    for (int i = 0; i < emptySpotsOnRow.Count; i++)
                    {
                        actions[blocksFilled] = PackageBlockSpawn(0, new Vector2Int(emptySpotsOnRow[i], curRow));
                        blocksFilled++;
                        blocksToFill--;
                    }
                }

                curRow++;
            } while (blocksToFill > 0);

            actions[5] = PackageBlockRemoval(myPos, true);
            actions[6] = PackageBlockSpawn(0, myPos);

            return actions;
        }

        private bool IsPosInsideGrid(object[,] grid, Vector2Int position)
        {
            return position.x >= 0 && position.y >= 0 && position.x < grid.GetLength(0) && position.y < grid.GetLength(1);
        }
    }
}

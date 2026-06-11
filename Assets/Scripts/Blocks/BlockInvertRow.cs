using System.Collections;
using System.Collections.Generic;
using Tetrex.DataStructures;
using UnityEngine;

namespace Tetrex.Blocks
{
    public class BlockInvertRow : NormalBlock
    {
        override public BlockAction[] Activate(NormalBlock[,] blockGrid)
        {
            Vector2Int myPos = GetPosInGrid(blockGrid);
            BlockAction[] actions = new BlockAction[blockGrid.GetLength(0) + 1];

            for (int x = 0; x < blockGrid.GetLength(0); x++)
            {
                if (blockGrid[x, myPos.y] == null)
                {
                    actions[x] = PackageBlockSpawn(0, new Vector2Int(x, myPos.y));
                }
                else
                {
                    actions[x] = PackageBlockRemoval(new Vector2Int(x, myPos.y));
                }
            }

            actions[blockGrid.GetLength(0)] = PackageBlockSpawn(0, myPos);

            return actions;
        }
    }
}

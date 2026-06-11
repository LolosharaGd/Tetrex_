using System.Collections;
using System.Collections.Generic;
using Tetrex.DataStructures;
using UnityEngine;

namespace Tetrex.Blocks
{
    public class BlockSpawnDashRight : NormalBlock
    {
        override public BlockAction[] Activate(NormalBlock[,] blockGrid)
        {
            Vector2Int myPos = GetPosInGrid(blockGrid);
            BlockAction[] actions = new BlockAction[blockGrid.GetLength(0) - myPos.x + 1];

            for (int x = myPos.x; x < blockGrid.GetLength(0); x++)
            {
                actions[x - myPos.x + 1] = PackageBlockSpawn(0, new Vector2Int(x, myPos.y));
            }

            actions[0] = PackageBlockRemoval(this);

            return actions;
        }
    }
}

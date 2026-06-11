using System.Collections;
using System.Collections.Generic;
using Tetrex.DataStructures;
using UnityEngine;

namespace Tetrex.Blocks
{
    public class BlockSpawnDive : NormalBlock
    {
        override public BlockAction[] Activate(NormalBlock[,] blockGrid)
        {
            Vector2Int myPos = GetPosInGrid(blockGrid);

            BlockAction[] actions = new BlockAction[myPos.y + 2];

            actions[0] = PackageBlockRemoval(this);

            for (int y = myPos.y; y >= 0; y--)
            {
                actions[myPos.y - y + 1] = PackageBlockSpawn(0, new Vector2Int(myPos.x, y));
            }

            return actions;
        }
    }
}

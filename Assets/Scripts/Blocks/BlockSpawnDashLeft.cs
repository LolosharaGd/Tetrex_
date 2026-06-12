using System.Collections;
using System.Collections.Generic;
using Tetrex.DataStructures;
using UnityEngine;

namespace Tetrex.Blocks
{
    public class BlockSpawnDashLeft : NormalBlock
    {
        override public BlockAction[] Activate(NormalBlock[,] blockGrid)
        {
            Vector2Int myPos = GetPosInGrid(blockGrid);
            BlockAction[] actions = new BlockAction[myPos.x + 2];

            for (int x = 0; x <= myPos.x; x++)
            {
                actions[x + 1] = PackageBlockSpawn(0, new Vector2Int(x, myPos.y));
            }

            actions[0] = PackageBlockRemoval(this, true);

            return actions;
        }
    }
}

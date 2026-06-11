using System.Collections;
using System.Collections.Generic;
using Tetrex.DataStructures;
using UnityEngine;

namespace Tetrex.Blocks
{
    public class BlockSpawnSquare : NormalBlock
    {
        override public BlockAction[] Activate(NormalBlock[,] blockGrid)
        {
            BlockAction[] actions = new BlockAction[10];
            Vector2Int myPos = GetPosInGrid(blockGrid);

            actions[0] = PackageBlockRemoval(this);
            actions[1] = PackageBlockSpawn(0, myPos);
            actions[2] = PackageBlockSpawn(0, myPos + Vector2Int.up + Vector2Int.right);
            actions[3] = PackageBlockSpawn(0, myPos + Vector2Int.up + Vector2Int.left);
            actions[4] = PackageBlockSpawn(0, myPos + Vector2Int.down + Vector2Int.right);
            actions[5] = PackageBlockSpawn(0, myPos + Vector2Int.down + Vector2Int.left);
            actions[6] = PackageBlockSpawn(0, myPos + Vector2Int.right);
            actions[7] = PackageBlockSpawn(0, myPos + Vector2Int.left);
            actions[8] = PackageBlockSpawn(0, myPos + Vector2Int.up);
            actions[9] = PackageBlockSpawn(0, myPos + Vector2Int.down);

            return actions;
        }
    }
}

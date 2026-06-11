using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tetrex.DataStructures;

namespace Tetrex.Blocks
{
    public class BlockRemoveCross : NormalBlock
    {
        override public BlockAction[] Activate(NormalBlock[,] blockGrid)
        {
            BlockAction[] actions = new BlockAction[5];
            Vector2Int myPos = GetPosInGrid(blockGrid);

            actions[0] = PackageBlockRemoval(this);
            actions[1] = PackageBlockRemoval(myPos + Vector2Int.up + Vector2Int.right);
            actions[2] = PackageBlockRemoval(myPos + Vector2Int.up + Vector2Int.left);
            actions[3] = PackageBlockRemoval(myPos + Vector2Int.down + Vector2Int.right);
            actions[4] = PackageBlockRemoval(myPos + Vector2Int.down + Vector2Int.left);

            return actions;
        }
    }
}

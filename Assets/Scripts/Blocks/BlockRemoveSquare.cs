using System.Collections;
using System.Collections.Generic;
using Tetrex.Blocks;
using Tetrex.DataStructures;
using UnityEngine;

namespace Tetrex.Blocks
{
    public class BlockRemoveSquare : NormalBlock
    {
        override public BlockAction[] Activate(NormalBlock[,] blockGrid)
        {
            BlockAction[] actions = new BlockAction[9];
            Vector2Int myPos = GetPosInGrid(blockGrid);

            actions[0] = PackageBlockRemoval(this);
            actions[1] = PackageBlockRemoval(myPos + Vector2Int.up + Vector2Int.right);
            actions[2] = PackageBlockRemoval(myPos + Vector2Int.up + Vector2Int.left);
            actions[3] = PackageBlockRemoval(myPos + Vector2Int.down + Vector2Int.right);
            actions[4] = PackageBlockRemoval(myPos + Vector2Int.down + Vector2Int.left);
            actions[5] = PackageBlockRemoval(myPos + Vector2Int.right);
            actions[6] = PackageBlockRemoval(myPos + Vector2Int.left);
            actions[7] = PackageBlockRemoval(myPos + Vector2Int.down);
            actions[8] = PackageBlockRemoval(myPos + Vector2Int.up);

            return actions;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Tetrex.DataStructures;
using UnityEngine;

namespace Tetrex.Blocks
{
    public class BlockEProtectedRow : NormalBlock
    {
        override public BlockAction[] Activate(NormalBlock[,] blockGrid)
        {
            Vector2Int myPos = GetPosInGrid(blockGrid);
            BlockAction[] actions = new BlockAction[12];

            for (int x = 0; x < 10; x++)
            {
                actions[x] = PackagePlaceEffect(new Vector2Int(x, myPos.y), BlockEffect.PROTECTED);
            }
            actions[10] = PackageBlockRemoval(this, true);
            actions[11] = PackageBlockSpawn(0, myPos);

            return actions;
        }
    }
}

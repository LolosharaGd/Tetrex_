using System.Collections;
using System.Collections.Generic;
using Tetrex.DataStructures;
using UnityEngine;

namespace Tetrex.Blocks
{
    public class BlockEProtectedSquare : NormalBlock
    {
        override public BlockAction[] Activate(NormalBlock[,] blockGrid)
        {
            Vector2Int myPos = GetPosInGrid(blockGrid);
            BlockAction[] actions = new BlockAction[11];

            actions[0] = PackagePlaceEffect(myPos + new Vector2Int(-1, -1), BlockEffect.PROTECTED);
            actions[1] = PackagePlaceEffect(myPos + new Vector2Int(0, -1), BlockEffect.PROTECTED);
            actions[2] = PackagePlaceEffect(myPos + new Vector2Int(1, -1), BlockEffect.PROTECTED);
            actions[3] = PackagePlaceEffect(myPos + new Vector2Int(-1, 0), BlockEffect.PROTECTED);
            actions[4] = PackagePlaceEffect(myPos + new Vector2Int(0, 0), BlockEffect.PROTECTED);
            actions[5] = PackagePlaceEffect(myPos + new Vector2Int(1, 0), BlockEffect.PROTECTED);
            actions[6] = PackagePlaceEffect(myPos + new Vector2Int(-1, 1), BlockEffect.PROTECTED);
            actions[7] = PackagePlaceEffect(myPos + new Vector2Int(0, 1), BlockEffect.PROTECTED);
            actions[8] = PackagePlaceEffect(myPos + new Vector2Int(1, 1), BlockEffect.PROTECTED);
            actions[9] = PackageBlockRemoval(this, true);
            actions[10] = PackageBlockSpawn(0, myPos);

            return actions;
        }
    }
}

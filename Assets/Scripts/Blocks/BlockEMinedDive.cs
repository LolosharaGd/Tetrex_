using System.Collections;
using System.Collections.Generic;
using Tetrex.DataStructures;
using UnityEngine;

namespace Tetrex.Blocks
{
    public class BlockEMinedDive : NormalBlock
    {
        override public BlockAction[] Activate(NormalBlock[,] blockGrid)
        {
            Vector2Int myPos = GetPosInGrid(blockGrid);

            BlockAction[] actions = new BlockAction[myPos.y + 2];

            actions[myPos.y + 1] = PackageBlockRemoval(this);

            for (int y = myPos.y; y >= 0; y--)
            {
                actions[y] = PackagePlaceEffect( new Vector2Int(myPos.x, y), BlockEffect.MINED);
            }

            return actions;
        }
    }
}

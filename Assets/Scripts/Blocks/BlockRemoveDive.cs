using System.Collections;
using System.Collections.Generic;
using Tetrex.DataStructures;
using UnityEngine;

namespace Tetrex.Blocks
{
    public class BlockRemoveDive : NormalBlock
    {
        override public BlockAction[] Activate(NormalBlock[,] blockGrid)
        {
            Vector2Int myPos = GetPosInGrid(blockGrid);

            BlockAction[] actions = new BlockAction[myPos.y + 1];

            for (int y = myPos.y; y >= 0; y--)
            {
                actions[myPos.y - y] = PackageBlockRemoval(new Vector2Int(myPos.x, y));
            }

            return actions;
        }
    }
}

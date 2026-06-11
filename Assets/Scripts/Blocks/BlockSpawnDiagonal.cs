using System.Collections;
using System.Collections.Generic;
using Tetrex.DataStructures;
using UnityEngine;

namespace Tetrex.Blocks
{
    public class BlockSpawnDiagonal : NormalBlock
    {
        override public BlockAction[] Activate(NormalBlock[,] blockGrid)
        {
            Vector2Int myPos = GetPosInGrid(blockGrid);
            BlockAction[] actions = new BlockAction[myPos.y * 2 + 2];

            actions[0] = PackageBlockRemoval(this);
            actions[myPos.y * 2 + 1] = PackageBlockSpawn(0, myPos);

            for (int i = 1; i <= myPos.y; i++)
            {
                Vector2Int newPosRight = myPos + new Vector2Int(i, -i);
                Vector2Int newPosLeft = myPos + new Vector2Int(-i, -i);

                actions[i] = PackageBlockSpawn(0, newPosRight);
                actions[i + myPos.y] = PackageBlockSpawn(0, newPosLeft);
            }

            return actions;
        }
    }
}

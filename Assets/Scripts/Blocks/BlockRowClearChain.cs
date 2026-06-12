using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tetrex.DataStructures;

namespace Tetrex.Blocks
{
    public class BlockRowClearChain : NormalBlock
    {
        override public BlockAction[] Activate(NormalBlock[,] blockGrid)
        {
            Vector2Int myPos = GetPosInGrid(blockGrid);

            return myPos.y == 0 ? new BlockAction[] { }  : new BlockAction[] { PackageRowClear(myPos.y - 1) };
        }
    }
}

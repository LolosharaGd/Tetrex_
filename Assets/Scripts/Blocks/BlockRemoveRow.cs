using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tetrex.DataStructures;

namespace Tetrex.Blocks
{
    public class BlockRemoveRow : NormalBlock
    {
        override public BlockAction[] Activate(NormalBlock[,] blockGrid)
        {
            Vector2Int myPos = GetPosInGrid(blockGrid);

            return new BlockAction[] { PackageRowClear(myPos.y) };
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Tetrex.DataStructures;
using UnityEngine;

namespace Tetrex.Blocks
{
    public class PhantomBlock : NormalBlock
    {
        override public BlockAction[] Activate(NormalBlock[,] blockGrid)
        {
            return new BlockAction[] { PackageBlockRemoval(this) };
        }
    }
}

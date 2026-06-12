using Codice.Client.BaseCommands.BranchExplorer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tetrex.DataStructures;
using UnityEngine;

namespace Tetrex.Blocks
{
    public class BlockSpecialGravity : NormalBlock
    {
        override public BlockAction[] Activate(NormalBlock[,] blockGrid)
        {
            List<BlockAction> actions = new();
            List<GameObject>[] columns = new List<GameObject>[blockGrid.GetLength(0)];
            for (int i = 0; i < columns.Length; i++)
            {
                columns[i] = new List<GameObject>();
            }

            for (int x = 0; x < blockGrid.GetLength(0); x++)
            {
                for (int y = 0; y < blockGrid.GetLength(1); y++)
                {
                    if (blockGrid[x, y])
                    {
                        columns[x].Add(blockGrid[x, y].gameObject);
                        actions.Add(PackageBlockRemoval(new Vector2Int(x, y), true));
                    }
                }
            }

            for (int x = 0; x < blockGrid.GetLength(0); x++)
            {
                for (int y = 0; y < columns[x].Count; y++)
                {
                    if (columns[x][y].Equals(gameObject))
                    {
                        actions.Add(PackageBlockSpawn(0, new Vector2Int(x, y)));
                    }
                    else
                    {
                        actions.Add(PackageBlockSpawn(columns[x][y], new Vector2Int(x, y)));
                    }
                }
            }

            return actions.ToArray();
        }

        private bool IsPosInsideGrid(object[,] grid, Vector2Int position)
        {
            return position.x >= 0 && position.y >= 0 && position.x < grid.GetLength(0) && position.y < grid.GetLength(1);
        }
    }
}

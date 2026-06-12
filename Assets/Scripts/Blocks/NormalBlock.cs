using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tetrex.DataStructures;

namespace Tetrex.Blocks
{
    public class NormalBlock : MonoBehaviour
    {
        public bool isControlled;

        public bool activateOnLanding;
        public bool activateOnClearRow;
        public bool activateOnUserInput;

        public TransitionTransformation transitionTransformation;
        public TransitionTransformation previewTT;

        void Start()
        {
            //Vector2 myPos = transform.position;
            //controller.blockGrid[(int)((myPos.x + 1.8) / 0.4) + 1, (int)((myPos.y + 0.4 * 11.5) / 0.4) - 2] = this;
        }

        void Update()
        {

        }

        public Vector2Int GetPosInGrid(NormalBlock[,] matrix)
        {
            for (int x = 0; x < matrix.GetLength(0); ++x)
            {
                for (int y = 0; y < matrix.GetLength(1); ++y)
                {
                    if (matrix[x, y] != null)
                        if (matrix[x, y].Equals(this))
                            return new Vector2Int(x, y);
                }

            }

            return new Vector2Int(-1, -1);
        }

        virtual public BlockAction[] Activate(NormalBlock[,] blockGrid)
        {
            return new BlockAction[0];
        }

        /// <summary>
        /// Packages removing a block into a BlockAction. Actions inside one action type are done in order they were queued. Order of types: RemoveEffect -> PlaceEffect -> RemoveBlock -> PlaceBlock -> Stop -> ClearRow
        /// </summary>
        /// <param name="targetBlock">NormalBlock block object of block being removed. Vector2Int position of the block being removed.</param>
        public BlockAction PackageBlockRemoval(object targetBlock, bool ignoreBE = false)
        {
            return new BlockAction
            {
                block = targetBlock,
                extra = ignoreBE,
                type = BlockActionType.REMOVEBLOCK
            };
        }

        /// <summary>
        /// Packages spawning a block into a BlockAction. Actions inside one action type are done in order they were queued. Order of types: RemoveEffect -> PlaceEffect -> RemoveBlock -> PlaceBlock -> Stop -> ClearRow
        /// </summary>
        /// <param name="targetBlock">GameObject prefab of the block peing spawned. Int index of the block in the blockPrefabs list. Null to spawn a weighed random block</param>
        /// <param name="position">Vector2Int position where to spawn</param>
        public BlockAction PackageBlockSpawn(object targetBlock, Vector2Int position)
        {
            return new BlockAction
            {
                block = targetBlock,
                position = position,
                type = BlockActionType.SPAWNBLOCK
            };
        }

        /// <summary>
        /// Packages a row clear action. Actions inside one action type are done in order they were queued. Order of types: RemoveEffect -> PlaceEffect -> RemoveBlock -> PlaceBlock -> Stop -> ClearRow
        /// </summary>
        /// <param name="y">Y value (from 0 bottom to 19 top) of the row being cleared</param>
        /// <param name="moveDownOtherRows">True to move rows above this row down</param>
        public BlockAction PackageRowClear(int y, bool moveDownOtherRows = true)
        {
            return new BlockAction
            {
                position = y,
                extra = moveDownOtherRows,
                type = BlockActionType.CLEARROW
            };
        }

        /// <summary>
        /// Packages a place effect action. Actions inside one action type are done in order they were queued. Order of types: RemoveEffect -> PlaceEffect -> RemoveBlock -> PlaceBlock -> Stop -> ClearRow
        /// </summary>
        /// <param name="position">Vector2Int position on grid of where to place effect</param>
        /// <param name="type">BlockEffect type of the effect</param>
        /// <returns></returns>
        public BlockAction PackagePlaceEffect(Vector2Int position, BlockEffect type)
        {
            return new BlockAction
            {
                type = BlockActionType.PLACEEFFECT,
                position = position,
                block = type
            };
        }

        /// <summary>
        /// Packages a remove effect action. Actions inside one action type are done in order they were queued. Order of types: RemoveEffect -> PlaceEffect -> RemoveBlock -> PlaceBlock -> Stop -> ClearRow
        /// </summary>
        /// <param name="position">Vector2Int position on grid of where to remove effect from</param>
        /// <returns></returns>
        public BlockAction PackageRemoveEffect(Vector2Int position)
        {
            return new BlockAction
            {
                type = BlockActionType.REMOVEEFFECT,
                position = position
            };
        }

        /// <summary>
        /// Packages a block stop action. Actions inside one action type are done in order they were queued. Order of types: RemoveEffect -> PlaceEffect -> RemoveBlock -> PlaceBlock -> Stop -> ClearRow
        /// </summary>
        public BlockAction PackageBlockStop()
        {
            return new BlockAction
            {
                type = BlockActionType.STOP
            };
        }
    }
}

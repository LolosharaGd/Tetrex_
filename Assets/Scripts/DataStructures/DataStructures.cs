using System;
using System.Collections.Generic;
using UnityEngine;

//public class DataStructures : MonoBehaviour
//{

//}

namespace Tetrex.DataStructures
{
    [System.Serializable]
    public class ShopBlock
    {
        public string name;
        public string description;
        public string notes;
        public bool isEquipped;
        public GameObject spritePrefab;
        public int cost;
        public float Weight
        {
            get
            {
                switch (rarity)
                {
                    case BlockRarity.COMMON: return 10f;
                    case BlockRarity.UNCOMMON: return 5f;
                    case BlockRarity.RARE: return 2f;
                    case BlockRarity.NONE: return 0f;
                    default: return 2f;
                }
            }
        }
        public BlockRarity rarity;
        public BlockAlignment alignment;

        public bool blockActivatesOnLanding = true;
        public bool blockActivatesOnClearRow = true;
        public bool blockActivatesOnUserInput = false;
        public bool blockSelfDestructs = true;

        public int saveBitIndex;

        public ShopBlock Copy()
        {
            return new ShopBlock()
            {
                name = name,
                description = description,
                notes = notes,
                isEquipped = isEquipped,
                spritePrefab = spritePrefab,
                cost = cost,
                saveBitIndex = saveBitIndex,
                rarity = rarity,
                alignment = alignment,
                blockActivatesOnClearRow=blockActivatesOnClearRow,
                blockActivatesOnLanding=blockActivatesOnLanding,
                blockActivatesOnUserInput=blockActivatesOnUserInput,
                blockSelfDestructs=blockSelfDestructs
            };
        }
    }

    /// <summary>
    /// A shape that blocks can be arranged in. Also stores all rotation movements blocks can do
    /// </summary>
    [System.Serializable]
    public class BlockShape : ICloneable
    {
        public string name;
        /// <summary>
        /// Starting position in a 4x4 matrix
        /// </summary>
        public Vector2Int startingPosition;
        /// <summary>
        /// Positions relatively to startingPosition
        /// </summary>
        public Vector2Int[] positions;
        /// <summary>
        /// Rotations each block can do. IMPORTANT: the rotations in this is MUST be in the same order as in the positions list
        /// </summary>
        public BlockShapeRotation[] rotations;

        public object Clone()
        {
            Vector2Int[] newPoses = new Vector2Int[positions.Length];
            BlockShapeRotation[] newBSRs = new BlockShapeRotation[rotations.Length];

            for (int i = 0; i < positions.Length; i++) newPoses[i] = positions[i];
            for (int i = 0; i < rotations.Length; i++) newBSRs[i] = (BlockShapeRotation)rotations[i].Clone();

            BlockShape newCopy = new()
            {
                name = name,
                startingPosition = new Vector2Int(startingPosition.x, startingPosition.y),
                positions = newPoses,
                rotations = newBSRs
            };
            return newCopy;
        }
    }

    /// <summary>
    /// The object that contains all the rotations ONE BLOCK can do. Shapes use a list of this
    /// </summary>
    [System.Serializable]
    public class BlockShapeRotation : ICloneable
    {
        /// <summary>
        /// List of movements of the block, in CLOCKWISE order
        /// </summary>
        public List<Vector2Int> rotationMovements = new();
        public int nextRotationIndex;

        /// <returns>Next rotation controlled by nextRotationIndex</returns>
        public Vector2Int GetNextRotationCW()
        {
            return rotationMovements[nextRotationIndex];
        }

        public Vector2Int GetNextRotationCCW()
        {
            int nextRot = nextRotationIndex - 1;
            if (nextRot < 0) nextRot = rotationMovements.Count - 1;
            if (nextRot >= rotationMovements.Count) nextRot = 0;
            return -rotationMovements[nextRot];
        }

        public Vector2Int GetNextRotation(bool clockwise)
        {
            return clockwise ? GetNextRotationCW() : GetNextRotationCCW();
        }

        public object Clone()
        {
            List<Vector2Int> newRotations = new();
            foreach (Vector2Int rotation in rotationMovements) newRotations.Add(rotation);

            return new BlockShapeRotation()
            {
                rotationMovements = newRotations,
                nextRotationIndex = nextRotationIndex,
            };
        }
    }

    [System.Serializable]
    public class WeighedBlock
    {
        public string name;
        public GameObject block;
        public GameObject previewBlock;
        public float Weight
        {
            get
            {
                switch (rarity)
                {
                    case BlockRarity.COMMON: return 2f;
                    case BlockRarity.UNCOMMON: return 1.5f;
                    case BlockRarity.RARE: return 1f;
                    case BlockRarity.NORMALBLOCK: return 100f;
                    default: return 2f;
                }
            }
        }
        public BlockRarity rarity;
    }

    /// <summary>
    /// Type of an action. Actions are done in order CLEARROW -> REMOVE -> SPAWN -> STOP
    /// </summary>
    public enum BlockActionType { REMOVEBLOCK, SPAWNBLOCK, CLEARROW, PLACEEFFECT, REMOVEEFFECT, STOP }

    [System.Serializable]
    public class BlockAction
    {
        public object block;
        public object position;
        public object extra;
        public BlockActionType type;

        public BlockAction Copy()
        {
            return new BlockAction()
            {
                block = block,
                position = position,
                extra = extra,
                type = type,
            };
        }
    }

    // Comparer to sort block actions in descending order of position y
    public class BASortDescent : IComparer<BlockAction>
    {
        public int Compare(BlockAction x, BlockAction y)
        {
            return ((int)y.position).CompareTo((int)x.position);
        }
    }

    [System.Serializable]
    public class TransitionTransformation
    {
        public string name;
        public GameObject gameObject;
        public Vector3 direction;
        public Vector3 startPosition;
    }

    [System.Serializable]
    public class RandomSound
    {
        public string name;
        public AudioClip[] clips;

        /// <summary>
        /// Random clip from clips list
        /// </summary>
        public AudioClip RandomClip
        {
            get => clips[UnityEngine.Random.Range(0, clips.Length)];
        }
    }

    public enum BlockRarity { COMMON, UNCOMMON, RARE, NORMALBLOCK, NONE }
    public enum BlockAlignment { CURSED, NORMAL, HOLY }

    public enum BlockEffect { NOTHING, MINED, PROTECTED }
}

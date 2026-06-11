using System.Collections;
using System.Collections.Generic;
using Tetrex.DataStructures;
using UnityEngine;

[CreateAssetMenu(fileName = "OptionsScriptableObject", menuName = "ScriptableObjects/Options")]
public class PropertiesScriptableObject : ScriptableObject
{
    [Header("-== GAME ==-")]
    /// <summary>
    /// List of weighedBlocks
    /// </summary>
    public WeighedBlock[] blockPrefabs;
    /// <summary>
    /// List of block effect GameObject prefabs. Index corresponds to place in BlockEffect enum
    /// </summary>
    public GameObject[] blockEffectPrefabs;
    /// <summary>
    /// List of all block shapes (like the L, J, S etc.). With positions of each block relative to one block, and their rotations.
    /// </summary>
    public List<BlockShape> blockShapes = new();
    /// <summary>
    /// Block down moves per second
    /// </summary>
    public float blockFallSpeed;
    /// <summary>
    /// Block horizontal moves per second
    /// </summary>
    public float blockHorizMoveSpeed;
    /// <summary>
    /// Amount in seconds to wait for after first horizontal move
    /// </summary>
    public float blockHorizMoveFirstWait;
    /// <summary>
    /// Block vertical moves per second
    /// </summary>
    public float blockVertMoveSpeed;
    /// <summary>
    /// Amount in seconds to wait for after first vertical move
    /// </summary>
    public float blockVertMoveFirstWait;
    /// <summary>
    /// Score rewards, where index is the number of rows. Last row gets repeatedly multiplied by 1.5 for every row after it
    /// </summary>
    public int[] scoreRewards;
    /// <summary>
    /// Score goal list in which the index is the level index, run starts at 1
    /// </summary>
    public int[] scoreGoals;

    [Header("-== SHOP ==-")]
    /// <summary>
    /// List of all possible ShopBlocks that can appear in the shop
    /// </summary>
    public ShopBlock[] allBlocks;
    /// <summary>
    /// Spacing between shop blocks in shop and equipped
    /// </summary>
    public float scrollBlockSpacing;
    /// <summary>
    /// Additional spacing above and below shop block in the center
    /// </summary>
    public float centralScrollBlockAddedSpacing;
    /// <summary>
    /// Normal size of shop block GameObjects
    /// </summary>
    public float normalBlockSize;
    /// <summary>
    /// Size of shop block GameObjects in the center
    /// </summary>
    public float centralBlockSize;
    /// <summary>
    /// Duration of blocks moving up or down while scrolling
    /// </summary>
    public float scrollBlockSwitchDuration;
    /// <summary>
    /// Duration of scrolls changing
    /// </summary>
    public float blockScrollSwitchDuration;

    [Header("-== VFX ==-")]
    /// <summary>
    /// Minimum HSV of the bg color
    /// </summary>
    public Color minColor;
    /// <summary>
    /// Maximum HSV of the bg color
    /// </summary>
    public Color maxColor;
    /// <summary>
    /// Speed of bg color change
    /// </summary>
    public float colorChangeSpeed;

    [Header("-== SFX ==-")]
    public RandomSound blockMoveSound;
    public RandomSound blockRotateSound;
    public RandomSound blockDashSound;
    public RandomSound blockTrashSound;
    /// <summary>
    /// Sounds for cleared rows. Index 0 should be left blank
    /// </summary>
    public RandomSound[] rowClearSounds;
    /// <summary>
    /// Music for stages. 1st stage is at index 1
    /// </summary>
    public RandomSound[] stageMusic;
    /// <summary>
    /// Music for stage bosses, 1st stage is at index 1
    /// </summary>
    public RandomSound[] stageBossMusic;
    /// <summary>
    /// Music for shops
    /// </summary>
    public RandomSound shopMusic;

    [Header("-== STARTING STATS ==-")]
    public int startMoney;
    public int startSellTokens;
    public int startShopSlots;
    public int startTrashTokens;
}

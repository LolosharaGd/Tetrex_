using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Tetrex.DataStructures;
using Tetrex.Blocks;

public class GameController : MonoBehaviour
{
    /// <summary>
    /// Scriptable object of options of the game
    /// </summary>
    public PropertiesScriptableObject prop;

    [Header("-== Other scripts ==-")]
    /// <summary>
    /// The sound controller
    /// </summary>
    [SerializeField] SoundController soundController;
    /// <summary>
    /// The score manager
    /// </summary>
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] VFXManager vfxManager;

    [Header("-== GameObject Parents ==-")]
    public bool[] equippedBlocks;
    /// <summary>
    /// Parent for all blocks
    /// </summary>
    [SerializeField] Transform blocksParent;
    /// <summary>
    /// Parent for all dash preview blocks
    /// </summary>
    [SerializeField] Transform blockDashPreviewsParent;
    /// <summary>
    /// Parent for all next shape preview blocks
    /// </summary>
    [SerializeField] Transform nextShapePreviewsParent;
    /// <summary>
    /// Parent for all block effect objects
    /// </summary>
    [SerializeField] Transform blockEffectsParent;

    [Header("-== Block lists ==-")]
    /// <summary>
    /// Grid of all blocks on the board
    /// </summary>
    public NormalBlock[,] blockGrid = new NormalBlock[10,24];
    /// <summary>
    /// Grid of all block effects on the board
    /// </summary>
    public BlockEffect[,] blockEffectGrid = new BlockEffect[10,24];
    /// <summary>
    /// Grid of all block effect GameObjects on the board
    /// </summary>
    public GameObject[,] blockEffectGOGrid = new GameObject[10,24];
    /// <summary>
    /// List of all blocks of the board that are currently controlled by the player
    /// </summary>
    public List<NormalBlock> controlledBlocks = new();

    [Header("-== Action queue ==-")]
    /// <summary>
    /// List of queued place effect actions
    /// </summary>
    public List<BlockAction> queuedActionsPlaceEffect = new();
    /// <summary>
    /// List of queued remove effect actions
    /// </summary>
    public List<BlockAction> queuedActionsRemoveEffect = new();
    /// <summary>
    /// List of queued remove block actions
    /// </summary>
    public List<BlockAction> queuedActionsRemoveBlock = new();
    /// <summary>
    /// List of all queued spawn block actions
    /// </summary>
    public List<BlockAction> queuedActionsSpawnBlock = new();
    /// <summary>
    /// List of all queued clear row actions
    /// </summary>
    public List<BlockAction> queuedActionsClearrow = new();
    /// <summary>
    /// Is the stop action queued
    /// </summary>
    public bool stopActionQueued;

    [Header("-== Shapes ==-")]
    /// <summary>
    /// Current shape (clone from list of blockShapes)
    /// </summary>
    public BlockShape currentShape;
    /// <summary>
    /// Next shape (clone from list of blockShapes)
    /// </summary>
    public BlockShape nextShape;
    /// <summary>
    /// Currently held shape (clone from list of blockShapes)
    /// </summary>
    public BlockShape heldShape;
    /// <summary>
    /// All blocks in currently held shape
    /// </summary>
    public List<GameObject> heldShapeBlocks = new();
    /// <summary>
    /// All dash preview blocks in currently held shape
    /// </summary>
    public List<GameObject> heldShapePreviews = new();
    [SerializeField] List<string> prevShapes = new();
    public int prevShapeBufferSize;

    [Header("-== Previews ==-")]
    /// <summary>
    /// List of POSITIONS of where will the blocks land if player were to dash right now
    /// </summary>
    List<Vector2Int> previewDashes = new();
    /// <summary>
    /// List of BLOCKS that are placed at the position where the controlled blocks would end up after dash
    /// </summary>
    List<Transform> previewDashBlocks = new();

    [Header("-== Next shape ==-")]
    /// <summary>
    /// List of blocks that are placed like next shape, positioned in the next shape indicator
    /// </summary>
    List<Transform> nextShapePreviewBlocks = new();
    /// <summary>
    /// List of Trasition transformations for block in the next shape indicator
    /// </summary>
    List<TransitionTransformation> nextShapeTTs = new();

    [Header("-== In-game text ==-")]
    [SerializeField] TextMeshProUGUI trashTokensText;

    [Header("-== Stats and tokens ==-")]
    public int currentTrashTokens;
    [SerializeField] int linesClearedForTrT;
    public int linesForTrashToken;
    /// <summary>
    /// Amount of extra lines that are counted when lines are cleared
    /// </summary>
    public int LineClearBonus
    {
        get {
            int total = 0;
            for (int i = 1; i < equippedBlocks.Length; i++)
            {
                foreach (var sb in prop.allBlocks) { if (equippedBlocks[i] && sb.saveBitIndex == i && sb.alignment == BlockAlignment.CURSED) { total++; break; } }
            }
            return total;
        }
    }

    /// <summary>
    /// How many second until the controlled blocks fall 1 block down
    /// </summary>
    float blockFallTimer;
    /// <summary>
    /// How many second until player can move the blocks horizontally
    /// </summary>
    float blockHorizMoveTimer;
    /// <summary>
    /// How many second until player can move the block down vertically
    /// </summary>
    float blockVertMoveTimer;
    // Variables for previous frame
    float prevX;
    float prevY;
    float prevRotate;
    float prevDash;
    float prevActivateBlocks;
    float prevHold;
    float prevTrash;
    float prevPause;

    /// <summary>
    /// Lines cleared before end of frame
    /// </summary>
    int extraClearedLines;

    /// <summary>
    /// Starting position in grid for the new spawned and controlled shape
    /// </summary>
    Vector2Int shapeStartingPosition = new Vector2Int(3, 20);
    /// <summary>
    /// Bottom left corner of preview indicator in grid
    /// </summary>
    Vector2Int nextShapePreviewStartingPosition = new Vector2Int(-5, 10);
    /// <summary>
    /// Bottom left corner of held shape indicator in grid
    /// </summary>
    Vector2Int heldShapePreviewStartingPosition = new Vector2Int(11, 10);

    [Header("-== Global ==-")]
    /// <summary>
    /// Did player lose
    /// </summary>
    public bool lost;
    public bool won;
    public bool paused;

    [Header("-== Debug ==-")]
    // Debug
    [SerializeField] bool debugRenderBoard;

    void Start()
    {
        blockFallTimer = 1 / prop.blockFallSpeed;
        nextShape = (BlockShape)prop.blockShapes[UnityEngine.Random.Range(0, prop.blockShapes.Count)].Clone();
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 24; y++)
            {
                blockEffectGrid[x, y] = BlockEffect.NOTHING;
            }
        }
        SpawnRandomShape(shapeStartingPosition);
        UpdateNextShapePreview();
    }

    float PressCheck(float cur, float prev)
    {
        if (cur == prev) return 0f;
        return cur;
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        float rotate = Input.GetAxisRaw("Rotate");
        float dash = Input.GetAxisRaw("Dash");
        float activateBlocks = Input.GetAxisRaw("Activate");
        float hold = Input.GetAxisRaw("Hold");
        float trash = Input.GetAxisRaw("Trash");
        float pause = Input.GetAxisRaw("Pause");

        if (!lost && !won && vfxManager.IGTransitionProgress <= 0f)
        {
            // Check pause
            if (PressCheck(prevPause, pause) == 1f) Unpause();
            if (PressCheck(pause, prevPause) == -1f)
            {
                if (paused) Unpause();
                else if (!paused) Pause();
            }
            if (PressCheck(pause, prevPause) == 1f) Pause();

            if (!paused)
            {
                // If pressing hoizontal movement
                if (x != 0)
                {
                    // If can move (timer is up)
                    if (blockHorizMoveTimer <= 0)
                    {
                        // Move
                        bool canMoveBlocks = MoveControlledBlocks(new Vector2Int((int)x, 0));

                        // Reset timer, reset to higher value if first click
                        blockHorizMoveTimer = prevX != x ? prop.blockHorizMoveFirstWait : 1 / prop.blockHorizMoveSpeed;

                        // Play the sound
                        soundController.PlayRandomMoveSound();
                    }
                }
                else
                {
                    // Set timer to 0 (timer's up)
                    blockHorizMoveTimer = 0;
                }

                // If pressing down
                if (y == -1)
                {
                    // If can move (timer is up)
                    if (blockVertMoveTimer <= 0)
                    {
                        // Move
                        blockFallTimer = 0;

                        // Reset timer, reset to higher value if first click
                        blockVertMoveTimer = prevY != y ? prop.blockVertMoveFirstWait : 1 / prop.blockVertMoveSpeed;

                        // Play the sound
                        soundController.PlayRandomMoveSound();
                    }
                }
                else
                {
                    // Set timer to 0 (timer's up)
                    blockVertMoveTimer = 0;
                }

                // Blocks fall
                if (blockFallTimer <= 0)
                {
                    ControlledBlocksFall();

                    blockFallTimer = 1 / prop.blockFallSpeed;
                }

                // Trash shape
                if (PressCheck(trash, prevTrash) != 0f && currentTrashTokens > 0)
                {
                    TrashControlledBlocks();

                    currentTrashTokens--;

                    soundController.PlayRandomTrashSound();
                }

                // Block dash down
                if (PressCheck(dash, prevDash) != 0f)
                {
                    int iteration = 0;
                    blockFallTimer = dash == 1 ? 0 : 1.5f / prop.blockFallSpeed;
                    bool canDashDown = true;
                    do
                    {
                        canDashDown = MoveControlledBlocks(new Vector2Int(0, -1));
                        iteration++;
                    }
                    while (canDashDown && iteration <= 30);
                }

                // If pressed one of the rotate buttons
                if (rotate != 0 && prevRotate != rotate)
                {
                    RotateControlledBLocks(rotate == 1);

                    soundController.PlayRandomRotateSound();
                }

                // Activate all blocks that activate on user input
                if (PressCheck(activateBlocks, prevActivateBlocks) != 0f)
                {
                    foreach (var block in controlledBlocks) if (block.activateOnUserInput) ActivateBlock(block);

                    DoQueuedBlockActions();
                }

                // If player pressed hold
                if (hold == 1 && prevHold == 0)
                {
                    SwapHeldBlocks();
                }

                // Update trash tokens text
                string ttText = Mathf.Clamp(currentTrashTokens, 0, 99999) + "";
                for (int i = ttText.Length; i < 5; i++) ttText = "0" + ttText;
                trashTokensText.text = ttText;

                blockFallTimer -= Time.deltaTime;
                blockHorizMoveTimer -= Time.deltaTime;
                blockVertMoveTimer -= Time.deltaTime;

                prevX = x;
                prevY = y;
                prevRotate = rotate;
                prevDash = dash;
                prevActivateBlocks = activateBlocks;
                prevHold = hold;
                prevTrash = trash;
            }
        }
        else if (won)
        {
            scoreManager.WinAnimationTick(Time.deltaTime);
        }

        prevPause = pause;
    }

    // -= Controlled blocks manipulation =-

    /// <summary>
    /// Moves blocks in the Vector2Int direction.
    /// </summary>
    /// <param name="delta">Direction in which move all controlled blocks</param>
    /// <returns>True if succesfully moved, False if the move failed</returns>
    public bool MoveControlledBlocks(Vector2Int delta)
    {
        Vector2Int blockPos = new Vector2Int(-1, -1);

        // Check if blocks are not obstructed
        foreach (var block in controlledBlocks)
        {
            blockPos = block.GetPosInGrid(blockGrid);

            Vector2Int newPos = blockPos + delta;

            if (!IsPosInsideGrid(newPos))
            {
                print("Can't move block: wall blocked the movement. Position: X " + newPos.x + "; Y " + newPos.y);
                return false;
            }

            if (blockGrid[newPos.x, newPos.y] != null)
                if (!blockGrid[newPos.x, newPos.y].isControlled)
                {
                    print("Can't move block: another block blocked the movement");
                    return false;
                }
        }

        // Store blocks' positions
        List<Vector2Int> inGridPositions = new();
        foreach (var block in controlledBlocks)
        {
            inGridPositions.Add(block.GetPosInGrid(blockGrid));
        }

        //List<Vector2Int> newPositions = new();
        // Remove blocks from grid and add new
        for (int i = 0; i < controlledBlocks.Count; i++)
        {
            if (blockGrid[inGridPositions[i].x, inGridPositions[i].y] != null)
                if (blockGrid[inGridPositions[i].x, inGridPositions[i].y].Equals(controlledBlocks[i]))
                    blockGrid[inGridPositions[i].x, inGridPositions[i].y] = null;

            //newPositions.Add(inGridPositions[i] + delta);

            blockGrid[inGridPositions[i].x + delta.x, inGridPositions[i].y + delta.y] = controlledBlocks[i];

            controlledBlocks[i].transform.position += new Vector3(0.4f * delta.x, 0.4f * delta.y, 0);
            controlledBlocks[i].transitionTransformation.startPosition = controlledBlocks[i].transform.position;
        }

        UpdateDashPreview();

        return true;
    }

    /// <summary>
    /// Universal function to rotate the controlled blocks, according to current shape
    /// </summary>
    /// <param name="clockwise">True to rotate clockwise, False to rotate counter clockwise</param>
    /// <returns>True if was able to rotate, False if rotation failed</returns>
    public bool RotateControlledBLocks(bool clockwise)
    {
        // Throw a tamper tantrum if something went wrong
        if (controlledBlocks.Count != 4) { print("Tried to rotate but number of blocks controlled is not 4"); return false; }
        if (currentShape == null) { print("Tried to rotate but current shape is null"); return false; }
        if (currentShape.rotations.Length != 4) { print("Tried to rotate but number of rotation blocks in current shape is not 4"); return false; }

        // Store blocks' positions
        List<Vector2Int> inGridPositions = new();
        foreach (var block in controlledBlocks)
        {
            inGridPositions.Add(block.GetPosInGrid(blockGrid));
        }

        // Check if blocks are not obstructed
        for (int i = 0; i < currentShape.rotations.Length; i++)
        {
            Vector2Int newPos = inGridPositions[i] + currentShape.rotations[i].GetNextRotation(clockwise);

            if (!IsPosInsideGrid(newPos))
            {
                print("Can't rotate block: wall blocked the rotation");
                return false;
            }

            if (blockGrid[newPos.x, newPos.y] != null)
                if (!blockGrid[newPos.x, newPos.y].isControlled)
                {
                    print("Can't rotate block: another block blocked the rotation");
                    return false;
                }
        }

        // Remove blocks from grid and add new
        for (int i = 0; i < inGridPositions.Count; i++)
        {
            if (blockGrid[inGridPositions[i].x, inGridPositions[i].y] != null)
                if (blockGrid[inGridPositions[i].x, inGridPositions[i].y].Equals(controlledBlocks[i]))
                    blockGrid[inGridPositions[i].x, inGridPositions[i].y] = null;

            Vector2Int nextRotation = currentShape.rotations[i].GetNextRotation(clockwise);

            blockGrid[inGridPositions[i].x + nextRotation.x, inGridPositions[i].y + nextRotation.y] = controlledBlocks[i];

            controlledBlocks[i].transform.position += new Vector3(0.4f * nextRotation.x, 0.4f * nextRotation.y, 0);
        }

        // Change next rotation
        foreach (var rotation in currentShape.rotations)
        {
            rotation.nextRotationIndex += clockwise ? 1 : -1;

            if (rotation.nextRotationIndex < 0) rotation.nextRotationIndex = rotation.rotationMovements.Count - 1;
            if (rotation.nextRotationIndex >= rotation.rotationMovements.Count) rotation.nextRotationIndex = 0;
        }

        UpdateDashPreview();

        return true;
    }

    /// <summary>
    /// Universal function for controlled blocks to fall under gravity
    /// </summary>
    public bool ControlledBlocksFall()
    {
        bool blocksCanFall = MoveControlledBlocks(new Vector2Int(0, -1));

        if (!blocksCanFall)
        {
            OnBlocksReachGround();
        }

        return blocksCanFall;
    }
    
    /// <summary>
    /// Function called when controlled blocks reach the ground while falling under gravity or user input
    /// </summary>
    public void OnBlocksReachGround()
    {
        // Land the blocks
        foreach (var block in controlledBlocks)
        {
            block.isControlled = false;
        }

        // Remove previous previews
        foreach (var block in controlledBlocks)
        {
            vfxManager.gameBlockTTs.Remove(block.previewTT);
            Destroy(block.previewTT.gameObject);
        }
        previewDashBlocks.Clear();

        // Check block effects
        List<NormalBlock> destrBLocks = new();
        foreach (var block in controlledBlocks.CloneViaFakeSerialization())
        {
            Vector2Int blockPos = block.GetPosInGrid(blockGrid);

            // If place the block landed on is mined, destroy it
            if (blockEffectGrid[blockPos.x, blockPos.y] == BlockEffect.MINED)
            {
                destrBLocks.Add(block);
                RemoveBlock(block);
                RemoveBlockEffect(blockPos);
            }
        }

        // Activate all controlled blocks that need to be
        foreach (var block in controlledBlocks) if (block.activateOnLanding && !destrBLocks.Contains(block)) ActivateBlock(block);

        // DEBUG: place a block effect where blocks landed
        //PlaceBlockEffect(controlledBlocks[0].GetPosInGrid(blockGrid), BlockEffect.MINED);
        //PlaceBlockEffect(controlledBlocks[1].GetPosInGrid(blockGrid), BlockEffect.MINED);
        //PlaceBlockEffect(controlledBlocks[2].GetPosInGrid(blockGrid), BlockEffect.MINED);
        //PlaceBlockEffect(controlledBlocks[3].GetPosInGrid(blockGrid), BlockEffect.MINED);

        // Clear controlled blocks list
        controlledBlocks.Clear();

        // Do actions that may have been queued by activated blocks
        DoQueuedBlockActions();

        // Check if there are any blocks in the top 4 rows
        for (int x = 0; x < blockGrid.GetLength(0); x++) for (int y = 20; y < blockGrid.GetLength(1); y++) if (blockGrid[x, y] != null) Lose();

        // Check if any rows are full
        List<int> fullRows = new();
        for (int y = blockGrid.GetLength(1) - 5; y >= 0; y--)
        {
            bool rowIsFull = true;
            for (int x = 0; x < blockGrid.GetLength(0); x++) rowIsFull = rowIsFull && (blockGrid[x, y] != null);
            if (rowIsFull) fullRows.Add(y);
        }

        // Clear full rows
        foreach (var fullRow in fullRows) ClearRow(fullRow);

        // Play the row clear sound
        soundController.PlayRowClearSound((int)MathF.Min(extraClearedLines, 5));

        // Add score
        scoreManager.AddScoreFromLines(extraClearedLines + (extraClearedLines == 0 ? 0 : LineClearBonus));
        extraClearedLines = 0;

        // Check the win condition
        won = scoreManager.CheckWinCondition();

        // If after adding the score player didn't win
        if (!won)
        {
            // Spawn new shape
            bool spawnedNewShape = SpawnRandomShape(shapeStartingPosition);
            if (!spawnedNewShape)
            {
                Lose();
            }

            // Spawn next shape preview
            UpdateNextShapePreview();

            // Change BG color
            vfxManager.ChangeBgColor();
        }

        // Camera shake
        if (fullRows.Count > 0) // If any rows were cleared
        {
            vfxManager.CameraShake(0.1f, 0.5f, 50);
        }
        else // If no rows were cleared
        {
            vfxManager.CameraShake(0.05f, 0.3f, 40);
        }

        soundController.PlayRandomDashSound();
    }

    /// <summary>
    /// Universal function to trash currently controlled blocks and spawn a new shape
    /// </summary>
    public void TrashControlledBlocks()
    {
        // Remove previous previews
        foreach (var block in controlledBlocks)
        {
            vfxManager.gameBlockTTs.Remove(block.previewTT);
            Destroy(block.previewTT.gameObject);
        }
        previewDashBlocks.Clear();

        // Remove every controlled block
        foreach (var block in controlledBlocks.CloneViaFakeSerialization())
        {
            RemoveBlock(block, true);
        }

        // Clear controlled blocks list
        controlledBlocks.Clear();

        // Spawn new shape
        bool spawnedNewShape = SpawnRandomShape(shapeStartingPosition);
        if (!spawnedNewShape)
        {
            Lose();
        }

        // Spawn next shape preview
        UpdateNextShapePreview();

        // Change BG color
        vfxManager.ChangeBgColor();
    }

    // -= Shape and block spawn and removal =-

    /// <summary>
    /// A function that spawn a new random shape, mainly used to create random controlled shape after the previous one lands
    /// </summary>
    /// <param name="position">Position of the shape, if exactPosition is false, then offset this parameter by shape's startingPosition</param>
    /// <param name="takeControl">True to immediately take control of all blocks of the shape</param>
    /// <param name="exactPosition">False to offset position by shape's startingPosition</param>
    /// <returns>True if the shape was succesfully spawned, false if the spawn failed</returns>
    public bool SpawnRandomShape(Vector2Int position, bool takeControl=true, bool exactPosition=false)
    {
        BlockShape newShape = (BlockShape)nextShape.Clone();

        // Add new shape to previous shapes list
        if (prevShapeBufferSize > 0)
        {
            if (prevShapes.Count >= prevShapeBufferSize) prevShapes.RemoveAt(0);
            prevShapes.Add(newShape.name);
        }
        bool newShapeIsAllowed = false;
        int iterations = 0;
        int maxIter = 100;
        while (!newShapeIsAllowed)
        {
            BlockShape newNextShape = (BlockShape)prop.blockShapes[UnityEngine.Random.Range(0, prop.blockShapes.Count)].Clone();

            newShapeIsAllowed = true;
            foreach (var prevShape in prevShapes)
            {
                if (prevShape == newNextShape.name)
                {
                    newShapeIsAllowed = false;
                    break;
                }
            }
            if (newNextShape.name == heldShape.name) newShapeIsAllowed = false;

            if (newShapeIsAllowed)
            {
                nextShape = newNextShape;
                break;
            }

            iterations++;
            if (iterations > maxIter) break;
        }

        if (takeControl)
        {
            // Land the blocks
            foreach (var block in controlledBlocks)
            {
                block.isControlled = false;
            }
        }

        bool shapeSpawnSuccess = SpawnNewShape(newShape, position, takeControl, exactPosition);

        if (shapeSpawnSuccess)
        {
            UpdateDashPreview();
        }

        return shapeSpawnSuccess;
    }

    /// <summary>
    /// Universal function to spawn a shape somewhere on the map
    /// </summary>
    /// <param name="shape">BlockShape: an exact shape. String: first shape with that letter as the name in shapes list, defaults to first. Int: Nth shape in shapes list</param>
    /// <param name="position">Position of starting point of the shape</param>
    /// <param name="takeControl">True to immediately add blocks of this shape to controlledBlocks list</param>
    /// <param name="exactPosition">False to offset the shape with its startingPosition variable</param>
    /// <returns>True if was able to spawn the shape, false if spawn failed</returns>
    public bool SpawnNewShape(object shape, Vector2Int position, bool takeControl=false, bool exactPosition=true)
    {
        BlockShape targetShape = prop.blockShapes[0];
        
        // Set the targetShape to shape that will be spawned
        if (shape is BlockShape shape_BlockShape) targetShape = shape_BlockShape;
        else if (shape is string shape_string)
        {
            foreach (var possibleShape in prop.blockShapes)
            {
                if (possibleShape.name == shape_string)
                {
                    targetShape = possibleShape;
                    break;
                }
            }
        }
        else if (shape is int shape_int) targetShape = prop.blockShapes[shape_int];

        List<Vector2Int> inGridPositions = new();

        // Prepare all the positions 
        foreach (var blockPosition in targetShape.positions)
        {
            Vector2Int newPos = position + blockPosition;

            if (!exactPosition) { newPos += targetShape.startingPosition; }

            inGridPositions.Add(newPos);
        }

        // Check if the shape is obstructed
        foreach (var blockPosition in inGridPositions)
        {
            if (!IsPosInsideGrid(blockPosition))
            {
                print("Can't place shape: wall blocked the placement");
                return false;
            }

            if (blockGrid[blockPosition.x, blockPosition.y] != null)
            {
                print("Can't place shape: another block blocked the placement");
                return false;
            }
        }

        // Spawn the shape and previews
        foreach (var blockPosition in inGridPositions)
        {
            GameObject spawnedBlock = SpawnNewBlock(blockPosition, addToPreviews:true);

            if (takeControl)
            {
                controlledBlocks.Add(spawnedBlock.GetComponent<NormalBlock>());
                spawnedBlock.GetComponent<NormalBlock>().isControlled = true;
            }
        }

        // Set current shape to target shape if takeControl is true
        if (takeControl) currentShape = (BlockShape)targetShape.Clone();

        UpdateDashPreview();

        return true;
    }

    /// <summary>
    /// Universal funtion to spawn a block on the grid
    /// </summary>
    /// <param name="position">Position in the grid where to place the block</param>
    /// <param name="block_prefab">Block GameObject to spawn. Leave null to spawn a weighed-random block.</param>
    /// <returns>GameObject of the block spawned. Null if block failed to spawn</returns>
    public GameObject SpawnNewBlock(Vector2Int position, GameObject block_prefab = null, bool addToPreviews = false)
    {
        if (!IsPosInsideGrid(position))
        {
            print("Can't place block: wall blocked the placement");
            return null;
        }

        if (blockGrid[position.x, position.y] != null)
        {
            print("Can't place block: another block blocked the placement");
            return null;
        }

        // Check if target place is mined
        if (blockEffectGrid[position.x, position.y] == BlockEffect.MINED)
        {
            print("Block placed on mine");
            RemoveBlockEffect(position);
            return null;
        }

        // Choose block GO to place
        GameObject targetBlock = block_prefab;
        int blockIndex = 0;
        if (targetBlock == null)
        {
            blockIndex = WeighedRandomBlock();
            targetBlock = prop.blockPrefabs[blockIndex].block;
        }
        else
        {
            for (int i = 0; i < prop.blockPrefabs.Length; i++)
            {
                if (prop.blockPrefabs[i].block == targetBlock)
                {
                    blockIndex = i;
                }
            }
        }

        // Get world position
        Vector3 blockPos = GridToWorldCoords(position) + Vector3.up;

        // Spawn GO
        GameObject spawnedBlock = Instantiate(targetBlock, blockPos, targetBlock.transform.rotation, blocksParent);

        // Place in grid
        blockGrid[position.x, position.y] = spawnedBlock.GetComponent<NormalBlock>();

        // Add this block to TTs
        TransitionTransformation newTT = new TransitionTransformation()
        {
            name = "Block",
            gameObject = spawnedBlock,
            direction = Vector3.up,
            startPosition = blockPos,
        };
        vfxManager.gameBlockTTs.Add(newTT);
        spawnedBlock.GetComponent<NormalBlock>().transitionTransformation = newTT;

        // If dash preview is needed, spawn it and add to all needed lists
        if (addToPreviews)
        {
            GameObject blockPreview = Instantiate(prop.blockPrefabs[blockIndex].previewBlock, blockPos, targetBlock.transform.rotation, blockDashPreviewsParent);

            previewDashBlocks.Add(blockPreview.transform);

            // Add this block preview to TTs
            TransitionTransformation prevTT = new TransitionTransformation()
            {
                name = "Block Preview",
                gameObject = blockPreview,
                direction = Vector3.down,
                startPosition = blockPos,
            };
            vfxManager.gameBlockTTs.Add(prevTT);

            spawnedBlock.GetComponent<NormalBlock>().previewTT = prevTT;
        }

        // Return GO
        return spawnedBlock;
    }

    /// <summary>
    /// Universal function to remove a block from controlled blocks and the block grid
    /// </summary>
    /// <param name="targetBlock">NormalBlock object of the block to delete. Vector2Int position of the block to remove in the block grid</param>
    /// <returns>True if succesfully removed the block, false if removal failed</returns>
    public bool RemoveBlock(object targetBlock, bool ignoreBlockEffects = false)
    {
        // If given block is a block object
        if (targetBlock is NormalBlock block)
        {
            Vector2Int blockPos = block.GetPosInGrid(blockGrid);

            if (blockPos.x == -1) return false;

            // Check for protected block effect
            if (blockEffectGrid[blockPos.x, blockPos.y] == BlockEffect.PROTECTED && !ignoreBlockEffects)
            {
                RemoveBlockEffect(blockPos);
                return false;
            }

            blockGrid[blockPos.x, blockPos.y] = null;

            // Check if this block is controlled, then remove from controlled blocks if it is
            if (block.isControlled) controlledBlocks.Remove(block);

            // Delete block TT
            vfxManager.gameBlockTTs.Remove(block.transitionTransformation);

            // Delete the gameObject
            Destroy(block.gameObject);

            return true;
        }
        else if (targetBlock is Vector2Int blockPos)
        {
            // Same but check if the position is in the map first
            if (!IsPosInsideGrid(blockPos)) return false;
            if (blockGrid[blockPos.x, blockPos.y] == null) return false;

            // Check for protected block effect
            if (blockEffectGrid[blockPos.x, blockPos.y] == BlockEffect.PROTECTED && !ignoreBlockEffects)
            {
                RemoveBlockEffect(blockPos);
                return false;
            }

            if (blockGrid[blockPos.x, blockPos.y].isControlled) controlledBlocks.Remove(blockGrid[blockPos.x, blockPos.y]);

            // Delete block TT
            vfxManager.gameBlockTTs.Remove(blockGrid[blockPos.x, blockPos.y].transitionTransformation);

            Destroy(blockGrid[blockPos.x, blockPos.y].gameObject);

            blockGrid[blockPos.x, blockPos.y] = null;

            return true;
        }

        return false;
    }

    /// <summary>
    /// Universal function that clears given row, and moves all rows above it down
    /// </summary>
    /// <param name="y">The row that will be cleared</param>
    /// <param name="moveDownOtherRows">False to not move other rows down</param>
    public void ClearRow(int y, bool moveDownOtherRows = true)
    {
        if (y >= 0 && y < blockGrid.GetLength(1) - 4)
        {
            // Activate all blocks on that row that are activated 
            for (int x = 0; x < blockGrid.GetLength(0); x++) if (blockGrid[x, y] != null) if (blockGrid[x, y].activateOnClearRow) ActivateBlock(blockGrid[x, y]);

            DoQueuedBlockActions();

            bool rowWillGivePoints = true;

            // Remove blocks on that row
            for (int x = 0; x < blockGrid.GetLength(0); x++)
            {
                if (blockGrid[x, y] != null) if (blockGrid[x, y].GetComponent<BlockCursedRow>() != null) rowWillGivePoints = false;
                RemoveBlock(new Vector2Int(x, y), true);
            }

            if (moveDownOtherRows)
            {
                // Move all blocks above this row down
                for (int aboveY = y + 1; aboveY < blockGrid.GetLength(1); aboveY++)
                {
                    for (int x = 0; x < blockGrid.GetLength(0); x++)
                    {
                        if (blockGrid[x, aboveY] != null) blockGrid[x, aboveY].transform.position -= Vector3.up * 0.4f;
                        blockGrid[x, aboveY - 1] = blockGrid[x, aboveY];
                        if (aboveY == blockGrid.GetLength(1) - 1) blockGrid[x, aboveY] = null;

                        //if (blockEffectGrid[x, aboveY - 1] == BlockEffect.MINED)
                        //{
                        //    RemoveBlock(new Vector2Int(x, aboveY - 1));
                        //    RemoveBlockEffect(new Vector2Int(x, aboveY - 1));
                        //}
                    }
                }
            }

            if (rowWillGivePoints) extraClearedLines++;
            linesClearedForTrT++;

            if (linesClearedForTrT >= linesForTrashToken)
            {
                linesClearedForTrT -= linesForTrashToken;
                currentTrashTokens++;
            }
        }
    }

    // -= Block Actions =-

    // UNUSED \/
    /// <summary>
    /// Universal function to queue a block remove action. Actions are done in order they were queued. But block removes are done after row clears
    /// </summary>
    /// <param name="targetBlock">NormalBlock block object of block being removed. Vector2Int position of the block being removed.</param>
    public void QueueBlockRemoval(object targetBlock)
    {
        queuedActionsRemoveBlock.Add(new BlockAction
        {
            block = targetBlock,
            type = BlockActionType.REMOVEBLOCK
        });
    }

    /// <summary>
    /// Universal function to queue a block spawn action. Actions are done in order they were queued. But block spawns are done after block removes
    /// </summary>
    /// <param name="targetBlock">GameObject prefab of the block peing spawned. Null to spawn a weighed random block</param>
    /// <param name="position">Vector2Int position where to spawn</param>
    public void QueueBlockSpawn(object targetBlock, Vector2Int position)
    {
        queuedActionsSpawnBlock.Add(new BlockAction
        {
            block = targetBlock,
            position = position,
            type = BlockActionType.SPAWNBLOCK
        });
    }

    /// <summary>
    /// Universal function to queue a row clear action. Actions are done in order they were queued. But row clears are done first
    /// </summary>
    /// <param name="y">Y value (from 0 bottom to 19 top) of the row being cleared</param>
    /// <param name="moveDownOtherRows">True to move rows above this row down</param>
    public void QueueRowClear(int y, bool moveDownOtherRows=true)
    {
        queuedActionsClearrow.Add(new BlockAction
        {
            position = y,
            extra = moveDownOtherRows,
            type = BlockActionType.CLEARROW
        });
    }

    /// <summary>
    /// Universal function to queue a block stop action. That is - just lands the blocks in place
    /// </summary>
    public void QueueBlockStop()
    {
        stopActionQueued = true;
    }
    // UNUSED /\

    public void DoQueuedBlockActions()
    {
        List<BlockAction> actionsClearrow = queuedActionsClearrow.CloneViaFakeSerialization();
        List<BlockAction> actionsRemoveBlock = queuedActionsRemoveBlock.CloneViaFakeSerialization();
        List<BlockAction> actionsSpawnBlock = queuedActionsSpawnBlock.CloneViaFakeSerialization();
        List<BlockAction> actionsRemoveEffect = queuedActionsRemoveEffect.CloneViaFakeSerialization();
        List<BlockAction> actionsSpawnEffect = queuedActionsPlaceEffect.CloneViaFakeSerialization();
        bool stopAction = stopActionQueued;
        queuedActionsClearrow.Clear();
        queuedActionsRemoveBlock.Clear();
        queuedActionsSpawnBlock.Clear();
        queuedActionsRemoveEffect.Clear();
        queuedActionsPlaceEffect.Clear();
        stopActionQueued = false;

        // List all row clears and then sort
        List<BlockAction> rowsToClear = new();
        foreach (var action in actionsClearrow) if (action.type == BlockActionType.CLEARROW) rowsToClear.Add(action.Copy()); rowsToClear.Sort(new BASortDescent());
        rowsToClear = rowsToClear.DistinctBy(x => x.position).ToList();
        // Do all effect removals
        foreach (var action in actionsRemoveEffect)
        {
            if (action.type == BlockActionType.REMOVEEFFECT)
            {
                RemoveBlockEffect((Vector2Int)action.position);
            }
        }
        // Do all effect places
        foreach (var action in actionsSpawnEffect)
        {
            if (action.type == BlockActionType.PLACEEFFECT)
            {
                PlaceBlockEffect((Vector2Int)action.position, (BlockEffect)action.block);
            }
        }
        // Do all block removals
        foreach (var action in actionsRemoveBlock)
        {
            if (action.type == BlockActionType.REMOVEBLOCK)
            {
                if (action.block != null) RemoveBlock(action.block);
                else RemoveBlock(action.position);
            }
        }
        // Do all block spawns
        foreach (var action in actionsSpawnBlock)
        {
            if (action.type == BlockActionType.SPAWNBLOCK)
            {
                if (action.block == null) SpawnNewBlock((Vector2Int)action.position, null);
                else if (action.block is int) SpawnNewBlock((Vector2Int)action.position, prop.blockPrefabs[(int)action.block].block);
                else SpawnNewBlock((Vector2Int)action.position, (GameObject)action.block);
            }
        }
        // Land the blocks in stop action was queued
        if (stopAction) OnBlocksReachGround();
        // Do all row clears
        foreach (var action in rowsToClear)
        {
            if (action.type == BlockActionType.CLEARROW)
            {
                ClearRow((int)action.position, (bool)action.extra);
            }
        }

        UpdateDashPreview();
    }

    public void ActivateBlock(NormalBlock block)
    {
        BlockAction[] returnedActions = block.Activate(blockGrid);

        foreach (var action in returnedActions)
        {
            if      (action.type == BlockActionType.REMOVEEFFECT)  queuedActionsRemoveEffect.Add(action);
            else if (action.type == BlockActionType.PLACEEFFECT)   queuedActionsPlaceEffect.Add(action);
            else if (action.type == BlockActionType.REMOVEBLOCK)   queuedActionsRemoveBlock.Add(action);
            else if (action.type == BlockActionType.SPAWNBLOCK)    queuedActionsSpawnBlock.Add(action);
            else if (action.type == BlockActionType.CLEARROW)      queuedActionsClearrow.Add(action);
            else if (action.type == BlockActionType.STOP)          stopActionQueued = true;
        }
    }

    // -= Shape holding =-

    /// <summary>
    /// A function that moves currently controlled shape to held shape. IMPORTANT: This function ignores the shape that is currently held
    /// </summary>
    public void MoveCurrentShapeToHeld()
    {
        // Update all TTs directions
        foreach (var block in controlledBlocks)
        {
            block.transitionTransformation.direction = Vector3.right;
        }

        // Delete controlled shape from the grid
        foreach (var block in controlledBlocks)
        {
            Vector2Int blockPos = block.GetPosInGrid(blockGrid);
            blockGrid[blockPos.x, blockPos.y] = null;
        }

        // Mark all controlled blocks as not controlled
        foreach (var block in controlledBlocks)
            block.isControlled = false;

        // Move all controlled blocks to held shape preview and update TTs
        for (int i = 0; i < controlledBlocks.Count; i++)
        {
            Vector3 newBlockPos = GridToWorldCoords(heldShapePreviewStartingPosition + currentShape.startingPosition + currentShape.positions[i]) + Vector3.up;
            controlledBlocks[i].transform.position = newBlockPos;
            controlledBlocks[i].transitionTransformation.startPosition = newBlockPos;
        }

        // Add all controlled blocks to heldShapeBlocks list
        foreach (var block in controlledBlocks)
            heldShapeBlocks.Add(block.gameObject);

        // Clear controlled blocks list
        controlledBlocks.Clear();

        // Deactivate all preview blocks
        foreach (var preview in previewDashBlocks)
            preview.gameObject.SetActive(false);

        // Store all previews in the heldShapePreviews list
        foreach (var preview in previewDashBlocks)
            heldShapePreviews.Add(preview.gameObject);

        // Clear dash preview blocks list
        previewDashBlocks.Clear();

        // Set heldShape to current shape
        heldShape = (BlockShape)currentShape.Clone();
        
        // Set all rotations in held shape to 0
        foreach (var rotation in heldShape.rotations) rotation.nextRotationIndex = 0;
    }

    /// <summary>
    /// Universal function to hold currently controlled block and release currently held one
    /// </summary>
    public void SwapHeldBlocks()
    {
        // If no shape is held
        if (heldShapeBlocks.Count == 0)
        {
            MoveCurrentShapeToHeld();

            // Spawn new shape
            bool spawnedNewShape = SpawnRandomShape(shapeStartingPosition);
            if (!spawnedNewShape)
            {
                Lose();
            }

            // Reset block fall timer
            blockFallTimer = 1 / prop.blockFallSpeed;
        }
        else // If some shape is held
        {
            // Reserve currently held shape and it's dash previews
            List<GameObject> reservedShapeBlocks = heldShapeBlocks.CloneViaFakeSerialization();
            List<GameObject> reservedShapePreviews = heldShapePreviews.CloneViaFakeSerialization();
            BlockShape reservedShape = (BlockShape)heldShape.Clone();

            // Clear held lists
            heldShapeBlocks.Clear();
            heldShapePreviews.Clear();

            // Move current shape to held
            MoveCurrentShapeToHeld();

            // Move reserved lists to actual lists
            foreach (var reservedBlock in reservedShapeBlocks)
                controlledBlocks.Add(reservedBlock.GetComponent<NormalBlock>());
            foreach (var reservedPreview in reservedShapePreviews)
                previewDashBlocks.Add(reservedPreview.transform);
            currentShape = reservedShape;

            // Activate dash previews
            foreach (var preview in previewDashBlocks)
                preview.gameObject.SetActive(true);

            // Set controlled in reserved blocks to true
            foreach (var block in controlledBlocks)
                block.isControlled = true;

            // Move reserved blocks to shape starting position and update TTs
            for (int i = 0; i < controlledBlocks.Count; i++)
            {
                Vector2Int blockPos = shapeStartingPosition + currentShape.positions[i] + currentShape.startingPosition;
                controlledBlocks[i].transform.position = GridToWorldCoords(blockPos) + Vector3.up;
                controlledBlocks[i].transitionTransformation.startPosition = GridToWorldCoords(blockPos) + Vector3.up;

                // Also add the block to grid
                blockGrid[blockPos.x, blockPos.y] = controlledBlocks[i];
            }

            // Reset block fall timer
            blockFallTimer = 1 / prop.blockFallSpeed;

            // Update TTs directions
            foreach (var block in controlledBlocks)
            {
                block.transitionTransformation.direction = Vector3.up;
            }
        }

        UpdateNextShapePreview();
        UpdateDashPreview();
    }

    // -= Previews =-

    public void UpdateDashPreview()
    {
        // TODO: Finish block TTs
        if (controlledBlocks.Count != 0)
        {
            List<Vector2Int> positions = new();
            foreach (var block in controlledBlocks) positions.Add(block.GetPosInGrid(blockGrid));

            bool newPosIsObstructed = false;
            do
            {
                List<Vector2Int> newPositions = new();
                foreach (var position in positions) newPositions.Add(position + Vector2Int.down);
                positions = newPositions.CloneViaFakeSerialization();
                newPosIsObstructed = false;
                // Check if blocks are not obstructed
                foreach (var position in positions)
                {
                    if (!IsPosInsideGrid(position))
                    {
                        newPosIsObstructed = true;
                        break;
                    }

                    if (blockGrid[position.x, position.y] != null)
                        if (!blockGrid[position.x, position.y].isControlled)
                        {
                            newPosIsObstructed = true;
                            break;
                        }
                }
                if (newPosIsObstructed) break;
            }
            while (!newPosIsObstructed);

            // When found obstructed step
            List<Vector2Int> newPositionsMovedUp = new();
            foreach (var position in positions) newPositionsMovedUp.Add(position + Vector2Int.up);
            previewDashes = newPositionsMovedUp.CloneViaFakeSerialization();

            // Set all preview gameObjects to their places
            for (int i = 0; i < previewDashes.Count; i++)
            {
                previewDashBlocks[i].position = GridToWorldCoords(previewDashes[i]) + blockDashPreviewsParent.position;
            }

            // Set all preview TTs to their places
            for (int i = 0; i < controlledBlocks.Count; i++)
            {
                controlledBlocks[i].previewTT.startPosition = GridToWorldCoords(previewDashes[i]) + blockDashPreviewsParent.position;
            }
        }
        else
        {
            previewDashes.Clear();
        }
    }

    public void UpdateNextShapePreview()
    {
        // Delete previous next shape preview
        for (int i = 0; i < nextShapePreviewBlocks.Count; i++)
        {
            vfxManager.gameBlockTTs.Remove(nextShapeTTs[i]);
            Destroy(nextShapePreviewBlocks[i].gameObject);
        }
        nextShapePreviewBlocks.Clear();
        nextShapeTTs.Clear();

        // Spawn new preview
        foreach (var blockPos in nextShape.positions)
        {
            Vector3 worldBlockPos = GridToWorldCoords(nextShapePreviewStartingPosition + blockPos + nextShape.startingPosition);
            GameObject newBlock = Instantiate(prop.blockPrefabs[0].previewBlock, worldBlockPos, prop.blockPrefabs[0].previewBlock.transform.rotation, nextShapePreviewsParent);

            TransitionTransformation newTT = new TransitionTransformation()
            {
                gameObject = newBlock,
                direction = Vector3.left,
                name = "Next Shape",
                startPosition = worldBlockPos
            };

            nextShapePreviewBlocks.Add(newBlock.transform);
            vfxManager.gameBlockTTs.Add(newTT);
            nextShapeTTs.Add(newTT);
        }
    }

    // -= Block Effects =-

    /// <summary>
    /// Universal function to place a block effect
    /// </summary>
    /// <param name="position">Vector2Int position of new block effect in grid</param>
    /// <param name="type">BlockEffect type of target block effect</param>
    public void PlaceBlockEffect(Vector2Int position, BlockEffect type)
    {
        if (!IsPosInsideGrid(position))
        {
            print("Can't place block effect: position is outside of grid");
            return;
        }

        blockEffectGrid[position.x, position.y] = type;

        if (blockEffectGOGrid[position.x, position.y] != null)
        {
            Destroy(blockEffectGOGrid[position.x, position.y]);
            blockEffectGOGrid[position.x, position.y] = null;
            vfxManager.beTTtGrid[position.x, position.y] = null;
        }

        if (type != BlockEffect.NOTHING)
        {
            GameObject newBlEfGO = Instantiate(prop.blockEffectPrefabs[(int)type], GridToWorldCoords(position) + Vector3.back*2, prop.blockEffectPrefabs[(int)type].transform.rotation, blockEffectsParent);

            blockEffectGOGrid[position.x, position.y] = newBlEfGO;

            vfxManager.beTTtGrid[position.x, position.y] = new TransitionTransformation()
            {
                name = "BE TT",
                gameObject = newBlEfGO,
                direction = position.y <= 11 ? Vector3.down : Vector3.up,
                startPosition = GridToWorldCoords(position) + Vector3.back * 2
            };
        }
    }

    /// <summary>
    /// Wrapper for PlaceBlockEffect with type BlockEffect.NOTHING
    /// </summary>
    /// <param name="position">Vector2Int position of new block effect in grid</param>
    public void RemoveBlockEffect(Vector2Int position)
    {
        PlaceBlockEffect(position, BlockEffect.NOTHING);
    }

    // -= Pause =-

    public void Pause()
    {
        paused = true;
        vfxManager.blurringNow = true;
    }

    public void Unpause()
    {
        paused = false;
        vfxManager.blurringNow = false;
    }

    // -= Extra functions =-

    public Vector3 GridToWorldCoords(Vector2Int position)
    {
        return new Vector3((-4.5f * 0.4f) + position.x * 0.4f, (-11.5f * 0.4f) + position.y * 0.4f, 0);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>An index of weighed random block from blockPrefabs list. If somehow the weight overflows past the list, returns 0</returns>
    public int WeighedRandomBlock()
    {
        float weightTotal = 0;

        for (int i = 0; i < prop.blockPrefabs.Length; i++) if (equippedBlocks[i]) weightTotal += prop.blockPrefabs[i].Weight;

        float randomWeight = UnityEngine.Random.Range(0, weightTotal);

        for (int i = 0; i < prop.blockPrefabs.Length; i++)
        {
            if (equippedBlocks[i])
            {
                if (randomWeight <= prop.blockPrefabs[i].Weight) { return i; }
                else { randomWeight -= prop.blockPrefabs[i].Weight; }
            }
        }

        return 0;
    }

    public bool IsPosInsideGrid(Vector2Int position)
    {
        return position.x >= 0 && position.y >= 0 && position.x < blockGrid.GetLength(0) && position.y < blockGrid.GetLength(1);
    }

    /// <summary>
    /// A function that is called when the player loses
    /// </summary>
    public void Lose()
    {
        print("you lost ig");
    }

    public void OnDrawGizmos()
    {
        for (int x = 0; x < blockGrid.GetLength(0); x++)
        {
            for (int y = 0; y < blockGrid.GetLength(1); y++)
            {
                if (debugRenderBoard)
                {
                    Gizmos.color = Color.yellow;
                    if (y >= 20)
                        Gizmos.color = Color.green;
                    if (blockGrid[x, y] != null)
                        Gizmos.color = Color.red;

                    bool isInPrev = false;
                    foreach (var blockPrev in previewDashes) { if (blockPrev == new Vector2Int(x, y)) { isInPrev = true; break; } }

                    if (isInPrev)
                        Gizmos.color = Color.magenta;

                    Gizmos.DrawSphere(GridToWorldCoords(new Vector2Int(x, y)), 0.1f);
                }
            }
        }
    }
}

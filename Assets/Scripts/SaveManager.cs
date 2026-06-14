using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using Tetrex.DataStructures;
using UnityEditor;

public class SaveManager : MonoBehaviour
{
    [SerializeField] PropertiesScriptableObject prop;

    [SerializeField] GameController controller;
    [SerializeField] ShopController shopController;
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] VFXManager vfxManager;
    [SerializeField] SoundController soundController;

    [SerializeField] bool inGame;
    [SerializeField] bool inShop;
    [SerializeField] bool inMenu;

    [SerializeField] Material shopBgMaterial;
    [SerializeField] Material transitionMaterial;

    void Awake()
    {
        if (inGame)
            RoundStart();
        else if (inShop)
            ShopStart();
    }

    public void LoadEquippedBlocks()
    {
        int savedNumbers = Mathf.CeilToInt(controller.equippedBlocks.Length / 32f);

        for (int intIndex = 0; intIndex < savedNumbers; intIndex++)
        {
            int saveNumber = PlayerPrefs.GetInt("EquippedBlocks" + intIndex, intIndex == 0 ? 1 : 0);

            for (int bitIndex = 0; bitIndex < 32; bitIndex++)
            {
                if (intIndex * 32 + bitIndex < controller.equippedBlocks.Length)
                    controller.equippedBlocks[intIndex * 32 + bitIndex] = GetBitInInt(saveNumber, bitIndex);
            }
        }
    }

    public void SaveEquippedBlocks()
    {
        int[] intsToSave = new int[Mathf.CeilToInt(shopController.prop.allBlocks.Length / 32f)];

        // Setup the packed info
        foreach (var eqBlock in shopController.equippedBlocks)
        {
            intsToSave[Mathf.FloorToInt(eqBlock.saveBitIndex / 32f)] |= 1 << (eqBlock.saveBitIndex % 32);
        }

        // Make sure normal block is always equipped
        intsToSave[0] |= 1;

        // Other forced blocks
        List<string> forcedBlocks = new();

        // Decide what blocks to force
        if (LevelToLevelInStage(PlayerPrefs.GetInt("level")) + 1 == 3) forcedBlocks.Add("Cursed Row");

        // Force the blocks
        foreach (var fBlock in forcedBlocks)
        {
            foreach (var sb in prop.allBlocks)
            {
                if (sb.name == fBlock)
                {
                    intsToSave[Mathf.FloorToInt(sb.saveBitIndex / 32f)] |= 1 << (sb.saveBitIndex % 32);
                }
            }
        }

        // Save
        for (int i = 0; i < intsToSave.Length; i++)
        {
            PlayerPrefs.SetInt("EquippedBlocks" + i, intsToSave[i]);
        }
        PlayerPrefs.Save();
    }

    public void RoundStart()
    {
        LoadEquippedBlocks();

        int level = PlayerPrefs.GetInt("level");

        scoreManager.scoreGoal = prop.scoreGoals[level];

        if (LevelToLevelInStage(level) == 3)
        {
            soundController.PlayStageBossMusic(LevelToStage(level));
        } else
        {
            soundController.PlayStageMusic(LevelToStage(level));
        }

        List<int> loadedPermInts = new();
        int savedPermIntsCount = PlayerPrefs.GetInt("SavedPermInts");
        for (int i = 0; i < savedPermIntsCount; i++) // Load
        {
            loadedPermInts.Add(PlayerPrefs.GetInt("BoughtPerm" + i));
        }
        for (int intIndex = 0; intIndex < loadedPermInts.Count; intIndex++) // Unpack
        {
            for (int byteIndex = 0; byteIndex < 4; byteIndex++)
            {
                controller.perms[intIndex * 4 + byteIndex] = (loadedPermInts[intIndex] >> (byteIndex * 8)) & 0b11111111;
            }
        }

        controller.currentTrashTokens = prop.startTrashTokens + controller.perms[(int)Perm.TrashToken];

        vfxManager.curLevel = level;
        vfxManager.UpdatePPBPointsTextures();
    }

    public void RoundEnd()
    {
        // Calculate and give money
        int moneyGain = 1 + Mathf.Max(0, 1 + Mathf.FloorToInt(Mathf.Log((scoreManager.score - scoreManager.scoreGoal) / 1000, 2f)));
        print("Score: " + scoreManager.score + "; Money gained: " + moneyGain + "; Previous money: " + PlayerPrefs.GetInt("money") + "; New money: " + (PlayerPrefs.GetInt("money") + moneyGain));
        PlayerPrefs.SetInt("money", PlayerPrefs.GetInt("money") + moneyGain);

        // If this was a boss
        if (LevelToLevelInStage(PlayerPrefs.GetInt("level")) == 3)
        {
            // Increase sell tokens
            PlayerPrefs.SetInt("sellTokens", PlayerPrefs.GetInt("sellTokens") + 1);

            // Increase shop slots
            PlayerPrefs.SetInt("shopSlots", PlayerPrefs.GetInt("shopSlots") + 1);

            // Prepare to refresh perm
            PlayerPrefs.SetInt("RefreshPerm", 1);
        }

        PlayerPrefs.Save();
        vfxManager.roundEndingAnimation = true;
    }

    public void ShopStart()
    {
        shopController.money = PlayerPrefs.GetInt("money", 0);
        shopController.sellTokens = PlayerPrefs.GetInt("sellTokens", 1);
        shopController.shopSlots = PlayerPrefs.GetInt("shopSlots", 2);

        // Load inventory
        List<int> loadedInts = new();
        int savedInventoryInts = PlayerPrefs.GetInt("SavedInventoryInts");
        for (int i = 0; i < savedInventoryInts; i++)
        {
            loadedInts.Add(PlayerPrefs.GetInt("EquippedInventory" + i));
        }

        foreach (int loadedInt in loadedInts)
        {
            for (int byteIndex = 0; byteIndex < 4; byteIndex++)
            {
                if (((loadedInt >> (byteIndex * 8)) & 0b11111111) != 0)
                {
                    shopController.AddItem(false, true, ((loadedInt >> (byteIndex * 8)) & 0b11111111) - 1);
                }
            }
        }

        // Load bought perms
        List<int> loadedPermInts = new();
        int savedPermIntsCount = PlayerPrefs.GetInt("SavedPermInts");
        for (int i = 0; i < savedPermIntsCount; i++) // Load
        {
            loadedPermInts.Add(PlayerPrefs.GetInt("BoughtPerm" + i));
        }
        for (int intIndex = 0; intIndex < loadedPermInts.Count; intIndex++) // Unpack
        {
            for (int byteIndex = 0; byteIndex < 4; byteIndex++)
            {
                shopController.boughtPerms[intIndex * 4 + byteIndex] = (loadedPermInts[intIndex] >> (byteIndex * 8)) & 0b11111111;
            }
        }

        // Refresh or load perm in shop
        if (PlayerPrefs.GetInt("RefreshPerm") == 1) // Refresh
        {
            int newPermId = shopController.WeighedRandomShopItem(prop.shopPerms, null, ItemPool.PERM);
            if (newPermId == -1) newPermId = 0;
            shopController.thisStagePermIndex = newPermId;
        }
        else // Load
        {
            shopController.thisStagePermIndex = PlayerPrefs.GetInt("StagePermID");
        }

        shopController.RestockShop();
        soundController.PlayShopMusic();
    }

    public void ShopEnd()
    {
        vfxManager.shopEndingAnimation = true;

        // Save equipped blocks so main game can use them
        SaveEquippedBlocks();

        PlayerPrefs.SetInt("money", shopController.money);

        // Save inventory
        List<byte> bytes = new();
        List<int> intsToSave = new();
        for (int blockInd = 0; blockInd < shopController.equippedBlocks.Count; blockInd++)
        {
            bytes.Add((byte)shopController.equippedBlocks[blockInd].saveBitIndex);

            if (bytes.Count >= 4)
            {
                int intToSave = bytes[0] + (bytes[1]<<8) + (bytes[2]<<16) + (bytes[3]<<24);
                intsToSave.Add(intToSave);
                bytes.Clear();
            }
        }

        // If some bytes were still not saved
        if (bytes.Count > 0)
        {
            int intToSave = bytes[0] + (bytes.Count > 1 ? bytes[1] << 8 : 0) + (bytes.Count > 2 ? bytes[2] << 16 : 0) + (bytes.Count > 3 ? bytes[3] << 24 : 0);
            intsToSave.Add(intToSave);
            bytes.Clear();
        }

        for (int intIndex = 0; intIndex < intsToSave.Count; intIndex++)
        {
            PlayerPrefs.SetInt("EquippedInventory" + intIndex, intsToSave[intIndex]);
        }
        PlayerPrefs.SetInt("SavedInventoryInts", intsToSave.Count);

        int fromlvl = PlayerPrefs.GetInt("level");

        // This does not allow for gaps in perm IDs
        // Save perms
        List<byte> permBytes = new();
        List<int> permInts = new();
        for (int i = 0; i < prop.shopPerms.Length; i++)
        {
            permBytes.Add((byte)shopController.boughtPerms[i]);

            if (permBytes.Count >= 4)
            {
                int permInt = permBytes[0] + (permBytes[1] << 8) + (permBytes[2] << 16) + (permBytes[3] << 24);
                permInts.Add(permInt);
                permBytes.Clear();
            }
        }
        // If some bytes were still not saved
        if (permBytes.Count > 0)
        {
            int permInt = permBytes[0] + (permBytes.Count > 1 ? permBytes[1] << 8 : 0) + (permBytes.Count > 2 ? permBytes[2] << 16 : 0) + (permBytes.Count > 3 ? permBytes[3] << 24 : 0);
            permInts.Add(permInt);
            permBytes.Clear();
        }

        for (int intIndex = 0; intIndex < permInts.Count; intIndex++)
        {
            PlayerPrefs.SetInt("BoughtPerm" + intIndex, permInts[intIndex]);
        }
        PlayerPrefs.SetInt("SavedPermInts", permInts.Count);
        
        // Save this stage perm
        PlayerPrefs.SetInt("StagePermID", shopController.stagePermBought ? int.MaxValue : shopController.thisStagePermIndex);
        PlayerPrefs.SetInt("RefreshPerm", 0);

        // Increase level by one
        PlayerPrefs.SetInt("level", fromlvl + 1);

        // Save sell tokens
        PlayerPrefs.SetInt("sellTokens", shopController.sellTokens);

        PlayerPrefs.Save();
    }

    public void StartRun()
    {
        // Delete prev run info
        for (int i = 0; i < 128; i++)
        {
            // I'm sure if I release this someone will find a way to equip 513 blocks and get a few permanently saved ones but this will do for now
            PlayerPrefs.DeleteKey("EquippedInventory" + i);
            PlayerPrefs.DeleteKey("EquippedBlocks" + i);
            PlayerPrefs.DeleteKey("BoughtPerm" + i);
        }

        PlayerPrefs.DeleteKey("SavedInventoryInts");
        PlayerPrefs.DeleteKey("SavedPermInts");

        // Set starting stats
        PlayerPrefs.SetInt("money", prop.startMoney);
        PlayerPrefs.SetInt("level", 1);
        PlayerPrefs.SetInt("sellTokens", prop.startSellTokens);
        PlayerPrefs.SetInt("shopSlots", prop.startShopSlots);

        // Reset materials
        shopBgMaterial.SetFloat("_Transition_Progress", 1f);
        transitionMaterial.SetFloat("_Transition_Progress", 1f);

        // Prepare perm refresh
        PlayerPrefs.SetInt("RefreshPerm", 1);

        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x">Int to check</param>
    /// <param name="index">Index of bit to check. Lowest (rightmost) value bit is 0 and highest (leftmost) is 31</param>
    /// <returns>True if given bit is 1, false if 0</returns>
    public bool GetBitInInt(int x, int index)
    {
        return ((x >> index) & 1) == 1;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x">Int to set</param>
    /// <param name="index">Index of bit to set. Lowest (rightmost) value bit is 0 and highest (leftmost) is 31</param>
    /// <param name="value">True to set the bit to 1, false to 0</param>
    /// <returns>The modified int</returns>
    public int SetBitInByte(int x, int index, bool value)
    {
        return (x & ~GetOneBitInt(index)) | (value ? GetOneBitInt(index) : 0);
    }

    public int GetOneBitInt(int index)
    {
        return 1 << index;
    }

    public int LevelToStage(int level)
    {
        return ((level - 1) / 3) + 1;
    }

    public int LevelToLevelInStage(int level)
    {
        return level - (LevelToStage(level) - 1) * 3;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Tetrex.DataStructures;

public class ShopController : MonoBehaviour
{
    public PropertiesScriptableObject prop;

    public SaveManager saveManager;
    public VFXManager vfxManager;

    [Header("Shop blocks")]
    public int selectedShopBlock;
    /// <summary>
    /// GameObjects of shop blocks that are currently in stock
    /// </summary>
    public List<GameObject> shopBlocksGO = new();
    /// <summary>
    /// ShopBlock objects of shop blocks that are currently in stock
    /// </summary>
    public List<ShopBlock> shopBlocks = new();
    public Transform shopBlocksParent;

    [Header("Equipped blocks")]
    public int selectedEquippedBlock;
    /// <summary>
    /// GameObjects of shop blocks that are currently equipped
    /// </summary>
    public List<GameObject> equippedBlocksGO = new();
    /// <summary>
    /// ShopBlock objects of shop blocks that are currently equipped
    /// </summary>
    public List<ShopBlock> equippedBlocks = new();
    public Transform equippedBlocksParent;

    [Header("Perms")]
    public int[] boughtPerms;
    public int thisStagePermIndex;
    public bool stagePermBought = false;

    [Header("Scroll blocks positioning")]
    public Transform shopBlocksStart;
    public Transform equippedBlocksStart;

    [Header("Blocks scrolling animation")]
    /// <summary>
    /// Direction of last scroll of shop blocks
    /// </summary>
    float shopBlockSwitchDirection;
    float shopBlockSwitchTimer;
    /// <summary>
    /// Direction of last scroll of equipped blocks
    /// </summary>
    float equippedBlockSwitchDirection;
    float equippedBlockSwitchTimer;

    [Header("Scroll switch animation")]
    public float blockScrollSwitchTimer;

    [Header("Selected item preview")]
    public GameObject blockPreview;
    public SpriteRenderer selectedBlockRenderer;
    public TextMeshPro desciptionText;
    public TextMeshPro notesText;
    public TextMeshPro priceText;
    public TextMeshPro currenciesText;

    [SerializeField] SpriteRenderer destructIcon;
    [SerializeField] SpriteRenderer activClearRowIcon;
    [SerializeField] SpriteRenderer activLandIcon;
    [SerializeField] SpriteRenderer activUserInputIcon;

    [SerializeField] Color blockPropActiveColor;
    [SerializeField] Color blockPropInactiveColor;

    [Header("Shop")]
    public bool boughtOneBlock;
    public TextObject buyExitGuideText;
    public int money;
    public int sellTokens;
    public int shopSlots;

    public bool shopScrollSelected = true;
    float prevX;
    float prevY;
    float prevBuyInp;

    public bool CanEndShop
    {
        get {
            return boughtOneBlock;
        }
    }
    
    void Start()
    {
        //RestockShop();
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        float buyInp = Input.GetAxisRaw("Buy");

        // Select appropriate scroll
        if (x != prevX && x != 0)
        {
            if (shopScrollSelected != (x == -1f))
            {
                SelectBlockScroll(x == -1f);
            }
        }

        // Move selection index
        if (y != 0 && prevY != y)
        {
            // Move appropriate selection index
            if (shopScrollSelected)
            {
                selectedShopBlock -= (int)y;

                // If selection actually moved
                if (selectedShopBlock == Mathf.Clamp(selectedShopBlock, 0, shopBlocksGO.Count - 1))
                {
                    shopBlockSwitchDirection = y;
                    shopBlockSwitchTimer = 1f;
                }

                selectedShopBlock = Mathf.Clamp(selectedShopBlock, 0, shopBlocksGO.Count - 1);
            }
            else
            {
                selectedEquippedBlock -= (int)y;

                // If selection actually moved
                if (selectedEquippedBlock == Mathf.Clamp(selectedEquippedBlock, 0, equippedBlocksGO.Count - 1))
                {
                    equippedBlockSwitchDirection = y;
                    equippedBlockSwitchTimer = 1f;
                }

                selectedEquippedBlock = Mathf.Clamp(selectedEquippedBlock, 0, equippedBlocksGO.Count - 1);
            }
        }

        // If pressed buy button, try to buy selected shop block
        if (buyInp == 1 && prevBuyInp == 0)
        {
            // If selected shop scroll
            if (shopScrollSelected)
            {
                BuyShopItem(selectedShopBlock);
            }
            else // If Selected equipped scroll
            {
                SellEquippedBlock(selectedEquippedBlock);
            }
        }

        // Place shop blocks
        for (int i = 0; i < shopBlocksGO.Count; i++)
        {
            float transitionProgress = vfxManager.shopBgMaterial.GetFloat("_Transition_Progress");

            // Place block in normal spot
            float switchAnimationOffset = (1f - Mathf.Cos(shopBlockSwitchTimer * Mathf.PI / 2f)) * shopBlockSwitchDirection;
            shopBlocksGO[i].transform.position =
                shopBlocksStart.position +
                new Vector3(
                    0f,
                    selectedShopBlock - i + switchAnimationOffset - (transitionProgress >= 0.9f ? 100f : Mathf.Pow(Mathf.Tan(transitionProgress * Mathf.PI / 2f), 3f)),
                    0f
                ) * prop.scrollBlockSpacing;

            // Block scroll switch stuff
            float rawSwitchMod = Mathf.Cos(blockScrollSwitchTimer * Mathf.PI / 2f);
            float scrollSwitchMod = shopScrollSelected ? rawSwitchMod : 1f - rawSwitchMod;

            // Move away from center
            float distToCenterUnscaled = shopBlocksGO[i].transform.position.y - shopBlocksStart.position.y;
            float addedY = prop.centralScrollBlockAddedSpacing * Mathf.Clamp(distToCenterUnscaled / prop.scrollBlockSpacing, -1f, 1f);
            shopBlocksGO[i].transform.position = shopBlocksGO[i].transform.position + Vector3.up * addedY * scrollSwitchMod;

            // Set block size
            float distToCenter = Mathf.Abs(shopBlocksStart.position.y - shopBlocksGO[i].transform.position.y);
            float sizeMod = GetBlockSizeMod(distToCenter, scrollSwitchMod);
            shopBlocksGO[i].transform.localScale = Vector3.one * Mathf.Lerp(prop.normalBlockSize, prop.centralBlockSize, sizeMod * scrollSwitchMod);
        }

        // Place equipped blocks
        for (int i = 0; i < equippedBlocksGO.Count; i++)
        {
            float transitionProgress = vfxManager.shopBgMaterial.GetFloat("_Transition_Progress");

            // Place block in normal spot
            float switchAnimationOffset = (1f - Mathf.Cos(equippedBlockSwitchTimer * Mathf.PI / 2f)) * equippedBlockSwitchDirection;
            equippedBlocksGO[i].transform.position =
                equippedBlocksStart.position +
                new Vector3(
                    0f,
                    selectedEquippedBlock - i + switchAnimationOffset - (transitionProgress >= 0.9f ? 100f : Mathf.Pow(Mathf.Tan(transitionProgress * Mathf.PI / 2f), 3f)),
                    0f
                ) * prop.scrollBlockSpacing;

            // Block scroll switch stuff
            float rawSwitchMod = Mathf.Cos(blockScrollSwitchTimer * Mathf.PI / 2f);
            float scrollSwitchMod = shopScrollSelected ? 1f - rawSwitchMod : rawSwitchMod;

            // Move away from center
            float distToCenterUnscaled = equippedBlocksGO[i].transform.position.y - equippedBlocksStart.position.y;
            float addedY = prop.centralScrollBlockAddedSpacing * Mathf.Clamp(distToCenterUnscaled / prop.scrollBlockSpacing, -1f, 1f);
            equippedBlocksGO[i].transform.position = equippedBlocksGO[i].transform.position + Vector3.up * addedY * scrollSwitchMod;

            // Set block size
            float distToCenter = Mathf.Abs(equippedBlocksStart.position.y - equippedBlocksGO[i].transform.position.y);
            float sizeMod = GetBlockSizeMod(distToCenter, scrollSwitchMod);
            equippedBlocksGO[i].transform.localScale = Vector3.one * Mathf.Lerp(prop.normalBlockSize, prop.centralBlockSize, sizeMod * scrollSwitchMod);
        }

        // Set block stats preview
        blockPreview.SetActive((shopScrollSelected && shopBlocksGO.Count > 0) || (!shopScrollSelected && equippedBlocksGO.Count > 0));
        if ((shopScrollSelected && shopBlocksGO.Count > 0) || (!shopScrollSelected && equippedBlocksGO.Count > 0))
        {
            ShopBlock selectedBlock = shopScrollSelected ? shopBlocks[selectedShopBlock] : equippedBlocks[selectedEquippedBlock];
            GameObject selectedBlockGO = shopScrollSelected ? shopBlocksGO[selectedShopBlock] : equippedBlocksGO[selectedEquippedBlock];

            // Block Preview
            selectedBlockRenderer.sprite = selectedBlockGO.GetComponent<SpriteRenderer>().sprite;

            // Block Text
            desciptionText.text = selectedBlock.description;
            notesText.text = selectedBlock.notes;
            priceText.text = shopScrollSelected ? selectedBlock.cost + "ψ" : Mathf.CeilToInt(selectedBlock.cost/2f) + "ψ";

            // Block Properties
            destructIcon.color = selectedBlock.blockSelfDestructs ? blockPropActiveColor : blockPropInactiveColor;
            activClearRowIcon.color = selectedBlock.blockActivatesOnClearRow ? blockPropActiveColor : blockPropInactiveColor;
            activLandIcon.color = selectedBlock.blockActivatesOnLanding ? blockPropActiveColor : blockPropInactiveColor;
            activUserInputIcon.color = selectedBlock.blockActivatesOnUserInput ? blockPropActiveColor : blockPropInactiveColor;
        }

        // If pressed E, save and get to the game
        if (buyInp == -1 && prevBuyInp != buyInp)
        {
            if (CanEndShop)
            saveManager.ShopEnd();
        }

        // Update the currencies text
        currenciesText.text = money + "ψ " + sellTokens + "π";

        // Previous variables
        shopBlockSwitchTimer = Mathf.Clamp(shopBlockSwitchTimer - Time.deltaTime / prop.scrollBlockSwitchDuration, 0f, 1f);
        equippedBlockSwitchTimer = Mathf.Clamp(equippedBlockSwitchTimer - Time.deltaTime / prop.scrollBlockSwitchDuration, 0f, 1f);
        blockScrollSwitchTimer = Mathf.Clamp(blockScrollSwitchTimer - Time.deltaTime / prop.blockScrollSwitchDuration, 0f, 1f);
        prevX = x;
        prevY = y;
        prevBuyInp = buyInp;
    }

    float GetBlockSizeMod(float distanceToCenter, float scrollSwitchMod = 1f)
    {
        float totalNeededDist = prop.scrollBlockSpacing + (prop.centralScrollBlockAddedSpacing * scrollSwitchMod);
        return Mathf.Max((totalNeededDist - distanceToCenter) / totalNeededDist, 0f);
    }

    /// <summary>
    /// Universal function to place an item in a scroll in shop. Perms get added to the top and block to the bottom
    /// </summary>
    /// <param name="inShopScroll">True to place block in shop scroll, false to equipped scroll</param>
    /// <param name="isBlock">True to place a block, False to place a perm. Ignored if inShopScroll is false</param>
    /// <param name="index">Index of the block in allBlocks/shopPerms list</param>
    public void AddItem(bool inShopScroll, bool isBlock, int index)
    {
        if (inShopScroll)
        {
            if (isBlock)
            {
                ShopBlock newBlock = prop.allBlocks[index].Copy();
                shopBlocks.Add(newBlock);
                GameObject newBlockGO = Instantiate(newBlock.spritePrefab, shopBlocksParent);
                shopBlocksGO.Add(newBlockGO);
            }
            else
            {
                ShopBlock newPerm = prop.shopPerms[index].Copy();
                shopBlocks.Insert(0, newPerm);
                GameObject newPermGO = Instantiate(newPerm.spritePrefab, shopBlocksParent);
                shopBlocksGO.Insert(0, newPermGO);
            }
        }
        else
        {
            ShopBlock newBlock = prop.allBlocks[index].Copy();
            equippedBlocks.Add(newBlock);
            GameObject newBlockGO = Instantiate(newBlock.spritePrefab, equippedBlocksParent);
            equippedBlocksGO.Add(newBlockGO);
        }
    }

    /// <summary>
    /// Universal function to buy an item from shop and put it in equipped blocks if it's a block
    /// </summary>
    /// <param name="index">Index of the item in shopBlocks list</param>
    /// <param name="forceBuy">True to ignore cost and buy it anyway</param>
    /// <param name="spendMoney">True to decrease money owned by block's cost, can go below 0 if forceBuy is true</param>
    /// <returns>True if succesfully bought the item, false if failed to buy. Always returns true if forceBuy is true</returns>
    public bool BuyShopItem(int index, bool forceBuy = false, bool spendMoney = true)
    {
        if (shopBlocks.Count <= index) return false; // If index is outside the bounds of the list
        if (index < 0) return false; // Again
        if (shopBlocks[index].cost > money && !forceBuy && spendMoney) return false; // If player has not enough money

        if (shopBlocks[index].isPerm) // If this is a perm
        {
            // Buy
            if (spendMoney) money -= shopBlocks[index].cost;                                  // Take away money
            boughtPerms[shopBlocks[index].saveBitIndex]++;                                    // Increase bought perm count
            if (shopBlocks[index].saveBitIndex == thisStagePermIndex) stagePermBought = true; // If this is the stage perm, mark it as bought
            shopBlocks.RemoveAt(index);                                                       // Remove this block from shop blocks
            Destroy(shopBlocksGO[index]);                                                     // Remove GameObjectfrom the scene
            shopBlocksGO.RemoveAt(index);                                                     // Remove the GameObject from shop blocks
        }
        else // If this is a block
        {
            // Buy
            if (spendMoney) money -= shopBlocks[index].cost;                                  // Take away money
            shopBlocks[index].isEquipped = true;                                              // Set equipped in relevant shopBlock to true
            equippedBlocks.Add(shopBlocks[index]);                                            // Add this block to equipped
            shopBlocks.RemoveAt(index);                                                       // Remove this block from shop blocks
            shopBlocksGO[index].transform.SetParent(equippedBlocksParent);                    // Set transform parent to equipped blocks parent
            equippedBlocksGO.Add(shopBlocksGO[index]);                                        // Add this block to equipped blocks
            shopBlocksGO.RemoveAt(index);                                                     // Remove the GameObject from shop blocks
        }

        // Clamp selected index
        selectedShopBlock = Mathf.Clamp(selectedShopBlock, 0, shopBlocksGO.Count - 1);

        // If last block has been bought, move selection to equipped scroll
        if (shopBlocks.Count == 0)
        {
            SelectBlockScroll(false);
            selectedShopBlock = 0;
        }

        // Enable shop exiting
        boughtOneBlock = true;
        string continueText = "Press E to exit shop";
        float timePerChar = 0.01f;
        buyExitGuideText.ChangeText(continueText, buyExitGuideText.text.text.Length * timePerChar, continueText.Length * timePerChar);

        return true;
    }

    /// <summary>
    /// Universal function to sell an equipped block
    /// </summary>
    /// <param name="index">Index of the block in equippedBlocks list</param>
    /// <param name="giveMoney">True to give some money back</param>
    /// <param name="moneyReturnPerc">Modifier of how much money to return, rounded upwards after multiplying</param>
    /// <returns>True if succesfully sold the block, false if failed to sell</returns>
    public bool SellEquippedBlock(int index, bool giveMoney = true, float moneyReturnPerc = 0.5f, bool forceSell = false, bool spendSellToken = true)
    {
        if (equippedBlocks.Count <= index) return false; // If index is outside the bounds of the list
        if (index < 0) return false; // Again
        if (sellTokens <= 0 && !forceSell && spendSellToken) return false; // If there are not enough sell tokens

        // Move
        if (giveMoney) money += Mathf.CeilToInt(equippedBlocks[index].cost * moneyReturnPerc);
        equippedBlocks.RemoveAt(index);
        Destroy(equippedBlocksGO[index]);
        equippedBlocksGO.RemoveAt(index);
        if (spendSellToken && sellTokens > 0) sellTokens--;

        // Clamp selected index
        selectedEquippedBlock = Mathf.Clamp(selectedEquippedBlock, 0, equippedBlocksGO.Count - 1);

        // If last block has been sold, move selection to shop scroll
        if (equippedBlocks.Count == 0)
        {
            SelectBlockScroll(true);
            selectedEquippedBlock = 0;
        }

        return true;
    }

    /// <summary>
    /// Universal function to select a scroll
    /// </summary>
    /// <param name="selectShopScroll">True to select shop scroll, false to select equipped scroll</param>
    void SelectBlockScroll(bool selectShopScroll)
    {
        if ((selectShopScroll && shopBlocksGO.Count > 0) || (!selectShopScroll && equippedBlocksGO.Count > 0))
        {
            blockScrollSwitchTimer = 1;
            shopScrollSelected = selectShopScroll;
        }
    }

    public void RestockPerm()
    {
        // Make sure to not restock if htere is already a perm in shop
        bool permAlreadyInShop = false;
        foreach (var sb in shopBlocks) permAlreadyInShop = permAlreadyInShop || sb.isPerm;
        if (permAlreadyInShop) return;

        // No perms detected, place one
        if (thisStagePermIndex != int.MaxValue) AddItem(true, false, thisStagePermIndex);
    }

    public void RestockShop(int amountRestocked = 0)
    {
        int restockAmount = amountRestocked == 0 ? shopSlots - shopBlocks.Count : amountRestocked;
        bool anyBlockIsAffordable = false;

        RestockPerm();

        // Stock all needed blocks
        for (int i = 0; i < restockAmount; i++)
        {
            int randomBlock = WeighedRandomShopItem();

            // If not failed to generate a random block
            if (randomBlock != -1)
            {
                // If block is not last OR any of the blocks before were affordable
                if (i != restockAmount - 1 || anyBlockIsAffordable)
                {
                    if (prop.allBlocks[randomBlock].cost <= money) anyBlockIsAffordable = true;

                    AddItem(true, true, randomBlock);
                }

                // If block is last AND no block before was affordable
                else
                {
                    // Create a list of affordable blocks' weights
                    float[] affordableWeights = new float[prop.allBlocks.Length];
                    for (int j = 0; j < prop.allBlocks.Length; j++)
                    {
                        if (prop.allBlocks[j].cost <= money) affordableWeights[j] = prop.allBlocks[j].Weight;
                        else affordableWeights[j] = 0f;
                    }

                    // Select one of them
                    int newRandomBlock = WeighedRandomShopItem(prop.allBlocks, affordableWeights);

                    AddItem(true, true, newRandomBlock);
                }
            }
        }
    }

    /// <summary>
    /// Defaults to blocks
    /// </summary>
    /// <param name="listOfItems">List of ShopBlocks to choose from, leave as null to use normal prop.allBlocks list</param>
    /// <param name="listOfWeights">Additional list of weights to add to listOfBlocks, leave as null to use listOfItems' weights</param>
    /// <param name="pool">ItemPool that will be checked to ensure no repeats. NONE to not check any, BLOCK (default) to check blocks and PERM to check perms</param>
    /// <returns>An index of weighed random block from allBlocks list. If somehow the weight overflows past the list, returns -1</returns>
    public int WeighedRandomShopItem(ShopBlock[] listOfItems = null, float[] listOfWeights = null, ItemPool pool = ItemPool.BLOCK)
    {
        ShopBlock[] itemsList = listOfItems ?? prop.allBlocks;

        float[] itemsWeight = new float[itemsList.Length];
        for (int i = 0; i < itemsList.Length; i++) itemsWeight[i] = itemsList[i].Weight;

        float[] weightList = listOfWeights ?? itemsWeight;

        float weightTotal = 0;

        for (int i = 0; i < itemsList.Length; i++)
            if (
                (IsBlockInPool(itemsList[i].saveBitIndex) && pool == ItemPool.BLOCK) ||
                (IsPermInPool(itemsList[i].saveBitIndex) && pool == ItemPool.PERM) ||
                pool == ItemPool.NONE
            )
                weightTotal += weightList[i];

        float randomWeight = Random.Range(0f, weightTotal);

        for (int i = 0; i < itemsList.Length; i++)
        {
            if (
                (IsBlockInPool(itemsList[i].saveBitIndex) && pool == ItemPool.BLOCK) ||
                (IsPermInPool(itemsList[i].saveBitIndex) && pool == ItemPool.PERM) ||
                pool == ItemPool.NONE
            )
            {
                if (randomWeight <= weightList[i]) { return i; }
                else { randomWeight -= weightList[i]; }
            }
        }

        return -1;
    }

    /// <summary>
    /// Universal function that determines if the block is in pool and can be put in shop
    /// </summary>
    /// <param name="name">Name of the block to check</param>
    /// <returns>True if block is in pool (Not in shopBlocks or equippedBlocks)</returns>
    public bool IsBlockInPool(int id)
    {
        foreach (var item in shopBlocks) if (item.saveBitIndex == id && !item.isPerm) return false;
        foreach (var block in equippedBlocks) if (block.saveBitIndex == id) return false;
        return true;
    }

    /// <summary>
    /// Universal function that determines if the perm is in pool and can be put in shop
    /// </summary>
    /// <param name="name">Name of the perm to check</param>
    /// <returns>True if perm is in pool (Not in shopBlocks or boughtPerms)</returns>
    public bool IsPermInPool(int id)
    {
        foreach (var item in shopBlocks) if (item.saveBitIndex == id && item.isPerm) return false;
        if (boughtPerms[id] > 0 && prop.shopPerms[id].permType != PermType.STACKABLE) return false;
        return true;
    }
}

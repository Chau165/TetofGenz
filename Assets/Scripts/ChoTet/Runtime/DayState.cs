using System;
using System.Collections.Generic;
using ChoTet.Data;

namespace ChoTet.Runtime
{
    public enum GamePhase
    {
        Shopping,
        ReturnHome,
        Temple,
        Evaluation
    }

    [Serializable]
    public class PurchasedItemData
    {
        public ItemDefinition item;
        public int pricePaid;
        public int quality;
        public int luck;
        public string vendorName;
        public string qualityNote;
        public bool delivered;
    }

    [Serializable]
    public class DayState
    {
        public int currentMinutes;
        public int closeMinutes;
        public int budget;
        public int startingBudget;
        public int capacityLimit;
        public int currentCarry;
        public int deliveriesUsed;
        public bool missionAccepted;
        public bool planningCompleted;
        public bool questStarted;
        public bool marketVisited;
        public bool evaluationShown;
        public bool rainActive;
        public bool crowdTriggered;
        public bool friendDiscountTriggered;
        public bool flashSaleTriggered;
        public bool policeClearTriggered;
        public bool closingWarningTriggered;
        public bool templeQuestStarted;
        public bool templeGateVisited;
        public bool templeOfferingDone;
        public bool incenseLit;
        public bool fortuneDrawn;
        public bool templeCompleted;
        public GamePhase currentPhase;
        public bool isShoppingComplete;
        public int deliveryCollisionCount;
        public int deliveryItemDamage;
        public int playerLives = 3;
        public const int MaxPlayerLives = 3;
        public string routePlanId = "balanced";
        public string routePlanLabel = "Lộ trình cân bằng";
        public string objectiveText = string.Empty;
        public string flashSaleVendorId = "";
        public string blockedVendorId = "";
        public List<PurchasedItemData> purchases = new List<PurchasedItemData>();

        public void Reset(int startMinutes, int newCloseMinutes, int startBudget, int carryLimit)
        {
            currentMinutes = startMinutes;
            closeMinutes = newCloseMinutes;
            budget = startBudget;
            startingBudget = startBudget;
            capacityLimit = carryLimit;
            currentCarry = 0;
            deliveriesUsed = 0;
            missionAccepted = false;
            planningCompleted = false;
            questStarted = false;
            marketVisited = false;
            evaluationShown = false;
            rainActive = false;
            crowdTriggered = false;
            friendDiscountTriggered = false;
            flashSaleTriggered = false;
            policeClearTriggered = false;
            closingWarningTriggered = false;
            templeQuestStarted = false;
            templeGateVisited = false;
            templeOfferingDone = false;
            incenseLit = false;
            fortuneDrawn = false;
            templeCompleted = false;
            currentPhase = GamePhase.Shopping;
            isShoppingComplete = false;
            deliveryCollisionCount = 0;
            deliveryItemDamage = 0;
            playerLives = MaxPlayerLives;
            routePlanId = "balanced";
            routePlanLabel = "Lộ trình cân bằng";
            objectiveText = string.Empty;
            flashSaleVendorId = string.Empty;
            blockedVendorId = string.Empty;
            purchases.Clear();
        }

        public bool HasPurchased(ItemDefinition item)
        {
            if (item == null)
            {
                return false;
            }

            for (int i = 0; i < purchases.Count; i++)
            {
                if (purchases[i].item == item)
                {
                    return true;
                }
            }

            return false;
        }

        public PurchasedItemData GetPurchase(ItemDefinition item)
        {
            if (item == null)
            {
                return null;
            }

            for (int i = 0; i < purchases.Count; i++)
            {
                if (purchases[i].item == item)
                {
                    return purchases[i];
                }
            }

            return null;
        }

        public void AddPurchase(ItemDefinition item, int pricePaid, int quality, int luck, string vendorName, string qualityNote)
        {
            PurchasedItemData entry = new PurchasedItemData();
            entry.item = item;
            entry.pricePaid = pricePaid;
            entry.quality = quality;
            entry.luck = luck;
            entry.vendorName = vendorName;
            entry.qualityNote = qualityNote;
            entry.delivered = false;
            purchases.Add(entry);

            if (item != null)
            {
                currentCarry += item.capacityCost;
            }

            budget -= pricePaid;
        }

        public void SendAllHome()
        {
            currentCarry = 0;
            deliveriesUsed += 1;
            for (int i = 0; i < purchases.Count; i++)
            {
                purchases[i].delivered = true;
            }
        }

        public void CompleteShopping()
        {
            isShoppingComplete = true;
            currentPhase = GamePhase.ReturnHome;
        }

        public void StartEvaluation()
        {
            currentPhase = GamePhase.Evaluation;
        }

        public void RegisterDeliveryDamage(int damageAmount)
        {
            deliveryCollisionCount += 1;
            deliveryItemDamage += Math.Max(0, damageAmount);
        }
    }
}

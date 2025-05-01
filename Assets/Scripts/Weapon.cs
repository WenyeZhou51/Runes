using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Weapon : MonoBehaviour
{
    // Use a singleton-like approach for tracking shared state across all weapons
    private static class SharedState {
        public static List<Cards> persistentCards;
        public static int index = 0;
        
        // Add explicit methods for index manipulation to ensure consistency
        public static void SetIndex(int newValue, string source) {
            Debug.Log($"[AST DEBUG] SharedState.SetIndex called: changing from {index} to {newValue}, source: {source}");
            index = newValue;
        }
        
        public static int GetIndex(string source) {
            Debug.Log($"[AST DEBUG] SharedState.GetIndex called: current value {index}, source: {source}");
            return index;
        }
    }

    // Expose the shared index through a property
    public int Index {
        get {
            return SharedState.GetIndex($"Getter from {gameObject.name}");
        }
        set {
            SharedState.SetIndex(value, $"Setter from {gameObject.name}");
        }
    }

    public List<Cards> cards;
    private float nextAttack = 0;
    public int size;
    public int reloadTime;
    public GameObject player;
    [SerializeField] List<Cards> defaultCards;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Debug.Log($"[AST DEBUG] Weapon Awake called on {gameObject.name}");

        player = GameObject.FindGameObjectWithTag("Player");

        if (SharedState.persistentCards == null)
        {
            Debug.Log($"[AST DEBUG] Initializing persistentCards and static index for the first time");
            SharedState.persistentCards = new List<Cards>();
            for (int i = 0; i < size; i++)
            {
                SharedState.persistentCards.Add(null);
            }

            for (int i = 0; i < defaultCards.Count; i++)
            {
                Cards defCard = defaultCards[i];
                this.addToWeapon(defCard, i);
            }
            // Reset index when initializing persistentCards
            SharedState.SetIndex(0, "Initial setup in Awake");
            Debug.Log($"[AST DEBUG] Static index initialized to {SharedState.GetIndex("Awake check")}");
        }
        else
        {
            Debug.Log($"[AST DEBUG] persistentCards already exists, current static index: {SharedState.GetIndex("Awake existing check")}");
        }

        cards = SharedState.persistentCards;  // Link the instance cards to the static list
    }

    public void addToWeapon(Cards card, int insertIndex)
    {
        if (insertIndex >= 0 && insertIndex < size)
        {
            SharedState.persistentCards.RemoveAt(insertIndex);
            SharedState.persistentCards.Insert(insertIndex, card);
            Debug.Log($"[AST DEBUG] Card {card?.getCardName() ?? "null"} added at index {insertIndex}");
        }
    }

    public void removeFromWeapon(int removeIndex)
    {
        if (removeIndex >= 0 && removeIndex < size && SharedState.persistentCards[removeIndex] != null)
        {
            Debug.Log($"[AST DEBUG] Removing card {SharedState.persistentCards[removeIndex].getCardName()} from index {removeIndex}");
            SharedState.persistentCards.RemoveAt(removeIndex);
            SharedState.persistentCards.Insert(removeIndex, null);
        }
    }

    public void fireWeapon(Vector3 position, Quaternion rotation, bool external = false)
    {
        Debug.Log($"[AST DEBUG] fireWeapon called, static index before: {SharedState.GetIndex("fireWeapon start")}, Object: {gameObject.name}");
        //pos rot is right here
        AuxCards curCard;
        if (GetTargetActionCard(Index) == (null, -1, float.NaN))
        {
            Debug.Log($"[AST DEBUG] No target action card found, returning");
            return;
        }
        (ActionCards actionCard, int actionIndex, float manaCost) = GetTargetActionCard(Index);
        Debug.Log($"[AST DEBUG] GetTargetActionCard returned: actionCard={actionCard?.name ?? "null"}, actionIndex={actionIndex}, manaCost={manaCost}");

        //add time delay
        //time delay messing with the onImpact code.
        if (external)
        {
            Debug.Log($"[AST DEBUG] External call to fireWeapon");
            if (actionCard != null)
            {
                Debug.Log($"[AST DEBUG] Using action card: {actionCard.name}");
                actionCard.Use(position, rotation);
                while (!(SharedState.persistentCards[Index] is ActionCards))
                {
                    if (SharedState.persistentCards[Index] != null)
                    {
                        curCard = (AuxCards)SharedState.persistentCards[Index];
                        Debug.Log($"[AST DEBUG] Applying aux card: {curCard.getCardName()} to action card: {actionCard.name}");
                        curCard.applyMod(actionCard);
                    }

                    if (Index < SharedState.persistentCards.Count - 1)
                    {
                        Debug.Log($"[AST DEBUG] Incrementing index from {Index} to {Index + 1}");
                        Index++;
                    }
                    else
                    {
                        Debug.Log($"[AST DEBUG] Resetting index from {Index} to 0");
                        Index = 0;
                        return;
                    }
                }
            }
        }
        else if (Time.time >= nextAttack)
        {
            Debug.Log($"[AST DEBUG] Normal fireWeapon execution, time check passed");
            bool reloaded = false;
            float fireCost = 0;

            if (actionCard != null)
            {
                if (manaCost < player.GetComponent<PlayerController>().mana)
                {
                    Debug.Log($"[AST DEBUG] Sufficient mana, using action card: {actionCard.name}");
                    player.GetComponent<PlayerController>().mana -= manaCost;
                    actionCard.Use(position, rotation);
                    Debug.Log($"[AST DEBUG] After actionCard.Use, current Index: {Index}");
                    
                    // Check if a DoubleCast has manually set the index
                    // If not, continue with normal processing
                    if (!DoubleCastCard.HasManuallySetIndex)
                    {
                        Debug.Log($"[AST DEBUG] No manual index update detected, continuing with normal processing");
                        while (!(SharedState.persistentCards[Index] is ActionCards))
                        {
                            Debug.Log($"[AST DEBUG] In while loop, checking card at index {Index}: {SharedState.persistentCards[Index]?.getCardName() ?? "null"}");
                            if (SharedState.persistentCards[Index] != null)
                            {
                                curCard = (AuxCards)SharedState.persistentCards[Index];
                                Debug.Log($"[AST DEBUG] Applying aux card: {curCard.getCardName()} to action card: {actionCard.name}");
                                curCard.applyMod(actionCard);
                                fireCost += curCard.getDelay();
                            }

                            if (Index < SharedState.persistentCards.Count - 1)
                            {
                                Debug.Log($"[AST DEBUG] Incrementing index from {Index} to {Index + 1}");
                                Index++;
                            }
                            else
                            {
                                if (external)
                                {
                                    Debug.Log($"[AST DEBUG] External call, truncated");
                                    return;
                                }
                                Debug.Log($"[AST DEBUG] Resetting index from {Index} to 0");
                                Index = 0;
                                if (reloaded == false)
                                {
                                    fireCost += reloadTime;
                                    Debug.Log($"[AST DEBUG] Added reload time: {reloadTime}, fireCost now: {fireCost}");
                                }
                                reloaded = true;
                            }
                        }

                        fireCost += actionCard.getDelay();
                        Debug.Log($"[AST DEBUG] Added actionCard delay: {actionCard.getDelay()}, fireCost now: {fireCost}");
                        if (actionIndex == size - 1)
                        {
                            fireCost += reloadTime;
                            Debug.Log($"[AST DEBUG] Added reload time (end of list): {reloadTime}, fireCost now: {fireCost}");
                        }
                        nextAttack = Time.time + fireCost / 100f;
                        Debug.Log($"[AST DEBUG] Set nextAttack to: {nextAttack}, delay: {fireCost / 100f}");

                        // Only update the index if DoubleCast hasn't manually set it
                        Debug.Log($"[AST DEBUG] FINAL - Checking if we should update index (DoubleCastCard.HasManuallySetIndex = {DoubleCastCard.HasManuallySetIndex})");
                        if (!DoubleCastCard.HasManuallySetIndex)
                        {
                            if (Index < SharedState.persistentCards.Count - 1)
                            {
                                Debug.Log($"[AST DEBUG] FINAL - Incrementing index from {Index} to {Index + 1}");
                                Index++;
                            }
                            else
                            {
                                Debug.Log($"[AST DEBUG] FINAL - Resetting index from {Index} to 0");
                                Index = 0;
                            }
                        }
                        else
                        {
                            Debug.Log($"[AST DEBUG] FINAL - Skipping index update because DoubleCast already set it to {Index}");
                        }
                    }
                    else
                    {
                        Debug.Log($"[AST DEBUG] DoubleCast already set index to {Index}, skipping normal index processing");
                        
                        // Still calculate the delay for proper timing
                        fireCost += actionCard.getDelay();
                        if (actionIndex == size - 1)
                        {
                            fireCost += reloadTime;
                        }
                        nextAttack = Time.time + fireCost / 100f;
                        Debug.Log($"[AST DEBUG] Set nextAttack to: {nextAttack}, delay: {fireCost / 100f}");
                    }
                }
                else
                {
                    Debug.Log($"[AST DEBUG] Insufficient mana: required {manaCost}, have {player.GetComponent<PlayerController>().mana}");
                }
            }
            else
            {
                Debug.Log($"[AST DEBUG] No action card found");
            }
        }
        else
        {
            Debug.Log($"[AST DEBUG] Time check failed, nextAttack: {nextAttack}, current time: {Time.time}");
        }
        
        Debug.Log($"[DOUBLECAST CRITICAL DEBUG] End of fireWeapon: HasManuallySetIndex={DoubleCastCard.HasManuallySetIndex}");
        // CRITICAL FIX: Reset the HasManuallySetIndex flag at the end of fireWeapon
        // This ensures the flag is properly cleared for the next weapon fire
        DoubleCastCard.ResetHasManuallySetIndex();
        Debug.Log($"[AST DEBUG] fireWeapon completed, static index after: {SharedState.GetIndex("fireWeapon end")}, Object: {gameObject.name}");
    }

    public (ActionCards, int, float) GetTargetActionCard(int curIndex)
    {
        Debug.Log($"[AST DEBUG] GetTargetActionCard called with curIndex: {curIndex}");
        float manaCost = 0;
        int entryIndex = curIndex;
        if (SharedState.persistentCards[curIndex] != null && SharedState.persistentCards[curIndex] is ActionCards)
        {
            Debug.Log($"[AST DEBUG] Found action card directly at index {curIndex}: {SharedState.persistentCards[curIndex].getCardName()}");
            return ((ActionCards)SharedState.persistentCards[curIndex], curIndex, SharedState.persistentCards[curIndex].getManaCost());
        }
        else
        {
            Debug.Log($"[AST DEBUG] No action card at index {curIndex}, searching forward");
            if (curIndex < SharedState.persistentCards.Count - 1)
            {
                curIndex++;
                Debug.Log($"[AST DEBUG] Incremented curIndex to {curIndex}");
            }
            else
            {
                curIndex = 0;
                Debug.Log($"[AST DEBUG] Reset curIndex to 0");
            }
            if (SharedState.persistentCards[curIndex] != null)
            {
                manaCost += SharedState.persistentCards[curIndex].getManaCost();
                Debug.Log($"[AST DEBUG] Added mana cost from card at index {curIndex}: {SharedState.persistentCards[curIndex].getManaCost()}, total: {manaCost}");
            }
        }

        while (!(SharedState.persistentCards[curIndex] is ActionCards) && curIndex != entryIndex)
        {
            Debug.Log($"[AST DEBUG] Checking card at index {curIndex}: {(SharedState.persistentCards[curIndex] != null ? SharedState.persistentCards[curIndex].getCardName() : "null")}");
            if (curIndex < SharedState.persistentCards.Count - 1)
            {
                curIndex++;
                Debug.Log($"[AST DEBUG] Incremented curIndex to {curIndex}");
            }
            else
            {
                curIndex = 0;
                Debug.Log($"[AST DEBUG] Reset curIndex to 0");
            }
            if (SharedState.persistentCards[curIndex] != null)
            {
                manaCost += SharedState.persistentCards[curIndex].getManaCost();
                Debug.Log($"[AST DEBUG] Added mana cost from card at index {curIndex}: {SharedState.persistentCards[curIndex].getManaCost()}, total: {manaCost}");
            }
        }
        if (entryIndex == curIndex)
        {
            Debug.Log($"[AST DEBUG] Wrapped around without finding action card, returning null");
            return (null, -1, float.NaN);
        }
        Debug.Log($"[AST DEBUG] Found action card at index {curIndex}: {SharedState.persistentCards[curIndex].getCardName()}, total mana cost: {manaCost}");
        return ((ActionCards)SharedState.persistentCards[curIndex], curIndex, manaCost);
    }
}

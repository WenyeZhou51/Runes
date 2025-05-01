using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Double Cast", menuName = "Action Card/Double Cast")]
public class DoubleCastCard : ActionCards
{
    // Flag to prevent infinite recursion
    private static bool isProcessingDoubleCast = false;
    // Add an identifier to differentiate DoubleCast instances during debug
    private static int doublecastCounter = 0;
    private int instanceId;
    
    // Add a property to track the total delay when double casting
    public int CombinedDelay { get; private set; }
    private bool hasCalculatedDelay = false;
    
    // Flag to indicate that this DoubleCast has manually set the weapon index
    public static bool HasManuallySetIndex { get; private set; } = false;

    // Add a public method to reset the HasManuallySetIndex flag
    public static void ResetHasManuallySetIndex()
    {
        Debug.Log("[DOUBLECAST CRITICAL DEBUG] Resetting HasManuallySetIndex to FALSE via public method");
        HasManuallySetIndex = false;
    }

    private void OnEnable()
    {
        instanceId = ++doublecastCounter;
        Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Card initialized");
        // Reset the delay tracking
        CombinedDelay = getDelay();
        hasCalculatedDelay = false;
    }

    public override void Use(Vector2 useLoc, Quaternion useDir)
    {
        Debug.Log($"[DOUBLECAST CRITICAL DEBUG] [DoubleCast-{instanceId}] Entry state: IsProcessing={isProcessingDoubleCast}, HasManuallySetIndex={HasManuallySetIndex}");
        
        Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Use method called at position {useLoc}, isProcessing={isProcessingDoubleCast}");
        
        // Create visual effect for this Double Cast
        Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Calling base.Use() to create visual effect");
        base.Use(useLoc, useDir);

        // The key fix: We still want to show the visual effect but avoid recursion
        // We can still be part of another DoubleCast's card block even if we're processing
        if (isProcessingDoubleCast)
        {
            Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Nested Double Cast detected, showing visual effect only");
            return;
        }

        // Remember the original state of the manually set index flag
        bool originalManuallySetIndex = HasManuallySetIndex;
        Debug.Log($"[DOUBLECAST CRITICAL DEBUG] [DoubleCast-{instanceId}] Saved originalManuallySetIndex={originalManuallySetIndex}");
        
        // Set flag to indicate we're processing a Double Cast
        Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Setting isProcessingDoubleCast flag to true");
        isProcessingDoubleCast = true;
        // Reset the manual index flag
        HasManuallySetIndex = false;

        try
        {
            // Find the weapon component in the scene
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) 
            {
                Debug.LogError($"[AST DEBUG] [DoubleCast-{instanceId}] Player not found!");
                return;
            }
            Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Found player: {player.name}");

            Weapon weapon = player.GetComponentInChildren<Weapon>();
            if (weapon == null) 
            {
                Debug.LogError($"[AST DEBUG] [DoubleCast-{instanceId}] Weapon not found on player!");
                return;
            }
            Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Found weapon with current index: {weapon.Index}, total cards: {weapon.cards.Count}");

            // Get current index
            int currentIndex = weapon.Index;
            Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Current weapon index: {currentIndex}");
            
            // Find the first action card after this Double Cast
            Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Finding first action card after index {currentIndex}");
            (ActionCards firstAction, int firstActionIndex, float firstManaCost) = FindNextActionCard(weapon, currentIndex);
            
            if (firstAction == null)
            {
                Debug.LogWarning($"[AST DEBUG] [DoubleCast-{instanceId}] Double Cast couldn't find first action card");
                return;
            }
            
            // Check if the first action card is also a DoubleCast - if so, find the next action card instead
            if (firstAction is DoubleCastCard)
            {
                Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] First action card is another DoubleCast, using it as a trigger for visual effect only");
                
                // Show the visual for the DoubleCast
                firstAction.Use(useLoc, useDir);
                
                // Now find the next action card after this DoubleCast
                currentIndex = (firstActionIndex < weapon.cards.Count - 1) ? firstActionIndex + 1 : 0;
                Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Looking for a non-DoubleCast action card after index {currentIndex}");
                (firstAction, firstActionIndex, firstManaCost) = FindNextActionCard(weapon, currentIndex);
                
                if (firstAction == null)
                {
                    Debug.LogWarning($"[AST DEBUG] [DoubleCast-{instanceId}] Couldn't find a non-DoubleCast action card");
                    return;
                }
                
                if (firstAction is DoubleCastCard)
                {
                    Debug.LogWarning($"[AST DEBUG] [DoubleCast-{instanceId}] Too many consecutive DoubleCasts, stopping to prevent issues");
                    return;
                }
            }
            
            Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Found first action card: {firstAction.name} at index {firstActionIndex}, mana cost: {firstManaCost}");

            // Store the current index after finding the first action card
            currentIndex = (firstActionIndex < weapon.cards.Count - 1) ? firstActionIndex + 1 : 0;
            Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Updated currentIndex to {currentIndex} after finding first action card");
            
            // Find the second action card
            Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Finding second action card after index {currentIndex}");
            (ActionCards secondAction, int secondActionIndex, float secondManaCost) = FindNextActionCard(weapon, currentIndex);
            
            if (secondAction == null)
            {
                Debug.LogWarning($"[AST DEBUG] [DoubleCast-{instanceId}] Double Cast couldn't find second action card");
                // If second card not found, still fire the first one
                Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Firing only the first action card: {firstAction.name}");
                firstAction.Use(useLoc, useDir);
                
                // Use only the first card's delay in this case
                CombinedDelay = firstAction.getDelay();
                hasCalculatedDelay = true;
                Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Using first action card delay only: {CombinedDelay}");
                
                return;
            }
            Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Found second action card: {secondAction.name} at index {secondActionIndex}, mana cost: {secondManaCost}");

            // Calculate the combined delay from both action cards
            CombinedDelay = firstAction.getDelay() + secondAction.getDelay();
            hasCalculatedDelay = true;
            Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Calculated combined delay: {firstAction.getDelay()} + {secondAction.getDelay()} = {CombinedDelay}");
            
            // Update this card's delay to be the combined delay
            setDelay(CombinedDelay);
            Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Updated DoubleCast delay to combined value: {CombinedDelay}");
            
            Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Double Cast firing: {firstAction.name} and {secondAction.name}");

            // Fire the first action card
            Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Firing first action card: {firstAction.name}");
            firstAction.Use(useLoc, useDir);
            
            // Apply any aux cards that are between the first and second action cards
            Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Applying aux cards between indices {firstActionIndex} and {secondActionIndex} to the second action card");
            ApplyAuxCardsBetween(weapon, firstActionIndex, secondActionIndex, secondAction);
            
            // Fire the second action card immediately (no delay)
            Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Firing second action card: {secondAction.name}");
            secondAction.Use(useLoc, useDir);
            
            // Update the weapon index to be after the second action card
            int newIndex = (secondActionIndex < weapon.cards.Count - 1) ? secondActionIndex + 1 : 0;
            Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] CRITICAL - Updating weapon index from {weapon.Index} to {newIndex}");
            
            // Forcefully set the static index directly
            weapon.Index = newIndex;
            
            // Set the flag to indicate that we've manually set the index
            HasManuallySetIndex = true;
            Debug.Log($"[DOUBLECAST CRITICAL DEBUG] [DoubleCast-{instanceId}] Setting HasManuallySetIndex to TRUE");
            
            // Verify the index was updated
            Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] VERIFICATION - After setting index, current value: {weapon.Index}, HasManuallySetIndex={HasManuallySetIndex}");
        }
        finally
        {
            // Reset the flag when we're done, even if there was an error
            Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Resetting isProcessingDoubleCast flag to false");
            
            // CRITICAL FIX: We must not reset the HasManuallySetIndex flag here
            // We set isProcessingDoubleCast to false without changing HasManuallySetIndex
            isProcessingDoubleCast = false;
            
            Debug.Log($"[DOUBLECAST CRITICAL DEBUG] [DoubleCast-{instanceId}] Exit state: HasManuallySetIndex={HasManuallySetIndex}");
        }
    }

    // Use a new method instead of trying to override getDelay
    public int GetCombinedDelay()
    {
        if (hasCalculatedDelay)
        {
            Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Returning combined delay: {CombinedDelay}");
            return CombinedDelay;
        }
        return getDelay();
    }

    private (ActionCards, int, float) FindNextActionCard(Weapon weapon, int startIndex)
    {
        Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] FindNextActionCard called with startIndex: {startIndex}");
        var result = weapon.GetTargetActionCard(startIndex);
        
        if (result.Item1 != null)
        {
            Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Found action card: {result.Item1.name} at index: {result.Item2}, mana cost: {result.Item3}");
        }
        else
        {
            Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] No action card found starting from index {startIndex}");
        }
        
        return result;
    }

    private void ApplyAuxCardsBetween(Weapon weapon, int firstActionIndex, int secondActionIndex, ActionCards secondAction)
    {
        Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] ApplyAuxCardsBetween from index {firstActionIndex} to {secondActionIndex}");
        int currentIndex = (firstActionIndex < weapon.cards.Count - 1) ? firstActionIndex + 1 : 0;
        Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Starting at index {currentIndex}");
        
        int auxCardsApplied = 0;
        
        // Loop through cards between first and second action
        while (currentIndex != secondActionIndex)
        {
            // Apply any aux cards we find to the second action
            if (weapon.cards[currentIndex] != null && weapon.cards[currentIndex] is AuxCards)
            {
                AuxCards auxCard = (AuxCards)weapon.cards[currentIndex];
                Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Applying AuxCard {auxCard.getCardName()} at index {currentIndex} to second action card {secondAction.name}");
                auxCard.applyMod(secondAction);
                auxCardsApplied++;
            }
            else if (weapon.cards[currentIndex] != null)
            {
                Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Skipping non-AuxCard {weapon.cards[currentIndex].getCardName()} at index {currentIndex}");
            }
            else
            {
                Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Skipping null card at index {currentIndex}");
            }
            
            // Move to next card
            if (currentIndex < weapon.cards.Count - 1)
            {
                currentIndex++;
            }
            else
            {
                currentIndex = 0;
            }
            Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Moving to next index: {currentIndex}");
        }
        
        Debug.Log($"[AST DEBUG] [DoubleCast-{instanceId}] Applied {auxCardsApplied} aux cards to the second action card");
    }
} 
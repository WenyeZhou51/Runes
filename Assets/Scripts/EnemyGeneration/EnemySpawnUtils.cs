using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utility class for enemy spawning operations
/// </summary>
public static class EnemySpawnUtils
{
    /// <summary>
    /// Spawns enemies in a specific pattern (circle, grid, etc.)
    /// </summary>
    public static List<GameObject> SpawnEnemiesInPattern(
        GameObject[] enemyPrefabs, 
        Vector2 centerPosition, 
        PatternType patternType, 
        int count, 
        float spacing,
        Transform parent = null)
    {
        List<GameObject> spawnedEnemies = new List<GameObject>();
        
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("No enemy prefabs provided for pattern spawning");
            return spawnedEnemies;
        }
        
        List<Vector2> positions = GeneratePatternPositions(centerPosition, patternType, count, spacing);
        
        foreach (Vector2 pos in positions)
        {
            GameObject prefab = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Length)];
            GameObject enemy = Object.Instantiate(prefab, pos, Quaternion.identity);
            
            if (parent != null)
            {
                enemy.transform.SetParent(parent);
            }
            
            spawnedEnemies.Add(enemy);
        }
        
        return spawnedEnemies;
    }
    
    /// <summary>
    /// Generates positions for a specific pattern
    /// </summary>
    public static List<Vector2> GeneratePatternPositions(
        Vector2 center, 
        PatternType patternType, 
        int count, 
        float spacing)
    {
        List<Vector2> positions = new List<Vector2>();
        
        switch (patternType)
        {
            case PatternType.Circle:
                positions = GenerateCirclePattern(center, count, spacing);
                break;
                
            case PatternType.Grid:
                positions = GenerateGridPattern(center, count, spacing);
                break;
                
            case PatternType.Line:
                positions = GenerateLinePattern(center, count, spacing);
                break;
                
            case PatternType.Random:
                positions = GenerateRandomPattern(center, count, spacing);
                break;
        }
        
        return positions;
    }
    
    /// <summary>
    /// Generates positions in a circle pattern
    /// </summary>
    private static List<Vector2> GenerateCirclePattern(Vector2 center, int count, float radius)
    {
        List<Vector2> positions = new List<Vector2>();
        
        for (int i = 0; i < count; i++)
        {
            float angle = i * (360f / count) * Mathf.Deg2Rad;
            float x = center.x + radius * Mathf.Cos(angle);
            float y = center.y + radius * Mathf.Sin(angle);
            positions.Add(new Vector2(x, y));
        }
        
        return positions;
    }
    
    /// <summary>
    /// Generates positions in a grid pattern
    /// </summary>
    private static List<Vector2> GenerateGridPattern(Vector2 center, int count, float spacing)
    {
        List<Vector2> positions = new List<Vector2>();
        
        // Calculate grid dimensions (try to make it as square as possible)
        int cols = Mathf.CeilToInt(Mathf.Sqrt(count));
        int rows = Mathf.CeilToInt((float)count / cols);
        
        // Calculate offset to center the grid
        float offsetX = (cols - 1) * spacing / 2;
        float offsetY = (rows - 1) * spacing / 2;
        
        int enemiesPlaced = 0;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (enemiesPlaced >= count)
                    break;
                    
                float x = center.x - offsetX + col * spacing;
                float y = center.y - offsetY + row * spacing;
                positions.Add(new Vector2(x, y));
                
                enemiesPlaced++;
            }
        }
        
        return positions;
    }
    
    /// <summary>
    /// Generates positions in a line pattern
    /// </summary>
    private static List<Vector2> GenerateLinePattern(Vector2 center, int count, float spacing)
    {
        List<Vector2> positions = new List<Vector2>();
        
        // Calculate offset to center the line
        float offset = (count - 1) * spacing / 2;
        
        for (int i = 0; i < count; i++)
        {
            float x = center.x - offset + i * spacing;
            positions.Add(new Vector2(x, center.y));
        }
        
        return positions;
    }
    
    /// <summary>
    /// Generates positions in a random pattern within a radius
    /// </summary>
    private static List<Vector2> GenerateRandomPattern(Vector2 center, int count, float radius)
    {
        List<Vector2> positions = new List<Vector2>();
        
        for (int i = 0; i < count; i++)
        {
            float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = UnityEngine.Random.Range(0f, radius);
            float x = center.x + distance * Mathf.Cos(angle);
            float y = center.y + distance * Mathf.Sin(angle);
            positions.Add(new Vector2(x, y));
        }
        
        return positions;
    }
    
    /// <summary>
    /// Checks if a position is valid for spawning (no colliders)
    /// </summary>
    public static bool IsValidSpawnPosition(Vector2 position, float radius = 0.5f)
    {
        return !Physics2D.OverlapCircle(position, radius);
    }
    
    /// <summary>
    /// Enum for different spawn patterns
    /// </summary>
    public enum PatternType
    {
        Circle,
        Grid,
        Line,
        Random
    }
} 
using UnityEngine;

public class DrawThresholdLine : MonoBehaviour
{
    [Range(0, 100)]
    public float thresholdPercentage = 50.0f; // Threshold percentage of the screen height

    private void OnGUI()
    {
        float threshold = Screen.height * (thresholdPercentage / 100.0f);
        DrawLine(new Vector2(0, threshold), new Vector2(Screen.width, threshold), 2, Color.red);
    }

    // Helper method to draw lines in GUI
    private void DrawLine(Vector2 start, Vector2 end, float width, Color color)
    {
        Vector2 d = end - start;
        float a = Mathf.Rad2Deg * Mathf.Atan2(d.y, d.x); // Use Atan2 for better angle calculation

        GUI.color = color;
        GUIUtility.RotateAroundPivot(a, start);
        GUI.DrawTexture(new Rect(start.x, start.y - width / 2, d.magnitude, width), Texture2D.whiteTexture);
        GUIUtility.RotateAroundPivot(-a, start); // Reset rotation
    }
}

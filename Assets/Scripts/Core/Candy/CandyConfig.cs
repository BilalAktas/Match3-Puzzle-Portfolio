using UnityEngine;

/// <summary>
/// Stores data about a candy's type, image, and color.
/// </summary>
[System.Serializable]
public struct CandyData
{
    public string CandyType;
    public string CandyResourceImage;
    public Color CandyColor;
    public float Percent;
    public string PoolName;
}

[CreateAssetMenu(menuName = "ScriptableObject/CandyData", fileName = "CandyData")]
public class CandyConfig : ScriptableObject
{
    [SerializeField] private CandyData _candyData;
    public CandyData CandyData => _candyData;
}
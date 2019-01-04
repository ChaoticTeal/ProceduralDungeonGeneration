using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DungeonLayoutGenerator : MonoBehaviour
{
    #region SerializeFields
    [Header("Basic Specs")]
    [SerializeField]
    [Tooltip("The total length and width of the floor, in tiles.")]
    private int gridDimensions = 20;
    [SerializeField]
    [Tooltip("The minimum length or width of a room, in tiles.")]
    private int minRoomDimensions = 2;
    [SerializeField]
    [Tooltip("The maximum length or width of a room, in tiles.")]
    private int maxRoomDimensions = 6;

    [Header("Text Characters")]
    [SerializeField]
    [Tooltip("The default character for empty space.")]
    private char emptySpaceChar = '■';
    [SerializeField]
    [Tooltip("The character for room tiles.")]
    private char roomChar = 'R';
    [SerializeField]
    [Tooltip("The character for hall tiles.")]
    private char hallChar = 'H';
    [SerializeField]
    [Tooltip("The character for door tiles.")]
    private char doorChar = 'D';
    #endregion

    // Private fields
    private char[,] dungeonLayout;

    // Use this for initialization
    void Start ()
    {
        InitializeDungeon();
	}

    /// <summary>
    /// Initializes dungeonLayout as a square array based on gridDimensions
    /// Sets each character to emptySpaceChar
    /// </summary>
    private void InitializeDungeon()
    {
        dungeonLayout = new char[gridDimensions, gridDimensions];
        for (int i = 0; i < gridDimensions; i++)
            for (int j = 0; j < gridDimensions; j++)
                dungeonLayout[i, j] = emptySpaceChar;
    }

    /// <summary>
    /// Prints dungeonLayout to a text file
    /// </summary>
    private void PrintDungeon()
    {
        string path = Application.dataPath + "/Text/DungeonLayout_" + 
            DateTime.Now.ToString().Replace("/","").Replace(":","").Replace(" ","") + ".txt";
        using (var sw = new StreamWriter(path))
        {
            for (int i = 0; i < gridDimensions; i++)
            {
                for (int j = 0; j < gridDimensions; j++)
                    sw.Write(dungeonLayout[i, j]);
                sw.Write(sw.NewLine);
            }
        }
    }
}

/// <summary>
/// A lightweight class for keeping track of room data
/// </summary>
class Room
{
    /// <summary>
    /// The position of the top left corner of the room
    /// </summary>
    public Vector2 TopLeft;
    /// <summary>
    /// The position of the bottom right corner of the room
    /// </summary>
    public Vector2 BottomRight;
    /// <summary>
    /// A list containing the coordinates of any doors in the room
    /// A valid room contains at least one
    /// </summary>
    public List<Vector2> Doors;
    /// <summary>
    /// Does this room have the floor's entry tile?
    /// </summary>
    public bool Entry;
    /// <summary>
    /// Does this room have the floor's exit tile?
    /// </summary>
    public bool Exit;
}
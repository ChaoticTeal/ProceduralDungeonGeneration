using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField]
    [Tooltip("The maximum number of rooms on the floor.")]
    private int maxRoomCount = 6;

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
    [SerializeField]
    [Tooltip("The character for exit tiles.")]
    private char exitChar = 'X';
    [SerializeField]
    [Tooltip("The character for entry tiles.")]
    private char entryChar = 'N';

    [Header("Visualization")]
    [SerializeField]
    [Tooltip("The text box to write the dungeon ASCII layout in.")]
    Text dungeonText;
    #endregion

    // Private fields
    private char[,] dungeonLayout;
    private List<Room> rooms;

    // Use this for initialization
    void Start ()
    {
        GenerateNewDungeon();
	}

    /// <summary>
    /// Calls dungeon generation functions in sequence
    /// </summary>
    private void GenerateNewDungeon()
    {
        ValidateVariables();
        InitializeDungeon();
        GenerateRooms();
        PrintDungeon();
        PrintDungeonToCanvas();
    }

    /// <summary>
    /// Ensure that variables are feasible values
    /// This includes preventing negatives and unreasonable floor sizes
    /// </summary>
    private void ValidateVariables()
    {
        gridDimensions = Mathf.Max(gridDimensions, 20);
        minRoomDimensions = Mathf.Max(minRoomDimensions, 2);
        maxRoomDimensions = Mathf.Max(maxRoomDimensions, minRoomDimensions);
        maxRoomCount = Mathf.Max(maxRoomCount, 1);
    }

    /// <summary>
    /// Initializes dungeonLayout as a square array based on gridDimensions
    /// Sets each character to emptySpaceChar
    /// </summary>
    private void InitializeDungeon()
    {
        dungeonLayout = new char[gridDimensions, gridDimensions];
        for (int x = 0; x < gridDimensions; x++)
            for (int z = 0; z < gridDimensions; z++)
                dungeonLayout[x, z] = emptySpaceChar;
    }

    /// <summary>
    /// Generates rooms of a random length and width
    /// </summary>
    private void GenerateRooms()
    {
        rooms = new List<Room>();
        bool keepGenerating = true, roomsOverlap = false;
        float stopChance = 0f;
        // Generate at least one room
        do
        {
            Room tempRoom = new Room();
            Rect tempRect = new Rect();
            // Make at least one attempt at generating a room Rect
            do
            {
                roomsOverlap = false;
                tempRect = GenerateRoomRect();
                if(rooms.Count > 0)
                {
                    foreach (Room r in rooms)
                        if (r.Bounds.Overlaps(tempRect))
                            roomsOverlap = true;
                }
            } while (rooms.Count > 0 && roomsOverlap);
            tempRoom.Bounds = tempRect;
            rooms.Add(tempRoom);
            if(rooms.Count < maxRoomCount)
            {
                if (UnityEngine.Random.value < stopChance)
                    keepGenerating = false;
                stopChance += 1f / (maxRoomCount * 1.1f);
            }
        // Keep generating until the variable is set false or we reach the max room count
        } while (rooms.Count < maxRoomCount && keepGenerating);
        AddRoomsToArray();
    }

    /// <summary>
    /// Adds rooms into the character array
    /// </summary>
    private void AddRoomsToArray()
    {
        foreach(Room r in rooms)
        {
            for(int x = Mathf.RoundToInt(r.Bounds.xMin); x < Mathf.RoundToInt(r.Bounds.xMax); x++)
            {
                for(int z = Mathf.RoundToInt(r.Bounds.yMin); z < Mathf.RoundToInt(r.Bounds.yMax); z++)
                {
                    dungeonLayout[x, z] = roomChar;
                }
            }
        }
    }

    /// <summary>
    /// Generates a Rect to be used for a room
    /// Length and width are based on the room dimensions specified above
    /// TopLeft variables must be far enough from the edge that the room doesn't exceed the grid's size
    /// </summary>
    /// <returns>A Rect that fits within the grid boundaries.</returns>
    private Rect GenerateRoomRect()
    {
        int length, width, topLeftX, topLeftZ;
        length = UnityEngine.Random.Range(minRoomDimensions, maxRoomDimensions + 1);
        width = UnityEngine.Random.Range(minRoomDimensions, maxRoomDimensions + 1);
        topLeftX = UnityEngine.Random.Range(0, gridDimensions - width);
        topLeftZ = UnityEngine.Random.Range(0, gridDimensions - length);
        return new Rect(new Vector2(topLeftX, topLeftZ), new Vector2(width, length));
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
            for (int x = 0; x < gridDimensions; x++)
            {
                for (int z = 0; z < gridDimensions; z++)
                    sw.Write(dungeonLayout[x, z]);
                sw.Write(sw.NewLine);
            }
        }
    }

    private void PrintDungeonToCanvas()
    {
        if(dungeonText != null)
        {
            dungeonText.text = "";
            for (int x = 0; x < gridDimensions; x++)
            {
                for (int z = 0; z < gridDimensions; z++)
                {
                    if (dungeonLayout[x, z] != emptySpaceChar)
                        dungeonText.text += " ";
                    dungeonText.text += dungeonLayout[x, z];
                }
                dungeonText.text += '\n';
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
    /// A rectangle representing the position of the room
    /// </summary>
    public Rect Bounds;
    /// <summary>
    /// A list containing the coordinates of any doors in the room
    /// A valid room contains at least one
    /// </summary>
    public List<Vector2> Doors;
    /// <summary>
    /// The location of the entry tile, if there is one
    /// </summary>
    public Vector2 EntryLocation;
    /// <summary>
    /// The location of the exit tile, if there is one
    /// </summary>
    public Vector2 ExitLocation;
}
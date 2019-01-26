using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DungeonBuilder : MonoBehaviour
{
    #region SerializeFields
    [Header("Basic Specs")]
    [Tooltip("Dimensions of the \"tiles\" to build with, in Unity units.")]
    [SerializeField]
    private int tileDimensions = 6;
    #endregion

    private char emptySpaceChar = '■';
    private char roomChar = 'R';
    private char hallChar = 'H';
    private char doorChar = 'D';
    private char exitChar = 'X';
    private char entryChar = 'N';

    private char[,] dungeonLayout;
    private char[,] tileSurroundings = new char[3, 3];
    private DungeonTile[,] dungeonTiles;

    private AssetBundle doorAssetBundle;
    private AssetBundle hallAssetBundle;
    private AssetBundle roomAssetBundle;

    private GameObject dungeonObject;

    private void Awake()
    {
        doorAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "dungeondoors"));
        hallAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "dungeonhalls"));
        roomAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "dungeonrooms"));
    }

    /// <summary>
    /// Initializes variables and starts the process of building the dungeon
    /// This version is used if characters differ from their default values
    /// </summary>
    /// <param name="empty">The default character for empty space.</param>
    /// <param name="room">The character for room tiles.</param>
    /// <param name="hall">The character for hall tiles.</param>
    /// <param name="door">The character for door tiles.</param>
    /// <param name="exit">The character for exit tiles.</param>
    /// <param name="entry">The character for entry tiles.</param>
    /// <param name="dungeon">The character array to build from.</param>
    public void Initialize(char empty, char room, char hall, char door, char exit, char entry, char[,] dungeon)
    {
        emptySpaceChar = empty;
        roomChar = room;
        hallChar = hall;
        doorChar = door;
        exitChar = exit;
        entryChar = entry;
        Initialize(dungeon);
    }

    /// <summary>
    /// Initializes variables and starts the process of building the dungeon
    /// This version uses the default values defined above
    /// </summary>
    /// <param name="dungeon">The character array to build from.</param>
    public void Initialize(char[,] dungeon)
    {
        if (dungeonObject == null)
            dungeonObject = new GameObject("dungeonObject");
        dungeonObject.transform.position = Vector3.zero;
        dungeonLayout = dungeon;
        dungeonTiles = new DungeonTile[dungeonLayout.GetLength(0), dungeonLayout.GetLength(1)];
        SetTiles();
        BuildDungeon();
    }

    /// <summary>
    /// Physically constructs the dungeon based on the tile array
    /// </summary>
    private void BuildDungeon()
    {
        if (dungeonObject.transform.childCount > 0)
            ClearOldDungeon();
        GameObject temp;
        for(int z = 0; z < dungeonLayout.GetLength(0); z++)
            for(int x = 0; x < dungeonLayout.GetLength(1); x++)
            {
                switch(dungeonTiles[z,x].tileType)
                {
                    case TileTypes.Empty:
                        break;
                    case TileTypes.Room:
                        temp = Instantiate(roomAssetBundle.LoadAsset<GameObject>(dungeonTiles[z, x].tileName));
                        temp.transform.parent = dungeonObject.transform;
                        temp.transform.position = new Vector3(z * tileDimensions, 0, (x + 1) * tileDimensions);
                        break;
                }
            }
    }

    /// <summary>
    /// Removes tiles from former dungeon to make room for a new one
    /// </summary>
    private void ClearOldDungeon()
    {
        for (int i = 0; i < dungeonObject.transform.childCount; i++)
            Destroy(dungeonObject.transform.GetChild(i).gameObject);
    }

    /// <summary>
    /// Parses the character array and determines which tile to use where
    /// </summary>
    private void SetTiles()
    {
        for(int z = 0; z < dungeonLayout.GetLength(0); z++)
            for(int x = 0; x < dungeonLayout.GetLength(1); x++)
            {
                dungeonTiles[z, x] = new DungeonTile();
                if (dungeonLayout[z, x] == emptySpaceChar)
                    continue;
                GetSurroundingTiles(z, x);
                GetTileType(z, x);
                if (dungeonTiles[z, x].tileType == TileTypes.Empty)
                    continue;
                switch(dungeonTiles[z, x].tileType)
                {
                    case TileTypes.Room:
                    case TileTypes.Entry:
                    case TileTypes.Exit:
                        DetermineRoomTile(z, x);
                        break;
                        // TODO Add functionality for doors and halls
                }
            }
    }

    /// <summary>
    /// Function looks at surrounding tiles to determine the name of the current tile
    /// Used for room, entry, and exit tiles
    /// </summary>
    /// <param name="z">The horizontal coordinate in the array</param>
    /// <param name="x">The vertical coordinate in the array</param>
    private void DetermineRoomTile(int z, int x)
    {
        bool open = false;
        int adjacentCount = 0, diagCount = 0;
        string direction = "", tileName = "";
        if (tileSurroundings[1, 2] == roomChar || tileSurroundings[1, 2] == doorChar ||
           tileSurroundings[1, 2] == exitChar || tileSurroundings[1, 2] == entryChar)
            adjacentCount++;
        else
            direction += "Forward";
        if (tileSurroundings[1, 0] == roomChar || tileSurroundings[1, 0] == doorChar ||
           tileSurroundings[1, 0] == exitChar || tileSurroundings[1, 0] == entryChar)
            adjacentCount++;
        else
            direction += "Back";
        if (tileSurroundings[0, 1] == roomChar || tileSurroundings[0, 1] == doorChar ||
           tileSurroundings[0, 1] == exitChar || tileSurroundings[0, 1] == entryChar)
            adjacentCount++;
        else
            direction += "Left";
        if (tileSurroundings[2, 1] == roomChar || tileSurroundings[2, 1] == doorChar ||
           tileSurroundings[2, 1] == exitChar || tileSurroundings[2, 1] == entryChar)
            adjacentCount++;
        else
            direction += "Right";
        for(int i = 0; i < 3; i += 2)
            for(int j = 0; j < 3; j += 2)
                if (tileSurroundings[i, j] == roomChar || tileSurroundings[i, j] == doorChar ||
                   tileSurroundings[i, j] == exitChar || tileSurroundings[i, j] == entryChar)
                    diagCount++;
        if (diagCount >= 2)
            open = true;
        if (open)
            tileName += "Open";
        if (adjacentCount == 4)
        {
            if (diagCount < 4)
                tileName += direction + "Corner";
            else
                tileName += "NoWall";
        }
        else
        {
            tileName += direction;
            if (adjacentCount == 3)
                tileName += "Wall";
            else if (adjacentCount < 3)
                tileName += "Corner";
        }
        dungeonTiles[z, x].tileName = tileName + ".prefab";
        Debug.Log("Tile Name: " + dungeonTiles[z, x].tileName);
    }

    /// <summary>
    /// Get the type of the current tile, based on the character in the array
    /// </summary>
    /// <param name="z">The horizontal coordinate in the array</param>
    /// <param name="x">The vertical coordinate in the array</param>
    private void GetTileType(int z, int x)
    {
        char tile = dungeonLayout[z, x];
        if (tile == roomChar)
            dungeonTiles[z, x].tileType = TileTypes.Room;
        else if (tile == doorChar)
            dungeonTiles[z, x].tileType = TileTypes.Door;
        else if (tile == hallChar)
            dungeonTiles[z, x].tileType = TileTypes.Hall;
        else if (tile == exitChar)
            dungeonTiles[z, x].tileType = TileTypes.Exit;
        else if (tile == entryChar)
            dungeonTiles[z, x].tileType = TileTypes.Entry;
        else
            dungeonTiles[z, x].tileType = TileTypes.Empty;
    }

    /// <summary>
    /// Validates and sets aside the current tile and its immediate surroundings
    /// Used to determine what type of tile to place
    /// </summary>
    /// <param name="z">The horizontal coordinate in the array</param>
    /// <param name="x">The vertical coordinate in the array</param>
    private void GetSurroundingTiles(int z, int x)
    {
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                tileSurroundings[i, j] = emptySpaceChar;
        tileSurroundings[1, 1] = dungeonLayout[z, x];
        if (x > 0)
            tileSurroundings[1, 0] = dungeonLayout[z, x - 1];
        if (x < dungeonLayout.GetLength(1))
            tileSurroundings[1, 2] = dungeonLayout[z, x + 1];
        if (z > 0)
        {
            tileSurroundings[0, 1] = dungeonLayout[z - 1, x];
            if (x > 0)
                tileSurroundings[0, 0] = dungeonLayout[z - 1, x - 1];
            if (x < dungeonLayout.GetLength(1))
                tileSurroundings[0, 2] = dungeonLayout[z - 1, x + 1];
        }
        if(z < dungeonLayout.GetLength(0))
        {
            tileSurroundings[2, 1] = dungeonLayout[z + 1, x];
            if (x > 0)
                tileSurroundings[2, 0] = dungeonLayout[z + 1, x - 1];
            if (x < dungeonLayout.GetLength(1))
                tileSurroundings[2, 2] = dungeonLayout[z + 1, x + 1];
        }
    }
}

public enum TileTypes { Empty, Room, Hall, Door, Exit, Entry }

public class DungeonTile
{
    /// <summary>
    /// The asset name to use for the tile
    /// </summary>
    public string tileName = "";
    /// <summary>
    /// The type of tile this is
    /// Based on the character in the array
    /// Determines which AssetBundle to pull from
    /// </summary>
    public TileTypes tileType = TileTypes.Empty;
}

using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public struct BoardElement {
    public const int Wall = 0;
    public const int Pellet = 1;
    public const int PowerPellet = 2;
    public const int Pacman = 3;
    public const int Empty = 4;
    public const int Clyde = 5;
    public const int Blinky = 6;
    public const int Pinky = 7;
    public const int Inky = 8;
}

public struct BoardElementPos {
    public const int Wall = 0;
    public const int Pellet = 1;
    public const int PowerPellet = 2;
    public const int Pacman = 3;
    public const int Empty = 4;
    public const int Clyde = 5;
    public const int Blinky = 6;
    public const int Pinky = 7;
    public const int Inky = 8;
}

public enum Direction {
    LEFT,
    RIGHT,
    UP,
    DOWN,
    NONE
}

public class Board : MonoBehaviour
{
    [SerializeField]
    private const int NUM_ROWS = 28;
    [SerializeField]
    private const int NUM_COLS = 31;
    [SerializeField]
    private const int NUM_STATES = 9;
    [SerializeField]
    private int[,,] board;
    [SerializeField]
    private Transform originTransform;
    [SerializeField]
    private Grid grid;

    private void Awake() {
        // Initialize the board
        Tilemap[] tilemaps = grid.GetComponentsInChildren<Tilemap>();
        Vector3Int origin = grid.WorldToCell(originTransform.transform.position);
        Debug.Log(origin);

        board = new int[NUM_ROWS, NUM_COLS, NUM_STATES];
        for (int i = 0; i < NUM_ROWS; i++) {
            for (int j = 0; j < NUM_COLS; j++) {
                for (int z = 0; z < NUM_STATES; z++) {
                    board[i, j, z] = BoardElement.Empty;
                }
            }
        }

        for (int i = 0; i < tilemaps.Length; i++) {
            Tilemap tilemap = tilemaps[i];
            BoundsInt bounds = tilemap.cellBounds;
            TileBase[] allTiles = tilemap.GetTilesBlock(bounds);
            for (int x = 0; x < bounds.size.x; x++) {
                for (int y = 0; y < bounds.size.y; y++) {
                    TileBase tile = allTiles[x + y * bounds.size.x];
                    if (tile != null) {
                        Vector3Int position = new Vector3Int(x + bounds.xMin, y + bounds.yMin, 0);
                        Vector2Int boardPosition = new Vector2Int(position.x - origin.x, position.y - origin.y);
                        if (tile.name.Contains("Wall")) {
                            board[boardPosition.x, boardPosition.y, BoardElementPos.Wall] = BoardElement.Wall;
                        } else if (tile.name.Contains("PowerPellet")) {
                            board[boardPosition.x, boardPosition.y, BoardElementPos.PowerPellet] = BoardElement.Pellet;
                        } else if (tile.name.Contains("Pellet")) {
                            board[boardPosition.x, boardPosition.y, BoardElementPos.Pellet] = BoardElement.PowerPellet;
                        }
                    }
                }
            }
        }
    }
    
    public Vector3 MoveElement(Vector3 worldPosition, Direction direction, int element) {
        Vector3Int origin = grid.WorldToCell(originTransform.transform.position);
        Vector3Int oldBoardPosition = grid.WorldToCell(worldPosition);
        Vector3Int newPosition = GetNewPosition(worldPosition, direction);
        Vector2Int newBoardPosition = new Vector2Int(newPosition.x - origin.x, newPosition.y - origin.y);

        if (board[newBoardPosition.x, newBoardPosition.y, BoardElementPos.Wall] == BoardElement.Wall) {
            return worldPosition;
        }

        int[,,] newBoard = CopyBoard(board);

        BoardElement oldElementInTheNewPos = newBoard[newBoardPosition.x][newBoardPosition.y];
        if (element == BoardElement.Ghost) {
            if (oldElementInTheNewPos != BoardElement.Ghost_Pellet && oldElementInTheNewPos != BoardElement.Ghost_PowerPellet) {
                newBoard[newBoardPosition.x][newBoardPosition.y] = ghostMovements.GetValueOrDefault(oldElementInTheNewPos, BoardElement.Ghost);
            }
            newBoard[oldBoardPosition.x][oldBoardPosition.y] = ghostMovements.GetValueOrDefault(oldElementInTheNewPos, BoardElement.Empty);


        } else {
            newBoard[newBoardPosition.x][newBoardPosition.y] = element;
            newBoard[oldBoardPosition.x][oldBoardPosition.y] = BoardElement.Empty;
        }

        board = newBoard;

        return grid.CellToWorld(newPosition);
    }

    private Vector3Int GetNewPosition(Vector3 worldPosition, Direction direction) {
        Vector3Int position = grid.WorldToCell(worldPosition);
        switch (direction) {
            case Direction.LEFT:
                position.x--;
                break;
            case Direction.RIGHT:
                position.x++;
                break;
            case Direction.UP:
                position.y++;
                break;
            case Direction.DOWN:
                position.y--;
                break;
        }
        return position;
    }

    private int[,,] CopyBoard(int[,,] board) {
        int[,,] newBoard = new int[NUM_ROWS, NUM_COLS, NUM_STATES];
        for (int i = 0; i < NUM_ROWS; i++) {
            for (int j = 0; j < NUM_COLS; j++) {
                for (int z = 0; z < NUM_STATES; z++) {
                    newBoard[i, j, z] = board[i, j, z];
                }
            }
        }
        return newBoard;
    }
}

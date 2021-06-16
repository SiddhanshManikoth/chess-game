using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; set; }
    private bool[,] allowedSMoves { get; set; }

    private const float TILE_SIZE = 1.0f;
    private const float TILE_OFFSET = 0.5f;

    private int selectionX = -1;
    private int selectionY = -1;

    public List<GameObject> SchessmanPrefabs;
    private List<GameObject> activeSChessman;

    private Quaternion whiteOrientation = Quaternion.Euler(0, 270, 0);
    private Quaternion blackOrientation = Quaternion.Euler(0, 90, 0);

    public SChessman[,] SChessmans { get; set; }
    private SChessman selectedSChessman;

    public bool isWhiteTurnchess = true;
    static public int win = 0;
    private Material previousMat;
    public Material selectedMat;

    public int[] EnPassantMove { set; get; }

    // Use this for initialization
    void Start()
    {
        Instance = this;
        SpawnAllSChessmans();
        EnPassantMove = new int[2] { -1, -1 };
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSelection();

        if (Input.GetMouseButtonDown(0))
        {
            if (selectionX >= 0 && selectionY >= 0)
            {
                if (selectedSChessman == null)
                {
                    // Select the chessman
                    SelectSChessman(selectionX, selectionY);
                }
                else
                {
                    // Move the chessman
                    chessMoveSChessman(selectionX, selectionY);
                }
            }
        }

        if (Input.GetKey("escape"))
            Application.Quit();
    }

    private void SelectSChessman(int x, int y)
    {
        if (SChessmans[x, y] == null) return;

        if (SChessmans[x, y].isWhitechess != isWhiteTurnchess) return;

        bool hasAtLeastOneMove = false;

        allowedSMoves = SChessmans[x, y].chessPossibleMoves();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (allowedSMoves[i, j])
                {
                    hasAtLeastOneMove = true;
                    i = 8;
                    break;
                }
            }
        }

        if (!hasAtLeastOneMove)
            return;

        selectedSChessman = SChessmans[x, y];
        previousMat = selectedSChessman.GetComponent<MeshRenderer>().material;
        selectedMat.mainTexture = previousMat.mainTexture;
        selectedSChessman.GetComponent<MeshRenderer>().material = selectedMat;

        BoardSHighlights.Instance.HighLightAllowedMoves(allowedSMoves);
    }

    private void chessMoveSChessman(int x, int y)
    {
        if (allowedSMoves[x, y])
        {
            SChessman c = SChessmans[x, y];

            if (c != null && c.isWhitechess != isWhiteTurnchess)
            {
                // Capture a piece

                if (c.GetType() == typeof(SKing))
                {
                    // End the game
                    EndGame();
                    return;
                }

                activeSChessman.Remove(c.gameObject);
                Destroy(c.gameObject);
            }
            if (x == EnPassantMove[0] && y == EnPassantMove[1])
            {
                if (isWhiteTurnchess)
                    c = SChessmans[x, y - 1];
                else
                    c = SChessmans[x, y + 1];

                activeSChessman.Remove(c.gameObject);
                Destroy(c.gameObject);
            }
            EnPassantMove[0] = -1;
            EnPassantMove[1] = -1;
            if (selectedSChessman.GetType() == typeof(SPawn))
            {
                if(y == 7) // White Promotion
                {
                    activeSChessman.Remove(selectedSChessman.gameObject);
                    Destroy(selectedSChessman.gameObject);
                    SpawnSChessman(1, x, y, true);
                    selectedSChessman = SChessmans[x, y];
                }
                else if (y == 0) // Black Promotion
                {
                    activeSChessman.Remove(selectedSChessman.gameObject);
                    Destroy(selectedSChessman.gameObject);
                    SpawnSChessman(7, x, y, false);
                    selectedSChessman = SChessmans[x, y];
                }
                EnPassantMove[0] = x;
                if (selectedSChessman.CurrentY == 1 && y == 3)
                    EnPassantMove[1] = y - 1;
                else if (selectedSChessman.CurrentY == 6 && y == 4)
                    EnPassantMove[1] = y + 1;
            }

            SChessmans[selectedSChessman.CurrentX, selectedSChessman.CurrentY] = null;
            selectedSChessman.transform.position = GetTileCenter(x, y);
            selectedSChessman.chessSetPosition(x, y);
            SChessmans[x, y] = selectedSChessman;
            isWhiteTurnchess = !isWhiteTurnchess;
        }

        selectedSChessman.GetComponent<MeshRenderer>().material = previousMat;

        BoardSHighlights.Instance.HideHighlights();
        selectedSChessman = null;
    }

    private void UpdateSelection()
    {
        if (!Camera.main) return;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 50.0f, LayerMask.GetMask("ChessPlane")))
        {
            selectionX = (int)hit.point.x;
            selectionY = (int)hit.point.z;
        }
        else
        {
            selectionX = -1;
            selectionY = -1;
        }
    }

    private void SpawnSChessman(int index, int x, int y, bool isWhite)
    {
        Vector3 position = GetTileCenter(x, y);
        GameObject go;

        if (isWhite)
        {
            go = Instantiate(SchessmanPrefabs[index], position, whiteOrientation) as GameObject;
        }
        else
        {
            go = Instantiate(SchessmanPrefabs[index], position, blackOrientation) as GameObject;
        }

        go.transform.SetParent(transform);
        SChessmans[x, y] = go.GetComponent<SChessman>();
        SChessmans[x, y].chessSetPosition(x, y);
        activeSChessman.Add(go);
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        Vector3 origin = Vector3.zero;
        origin.x += (TILE_SIZE * x) + TILE_OFFSET;
        origin.z += (TILE_SIZE * y) + TILE_OFFSET;

        return origin;
    }

    private void SpawnAllSChessmans()
    {
        activeSChessman = new List<GameObject>();
        SChessmans = new SChessman[8, 8];

        /////// White ///////

        // King
        SpawnSChessman(0, 3, 0, true);

        // Queen
        SpawnSChessman(1, 4, 0, true);

        // Rooks
        SpawnSChessman(2, 0, 0, true);
        SpawnSChessman(2, 7, 0, true);

        // Bishops
        SpawnSChessman(3, 2, 0, true);
        SpawnSChessman(3, 5, 0, true);

        // Knights
        SpawnSChessman(4, 1, 0, true);
        SpawnSChessman(4, 6, 0, true);

        // Pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnSChessman(5, i, 1, true);
        }


        /////// Black ///////

        // King
        SpawnSChessman(6, 4, 7, false);

        // Queen
        SpawnSChessman(7, 3, 7, false);

        // Rooks
        SpawnSChessman(8, 0, 7, false);
        SpawnSChessman(8, 7, 7, false);

        // Bishops
        SpawnSChessman(9, 2, 7, false);
        SpawnSChessman(9, 5, 7, false);

        // Knights
        SpawnSChessman(10, 1, 7, false);
        SpawnSChessman(10, 6, 7, false);

        // Pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnSChessman(11, i, 6, false);
        }
    }

    private void EndGame()
    {
        if (isWhiteTurnchess)
        {

            SceneManager.LoadScene("GameOver");
            Debug.Log("White wins");
        }
        else
        {
            SceneManager.LoadScene("GameOver");
            win = 1;
            Debug.Log("Black wins");
        }
        foreach (GameObject go in activeSChessman)
        {
            Destroy(go);
        }

        isWhiteTurnchess = true;
        BoardSHighlights.Instance.HideHighlights();
        SpawnAllSChessmans();
    }
}



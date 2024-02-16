using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

namespace CodeZash.WaveFunction {
    public class WaveFunctionCollapse : MonoBehaviour {
        public Vector2 dimensions = new(10, 10);
        public Transform tileParent;
        public Tile[] tileObjects;
        public List<Cell> gridComponents;
        public Cell cellObj;
        public Vector2 cellObjOffset = new Vector2(0.5f, 0.5f);

        public Tile backupTile;

        [Header("Regenerate Settings")]
        public TMP_InputField xInput;
        public TMP_InputField yInput;

        private int iteration;

        private void Awake() {
            Init();
        }

        public void Init() {
            iteration = 0;
            gridComponents = new List<Cell>();
            InitializeGrid();
        }

        public void Recreate() {
            // Stop all coroutines to prevent any ongoing processes
            StopAllCoroutines();

            // Destroy existing cells and tiles
            DestroyGrid();

            // Get the new dimensions from the input fields
            int x = GetValueInputField(xInput) == 0 ? 10 : int.Parse(xInput.text);
            int y = GetValueInputField(yInput) == 0 ? 10 : int.Parse(yInput.text);
            dimensions = new Vector2(x, y);

            // Reinitialize the grid
            Init();

            // Start the process of collapsing the grid again
            StartCoroutine(CheckEntropy());
        }

        int GetValueInputField(TMP_InputField inputField) {
            if (string.IsNullOrEmpty(inputField.text)) {
                inputField.text = "10";
                return 0;
            }

            if (int.Parse(inputField.text) < 0) {
                inputField.text = "10";
                return 0;
            }

            return int.Parse(inputField.text);
        }

        // Method to destroy existing grid components
        private void DestroyGrid() {
            foreach (Cell cell in gridComponents) {
                Destroy(cell.gameObject);
            }

            gridComponents.Clear(); // Clear the list after destroying the objects

            foreach (Transform tile in tileParent) {
                Destroy(tile.gameObject);
            }
        }

        void InitializeGrid() {
            // Make sure to instantiate the grid with the correct offset
            InstantiateGrid();

            // Start the process of collapsing the grid
            StartCoroutine(CheckEntropy());
        }

        private void InstantiateGrid() {
            float xPosition = 0, zPosition = 0;
            for (int y = 0; y < dimensions.y; y++) {
                if (y != 0) {
                    zPosition += cellObjOffset.y + 1; // 1 is the scale of the cell and cellObjOffset.y is the offset
                } else {
                    zPosition = 0;
                }

                for (int x = 0; x < dimensions.x; x++) {
                    if (x != 0) {
                        xPosition += cellObjOffset.x + 1; // 1 is the scale of the cell and cellObjOffset.x is the offset
                    } else {
                        xPosition = 0;
                    }

                    Cell newCell = Instantiate(cellObj, new Vector3(xPosition, 0, zPosition), Quaternion.identity);
                    newCell.CreateCell(false, tileObjects);
                    gridComponents.Add(newCell);
                }
            }
        }

        IEnumerator CheckEntropy() {
            List<Cell> tempGrid = new List<Cell>(gridComponents);
            tempGrid.RemoveAll(c => c.isCollapsed);
            tempGrid.Sort((a, b) => a.tileOptions.Length - b.tileOptions.Length);
            tempGrid.RemoveAll(a => a.tileOptions.Length != tempGrid[0].tileOptions.Length);

            yield return new WaitForSeconds(0.025f);

            CollapseCell(tempGrid);
        }

        void CollapseCell(List<Cell> tempGrid) {
            int randIndex = UnityEngine.Random.Range(0, tempGrid.Count);
            Cell cellToCollapse = tempGrid[randIndex];

            // Check if the cell is already collapsed
            if (cellToCollapse.isCollapsed) {
                // Skip collapsing this cell
                UpdateGeneration(); // Proceed to the next iteration
                return;
            }

            cellToCollapse.isCollapsed = true;
            Tile selectedTile = (cellToCollapse.tileOptions.Length > 0) ? cellToCollapse.tileOptions[UnityEngine.Random.Range(0, cellToCollapse.tileOptions.Length)] : backupTile;
            cellToCollapse.RecreateCell(new Tile[] { selectedTile });

            Tile foundTile = cellToCollapse.tileOptions[0];
            Instantiate(foundTile, cellToCollapse.transform.position, Quaternion.identity).transform.SetParent(tileParent);

            UpdateGeneration();
        }


        void UpdateGeneration() {

            List<Cell> newGenerationCell = new List<Cell>(gridComponents);

            for (int y = 0; y < dimensions.y; y++) {
                for (int x = 0; x < dimensions.x; x++) {
                    // get index of the cell and make sure it's within the bounds of the grid
                    int index = (int)(x + y * dimensions.x);

                    // Check if the index is within the bounds of the grid
                    if (index >= gridComponents.Count)
                        continue; // Skip this iteration if the index is out of bounds

                    if (gridComponents[index].isCollapsed) {
                        newGenerationCell[index] = gridComponents[index];
                    } else {
                        List<Tile> options = new List<Tile>();
                        foreach (Tile t in tileObjects) {
                            options.Add(t);
                        }

                        if (y > 0) {
                            Cell up = gridComponents[(int)(x + (y - 1) * dimensions.x)];
                            List<Tile> validOptions = new List<Tile>();

                            foreach (Tile possibleOptions in up.tileOptions) {
                                var validOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                                var valid = tileObjects[validOption].downNeighbors;

                                validOptions = validOptions.Concat(valid).ToList();
                            }

                            CheckValidity(options, validOptions);
                        }

                        // Similarly check for other directions

                        Tile[] newTileList = new Tile[options.Count];

                        for (int i = 0; i < options.Count; i++) {
                            newTileList[i] = options[i];
                        }

                        newGenerationCell[index].RecreateCell(newTileList);
                    }
                }
            }

            gridComponents = newGenerationCell;
            iteration++;

            // Check if there are still uncollapsed cells
            if (gridComponents.Any(cell => !cell.isCollapsed)) {
                StartCoroutine(CheckEntropy());
            }
        }

        void CheckValidity(List<Tile> optionList, List<Tile> validOption) {
            for (int x = optionList.Count - 1; x >= 0; x--) {
                var element = optionList[x];
                if (!validOption.Contains(element)) {
                    optionList.RemoveAt(x);
                }
            }
        }
    }
}

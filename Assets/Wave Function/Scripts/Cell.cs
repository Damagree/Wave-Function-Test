using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CodeZash.WaveFunction {
    public class Cell : MonoBehaviour {

        public bool isCollapsed;
        public Tile[] tileOptions;

        public void CreateCell(bool isCollapsed, Tile[] tileOptions) {
            this.isCollapsed = isCollapsed;
            this.tileOptions = tileOptions;
        }

        public void RecreateCell(Tile[] tiles) {
            this.tileOptions = tiles;
        }

    }
}

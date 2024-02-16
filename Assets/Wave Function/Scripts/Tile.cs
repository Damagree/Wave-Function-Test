using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace CodeZash.WaveFunction {
    public class Tile : MonoBehaviour {

        public Tile[] upNeighbors;
        public Tile[] downNeighbors;
        public Tile[] leftNeighbors;
        public Tile[] rightNeighbors;

        private void Awake() {
            transform.localScale = Vector3.zero;

            transform.DOScale(Vector3.one, 1f)
                .SetEase(Ease.OutElastic);
        }

    }
}
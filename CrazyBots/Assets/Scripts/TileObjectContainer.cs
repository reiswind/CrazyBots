using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class MineralContainer
    {

        public MineralContainer()
        {
            mineralCubes = new List<GameObject>();
        }

        private List<GameObject> mineralCubes;
        private int filled;
        private int max;

        private bool AddMinerals(GameObject container)
        {
            if (container.name.StartsWith("Mineral"))
            {
                mineralCubes.Add(container);
                container.SetActive(false);
            }
            else
            {
                for (int i = 0; i < container.transform.childCount; i++)
                {
                    GameObject child = container.transform.GetChild(i).gameObject;
                    AddMinerals(child);
                }
            }
            return true;
        }
        public void ClearContent()
        {
            foreach (GameObject gameObject in mineralCubes)
            {
                gameObject.SetActive(false);
            }
        }

        public void UpdateContent(HexGrid hexGrid, GameObject gameObject, List<TileObject> tileObjects, int? capacity)
        {
            if (capacity.HasValue && capacity <= 0)
                return;

            int minerals = tileObjects.Count;

            if (mineralCubes.Count == 0)
            {
                AddMinerals(gameObject);

                filled = 0;
                max = mineralCubes.Count;
            }

            int mins = minerals;

            int minPercent = mins * 100 / capacity.Value;
            mins = minPercent * max / 100;

            if (minerals > 0 && mins == 0)
                mins = 1;

            if (mins != filled)
            {
                while (filled > mins)
                {
                    filled--;
                    if (filled < mineralCubes.Count)
                        mineralCubes[filled].SetActive(false);
                }
                while (filled < mins)
                {
                    if (filled < mineralCubes.Count)
                        mineralCubes[filled].SetActive(true);
                    filled++;
                }
            }
        }
    }
}

/**
 * Copyright 2019 The Knights Of Unity, created by Piotr Stoch
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DemoGame.Scripts.Gameplay.Nodes
{

    /// <summary>
    /// Generates node hex map on scene start
    /// </summary>
    public class NodeMapGenerator : MonoBehaviour
    {
        /// <summary>
        /// Size of map
        /// </summary>
        public Vector2Int Size;

        /// <summary>
        /// Distance in units between nodes
        /// </summary>
        public float SpaceDistance;

        /// <summary>
        /// Size of cut in the center of map
        /// </summary>
        public float CenterCutSize;

        [SerializeField] private GameObject _nodePrefab = null;

        private Node[,] Nodes;

        /// <summary>
        /// Creating hex map
        /// </summary>
        private void Awake()
        {
            Nodes = new Node[Size.x, Size.y];

            Vector2 center = new Vector2((Size.x - 1) * SpaceDistance, (Size.y - 1) * ((SpaceDistance * 0.5f / Mathf.Sin(Mathf.PI / 3f)) + 0.25f * SpaceDistance)) / 2f;
            print(center);

            for (int x = 0; x < Size.x; x++)
            {
                for (int y = 0; y < Size.y; y++)
                {
                    Vector3 position;
                    if (y % 2 == 0)
                    {
                        position = new Vector3(x * SpaceDistance, 0, y * ((SpaceDistance * 0.5f / Mathf.Sin(Mathf.PI / 3f)) + 0.25f * SpaceDistance));
                    }
                    else
                    {
                        position = new Vector3(x * SpaceDistance + 0.5f * SpaceDistance, 0, y * ((SpaceDistance * 0.5f / Mathf.Sin(Mathf.PI / 3f)) + 0.25f * SpaceDistance));
                    }

                    if (Vector2.Distance(center, new Vector2(position.x, position.z)) > CenterCutSize * SpaceDistance / 2f)
                    {
                        if (y % 2 == 0 || x < Size.x - 1)
                        {
                            InstantiateNode(x, y, position);
                        }
                    }
                }
            }
            GameManager.Instance.InitMap(Nodes, Size);
        }

        /// <summary>
        /// Viewing node map in editor
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
            {
                foreach (Node node in Nodes)
                {
                    if (node != null)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawSphere(node.transform.position, SpaceDistance * 0.1f);

                        Gizmos.color = Color.green;
                        foreach (var neighbour in node.ConnectedNodes)
                        {
                            Gizmos.DrawLine(node.transform.position, neighbour.Key.transform.position);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Instanties node and connects it to other nodes
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="position"></param>
        private void InstantiateNode(int x, int y, Vector3 position)
        {
            GameObject go = Instantiate(_nodePrefab, position, Quaternion.identity, transform) as GameObject;

            Node node = go.GetComponent<Node>();

            Nodes[x, y] = node;

            node.SetPosition(x, y);

            if (y % 2 == 0 && x - 1 >= 0)
            {
                if (y - 1 >= 0)
                {
                    if (Nodes[x - 1, y - 1] != null)
                    {
                        ConnectNodes(x, y, x - 1, y - 1);
                    }
                }

                if (y + 1 < Size.y)
                {
                    if (Nodes[x - 1, y + 1] != null)
                    {
                        ConnectNodes(x, y, x - 1, y + 1);
                    }
                }
            }

            if (y - 1 >= 0)
            {
                if (Nodes[x, y - 1] != null)
                {
                    ConnectNodes(x, y, x, y - 1);
                }
            }

            if (x - 1 >= 0)
            {
                if (Nodes[x - 1, y] != null)
                {
                    ConnectNodes(x, y, x - 1, y);
                }
            }
        }

        /// <summary>
        /// Conects pair of nodes
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        private void ConnectNodes(int x1, int y1, int x2, int y2)
        {
            Nodes[x1, y1].AddConnectedNode(Nodes[x2, y2]);
            Nodes[x2, y2].AddConnectedNode(Nodes[x1, y1]);
        }
    }

}
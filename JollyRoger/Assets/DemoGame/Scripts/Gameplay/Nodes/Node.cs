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
using System.Collections.Generic;
using DemoGame.Scripts.Gameplay.Units;
using UnityEngine;

namespace DemoGame.Scripts.Gameplay.Nodes
{

    /// <summary>
    /// Base field on the map on which units can stand and move between them
    /// </summary>
    public class Node : MonoBehaviour
    {
        /// <summary>
        /// True if any unit is on this node
        /// </summary>
        public bool Occupied;

        /// <summary>
        /// Unit that currently occupias this node 
        /// </summary>
        public Unit Unit;

        /// <summary>
        /// Position in node map
        /// </summary>
        public Vector2Int Position { get; private set; }

        /// <summary>
        /// List of all connected nodes with this node
        /// </summary>
        public Dictionary<Node, float> ConnectedNodes = new Dictionary<Node, float>();

        /// <summary>
        /// Sets position value basing on node map
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetPosition(int x, int y)
        {
            Position = new Vector2Int(x, y);
        }

        /// <summary>
        /// Adds neighbour node
        /// </summary>
        /// <param name="node"></param>
        public void AddConnectedNode(Node node)
        {
            float angle = Vector3.SignedAngle(Vector3.forward, node.transform.position - transform.position, Vector3.up);
            ConnectedNodes.Add(node, angle);
        }

        /// <summary>
        /// Returns neighbour node in given direction
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public Node GetNeighbourNodeWithNearestAngle(float angle)
        {
            Node nearestNode = null;
            float nearestDeltaAngle = 360f;

            foreach (var node in ConnectedNodes)
            {
                if (node.Key.Occupied)
                {
                    continue;
                }
                float deltaAngle = Mathf.Abs(Mathf.DeltaAngle(node.Value, angle));
                if (deltaAngle < nearestDeltaAngle)
                {
                    nearestDeltaAngle = deltaAngle;
                    nearestNode = node.Key;
                }
            }

            return nearestNode;
        }
    }

}
using Codice.Client.Common.GameUI;
using BaseServices;
using BaseServices.InputServices;
using Match.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;

namespace Match
{
    public class MoveController
    {
        private int moveMade = 0;
        private bool canInteract = false;
        private Stack<Vector3Int> linkStack = new();
        private BaseMatchItemConfig linkConfig;

        public Action<Vector3, Vector3Int> OnSwipeDetected;

        public void Init()
        {
            canInteract = true;
            InputService.Instance.OnInputReceived += OnInputReceived;
        }

        private void OnInputReceived(InputType inputType, List<Vector3> args)
        {
            if (!canInteract) return;

            int moveCount = MatchGameService.MatchLevelData.moveCount;

            // all moves used
            if (moveCount < moveMade) return;

            if (inputType == InputType.Drag)
            {
                var cam = Camera.main;

                // add start coord if not already present
                if (linkStack.Count < 1)
                {
                    Vector3 startPos = cam.ScreenToWorldPoint(new Vector3(args[0].x, args[0].y, cam.nearClipPlane));
                    Vector3Int coordinate = MatchGameService.GridController.Grid.WorldToCell(startPos);

                    if (MatchGameService.GridController.TryGetGridItem(coordinate, out var item))
                    {
                        linkConfig = item.MatchItemConfig; // sets link config
                        linkStack.Push(coordinate);
                    }
                }

                // get the last coordinate
                Vector3 currentFingerPos = cam.ScreenToWorldPoint(new Vector3(args[1].x, args[1].y, cam.nearClipPlane));
                Vector3Int currentCoordinate = MatchGameService.GridController.Grid.WorldToCell(currentFingerPos);

                if (linkStack.Count > 0)
                {
                    // current coordinate is not the last coordinate
                    if (linkStack.Peek() != currentCoordinate)
                    {
                        if (linkStack.Contains(currentCoordinate))
                        {
                            // if the current coordinate is already in the stack, pop all coordinates until we reach it
                            while (linkStack.Count > 0 && linkStack.Peek() != currentCoordinate)
                            {
                                linkStack.Pop();
                            }

                            string s = "";
                            foreach (var item in linkStack)
                            {
                                s += item.ToString() + " ";
                            }
                            Debug.Log($"Link Stack: {s}");
                        }
                        else if (CanCoordinateBeAdded(currentCoordinate))
                        {
                            // if the current coordinate is not in the stack, push it
                            linkStack.Push(currentCoordinate);

                            string s = "";
                            foreach (var item in linkStack)
                            {
                                s += item.ToString() + " ";
                            }
                            Debug.Log($"Link Stack: {s}");
                        }

                    }
                }
                else if (MatchGameService.GridController.TryGetGridItem(currentCoordinate, out var item))
                {
                    linkConfig = item.MatchItemConfig; // sets link config
                    linkStack.Push(currentCoordinate);
                }

                //OnSwipeDetected?.Invoke(startPos, swipeDir);
            }
        }

        private bool CanCoordinateBeAdded(Vector3Int coordinate)
        {
            // check if the coordinate is a neighbour of the last coordinate
            Vector3Int lastCoordinate = linkStack.Peek();

            if (MatchGameService.GridController.TryGetGridItem(coordinate, out var gridItem) == false || gridItem.MatchItemConfig != linkConfig)
            {
                return false;
            }


            Vector3Int diff = lastCoordinate - coordinate;

            // not a adjecent neighbour or diagonal neighbour
            if (MatchConstants.CellNeighbours.Contains(diff) == false && MatchConstants.CellDiagonalNeighbours.Contains(diff) == false)
            {
                return false;
            }


            return true;
        }

        public void SetCanInteract(bool val)
        {
            canInteract = val;
        }
    }
}
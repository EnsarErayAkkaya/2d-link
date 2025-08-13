using BaseServices.InputServices;
using Match.GridItems;
using Match.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Match
{
    public class MoveController
    {
        private bool canInteract = false;
        private bool isLinking = false;

        private Stack<Vector3Int> linkStack = new();
        private BaseMatchItemConfig linkConfig = null;

        public Action<Stack<Vector3Int>> OnLinkUpdated;
        public Action<Stack<Vector3Int>> OnLinkCompleted;

        public int MoveMade { get; private set; }

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
            if (MoveMade >= moveCount) return;

            if (inputType == InputType.Down)
            {
                linkStack.Clear();
                linkConfig = null;
                isLinking = true;
            }
            else if (inputType == InputType.Drag && isLinking)
            {
                var cam = Camera.main;

                // add start coord if not already present
                if (linkStack.Count < 1)
                {
                    Vector3 startPos = cam.ScreenToWorldPoint(new Vector3(args[0].x, args[0].y, cam.nearClipPlane));
                    Vector3Int coordinate = MatchGameService.GridController.Grid.WorldToCell(startPos);
                    coordinate.z = 0;

                    if (MatchGameService.GridController.TryGetGridItem(coordinate, out var item))
                    {
                        linkConfig = item.MatchItemConfig; // sets link config
                        linkStack.Push(coordinate);

                        OnLinkUpdated?.Invoke(linkStack);
                    }
                }

                // get the last coordinate
                Vector3 currentFingerPos = cam.ScreenToWorldPoint(new Vector3(args[1].x, args[1].y, cam.nearClipPlane));
                Vector3Int currentCoordinate = MatchGameService.GridController.Grid.WorldToCell(currentFingerPos);
                currentCoordinate.z = 0;

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

                            OnLinkUpdated?.Invoke(linkStack);
                        }
                        else if (CanCoordinateBeAdded(currentCoordinate, out BaseGridItem gridItem))
                        {
                            // if the current coordinate is not in the stack, push it
                            linkStack.Push(currentCoordinate);

                            OnLinkUpdated?.Invoke(linkStack);
                        }

                    }
                }
                else if (MatchGameService.GridController.TryGetGridItem(currentCoordinate, out var item))
                {
                    linkConfig = item.MatchItemConfig; // sets link config
                    linkStack.Push(currentCoordinate);

                    OnLinkUpdated?.Invoke(linkStack);
                }

                if (linkStack.Count > 0)
                    MatchGameService.GridController.ApplySelected(linkStack.Peek());

            }
            else if (inputType == InputType.Up)
            {
                isLinking = false;

                //apply link if exist
                if (linkStack.Count > 2)
                {
                    MoveMade++;

                    Stack<Vector3Int> linkStackCopy = new(linkStack);
                    OnLinkCompleted?.Invoke(linkStackCopy);

                    linkStack.Clear();
                }
                else
                {
                    linkStack.Clear();

                    MatchGameService.GridController.ClearSelected();
                    MatchGameService.GridController.ClearForceFocus();
                }

                OnLinkUpdated?.Invoke(linkStack);
            }
        }

        private bool CanCoordinateBeAdded(Vector3Int coordinate, out BaseGridItem gridItem)
        {
            // check if the coordinate is a neighbour of the last coordinate
            Vector3Int lastCoordinate = linkStack.Peek();

            if (MatchGameService.GridController.TryGetGridItem(coordinate, out gridItem) == false || gridItem.MatchItemConfig != linkConfig)
            {
                return false;
            }

            Vector3Int diff = lastCoordinate - coordinate;

            // not a adjecent neighbour
            if (MatchConstants.CellNeighbours.Contains(diff) == false)
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
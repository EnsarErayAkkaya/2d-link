using Codice.Client.Common.GameUI;
using BaseServices;
using BaseServices.InputServices;
using Match.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Match
{
    public class MoveController
    {
        private int moveMade = 0;
        private bool canInteract = false;

        public Action<Vector3, Vector3Int> OnSwipeDetected;

        public void Init()
        {
            canInteract = true;
            InputService.Instance.OnInputReceived += OnInputReceived;
        }

        private void OnInputReceived(InputType inputType, List<Vector3> args)
        {
            if (!canInteract) return;

            // all moves used
            if (((MatchLevelConfig)ResolveServices.LevelService.ActiveLevelConfig).matchLevelData.moveCount < 1) return;

            if (inputType == InputType.Swipe)
            {
                canInteract = false;

                var cam = Camera.main;

                Vector3 startPos = cam.ScreenToWorldPoint(new Vector3(args[0].x, args[0].y, cam.nearClipPlane));
                Vector3 endPos = cam.ScreenToWorldPoint(new Vector3(args[1].x, args[1].y, cam.nearClipPlane));

                Vector3 normalizedSwipe = (endPos - startPos).normalized;
                Vector3Int swipeDir = new Vector3Int();
                
                if (Mathf.Abs(normalizedSwipe.x) >  .5f)
                {
                    if (normalizedSwipe.x > 0)
                    {
                        swipeDir.Set(1, 0, 0);
                    }
                    else
                    {
                        swipeDir.Set(-1, 0, 0);
                    }
                }
                else
                {
                    if (normalizedSwipe.y > 0)
                    {
                        swipeDir.Set(0, 1, 0);
                    }
                    else
                    {
                        swipeDir.Set(0, -1, 0);
                    }
                }

                OnSwipeDetected?.Invoke(startPos, swipeDir);
            }
        }

        public void SetCanInteract(bool val)
        {
            canInteract = val;
        }
    }
}
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.Screens
{
    public class BaseScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        public void Show()
        {
            gameObject.SetActive(true);
            canvasGroup.DOFade(1f, 0.5f);
        }

        public void Hide()
        {
            canvasGroup.DOFade(0f, 0.5f).OnComplete(() => 
            {
                gameObject.SetActive(false);
            });
        }
    }
}

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Match.UI
{
    public class BaseScreen : MonoBehaviour
    {
        [Header("Base Screen")]
        [SerializeField] private CanvasGroup canvasGroup;
     
        public virtual void Show()
        {
            gameObject.SetActive(true);
            canvasGroup.DOFade(1f, 0.5f);
        }

        public virtual void Hide()
        {
            canvasGroup.DOFade(0f, 0.5f).OnComplete(() => 
            {
                gameObject.SetActive(false);
            });
        }
    }
}

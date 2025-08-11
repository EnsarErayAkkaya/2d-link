using DG.Tweening;
using BaseServices;
using Match.Settings;
using System.Linq;
using UnityEngine;

namespace Match.GridItems
{
    public class FlyingBoosterItem : BoosterItem
    {
        [SerializeField] private BoosterConfig boosterConfig;
        [SerializeField] private BoosterData boosterData;

        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;

        [SerializeField] private float flyDuration = 1.25f;

        private Vector3 originalSpriteScale;

        private BoosterItem attachedBooster = null;

        public override BaseMatchItemData MatchItemData => boosterData;
        public override BaseMatchItemConfig MatchItemConfig => boosterConfig;

        private void Awake()
        {
            originalSpriteScale = new Vector3(spriteRenderer.transform.localScale.x, spriteRenderer.transform.localScale.y, 1);
        }

        public override void Init(Vector3Int gridCoord)
        {
            base.Init(gridCoord);

            animator.gameObject.SetActive(false);

            MatchLevelData ld = (MatchLevelData)null;//ResolveServices.LevelService.ActiveLevelConfig.GetLevelData();

            int side = Random.value >= 0.5 ? 1 : -1;

            Vector3 animStartPos = new Vector3(MatchGameService.GridController.CellSize.x * ld.size.x * side, 0);

            FlyToPosition(animStartPos, Vector3.zero);
        }

        public void AttachBooster(BoosterItem boosterItem)
        {
            attachedBooster = boosterItem;
        }

        public override Sequence PlayBooster()
        {
            var gridController = MatchGameService.GridController;

            string damageId = System.Guid.NewGuid().ToString();

            void RemoveItem(Vector3Int coord)
            {
                if (coord == GridCoordinate) return;

                gridController.DamageGridItem(coord, damageId, true);
            }

            Vector3Int targetCoordinate = MatchGameService.Instance.FindMostImportantLevelTargetCoordinate();

            while (targetCoordinate == GridCoordinate)
            {
                targetCoordinate = MatchGameService.Instance.FindMostImportantLevelTargetCoordinate();
            }

            Vector3 targetPos = gridController.Grid.GetCellCenterWorld(targetCoordinate) - transform.position;
            
            return FlyToPosition(Vector3.zero, targetPos).AppendCallback(() =>
            {
                RemoveItem(targetCoordinate);

                if (attachedBooster != null)
                {
                    gridController.GridItems.Remove(attachedBooster.GridCoordinate);
                    attachedBooster.Init(targetCoordinate);
                    gridController.SetUpGridItem(attachedBooster);
                    attachedBooster.ActivateBooster();
                }

                gridController.RemoveGridItem(GridCoordinate);

            });
        }

        public Sequence FlyToPosition(Vector3 startPos, Vector3 targetPos)
        {
            var seq = DOTween.Sequence();

            seq.AppendCallback(() =>
            {
                animator.gameObject.SetActive(true);

                spriteRenderer.transform.localPosition = startPos;

                PlayFlyAnim(/*startPos.x > 0*/);
            });

            float val = 0;

            Vector3 verticalMovementTarget1 = new Vector3(0, MatchGameService.GridController.CellSize.y, -1.5f);
            Vector3 verticalMovementTarget2 = new Vector3(0, -MatchGameService.GridController.CellSize.y, -1.5f);


            seq.Append(DOTween.To(() => val, (x) => val = x, 1, flyDuration).OnUpdate(() =>
            {
                Vector3 pos = Vector3.Lerp(startPos, targetPos, val);

                if (val < .33f)
                {
                    pos += Vector3.Lerp(Vector3.zero, verticalMovementTarget1, val / .33f); 
                }
                else if (val < .66f)
                {
                    pos += Vector3.Lerp(verticalMovementTarget1, verticalMovementTarget2, (val - .33f) / .33f);
                }
                else if (val < 1f)
                {
                    pos += Vector3.Lerp(verticalMovementTarget2, Vector3.zero, (val - .66f) / .34f);
                }

                if (val < .5f)
                {
                    spriteRenderer.transform.localScale = originalSpriteScale * Mathf.Lerp(1.5f, 3.0f, val / .5f);
                }
                else
                {
                    spriteRenderer.transform.localScale = originalSpriteScale * Mathf.Lerp(3.0f, 1.0f, (val - .5f) / .5f);
                }

                spriteRenderer.transform.localPosition = pos;

            }).SetEase(Ease.InOutSine));

            seq.AppendCallback(() =>
            {
                spriteRenderer.transform.localPosition = targetPos;
                spriteRenderer.transform.localScale = originalSpriteScale;
            });

            seq.SetUpdate(false);

            return seq;
        }

        private float PlayFlyAnim(/*bool isMirrored*/)
        {
            animator.SetTrigger("Fly");
            //spriteRenderer.flipX = isMirrored;

            return animator.runtimeAnimatorController.animationClips.FirstOrDefault(s => s.name == "Fly").length;
        }

        public override void OnSpawn()
        {
            isActivated = false;
            spriteRenderer.transform.localScale = originalSpriteScale;
            attachedBooster = null;
        }
    }
}
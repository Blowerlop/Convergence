using System;
using System.Collections.Generic;
using System.Linq;
using Project.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Project
{
    [Serializable]
    public class Page
    {
        [ReadOnly] public string gameObjectName;
        [ReadOnly] public CanvasGroup canvasGroup;
        public int pageIndex;
        public UnityEvent onPageSelectedEvent = new UnityEvent();

        public Page(string gameObjectName, CanvasGroup canvasGroup, int pageIndex)
        {
            this.gameObjectName = gameObjectName;
            this.canvasGroup = canvasGroup;
            this.pageIndex = pageIndex;
        }
    }


    public class Pager : MonoBehaviour
    {
        private RectTransform _rectTransform;

        public List<Page> _pages = new List<Page>();
        [SerializeField, ReadOnly] private int _currentPageIndex;
        [SerializeField, ReadOnly] private int _previousPageIndex;
        [SerializeField] private bool pageLoop = true;
        [SerializeField] private bool _disablePages = true;
        [SerializeField] private bool crossFadePages;
        [SerializeField] private float _crossFadeDuration = 0.25f;
        private Coroutine _crossFadeCoroutineCurrentPage;
        private Coroutine _crossFadeCoroutinePreviousPage;


        private void Start()
        {
            GoToPage(_currentPageIndex, true);
        }

        [Button]
        public void NextPage()
        {
            if (_currentPageIndex + 1 > _pages.Count - 1)
            {
                if (pageLoop)
                {
                    _previousPageIndex = _currentPageIndex;
                    _currentPageIndex = 0;
                }
                else
                {
                    return;
                }
            }
            else
            {
                _previousPageIndex = _currentPageIndex;
                _currentPageIndex++;
            }

            if (crossFadePages)
            {
                CancelCrossPages();
            }

            GetPage(_currentPageIndex).onPageSelectedEvent.Invoke();
            ChangePageVisualBehaviour();
        }

        [Button]
        public void PreviousPage()
        {
            if (_currentPageIndex - 1 < 0)
            {
                if (pageLoop)
                {
                    _previousPageIndex = _currentPageIndex;
                    _currentPageIndex = _pages.Count - 1;
                }
                else
                {
                    return;
                }
            }
            else
            {
                _previousPageIndex = _currentPageIndex;
                _currentPageIndex--;
            }


            if (crossFadePages)
            {
                CancelCrossPages();
            }

            GetPage(_currentPageIndex).onPageSelectedEvent.Invoke();
            ChangePageVisualBehaviour();

        }

        [Button]
        public void RefreshPager()
        {
            Debug.Log("Refreshing the pager...");
            _pages.Clear();

            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            List<RectTransform> children =
                _rectTransform.GetComponentsInChildrenFirstDepthWithoutTheParent<RectTransform>();
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].TryGetComponent(out CanvasGroup canvasGroup))
                {
                    _pages.Add(new Page(canvasGroup.name, canvasGroup, i));
                }
                else
                {
                    CanvasGroup childrenCanvasGroup = children[i].gameObject.AddComponent<CanvasGroup>();
                    _pages.Add(new Page(childrenCanvasGroup.name, childrenCanvasGroup, i));
                }
            }

            ChangePageVisualBehaviour();
            
            Debug.Log("Pager refreshed !");
        }

        public void GoToPage(CanvasGroup canvasGroup)
        {
            if (GetPage(_currentPageIndex).canvasGroup == canvasGroup) return;

            for (int i = 0; i < _pages.Count; i++)
            {
                if (GetPage(i).canvasGroup == canvasGroup)
                {
                    _previousPageIndex = _currentPageIndex;
                    _currentPageIndex = i;

                    GetPage(_currentPageIndex).onPageSelectedEvent.Invoke();
                    ChangePageVisualBehaviour();
                    break;
                }
            }
        }

        public void GoToPage(int pageIndex) => GoToPage(pageIndex, false);

        public void GoToPage(int pageIndex, bool forceGoToPage)
        {
            if (GetPage(pageIndex).pageIndex == _currentPageIndex && forceGoToPage == false) return;

            for (int i = 0; i < _pages.Count; i++)
            {
                if (GetPage(i).pageIndex == pageIndex)
                {
                    _previousPageIndex = _currentPageIndex;
                    _currentPageIndex = i;

                    GetPage(_currentPageIndex).onPageSelectedEvent.Invoke();
                    ChangePageVisualBehaviour();
                    break;
                }
            }
        }
        
        private Page GetPage(int pageIndex) => _pages[pageIndex];

        private void CrossFadePages()
        {
            CanvasGroup previousPageCanvasGroup = GetPage(_previousPageIndex).canvasGroup;
            CanvasGroup currentCanvasGroup = GetPage(_currentPageIndex).canvasGroup;

            #if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                previousPageCanvasGroup.alpha = 0.0f;
                currentCanvasGroup.alpha = 1.0f;
            }
            else
            #endif
            {
                _crossFadeCoroutinePreviousPage =
                    StartCoroutine(Utilities.LerpInTimeCoroutine(_crossFadeDuration, 1.0f, 0.0f, lerpCurrentValue =>
                    {
                        previousPageCanvasGroup.alpha = lerpCurrentValue;
                    }));
                
                _crossFadeCoroutineCurrentPage =
                    StartCoroutine(Utilities.LerpInTimeCoroutine(_crossFadeDuration, 0.0f, 1.0f, lerpCurrentValue =>
                    {
                        currentCanvasGroup.alpha = lerpCurrentValue;
                    }));
            }
        }

        private void CancelCrossPages()
        {
            if (_crossFadeCoroutinePreviousPage != null)
            {
                CanvasGroup previousPageCanvasGroup = GetPage(_previousPageIndex).canvasGroup;


                if (previousPageCanvasGroup.gameObject.activeInHierarchy)
                {
                    StopCoroutine(_crossFadeCoroutinePreviousPage);
                    previousPageCanvasGroup.alpha = 0.0f;
                    if (_disablePages) previousPageCanvasGroup.gameObject.SetActive(false);
                }

                _crossFadeCoroutinePreviousPage = null;
            }

            if (_crossFadeCoroutineCurrentPage != null)
            {
                CanvasGroup currentCanvasGroup = GetPage(_currentPageIndex).canvasGroup;

                if (currentCanvasGroup.gameObject.activeInHierarchy)
                {
                    StopCoroutine(_crossFadeCoroutineCurrentPage);
                    currentCanvasGroup.alpha = 0.0f;
                    if (_disablePages) currentCanvasGroup.gameObject.SetActive(false);
                }

                _crossFadeCoroutineCurrentPage = null;
            }
        }

        private void ChangePageVisualBehaviour()
        {
            if (crossFadePages)
            {
                CrossFadePages();   
            }
            
            if (_disablePages)
            {
                _pages[_previousPageIndex].canvasGroup.gameObject.SetActive(false);
                _pages[_currentPageIndex].canvasGroup.gameObject.SetActive(true);
            }
            else
            {
                _pages[_previousPageIndex].canvasGroup.alpha = 0.0f;
                _pages[_previousPageIndex].canvasGroup.blocksRaycasts = false;
                
                _pages[_currentPageIndex].canvasGroup.alpha = 1.0f;
                _pages[_currentPageIndex].canvasGroup.blocksRaycasts = true;
            }
        }
    }
}
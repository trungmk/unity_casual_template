using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class UIViewStack : MonoBehaviour
    {
        public UILayer Layer = UILayer.None;
        
        [SerializeField]
        private Transform _blackImage = null;
        
        private Transform _transform = null;

        private CanvasGroup _blackImageCanvasGroup;
            
        private void Awake()
        {
            _transform = transform;

            if (_blackImage != null)
            {
                _blackImageCanvasGroup = _blackImage.GetComponent<CanvasGroup>();
                
                if (_blackImageCanvasGroup == null)
                {
                    Debug.LogError("UIViewStack: Black Image of " + gameObject.name + " is missing canvas group!");
                }
                else
                {
                    _blackImageCanvasGroup.alpha = 0;
                    _blackImageCanvasGroup.blocksRaycasts = false;
                }
            }
        }
    
        private BaseView GetUIUsingBlackImage(BaseView viewShouldHide)
        {        
            for (int i = _transform.childCount - 1; i >= 0; i--)
            {
                BaseView ui = _transform.GetChild(i).GetComponent<BaseView>();
    
                if (ui == null)
                {
                    continue;
                }
                
                if (ui != viewShouldHide && ui.IsPresenting && ui.UseBlackImage)
                {
                    return ui;
                }
            }
    
            return null;
        }
    
        public void PopView(BaseView uiView)
        {
            BaseView nextUiIsUsingBlackImage = GetUIUsingBlackImage(uiView);
            uiView.transform.SetAsFirstSibling();
            
            if (uiView.UseBlackImage)
            {
                if (nextUiIsUsingBlackImage != null)
                {
                    nextUiIsUsingBlackImage.transform.SetAsLastSibling();
                    int uiSiblingIndex = nextUiIsUsingBlackImage.transform.GetSiblingIndex();
                    SetBlackImageSibling(uiSiblingIndex);
                }
            }
    
            // If the stack view has black image
            // and there is no view use black image
            // and black image is showing then hide black image.
            if (_blackImageCanvasGroup != null
                && nextUiIsUsingBlackImage == null
                && _blackImageCanvasGroup.alpha > 0)
            {
                _blackImageCanvasGroup.alpha = 0;
                _blackImageCanvasGroup.blocksRaycasts = false;
            }
        }
    
        public void PushView(BaseView uiView)
        {
            if(uiView.transform.parent != _transform)
            {
                uiView.transform.SetParent(_transform, false);        
            }
            
            uiView.transform.SetAsLastSibling();
            
            if (uiView.UseBlackImage && _blackImageCanvasGroup != null)
            {
                int uiSiblingIndex = uiView.transform.GetSiblingIndex();
                SetBlackImageSibling(uiSiblingIndex);
                
                if (_blackImageCanvasGroup.alpha < 1)
                {
                    _blackImageCanvasGroup.alpha = 1;
                    _blackImageCanvasGroup.blocksRaycasts = true;
                }
            }
    
            uiView.Present();
        }
    
        private void SetBlackImageSibling(int uiSiblingIndex)
        {
            int blackImageSiblingIndex = uiSiblingIndex - 1;
            if (blackImageSiblingIndex >= 0)
            {
                _blackImageCanvasGroup.transform.SetSiblingIndex(blackImageSiblingIndex);
            }
            else
            {
                Debug.LogWarning("UIViewStack: Black Image of " + gameObject.name + " has negative sibling index := " + blackImageSiblingIndex);
            }
        }
    }
}


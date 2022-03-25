using Assets.UI.Colors;
//using GameClient.Plugins.UrlLinks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIKit
{
    public class LinkTextStyleComponent : TextStyleComponent, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private ColorLibraryItem _linkColor;
        
        [SerializeField]
        private ColorLibraryItem _linkHoverColor;

        private bool _isHovering;
        private int _linkIndex = -1;
        private Camera _linkCamera;

        public override void UpdateText()
        {
            base.UpdateText();

            foreach (var linkInfo in TextComponent.textInfo.linkInfo)
                LinkToColor(linkInfo, false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _linkCamera = eventData.enterEventCamera;
            _isHovering = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovering = false;
            SetLinkIndex(-1);
        }

        private void LinkToColor(TMP_LinkInfo linkInfo, bool isHover)
        {
            var color = isHover ? _linkHoverColor.Color : _linkColor;

            for (var i = 0; i < linkInfo.linkTextLength; i++)
            {
                var characterIndex = linkInfo.linkTextfirstCharacterIndex + i;
                var charInfo = TextComponent.textInfo.characterInfo[characterIndex];
                /*
                if(isHover)
                    charInfo.style |= FontStyles.Underline;
                else
                    charInfo.style ^= FontStyles.Underline;
                */

                var meshIndex = charInfo.materialReferenceIndex;
                var vertexIndex = charInfo.vertexIndex;

                var vertexColors = TextComponent.textInfo.meshInfo[meshIndex].colors32;

                vertexColors[vertexIndex + 0] = color;
                vertexColors[vertexIndex + 1] = color;
                vertexColors[vertexIndex + 2] = color;
                vertexColors[vertexIndex + 3] = color;
            }

            TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        }

        private void SetLinkIndex(int index)
        {
            if (_linkIndex == index) 
                return;

            if (_linkIndex >= 0)
                LinkToColor(TextComponent.textInfo.linkInfo[_linkIndex], false);

            if (index >= 0)
                LinkToColor(TextComponent.textInfo.linkInfo[index], true);

            _linkIndex = index;
        }

        private void LateUpdate()
        {
            SetLinkIndex(_isHovering ? TMP_TextUtilities.FindIntersectingLink(TextComponent, Input.mousePosition, _linkCamera) : -1);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_linkIndex < 0)
                return;

            var linkInfo = TextComponent.textInfo.linkInfo[_linkIndex];
            //UrlLinks.OpenUrl(linkInfo.GetLinkID());
            //LinkClick?.Invoke(linkInfo.GetLinkID());
        }
    }
}
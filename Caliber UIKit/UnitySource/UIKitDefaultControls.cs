using UnityEngine;
using TheraBytes.BetterUi;
using TMPro;
using UnityEngine.UI;

namespace UIKit
{
    /// <summary>
    /// Является копией класса UnityEditor.UI.DefaultControls с небольшими правками
    /// https://bitbucket.org/Unity-Technologies/ui/src/f0c70f707cf09f959ad417049cb070f8e296ffe2/UnityEngine.UI/UI/Core/DefaultControls.cs?at=5.5&fileviewer=file-view-default
    /// </summary>

    public static class UIKitDefaultControls
    {
        public struct Resources
        {
            public Sprite standard;
            public Sprite background;
            public Sprite inputField;
            public Sprite knob;
            public Sprite checkmark;
            public Sprite dropdown;
            public Sprite mask;
        }

//        private const float kWidth = 160f;
//        private const float kThickHeight = 30f;
//        private const float kThinHeight = 20f;
//        private static Vector2 s_ThickElementSize = new Vector2(kWidth, kThickHeight);
//        private static Vector2 s_ThinElementSize = new Vector2(kWidth, kThinHeight);
//        private static Vector2 s_ImageElementSize = new Vector2(100f, 100f);
//        private static Color s_DefaultSelectableColor = new Color(1f, 1f, 1f, 1f);
//        private static Color s_PanelColor = new Color(1f, 1f, 1f, 0.392f);
        private static Color s_TextColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);

        // Helper methods at top

        private static GameObject CreateUIElementRoot(string name, Vector2 size)
        {
            GameObject child = new GameObject(name);
            RectTransform rectTransform = child.AddComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            return child;
        }

        static GameObject CreateUIObject(string name, GameObject parent)
        {
            GameObject go = new GameObject(name);
            go.AddComponent<RectTransform>();
            SetParentAndAlign(go, parent);
            return go;
        }

        private static void SetDefaultTextValues(Text lbl)
        {
            // Set text values we want across UI elements in default controls.
            // Don't set values which are the same as the default values for the Text component,
            // since there's no point in that, and it's good to keep them as consistent as possible.
            lbl.color = s_TextColor;

            // Reset() is not called when playing. We still want the default font to be assigned
            //lbl.AssignDefaultFont();
            lbl.fontSize = 14;
        }

        //        private static void SetDefaultTextMeshProValues(TextMeshProUGUI lbl)
        //        {
        //            // Set text values we want across UI elements in default controls.
        //            // Don't set values which are the same as the default values for the Text component,
        //            // since there's no point in that, and it's good to keep them as consistent as possible.
        //            lbl.color = s_TextColor;
        //
        //            // Reset() is not called when playing. We still want the default font to be assigned
        //            //lbl.AssignDefaultFont();
        //            lbl.fontSize = 14;
        //        }

        private static void SetDefaultColorTransitionValues(UIKitSelectable slider)
        {
            ColorBlock colors = slider.colors;
            colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
            colors.pressedColor = new Color(0.698f, 0.698f, 0.698f);
            colors.disabledColor = new Color(0.521f, 0.521f, 0.521f);
        }

        private static void SetParentAndAlign(GameObject child, GameObject parent)
        {
            if (parent == null)
                return;

            child.transform.SetParent(parent.transform, false);
            SetLayerRecursively(child, parent.layer);
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }

        // Actual controls

#if UNITY_EDITOR
        public static GameObject CreatePanel(Resources resources)
        {
            return UIKitManager.AddUIKitGameObject(UIKitManager.UIKitGameObjectType.Panel);
            /*
            GameObject panelRoot = CreateUIElementRoot("Panel", s_ThickElementSize);

            // Set RectTransform to stretch
            RectTransform rectTransform = panelRoot.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;

            Image image = panelRoot.AddComponent<BetterImage>();
            image.sprite = resources.background;
            image.type = Image.Type.Sliced;
            image.color = s_PanelColor;

            return panelRoot;
            */
        }

        public static GameObject CreateButton(Resources resources)
        {
            return UIKitManager.AddUIKitGameObject(UIKitManager.UIKitGameObjectType.Button);
            /*
            GameObject buttonRoot = CreateUIElementRoot("Button", s_ThickElementSize);

            GameObject childText = UIKitManager.AddUIKitGameObject(UIKitManager.UIKitGameObjectType.Text, buttonRoot);
            GameObject childFocusIndicator = UIKitManager.AddUIKitGameObject(UIKitManager.UIKitGameObjectType.FocusIndicator, buttonRoot);

            Image image = buttonRoot.AddComponent<BetterImage>();
            image.sprite = resources.standard;
            image.type = Image.Type.Sliced;
            image.color = s_DefaultSelectableColor;

            UIKitButton bt = buttonRoot.AddComponent<BetterButton>();
            SetDefaultColorTransitionValues(bt);

            TextStyleComponent text = childText.GetComponent<TextStyleComponent>();
            text.text = "Button";

            FocusIndicatorComponent focusIndicatorComponent = childFocusIndicator.GetComponent<FocusIndicatorComponent>();
            bt.focusIndicator = focusIndicatorComponent;

            RectTransform textRectTransform = childText.GetComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.sizeDelta = Vector2.zero;

            return buttonRoot;
            */
        }

        public static GameObject CreateImage(Resources resources)
        {
            return UIKitManager.AddUIKitGameObject(UIKitManager.UIKitGameObjectType.Image);
            /*
            GameObject go = CreateUIElementRoot("Image", s_ImageElementSize);
            go.AddComponent<BetterImage>();
            return go;
            */
        }

        public static GameObject CreateSlider(Resources resources)
        {
            return UIKitManager.AddUIKitGameObject(UIKitManager.UIKitGameObjectType.Slider);
            /*
            // Create GOs Hierarchy
            GameObject root = CreateUIElementRoot("Slider", s_ThinElementSize);

            GameObject background = CreateUIObject("Background", root);
            GameObject fillArea = CreateUIObject("Fill Area", root);
            GameObject fill = CreateUIObject("Fill", fillArea);
            GameObject handleArea = CreateUIObject("Handle Slide Area", root);
            GameObject handle = CreateUIObject("Handle", handleArea);
            GameObject childFocusIndicator = UIKitManager.AddUIKitGameObject(UIKitManager.UIKitGameObjectType.FocusIndicator, root);

            // Background
            Image backgroundImage = background.AddComponent<BetterImage>();
            backgroundImage.sprite = resources.background;
            backgroundImage.type = Image.Type.Sliced;
            backgroundImage.color = s_DefaultSelectableColor;
            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0, 0.25f);
            backgroundRect.anchorMax = new Vector2(1, 0.75f);
            backgroundRect.sizeDelta = new Vector2(0, 0);

            // Fill Area
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1, 0.75f);
            fillAreaRect.anchoredPosition = new Vector2(-5, 0);
            fillAreaRect.sizeDelta = new Vector2(-20, 0);

            // Fill
            Image fillImage = fill.AddComponent<BetterImage>();
            fillImage.sprite = resources.standard;
            fillImage.type = Image.Type.Sliced;
            fillImage.color = s_DefaultSelectableColor;

            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.sizeDelta = new Vector2(10, 0);

            // Handle Area
            RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
            handleAreaRect.sizeDelta = new Vector2(-20, 0);
            handleAreaRect.anchorMin = new Vector2(0, 0);
            handleAreaRect.anchorMax = new Vector2(1, 1);

            // Handle
            Image handleImage = handle.AddComponent<BetterImage>();
            handleImage.sprite = resources.knob;
            handleImage.color = s_DefaultSelectableColor;

            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 0);

            // Setup slider component
            UIKitSlider slider = root.AddComponent<BetterSlider>();
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handleImage;
            slider.direction = UIKitSlider.Direction.LeftToRight;
            SetDefaultColorTransitionValues(slider);

            FocusIndicatorComponent focus = childFocusIndicator.GetComponent<FocusIndicatorComponent>();
            slider.focusIndicator = focus;

            return root;
            */
        }

        public static GameObject CreateProgressBar(Resources resources)
        {
            return UIKitManager.AddUIKitGameObject(UIKitManager.UIKitGameObjectType.ProgressBar);
        }

        public static GameObject CreateScrollbar(Resources resources)
        {
            return UIKitManager.AddUIKitGameObject(UIKitManager.UIKitGameObjectType.ScrollBar);
            /*
            // Create GOs Hierarchy
            GameObject scrollbarRoot = CreateUIElementRoot("Scrollbar", s_ThinElementSize);

            GameObject sliderArea = CreateUIObject("Sliding Area", scrollbarRoot);
            GameObject handle = CreateUIObject("Handle", sliderArea);
            GameObject childFocusIndicator = UIKitManager.AddUIKitGameObject(UIKitManager.UIKitGameObjectType.FocusIndicator, scrollbarRoot);

            Image bgImage = scrollbarRoot.AddComponent<BetterImage>();
            bgImage.sprite = resources.background;
            bgImage.type = Image.Type.Sliced;
            bgImage.color = s_DefaultSelectableColor;

            Image handleImage = handle.AddComponent<BetterImage>();
            handleImage.sprite = resources.standard;
            handleImage.type = Image.Type.Sliced;
            handleImage.color = s_DefaultSelectableColor;

            RectTransform sliderAreaRect = sliderArea.GetComponent<RectTransform>();
            sliderAreaRect.sizeDelta = new Vector2(-20, -20);
            sliderAreaRect.anchorMin = Vector2.zero;
            sliderAreaRect.anchorMax = Vector2.one;

            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 20);

            UIKitScrollbar scrollbar = scrollbarRoot.AddComponent<BetterScrollbar>();
            scrollbar.handleRect = handleRect;
            scrollbar.targetGraphic = handleImage;
            SetDefaultColorTransitionValues(scrollbar);

            FocusIndicatorComponent focus = childFocusIndicator.GetComponent<FocusIndicatorComponent>();
            scrollbar.focusIndicator = focus;

            return scrollbarRoot;
            */
        }

        public static GameObject CreateToggle(Resources resources)
        {
            return UIKitManager.AddUIKitGameObject(UIKitManager.UIKitGameObjectType.Toggle);
            /*
            // Set up hierarchy
            GameObject toggleRoot = CreateUIElementRoot("Toggle", s_ThinElementSize);

            GameObject background = CreateUIObject("Background", toggleRoot);
            GameObject checkmark = CreateUIObject("Checkmark", background);
            GameObject childLabel = UIKitManager.AddUIKitGameObject(UIKitManager.UIKitGameObjectType.Text, toggleRoot);
            GameObject childFocusIndicator = UIKitManager.AddUIKitGameObject(UIKitManager.UIKitGameObjectType.FocusIndicator, toggleRoot);

            // Set up components
            UIKitToggle toggle = toggleRoot.AddComponent<BetterToggle>();
            toggle.isOn = true;

            Image bgImage = background.AddComponent<BetterImage>();
            bgImage.sprite = resources.standard;
            bgImage.type = Image.Type.Sliced;
            bgImage.color = s_DefaultSelectableColor;

            Image checkmarkImage = checkmark.AddComponent<BetterImage>();
            checkmarkImage.sprite = resources.checkmark;

            TextStyleComponent text = childLabel.GetComponent<TextStyleComponent>();
            text.text = "Toggle";

            FocusIndicatorComponent focus = childFocusIndicator.GetComponent<FocusIndicatorComponent>();
            toggle.focusIndicator = focus;

            toggle.graphic = checkmarkImage;
            toggle.targetGraphic = bgImage;
            SetDefaultColorTransitionValues(toggle);

            RectTransform bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 1f);
            bgRect.anchorMax = new Vector2(0f, 1f);
            bgRect.anchoredPosition = new Vector2(10f, -10f);
            bgRect.sizeDelta = new Vector2(kThinHeight, kThinHeight);

            RectTransform checkmarkRect = checkmark.GetComponent<RectTransform>();
            checkmarkRect.anchorMin = new Vector2(0.5f, 0.5f);
            checkmarkRect.anchorMax = new Vector2(0.5f, 0.5f);
            checkmarkRect.anchoredPosition = Vector2.zero;
            checkmarkRect.sizeDelta = new Vector2(20f, 20f);

            RectTransform labelRect = childLabel.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = new Vector2(23f, 1f);
            labelRect.offsetMax = new Vector2(-5f, -2f);

            return toggleRoot;
            */
        }

        public static GameObject CreateInputField(Resources resources)
        {
            return UIKitManager.AddUIKitGameObject(UIKitManager.UIKitGameObjectType.InputField);
            /*
            GameObject root = CreateUIElementRoot("InputField", s_ThickElementSize);

            GameObject textArea = CreateUIObject("Text Area", root);
            GameObject childPlaceholder = UIKitManager.AddUIKitGameObject(UIKitManager.UIKitGameObjectType.Text, textArea);
            GameObject childText = UIKitManager.AddUIKitGameObject(UIKitManager.UIKitGameObjectType.Text, textArea);
            GameObject childFocusIndicator = UIKitManager.AddUIKitGameObject(UIKitManager.UIKitGameObjectType.FocusIndicator, root);

            Image image = root.AddComponent<BetterImage>();
            image.sprite = resources.inputField;
            image.type = Image.Type.Sliced;
            image.color = s_DefaultSelectableColor;

            BetterTMPInputField inputField = root.AddComponent<BetterTMPInputField>();
            SetDefaultColorTransitionValues(inputField);

            // Use UI.Mask for Unity 5.0 - 5.1 and 2D RectMask for Unity 5.2 and up
            textArea.AddComponent<RectMask2D>();

            RectTransform textAreaRectTransform = textArea.GetComponent<RectTransform>();
            textAreaRectTransform.anchorMin = Vector2.zero;
            textAreaRectTransform.anchorMax = Vector2.one;
            textAreaRectTransform.sizeDelta = Vector2.zero;
            textAreaRectTransform.offsetMin = new Vector2(10, 6);
            textAreaRectTransform.offsetMax = new Vector2(-10, -7);

            TextStyleComponent textStyle = childText.GetComponent<TextStyleComponent>();
            textStyle.isLocalizationRequired = false;
            textStyle.text = "";

            TextMeshProUGUI text = childText.GetComponent<TextMeshProUGUI>();
            text.enableWordWrapping = false;
            text.extraPadding = true;
            text.richText = true;

            childPlaceholder.name = "Placeholder";

            TextStyleComponent placeholderTextStyle = childPlaceholder.GetComponent<TextStyleComponent>();
            placeholderTextStyle.text = "ENTER_TEXT_KEY";

            TextMeshProUGUI placeholder = childPlaceholder.GetComponent<TextMeshProUGUI>();
            placeholder.enableWordWrapping = false;
            placeholder.extraPadding = true;

            // Make placeholder color half as opaque as normal text color.
            //            Color placeholderColor = text.color;
            //            placeholderColor.a *= 0.5f;
            //            placeholder.color = placeholderColor;

            RectTransform textRectTransform = childText.GetComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.sizeDelta = Vector2.zero;
            textRectTransform.offsetMin = new Vector2(0, 0);
            textRectTransform.offsetMax = new Vector2(0, 0);

            RectTransform placeholderRectTransform = childPlaceholder.GetComponent<RectTransform>();
            placeholderRectTransform.anchorMin = Vector2.zero;
            placeholderRectTransform.anchorMax = Vector2.one;
            placeholderRectTransform.sizeDelta = Vector2.zero;
            placeholderRectTransform.offsetMin = new Vector2(0, 0);
            placeholderRectTransform.offsetMax = new Vector2(0, 0);

            FocusIndicatorComponent focus = childFocusIndicator.GetComponent<FocusIndicatorComponent>();

            inputField.focusIndicator = focus;
            inputField.textViewport = textAreaRectTransform;
            inputField.textComponent = text;
            inputField.placeholder = placeholder;

            return root;
            */
        }

        public static GameObject CreateDropdown(Resources resources)
        {
            return UIKitManager.AddUIKitGameObject(UIKitManager.UIKitGameObjectType.Dropdown);
            /*
            GameObject root = CreateUIElementRoot("Dropdown", s_ThickElementSize);

            GameObject label = UIKitManager.AddUIKitGameObject(UIKitManager.UIKitGameObjectType.Text, root);
            GameObject arrow = CreateUIObject("Arrow", root);
            GameObject childFocusIndicator = UIKitManager.AddUIKitGameObject(UIKitManager.UIKitGameObjectType.FocusIndicator, root);
            GameObject template = CreateUIObject("Template", root);
            GameObject viewport = CreateUIObject("Viewport", template);
            GameObject content = CreateUIObject("Content", viewport);
            GameObject item = CreateUIObject("Item", content);
            GameObject itemBackground = CreateUIObject("Item Background", item);
            GameObject itemCheckmark = CreateUIObject("Item Checkmark", item);
            GameObject itemLabel = UIKitManager.AddUIKitGameObject(UIKitManager.UIKitGameObjectType.Text, item);

            // Sub controls.

            GameObject scrollbar = CreateScrollbar(resources);
            scrollbar.name = "Scrollbar";
            SetParentAndAlign(scrollbar, template);

            UIKitScrollbar scrollbarScrollbar = scrollbar.GetComponent<BetterScrollbar>();
            scrollbarScrollbar.SetDirection(UIKitScrollbar.Direction.BottomToTop, true);

            RectTransform vScrollbarRT = scrollbar.GetComponent<RectTransform>();
            vScrollbarRT.anchorMin = Vector2.right;
            vScrollbarRT.anchorMax = Vector2.one;
            vScrollbarRT.pivot = Vector2.one;
            vScrollbarRT.sizeDelta = new Vector2(vScrollbarRT.sizeDelta.x, 0);

            // Setup item UI components.

            itemLabel.name = "Item Label";
            TextStyleComponent itemLabelTextStyle = itemLabel.GetComponent<TextStyleComponent>();

            //            TextStyleComponent itemLabelText = itemLabel.AddComponent<TextStyleComponent>();
            //            SetDefaultTextValues(itemLabelText);
            //            itemLabelText.alignment = TextAnchor.MiddleLeft;

            Image itemBackgroundImage = itemBackground.AddComponent<BetterImage>();
            itemBackgroundImage.color = new Color32(245, 245, 245, 255);

            Image itemCheckmarkImage = itemCheckmark.AddComponent<BetterImage>();
            itemCheckmarkImage.sprite = resources.checkmark;

            UIKitToggle itemToggle = item.AddComponent<BetterToggle>();
            itemToggle.targetGraphic = itemBackgroundImage;
            itemToggle.graphic = itemCheckmarkImage;
            itemToggle.isOn = true;

            // Setup template UI components.

            Image templateImage = template.AddComponent<BetterImage>();
            templateImage.sprite = resources.standard;
            templateImage.type = Image.Type.Sliced;

            UIKitScrollRect templateScrollRect = template.AddComponent<BetterScrollRect>();
            templateScrollRect.content = (RectTransform)content.transform;
            templateScrollRect.viewport = (RectTransform)viewport.transform;
            templateScrollRect.horizontal = false;
            templateScrollRect.movementType = UIKitScrollRect.MovementType.Clamped;
            templateScrollRect.verticalScrollbar = scrollbarScrollbar;
            templateScrollRect.verticalScrollbarVisibility = UIKitScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            templateScrollRect.verticalScrollbarSpacing = -3;

            Mask scrollRectMask = viewport.AddComponent<Mask>();
            scrollRectMask.showMaskGraphic = false;

            Image viewportImage = viewport.AddComponent<BetterImage>();
            viewportImage.sprite = resources.mask;
            viewportImage.type = Image.Type.Sliced;

            // Setup dropdown UI components.

            label.name = "Label";
            TextStyleComponent labelTextStyle = label.GetComponent<TextStyleComponent>();

            //            labelText.alignment = TextAnchor.MiddleLeft;
            //            Text labelText = label.AddComponent<Text>();
            //            SetDefaultTextValues(labelText);
            //            labelText.alignment = TextAnchor.MiddleLeft;

            Image arrowImage = arrow.AddComponent<BetterImage>();
            arrowImage.sprite = resources.dropdown;

            Image backgroundImage = root.AddComponent<BetterImage>();
            backgroundImage.sprite = resources.standard;
            backgroundImage.color = s_DefaultSelectableColor;
            backgroundImage.type = Image.Type.Sliced;

            UIKitDropdown dropdown = root.AddComponent<BetterDropdown>();
            dropdown.targetGraphic = backgroundImage;
            SetDefaultColorTransitionValues(dropdown);
            dropdown.template = template.GetComponent<RectTransform>();
            dropdown.captionText = labelTextStyle;
            dropdown.itemText = itemLabelTextStyle;

            FocusIndicatorComponent focus = childFocusIndicator.GetComponent<FocusIndicatorComponent>();
            dropdown.focusIndicator = focus;

            // Setting default Item list.
            itemLabelTextStyle.text = "Option A";
            dropdown.options.Add(new UIKitDropdown.OptionData { text = "Option A" });
            dropdown.options.Add(new UIKitDropdown.OptionData { text = "Option B" });
            dropdown.options.Add(new UIKitDropdown.OptionData { text = "Option C" });
            dropdown.RefreshShownValue();

            // Set up RectTransforms.

            RectTransform labelRT = label.GetComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = new Vector2(10, 6);
            labelRT.offsetMax = new Vector2(-25, -7);

            RectTransform arrowRT = arrow.GetComponent<RectTransform>();
            arrowRT.anchorMin = new Vector2(1, 0.5f);
            arrowRT.anchorMax = new Vector2(1, 0.5f);
            arrowRT.sizeDelta = new Vector2(20, 20);
            arrowRT.anchoredPosition = new Vector2(-15, 0);

            RectTransform templateRT = template.GetComponent<RectTransform>();
            templateRT.anchorMin = new Vector2(0, 0);
            templateRT.anchorMax = new Vector2(1, 0);
            templateRT.pivot = new Vector2(0.5f, 1);
            templateRT.anchoredPosition = new Vector2(0, 2);
            templateRT.sizeDelta = new Vector2(0, 150);

            RectTransform viewportRT = viewport.GetComponent<RectTransform>();
            viewportRT.anchorMin = new Vector2(0, 0);
            viewportRT.anchorMax = new Vector2(1, 1);
            viewportRT.sizeDelta = new Vector2(-18, 0);
            viewportRT.pivot = new Vector2(0, 1);

            RectTransform contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0f, 1);
            contentRT.anchorMax = new Vector2(1f, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.anchoredPosition = new Vector2(0, 0);
            contentRT.sizeDelta = new Vector2(0, 28);

            RectTransform itemRT = item.GetComponent<RectTransform>();
            itemRT.anchorMin = new Vector2(0, 0.5f);
            itemRT.anchorMax = new Vector2(1, 0.5f);
            itemRT.sizeDelta = new Vector2(0, 20);

            RectTransform itemBackgroundRT = itemBackground.GetComponent<RectTransform>();
            itemBackgroundRT.anchorMin = Vector2.zero;
            itemBackgroundRT.anchorMax = Vector2.one;
            itemBackgroundRT.sizeDelta = Vector2.zero;

            RectTransform itemCheckmarkRT = itemCheckmark.GetComponent<RectTransform>();
            itemCheckmarkRT.anchorMin = new Vector2(0, 0.5f);
            itemCheckmarkRT.anchorMax = new Vector2(0, 0.5f);
            itemCheckmarkRT.sizeDelta = new Vector2(20, 20);
            itemCheckmarkRT.anchoredPosition = new Vector2(10, 0);

            RectTransform itemLabelRT = itemLabel.GetComponent<RectTransform>();
            itemLabelRT.anchorMin = Vector2.zero;
            itemLabelRT.anchorMax = Vector2.one;
            itemLabelRT.offsetMin = new Vector2(20, 1);
            itemLabelRT.offsetMax = new Vector2(-10, -2);

            template.SetActive(false);

            return root;
            */
        }

        public static GameObject CreateScrollView(Resources resources)
        {
            return UIKitManager.AddUIKitGameObject(UIKitManager.UIKitGameObjectType.ScrollView);
            /*
            GameObject root = CreateUIElementRoot("Scroll View", new Vector2(200, 200));

            GameObject viewport = CreateUIObject("Viewport", root);
            GameObject content = CreateUIObject("Content", viewport);

            // Sub controls.

            GameObject hScrollbar = CreateScrollbar(resources);
            hScrollbar.name = "Scrollbar Horizontal";
            SetParentAndAlign(hScrollbar, root);
            RectTransform hScrollbarRT = hScrollbar.GetComponent<RectTransform>();
            hScrollbarRT.anchorMin = Vector2.zero;
            hScrollbarRT.anchorMax = Vector2.right;
            hScrollbarRT.pivot = Vector2.zero;
            hScrollbarRT.sizeDelta = new Vector2(0, hScrollbarRT.sizeDelta.y);

            GameObject vScrollbar = CreateScrollbar(resources);
            vScrollbar.name = "Scrollbar Vertical";
            SetParentAndAlign(vScrollbar, root);
            vScrollbar.GetComponent<UIKitScrollbar>().SetDirection(UIKitScrollbar.Direction.BottomToTop, true);
            RectTransform vScrollbarRT = vScrollbar.GetComponent<RectTransform>();
            vScrollbarRT.anchorMin = Vector2.right;
            vScrollbarRT.anchorMax = Vector2.one;
            vScrollbarRT.pivot = Vector2.one;
            vScrollbarRT.sizeDelta = new Vector2(vScrollbarRT.sizeDelta.x, 0);

            // Setup RectTransforms.

            // Make viewport fill entire scroll view.
            RectTransform viewportRT = viewport.GetComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.sizeDelta = Vector2.zero;
            viewportRT.pivot = Vector2.up;

            // Make context match viewpoprt width and be somewhat taller.
            // This will show the vertical scrollbar and not the horizontal one.
            RectTransform contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin = Vector2.up;
            contentRT.anchorMax = Vector2.one;
            contentRT.sizeDelta = new Vector2(0, 300);
            contentRT.pivot = Vector2.up;

            // Setup UI components.

            UIKitScrollRect scrollRect = root.AddComponent<BetterScrollRect>();
            scrollRect.content = contentRT;
            scrollRect.viewport = viewportRT;
            scrollRect.horizontalScrollbar = hScrollbar.GetComponent<UIKitScrollbar>();
            scrollRect.verticalScrollbar = vScrollbar.GetComponent<UIKitScrollbar>();
            scrollRect.horizontalScrollbarVisibility = UIKitScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.verticalScrollbarVisibility = UIKitScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.horizontalScrollbarSpacing = -3;
            scrollRect.verticalScrollbarSpacing = -3;

            Image rootImage = root.AddComponent<BetterImage>();
            rootImage.sprite = resources.background;
            rootImage.type = Image.Type.Sliced;
            rootImage.color = s_PanelColor;

            Mask viewportMask = viewport.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            Image viewportImage = viewport.AddComponent<BetterImage>();
            viewportImage.sprite = resources.mask;
            viewportImage.type = Image.Type.Sliced;

            return root;
            */
        }
#endif
    }
}

using KineTutor3D.App.Fairino;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.Editor
{
    public static class FairinoLiveSmokePanelAuthoring
    {
        private const string ScenePath = "Assets/Scenes/FR5_Template_Demo.unity";
        private const string RootName = "FairinoLiveSmokeCanvas";
        private const float UiScale = 1.2f;

        [MenuItem("RobotTemplate/Author FAIRINO Live Smoke UI In Demo Scene")]
        public static void AuthorUiInDemoScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var root = GetOrCreateCanvasRoot();
            var panel = GetOrCreatePanel(root.transform);

            var ipInput = GetOrCreateInputField(panel.transform, "IpInput", SVec(16f, -64f), "192.168.58.2");
            var portInput = GetOrCreateInputField(panel.transform, "PortInput", SVec(220f, -64f), "8080");
            var runButton = GetOrCreateButton(panel.transform, "RunButton", SVec(16f, -116f), SVec(388f, 34f), "연결 신호 확인");
            var statusValue = GetOrCreateText(panel.transform, "StatusValue", SVec(16f, -166f), SVec(388f, 22f), "미실행", SInt(14), FontStyle.Bold, TextAnchor.MiddleLeft);
            var detailBox = GetOrCreateBox(panel.transform, "DetailBox", SVec(16f, -196f), SVec(388f, 138f), new Color(0.11f, 0.12f, 0.17f, 0.95f));
            var detailValue = GetOrCreateText(detailBox.transform, "DetailValue", SVec(10f, -10f), SVec(368f, 118f), "버튼을 눌러 연결 신호만 확인하세요.", SInt(12), FontStyle.Normal, TextAnchor.UpperLeft);

            GetOrCreateText(panel.transform, "Title", SVec(16f, -16f), SVec(388f, 24f), "FAIRINO Read-Only Live Smoke", SInt(16), FontStyle.Bold, TextAnchor.MiddleLeft);
            GetOrCreateText(panel.transform, "Subtitle", SVec(16f, -38f), SVec(388f, 18f), "Connect -> Version -> State -> Disconnect만 수행합니다.", SInt(11), FontStyle.Normal, TextAnchor.MiddleLeft);
            GetOrCreateText(panel.transform, "IpLabel", SVec(16f, -48f), SVec(80f, 14f), "IP", SInt(11), FontStyle.Bold, TextAnchor.MiddleLeft);
            GetOrCreateText(panel.transform, "PortLabel", SVec(220f, -48f), SVec(80f, 14f), "Port", SInt(11), FontStyle.Bold, TextAnchor.MiddleLeft);
            GetOrCreateText(panel.transform, "StatusLabel", SVec(16f, -146f), SVec(80f, 16f), "상태", SInt(11), FontStyle.Bold, TextAnchor.MiddleLeft);

            var smokePanel = root.GetComponent<FairinoLiveSmokePanel>();
            if (smokePanel == null)
            {
                smokePanel = root.AddComponent<FairinoLiveSmokePanel>();
            }

            AssignField(smokePanel, "ipInput", ipInput);
            AssignField(smokePanel, "portInput", portInput);
            AssignField(smokePanel, "runButton", runButton);
            AssignField(smokePanel, "statusText", statusValue);
            AssignField(smokePanel, "detailText", detailValue);

            EditorUtility.SetDirty(root);
            EditorUtility.SetDirty(smokePanel);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[RobotTemplate] FAIRINO live smoke UI authored into FR5_Template_Demo.");
        }

        private static GameObject GetOrCreateCanvasRoot()
        {
            var existing = GameObject.Find(RootName);
            if (existing != null)
            {
                EnsureCanvasSetup(existing);
                return existing;
            }

            var go = new GameObject(RootName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            EnsureCanvasSetup(go);
            EnsureEventSystem();
            return go;
        }

        private static void EnsureCanvasSetup(GameObject go)
        {
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null)
            {
                return;
            }

            new GameObject(
                "EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
        }

        private static GameObject GetOrCreatePanel(Transform parent)
        {
            var existing = parent.Find("Panel");
            if (existing != null)
            {
                ApplyRect(existing.GetComponent<RectTransform>(), SVec(420f, 360f), new Vector2(1f, 1f), new Vector2(1f, 1f), SVec(-12f, -12f));
                var existingImage = existing.GetComponent<Image>();
                if (existingImage != null)
                {
                    existingImage.color = new Color(0.09f, 0.10f, 0.15f, 0.92f);
                }
                return existing.gameObject;
            }

            var panel = CreateUiObject("Panel", parent, SVec(420f, 360f), new Vector2(1f, 1f), new Vector2(1f, 1f), SVec(-12f, -12f));
            var image = panel.AddComponent<Image>();
            image.color = new Color(0.09f, 0.10f, 0.15f, 0.92f);
            return panel;
        }

        private static InputField GetOrCreateInputField(Transform parent, string name, Vector2 position, string defaultValue)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                ApplyRect(existing.GetComponent<RectTransform>(), SVec(184f, 28f), new Vector2(0f, 1f), new Vector2(0f, 1f), position);
                var existingImage = existing.GetComponent<Image>();
                if (existingImage != null)
                {
                    existingImage.color = Color.white;
                }

                var existingInput = existing.GetComponent<InputField>();
                existingInput.textComponent = GetOrCreateText(existing, "Text", SVec(10f, -4f), SVec(164f, 20f), existingInput.text, SInt(12), FontStyle.Normal, TextAnchor.MiddleLeft);
                existingInput.placeholder = GetOrCreateText(existing, "Placeholder", SVec(10f, -4f), SVec(164f, 20f), defaultValue, SInt(12), FontStyle.Italic, TextAnchor.MiddleLeft, new Color(0.45f, 0.45f, 0.45f, 0.8f));
                return existingInput;
            }

            var root = CreateUiObject(name, parent, SVec(184f, 28f), new Vector2(0f, 1f), new Vector2(0f, 1f), position);
            var image = root.AddComponent<Image>();
            image.color = Color.white;

            var input = root.AddComponent<InputField>();
            input.text = defaultValue;
            input.textComponent = GetOrCreateText(root.transform, "Text", SVec(10f, -4f), SVec(164f, 20f), defaultValue, SInt(12), FontStyle.Normal, TextAnchor.MiddleLeft);
            input.placeholder = GetOrCreateText(root.transform, "Placeholder", SVec(10f, -4f), SVec(164f, 20f), defaultValue, SInt(12), FontStyle.Italic, TextAnchor.MiddleLeft, new Color(0.45f, 0.45f, 0.45f, 0.8f));
            return input;
        }

        private static Button GetOrCreateButton(Transform parent, string name, Vector2 position, Vector2 size, string label)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                ApplyRect(existing.GetComponent<RectTransform>(), size, new Vector2(0f, 1f), new Vector2(0f, 1f), position);
                var existingImage = existing.GetComponent<Image>();
                if (existingImage != null)
                {
                    existingImage.color = new Color(0.16f, 0.49f, 0.88f, 1f);
                }

                var existingButton = existing.GetComponent<Button>();
                var existingColors = existingButton.colors;
                existingColors.normalColor = new Color(0.16f, 0.49f, 0.88f, 1f);
                existingColors.highlightedColor = new Color(0.22f, 0.56f, 0.94f, 1f);
                existingColors.pressedColor = new Color(0.11f, 0.40f, 0.78f, 1f);
                existingColors.selectedColor = existingColors.highlightedColor;
                existingButton.colors = existingColors;

                GetOrCreateText(existing, "Text", Vector2.zero, size, label, SInt(13), FontStyle.Bold, TextAnchor.MiddleCenter);
                return existingButton;
            }

            var root = CreateUiObject(name, parent, size, new Vector2(0f, 1f), new Vector2(0f, 1f), position);
            var image = root.AddComponent<Image>();
            image.color = new Color(0.16f, 0.49f, 0.88f, 1f);

            var button = root.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = new Color(0.22f, 0.56f, 0.94f, 1f);
            colors.pressedColor = new Color(0.11f, 0.40f, 0.78f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            GetOrCreateText(root.transform, "Text", Vector2.zero, size, label, SInt(13), FontStyle.Bold, TextAnchor.MiddleCenter);
            return button;
        }

        private static GameObject GetOrCreateBox(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                ApplyRect(existing.GetComponent<RectTransform>(), size, new Vector2(0f, 1f), new Vector2(0f, 1f), position);
                var existingImage = existing.GetComponent<Image>();
                if (existingImage != null)
                {
                    existingImage.color = color;
                }
                return existing.gameObject;
            }

            var root = CreateUiObject(name, parent, size, new Vector2(0f, 1f), new Vector2(0f, 1f), position);
            var image = root.AddComponent<Image>();
            image.color = color;
            return root;
        }

        private static Text GetOrCreateText(
            Transform parent,
            string name,
            Vector2 position,
            Vector2 size,
            string text,
            int fontSize,
            FontStyle fontStyle,
            TextAnchor anchor,
            Color? color = null)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                var existingText = existing.GetComponent<Text>();
                ApplyRect(existing.GetComponent<RectTransform>(), size, new Vector2(0f, 1f), new Vector2(0f, 1f), position);
                existingText.font = GetBuiltinFont();
                existingText.text = text;
                existingText.fontSize = fontSize;
                existingText.fontStyle = fontStyle;
                existingText.alignment = anchor;
                existingText.horizontalOverflow = HorizontalWrapMode.Wrap;
                existingText.verticalOverflow = VerticalWrapMode.Overflow;
                existingText.color = color ?? new Color(0.93f, 0.95f, 0.98f, 1f);
                return existingText;
            }

            var root = CreateUiObject(name, parent, size, new Vector2(0f, 1f), new Vector2(0f, 1f), position);
            var uiText = root.AddComponent<Text>();
            uiText.font = GetBuiltinFont();
            uiText.text = text;
            uiText.fontSize = fontSize;
            uiText.fontStyle = fontStyle;
            uiText.alignment = anchor;
            uiText.horizontalOverflow = HorizontalWrapMode.Wrap;
            uiText.verticalOverflow = VerticalWrapMode.Overflow;
            uiText.color = color ?? new Color(0.93f, 0.95f, 0.98f, 1f);
            return uiText;
        }

        private static GameObject CreateUiObject(
            string name,
            Transform parent,
            Vector2 size,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            return go;
        }

        private static void ApplyRect(RectTransform rect, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
        }

        private static Vector2 SVec(float x, float y)
        {
            return new Vector2(S(x), S(y));
        }

        private static float S(float value)
        {
            return value * UiScale;
        }

        private static int SInt(int value)
        {
            return Mathf.RoundToInt(value * UiScale);
        }

        private static Font GetBuiltinFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static void AssignField(Object target, string fieldName, Object value)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(fieldName);
            if (property == null)
            {
                return;
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}

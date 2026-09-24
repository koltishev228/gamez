using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Zomboid.Client.UI.Inventory;
using System.IO;

namespace Zomboid.Editor.UI
{
    public static class UIGenerator
    {
        private const string UIPrefabPath = "Assets/_Game/Prefabs/UI";

        [MenuItem("Zomboid/UI/Generate Inventory Prefabs")]
        public static void GenerateInventoryUI()
        {
            if (!AssetDatabase.IsValidFolder(UIPrefabPath))
            {
                Directory.CreateDirectory(UIPrefabPath);
                AssetDatabase.Refresh();
            }

            // 1. Создаем префаб слота
            GameObject slotObj = CreateInventorySlot();
            string slotPath = $"{UIPrefabPath}/InventorySlot.prefab";
            GameObject slotPrefab = PrefabUtility.SaveAsPrefabAsset(slotObj, slotPath);
            GameObject.DestroyImmediate(slotObj);

            // 2. Создаем префаб главного окна
            GameObject canvasObj = CreateInventoryCanvas(slotPrefab.GetComponent<InventorySlotUI>());
            string canvasPath = $"{UIPrefabPath}/InventoryCanvas.prefab";
            GameObject canvasPrefab = PrefabUtility.SaveAsPrefabAsset(canvasObj, canvasPath);
            GameObject.DestroyImmediate(canvasObj);

            Debug.Log($"[UI Generator] Готово! Префабы инвентаря созданы в {UIPrefabPath}.");
        }

        [MenuItem("Zomboid/UI/Generate Main Menu Prefab")]
        public static void GenerateMainMenuUI()
        {
            if (!AssetDatabase.IsValidFolder(UIPrefabPath))
            {
                Directory.CreateDirectory(UIPrefabPath);
                AssetDatabase.Refresh();
            }

            GameObject canvasObj = CreateMainMenuCanvas();
            string canvasPath = $"{UIPrefabPath}/MainMenuCanvas.prefab";
            GameObject canvasPrefab = PrefabUtility.SaveAsPrefabAsset(canvasObj, canvasPath);
            GameObject.DestroyImmediate(canvasObj);

            Debug.Log($"[UI Generator] Готово! Префаб главного меню создан в {UIPrefabPath}.");
        }

        private static GameObject CreateMainMenuCanvas()
        {
            GameObject canvasObj = new GameObject("MainMenuCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Главный фон (темный)
            GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(canvasObj.transform, false);
            RectTransform bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.05f, 1f); 

            // Левая панель (как в Zomboid)
            GameObject leftPanel = new GameObject("LeftPanel", typeof(RectTransform), typeof(Image));
            leftPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform leftRt = leftPanel.GetComponent<RectTransform>();
            leftRt.anchorMin = new Vector2(0, 0);
            leftRt.anchorMax = new Vector2(0, 1); // Тянется по вертикали слева
            leftRt.pivot = new Vector2(0, 0.5f);
            leftRt.anchoredPosition = Vector2.zero;
            leftRt.sizeDelta = new Vector2(600, 0);
            leftPanel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

            // Красная полоска сбоку для стиля
            GameObject redStripe = new GameObject("RedStripe", typeof(RectTransform), typeof(Image));
            redStripe.transform.SetParent(leftPanel.transform, false);
            RectTransform stripeRt = redStripe.GetComponent<RectTransform>();
            stripeRt.anchorMin = new Vector2(1, 0);
            stripeRt.anchorMax = new Vector2(1, 1);
            stripeRt.pivot = new Vector2(1, 0.5f);
            stripeRt.anchoredPosition = Vector2.zero;
            stripeRt.sizeDelta = new Vector2(5, 0);
            redStripe.GetComponent<Image>().color = new Color(0.6f, 0.1f, 0.1f, 1f);

            // Заголовок
            GameObject titleObj = CreateText("Title", leftPanel.transform);
            RectTransform titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 1);
            titleRt.anchorMax = new Vector2(1, 1);
            titleRt.pivot = new Vector2(0.5f, 1);
            titleRt.anchoredPosition = new Vector2(0, -100);
            titleRt.sizeDelta = new Vector2(-100, 200); // Отступы по краям
            var titleTxt = titleObj.GetComponent<TextMeshProUGUI>();
            titleTxt.alignment = TextAlignmentOptions.Left | TextAlignmentOptions.Top;
            titleTxt.fontSize = 80;
            titleTxt.fontStyle = FontStyles.Bold;
            titleTxt.color = new Color(0.8f, 0.1f, 0.1f);
            titleTxt.text = "ZOMBIE\nSURVIVAL";
            titleTxt.lineSpacing = -20f;

            // Панель кнопок
            GameObject buttonPanel = new GameObject("ButtonPanel", typeof(RectTransform), typeof(VerticalLayoutGroup));
            buttonPanel.transform.SetParent(leftPanel.transform, false);
            RectTransform panelRt = buttonPanel.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0, 0);
            panelRt.anchorMax = new Vector2(1, 0);
            panelRt.pivot = new Vector2(0.5f, 0);
            panelRt.anchoredPosition = new Vector2(0, 150);
            panelRt.sizeDelta = new Vector2(-100, 400); // 50px отступы по бокам

            var vlg = buttonPanel.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 15;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false; // Высоту кнопок зададим вручную
            vlg.childForceExpandHeight = false;

            // Кнопки
            string[] buttons = { "СОЗДАТЬ СЕРВЕР", "ПОДКЛЮЧИТЬСЯ", "НАСТРОЙКИ", "ВЫХОД" };
            foreach (var btnName in buttons)
            {
                GameObject btnObj = new GameObject(btnName, typeof(RectTransform), typeof(Image), typeof(Button));
                btnObj.transform.SetParent(buttonPanel.transform, false);
                
                RectTransform btnRt = btnObj.GetComponent<RectTransform>();
                btnRt.sizeDelta = new Vector2(0, 60); // Высота кнопки 60
                btnObj.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 1f);
                
                GameObject txtObj = CreateText("Text", btnObj.transform);
                RectTransform txtRt = txtObj.GetComponent<RectTransform>();
                txtRt.anchorMin = Vector2.zero;
                txtRt.anchorMax = Vector2.one;
                txtRt.offsetMin = new Vector2(20, 0); // Текст смещен чуть вправо
                txtRt.offsetMax = Vector2.zero;
                var txt = txtObj.GetComponent<TextMeshProUGUI>();
                txt.alignment = TextAlignmentOptions.Left | TextAlignmentOptions.Midline;
                txt.fontSize = 26;
                txt.fontStyle = FontStyles.Bold;
                txt.color = new Color(0.9f, 0.9f, 0.9f);
                txt.text = btnName;
            }

            return canvasObj;
        }

        private static GameObject CreateInventorySlot()
        {
            GameObject slot = new GameObject("InventorySlot", typeof(RectTransform), typeof(Image), typeof(InventorySlotUI));
            RectTransform rt = slot.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 65); // 0 width means it stretches to layout
            
            // Фон слота
            slot.GetComponent<Image>().color = new Color(0.12f, 0.12f, 0.12f, 0.9f);

            var slotUI = slot.GetComponent<InventorySlotUI>();

            // Иконка (с рамкой)
            GameObject iconBg = new GameObject("IconBG", typeof(RectTransform), typeof(Image));
            iconBg.transform.SetParent(slot.transform, false);
            RectTransform iconBgRt = iconBg.GetComponent<RectTransform>();
            iconBgRt.anchorMin = new Vector2(0, 0.5f);
            iconBgRt.anchorMax = new Vector2(0, 0.5f);
            iconBgRt.pivot = new Vector2(0, 0.5f);
            iconBgRt.anchoredPosition = new Vector2(10, 0);
            iconBgRt.sizeDelta = new Vector2(50, 50);
            iconBg.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.05f, 1f);

            GameObject icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(iconBg.transform, false);
            RectTransform iconRt = icon.GetComponent<RectTransform>();
            iconRt.anchorMin = Vector2.zero;
            iconRt.anchorMax = Vector2.one;
            iconRt.offsetMin = new Vector2(2, 2);
            iconRt.offsetMax = new Vector2(-2, -2);
            slotUI.IconImage = icon.GetComponent<Image>();

            // Название предмета
            GameObject nameObj = CreateText("NameText", slot.transform);
            RectTransform nameRt = nameObj.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0, 0.5f);
            nameRt.anchorMax = new Vector2(1, 1);
            nameRt.offsetMin = new Vector2(75, 5);
            nameRt.offsetMax = new Vector2(-80, -5);
            var nameTxt = nameObj.GetComponent<TextMeshProUGUI>();
            nameTxt.alignment = TextAlignmentOptions.BottomLeft;
            nameTxt.fontSize = 18;
            nameTxt.fontStyle = FontStyles.Bold;
            nameTxt.text = "Item Name";
            slotUI.NameText = nameTxt;

            // Категория предмета (Визуальная заглушка)
            GameObject catObj = CreateText("CategoryText", slot.transform);
            RectTransform catRt = catObj.GetComponent<RectTransform>();
            catRt.anchorMin = new Vector2(0, 0);
            catRt.anchorMax = new Vector2(1, 0.5f);
            catRt.offsetMin = new Vector2(75, 5);
            catRt.offsetMax = new Vector2(-80, -5);
            var catTxt = catObj.GetComponent<TextMeshProUGUI>();
            catTxt.alignment = TextAlignmentOptions.TopLeft;
            catTxt.fontSize = 14;
            catTxt.color = new Color(0.6f, 0.6f, 0.6f);
            catTxt.text = "Оружие ближнего боя";

            // Вес
            GameObject weightObj = CreateText("WeightText", slot.transform);
            RectTransform weightRt = weightObj.GetComponent<RectTransform>();
            weightRt.anchorMin = new Vector2(1, 0);
            weightRt.anchorMax = new Vector2(1, 1);
            weightRt.pivot = new Vector2(1, 0.5f);
            weightRt.anchoredPosition = new Vector2(-15, 0);
            weightRt.sizeDelta = new Vector2(60, 0);
            var weightTxt = weightObj.GetComponent<TextMeshProUGUI>();
            weightTxt.alignment = TextAlignmentOptions.Right | TextAlignmentOptions.Midline;
            weightTxt.fontSize = 16;
            weightTxt.color = new Color(0.8f, 0.8f, 0.8f);
            weightTxt.text = "1.5 кг";
            slotUI.WeightText = weightTxt;

            // Количество (бейдж)
            GameObject amountObj = CreateText("AmountText", slot.transform);
            RectTransform amountRt = amountObj.GetComponent<RectTransform>();
            amountRt.anchorMin = new Vector2(0, 0);
            amountRt.anchorMax = new Vector2(0, 0);
            amountRt.pivot = new Vector2(0, 0);
            amountRt.anchoredPosition = new Vector2(40, 5);
            amountRt.sizeDelta = new Vector2(30, 20);
            var amountTxt = amountObj.GetComponent<TextMeshProUGUI>();
            amountTxt.alignment = TextAlignmentOptions.Right | TextAlignmentOptions.Bottom;
            amountTxt.fontSize = 14;
            amountTxt.fontStyle = FontStyles.Bold;
            amountTxt.color = Color.white;
            amountTxt.text = "x2";
            // Тень для бейджа
            var outline = amountObj.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1, -1);
            
            slotUI.AmountText = amountTxt;
            amountObj.SetActive(false);

            return slot;
        }

        private static GameObject CreateInventoryCanvas(InventorySlotUI slotPrefab)
        {
            GameObject canvasObj = new GameObject("InventoryCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Главное окно
            GameObject window = new GameObject("InventoryWindow", typeof(RectTransform), typeof(Image), typeof(InventoryUI));
            window.transform.SetParent(canvasObj.transform, false);
            RectTransform windowRt = window.GetComponent<RectTransform>();
            windowRt.anchorMin = new Vector2(1, 0.5f);
            windowRt.anchorMax = new Vector2(1, 0.5f);
            windowRt.pivot = new Vector2(1, 0.5f);
            windowRt.anchoredPosition = new Vector2(-50, 0);
            windowRt.sizeDelta = new Vector2(450, 750);
            window.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.95f); // Темный фон

            var inventoryUI = window.GetComponent<InventoryUI>();
            inventoryUI.SlotPrefab = slotPrefab;

            // Заголовок
            GameObject header = new GameObject("Header", typeof(RectTransform));
            header.transform.SetParent(window.transform, false);
            RectTransform headerRt = header.GetComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0, 1);
            headerRt.anchorMax = new Vector2(1, 1);
            headerRt.offsetMin = new Vector2(20, -50);
            headerRt.offsetMax = new Vector2(-20, -10);

            GameObject titleObj = CreateText("Title", header.transform);
            RectTransform titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 0);
            titleRt.anchorMax = new Vector2(1, 1);
            titleRt.offsetMin = Vector2.zero;
            titleRt.offsetMax = Vector2.zero;
            var titleTxt = titleObj.GetComponent<TextMeshProUGUI>();
            titleTxt.alignment = TextAlignmentOptions.Left | TextAlignmentOptions.Midline;
            titleTxt.fontSize = 26;
            titleTxt.fontStyle = FontStyles.Bold;
            titleTxt.color = Color.white;
            titleTxt.text = "ИНВЕНТАРЬ";

            // Вес (Текст)
            GameObject weightTotalObj = CreateText("WeightTotal", header.transform);
            RectTransform weightTotalRt = weightTotalObj.GetComponent<RectTransform>();
            weightTotalRt.anchorMin = new Vector2(0, 0);
            weightTotalRt.anchorMax = new Vector2(1, 1);
            weightTotalRt.offsetMin = Vector2.zero;
            weightTotalRt.offsetMax = Vector2.zero;
            var wTotalTxt = weightTotalObj.GetComponent<TextMeshProUGUI>();
            wTotalTxt.alignment = TextAlignmentOptions.Right | TextAlignmentOptions.Midline;
            wTotalTxt.fontSize = 18;
            wTotalTxt.color = new Color(0.7f, 0.7f, 0.7f);
            wTotalTxt.text = "0.0 / 20.0 кг";

            // Табы (Категории)
            GameObject tabsObj = new GameObject("Tabs", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            tabsObj.transform.SetParent(window.transform, false);
            RectTransform tabsRt = tabsObj.GetComponent<RectTransform>();
            tabsRt.anchorMin = new Vector2(0, 1);
            tabsRt.anchorMax = new Vector2(1, 1);
            tabsRt.offsetMin = new Vector2(10, -90);
            tabsRt.offsetMax = new Vector2(-10, -50);

            var hlg = tabsObj.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 5;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            string[] tabNames = { "Все", "Оружие", "Медицина", "Еда", "Крафт" };
            int[] tabCats = { 0, 1, 2, 3, 4 };

            for (int i = 0; i < tabNames.Length; i++)
            {
                string tabName = tabNames[i];
                int catId = tabCats[i];

                GameObject tab = new GameObject($"Tab_{tabName}", typeof(RectTransform), typeof(Image), typeof(Button));
                tab.transform.SetParent(tabsObj.transform, false);
                tab.GetComponent<Image>().color = tabName == "Все" ? new Color(0.3f, 0.3f, 0.3f, 1f) : new Color(0.15f, 0.15f, 0.15f, 1f);
                
                Button btn = tab.GetComponent<Button>();
                var targetInfo = UnityEngine.Events.UnityEvent.GetValidMethodInfo(inventoryUI, nameof(InventoryUI.SetFilterInt), new System.Type[] { typeof(int) });
                UnityEditor.Events.UnityEventTools.AddIntPersistentListener(btn.onClick, new UnityEngine.Events.UnityAction<int>(inventoryUI.SetFilterInt), catId);

                GameObject tabTextObj = CreateText("Text", tab.transform);
                RectTransform tabTextRt = tabTextObj.GetComponent<RectTransform>();
                tabTextRt.anchorMin = Vector2.zero;
                tabTextRt.anchorMax = Vector2.one;
                tabTextRt.offsetMin = Vector2.zero;
                tabTextRt.offsetMax = Vector2.zero;
                var tabTxt = tabTextObj.GetComponent<TextMeshProUGUI>();
                tabTxt.alignment = TextAlignmentOptions.Center | TextAlignmentOptions.Midline;
                tabTxt.fontSize = 14;
                tabTxt.text = tabName;
                tabTxt.color = tabName == "Все" ? Color.white : new Color(0.6f, 0.6f, 0.6f);
            }

            // Scroll View
            GameObject scrollView = new GameObject("Scroll View", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scrollView.transform.SetParent(window.transform, false);
            RectTransform scrollRt = scrollView.GetComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0, 0);
            scrollRt.anchorMax = new Vector2(1, 1);
            scrollRt.offsetMin = new Vector2(10, 10);
            scrollRt.offsetMax = new Vector2(-10, -100); // Место для заголовка и табов
            scrollView.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.05f, 0.8f); // Чуть темнее фон списка

            // Viewport
            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollView.transform, false);
            RectTransform viewRt = viewport.GetComponent<RectTransform>();
            viewRt.anchorMin = new Vector2(0, 0);
            viewRt.anchorMax = new Vector2(1, 1);
            viewRt.offsetMin = Vector2.zero;
            viewRt.offsetMax = Vector2.zero;
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            // Content
            GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = new Vector2(0, 0);

            var vlg = content.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(5, 5, 5, 5);
            vlg.spacing = 5;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;

            var csf = content.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Настройка ScrollRect
            ScrollRect sr = scrollView.GetComponent<ScrollRect>();
            sr.content = contentRt;
            sr.viewport = viewRt;
            sr.horizontal = false;
            sr.vertical = true;
            sr.scrollSensitivity = 25f;

            inventoryUI.SlotsContainer = content.transform;

            return canvasObj;
        }

        private static GameObject CreateText(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, false);
            return obj;
        }
    }
}

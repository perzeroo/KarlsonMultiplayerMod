using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KarlsonMultiplayer.UI
{
    public class UI : MonoBehaviour
    {
        public static UI instance;
        
        public GameObject canvasGO;
        public GameObject inputFieldGO;
        private Text text;
        private InputField inputField;

        private void Awake()
        {
            instance = this;
            
            SetupUI();
        }

        public void SetupUI()
        {
            Font arial;
            arial = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");

            // Create Canvas GameObject.
            canvasGO = new GameObject();
            canvasGO.name = "Canvas";
            canvasGO.AddComponent<Canvas>();
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            
            //Thanks Sinai for help
            canvasGO.AddComponent<EventSystem>();
            canvasGO.AddComponent<StandaloneInputModule>();
            typeof(BaseInputModule).GetField("m_EventSystem", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(canvasGO.GetComponent<StandaloneInputModule>(), canvasGO.GetComponent<EventSystem>());
            EventSystem.current = canvasGO.GetComponent<EventSystem>();

            // Get canvas from the GameObject.
            Canvas canvas;
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Create the Text GameObject.
            inputFieldGO = new GameObject();
            inputFieldGO.transform.parent = canvasGO.transform;
            inputFieldGO.AddComponent<InputField>();
            inputFieldGO.name = "InputField";
            
            GameObject textGO = new GameObject();
            textGO.transform.parent = inputFieldGO.transform;
            textGO.AddComponent<Text>();
            textGO.name = "Text";
            

            // Set Text component properties.
            text = textGO.GetComponent<Text>();
            text.font = arial;
            text.text = "Press space key";
            text.fontSize = 30;

            inputField = inputFieldGO.GetComponent<InputField>();
            inputField.text = "Press T to type in chat";
            // inputField.placeholder = text;
            inputField.textComponent = text;
            inputField.inputType = InputField.InputType.Standard;
            inputField.characterLimit = 0;
            inputField.lineType = InputField.LineType.SingleLine;
            inputField.readOnly = false;
            inputField.contentType = InputField.ContentType.Standard;
            var position1 = canvasGO.transform.position;
            inputFieldGO.transform.localPosition = new Vector3(-(position1.x + position1.x / 2F), 0);
            inputFieldGO.transform.position = new Vector3(inputFieldGO.transform.position.x, 30);
            // inputFieldGO.transform.position = position1;

            // inputFieldGO.transform.position = canvasGO.GetComponent<Canvas>().

            // EventSystem.current.SetSelectedGameObject(textGO);
            // inputField.OnPointerClick(null);

            // Provide Text position and size using RectTransform.
            RectTransform rectTransform;
            rectTransform = text.GetComponent<RectTransform>();
            var position = canvasGO.transform.position;
            rectTransform.localPosition = new Vector3(position.x, 0, 0);
            rectTransform.sizeDelta = new Vector2(position.x, 100);
            
            inputFieldGO.SetActive(false);
            
            DontDestroyOnLoad(canvasGO);
        }

        private void FixedUpdate()
        {
            if (Input.GetKey(KeyCode.T))
            {
                inputFieldGO.SetActive(true);

                // EventSystem current;
                // (current = EventSystem.current).SetSelectedGameObject(inputField.gameObject, null);
                // inputField.OnPointerClick(new PointerEventData(current));
                inputField.ActivateInputField();
            }

            if (Input.GetKey(KeyCode.Return))
            {
                if (inputFieldGO.activeInHierarchy)
                {
                    UnityEngine.Debug.Log(inputField.text);
                    
                    inputFieldGO.SetActive(false);
                    inputField.text = "Press T to type in chat";
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace EngineThrustController
{
    class EngineThrustControllerGUI
    {
        private static EngineThrustControllerGUI s_singleton = null;
        public static EngineThrustControllerGUI GetInstance() { return CreateInstance(); }
        private static EngineThrustControllerGUI CreateInstance() { if (s_singleton == null) s_singleton = new EngineThrustControllerGUI(); return s_singleton; }

        #region GUI Related
        Rect WindowPos = new Rect(Screen.width - 350, 10, 10, 10);
        bool isMinimized = false;
        bool windowUpdated = false;
        bool editorLocked = false;
        Vector2 verticalScroll = Vector2.zero;
        #endregion

        private List<EngineThrustControllerGUIItem> m_guiItems = new List<EngineThrustControllerGUIItem>();
        
        public void RegisterGUIItem(EngineThrustControllerGUIItem item)
        {
            if(m_guiItems.Count == 0)
                AddGUI();

            if(m_guiItems.Contains(item)) return;
            m_guiItems.Add(item);
        }

        public void UnregisterGUIItem(EngineThrustControllerGUIItem item)
        {
            if(m_guiItems.Contains(item))
                m_guiItems.Remove(item);

            if(m_guiItems.Count == 0) DeleteGUI();
        }

        public void ClearGUIItem()
        {
            m_guiItems.Clear();
            DeleteGUI();
        }

        public void CheckClear()
        {
            Debug.Log("CheckClear()");
            for (int i = 0; i < m_guiItems.Count; ++i)
            {
                if (m_guiItems[i] == null)
                {
                    m_guiItems.RemoveAt(i);
                    --i;
                }
                else if (m_guiItems[i].CheckValid() == false)
                {
                    m_guiItems.RemoveAt(i);
                    --i;
                }
            }
            if (m_guiItems.Count == 0)
            {
                Debug.Log("Nothing left.");
                DeleteGUI();
            }
        }

        private void AddGUI()
        {
            Debug.Log("AddGUI"); 
            RenderingManager.AddToPostDrawQueue(3, DrawGUI);
        }

        private void DeleteGUI()
        {
            Debug.Log("DeleteGUI");
			if (editorLocked)
			{
				EditorLogic.fetch.Unlock("ETC");
				editorLocked = false;
			}
            RenderingManager.RemoveFromPostDrawQueue(3, DrawGUI);
        }

        public void DrawGUI()
        {
            if (windowUpdated)
            {
                WindowPos.width = WindowPos.height = 10;
                windowUpdated = false;
            }

            WindowPos = GUILayout.Window(2121314, WindowPos, WindowFunc, "Engine Thrust Controller", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.MinWidth(200));
            Vector3 mousePos = Input.mousePosition;         //Mouse location; based on Kerbal Engineer Redux code
            mousePos.y = Screen.height - mousePos.y;
            bool cursorInGUI = WindowPos.Contains(mousePos);
            //This locks and unlocks the editor as necessary; cannot constantly call the lock or unlock functions as that causes the editor to be constantly locked
            if (cursorInGUI && !editorLocked)
            {
                EditorLogic.fetch.Lock(true, true, true, "ETC");
                editorLocked = true;
            }
            else if (!cursorInGUI && editorLocked)
            {
                EditorLogic.fetch.Unlock("ETC");
                editorLocked = false;
            }
        }

        public void WindowFunc(int id)
        {
            bool isButtonClicked = false;
            if (isMinimized)
            {
                GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.MinWidth(Screen.width / 4), GUILayout.MaxWidth(Screen.width / 3));
            }
            else
            {
                GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.MinWidth(Screen.width / 4), GUILayout.MaxWidth(Screen.width / 3), GUILayout.MinHeight(Screen.height / 2));
            }
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.MinWidth(Screen.width / 4));
                {
                    isButtonClicked = GUILayout.Button((isMinimized ? "Restore" : "Minimize"), GUILayout.ExpandWidth(true));
                }
                GUILayout.EndHorizontal();

                for(int i = 0; i < m_guiItems.Count; ++i)
                {
                    EngineThrustControllerGUIItem item = m_guiItems[i];
                    if (item.CheckValid() == false)
                    {
                        item.UnregisterGUI(this);
                        --i;
                    }
                }

                if (isMinimized == false)
                {
                    verticalScroll = GUILayout.BeginScrollView(verticalScroll, GUILayout.ExpandWidth(true), GUILayout.MinWidth(Screen.width / 4), GUILayout.ExpandHeight(true), GUILayout.MinHeight(Screen.height / 8), GUILayout.MaxHeight(Screen.height / 2));
                    {
                        foreach (EngineThrustControllerGUIItem item in m_guiItems)
                        {
                            if (item.CheckAttached())
                                item.RenderGUI();
                        }
                    }
                    GUILayout.EndScrollView();
                }
            }
            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, 2000, 30));

            if (isButtonClicked)
            {
                isMinimized = !isMinimized;
                windowUpdated = true;
            }
        }
    }

    class EngineThrustControllerGUIItem
    {
        public EngineThrustControllerGUI m_gui = null;
        public ModuleEngineThrustController m_controller = null;

        #region GUI Related
        bool isExpanded = false;
        #endregion

        public EngineThrustControllerGUIItem(EngineThrustControllerGUI gui, ModuleEngineThrustController controller)
        {
            m_gui = gui;
            m_controller = controller;

            RegisterGUI(gui);
        }

        public void RegisterGUI(EngineThrustControllerGUI gui)
        {
            EngineThrustControllerGUI.GetInstance().RegisterGUIItem(this);
        }

        public void UnregisterGUI(EngineThrustControllerGUI gui)
        {
            EngineThrustControllerGUI.GetInstance().UnregisterGUIItem(this);
        }

        public bool CheckValid()
        {
            if (this.m_controller == null)
                return false;

            if (this.m_controller.part == null)
                return false;

            return true;
        }

        public bool CheckAttached()
        {
            if (CheckValid() == false) return false;

            if (this.m_controller.part.localRoot == EditorLogic.startPod)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void RenderGUI()
        {
            bool isIncreaseButtonClicked = false;
            bool isDecreaseButtonClicked = false;
            bool isExpandButtonClicked = false;

            GUIStyle sty = new GUIStyle(GUI.skin.button);
            sty.normal.textColor = sty.focused.textColor = Color.white;
            sty.hover.textColor = sty.active.textColor = Color.yellow;
            sty.onNormal.textColor = sty.onFocused.textColor = sty.onHover.textColor = sty.onActive.textColor = Color.green;
            sty.padding = new RectOffset(4, 4, 4, 4);

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            {
                isExpandButtonClicked = GUILayout.Button((isExpanded ? "-" : "+"), sty, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(30.0f));
                GUILayout.Label(m_controller.part.partInfo.title, sty, GUILayout.ExpandWidth(true));
            }
            GUILayout.EndHorizontal();
            if (isExpanded)
            {
                m_controller.part.SetHighlightType(Part.HighlightType.AlwaysOn);
                m_controller.part.SetHighlight(true);
                GUILayout.Label("Adjustment Range: (" + m_controller.minimumThrustPercent.ToString("0%") + " - " + m_controller.maximumThrustPercent.ToString("0%") + "), Step: " + m_controller.percentAdjustmentStep.ToString("0%"), sty, GUILayout.ExpandWidth(true));
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                {
                    isDecreaseButtonClicked = GUILayout.Button("-", sty, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(30.0f));
                    GUILayout.Box(m_controller.initialThrust.ToString("0%"), sty, GUILayout.ExpandWidth(true));
                    isIncreaseButtonClicked = GUILayout.Button("+", sty, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(30.0f));     

                }
                GUILayout.EndHorizontal();
    			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				{
					GUILayout.Box("Current group:"+m_controller.gp.ToString(), sty, GUILayout.ExpandWidth(true));
					if(GUILayout.Button("Set to Group 1", sty, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(300.0f)))m_controller.gp=1;
					if(GUILayout.Button("Set to Group 2", sty, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(300.0f)))m_controller.gp=2;
				}
				GUILayout.EndHorizontal();
            }
            else
            {
                if (m_controller.part.highlightType == Part.HighlightType.AlwaysOn)
                {
                    m_controller.part.SetHighlightType(Part.HighlightType.OnMouseOver);
                        m_controller.part.SetHighlight(false);
                }
            }

            if (isExpandButtonClicked)
                isExpanded = !isExpanded;
            if (isDecreaseButtonClicked)
                m_controller.DecreaseInitialThrust();
            if (isIncreaseButtonClicked)
                m_controller.IncreaseInitialThrust();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace EngineThrustController
{
    public partial class ModuleEngineThrustController : PartModule
    {
        [KSPField(isPersistant = true)]
        public bool canAdjustAtAnytime = true;
		[KSPField]
		public bool canAdjustOverride = false;

        [KSPField]
        public float percentAdjustmentStep = 0.1f;
		[KSPField(isPersistant = true)]
		public int gp=0;
        [KSPField]
        public float minimumThrustPercent = 0.4f;
        [KSPField]
        public float maximumThrustPercent = 1.0f;
        [KSPField(isPersistant = true)]
        public float initialThrust = 1.0f;

        [KSPField]
        float originalMaxThrust;
        [KSPField]
        float originalHeatProduction;

        [KSPField(isPersistant = true, guiActive = true, guiName = "Thrust Percent", guiFormat = "0%")]
		float thrustPercent;

		[KSPField]
		public bool showItemInList = true;

        ModuleEngines engine = null;
		StartState m_state = StartState.None;

        private void BindEngine()
        {
            engine = null;
            foreach (PartModule module in this.part.Modules)
            {
                if (module is ModuleEngines)
                {
                    engine = module as ModuleEngines;
                    break;
                }
            }
        }

        public override void OnStart(StartState state)
        {
            Debug.Log("ModuleEngineThrustController OnStart(" + state.ToString() + ")");
			m_state = state;

            if (state == StartState.None) return;
            
            BindEngine();
            
            if (minimumThrustPercent < 0.0f) minimumThrustPercent = 0.0f;
            if (minimumThrustPercent > 1.0f) minimumThrustPercent = 1.0f;

            if (maximumThrustPercent < 0.0f) maximumThrustPercent = 0.0f;
            if (maximumThrustPercent > 1.0f) maximumThrustPercent = 1.0f;

            if (minimumThrustPercent > maximumThrustPercent) minimumThrustPercent = maximumThrustPercent;

            if (initialThrust < minimumThrustPercent) initialThrust = minimumThrustPercent;
            if (initialThrust > maximumThrustPercent) initialThrust = maximumThrustPercent;

            // Save original engine data.
            if (engine != null)
            {
                originalMaxThrust = engine.maxThrust;
                originalHeatProduction = engine.heatProduction;
                if (((int)state & (int)StartState.PreLaunch) > 0)
                    thrustPercent = initialThrust;
                else if (state == StartState.Editor)
                    thrustPercent = initialThrust;

				engine.maxThrust = originalMaxThrust * thrustPercent;
				engine.heatProduction = originalHeatProduction * thrustPercent;
			}
            Debug.Log("Data saved:" + originalMaxThrust.ToString() + " " + thrustPercent.ToString("0%"));

            if (state == StartState.Editor)
            {
                EngineThrustControllerGUI.GetInstance().CheckClear();
				if (showItemInList == true)
				{
					EngineThrustControllerGUIItem item = new EngineThrustControllerGUIItem(EngineThrustControllerGUI.GetInstance(), this);
				}
                return;
            }
            else
            {
                EngineThrustControllerGUI.GetInstance().ClearGUIItem();
            }

			if (canAdjustAtAnytime == true)
			{
				Events["ContextMenuIncreaseThrust"].guiName = "Increase Thrust by " + percentAdjustmentStep.ToString("0%");
				Events["ContextMenuDecreaseThrust"].guiName = "Decrease Thrust by " + percentAdjustmentStep.ToString("0%");
			}
			else
			{
				Events["ContextMenuIncreaseThrust"].guiActive = false;
				Events["ContextMenuIncreaseThrust"].active = false;
				Events["ContextMenuDecreaseThrust"].guiActive = false;
				Events["ContextMenuDecreaseThrust"].active = false;
			}
			Events["Group1"].guiName = "Set Group 1";
			Events["Group2"].guiName = "Set Group 2";
			Events["Group1"].guiActive = false;
			Events["Group2"].guiActive = false;
            base.OnStart(state);
        }
		[KSPEvent(name = "Group1", guiActive = true, guiName = "Set Group 1", active = true, category = "Grouping")]
		public void Group1 ()
		{
			gp = 1;
		}
		[KSPEvent(name = "Group2", guiActive = true, guiName = "Set Group 1", active = true, category = "Grouping")]
		public void Group2 ()
		{
			gp = 2;
		}
        [KSPEvent(name = "ContextMenuIncreaseThrust", guiActive = true, guiName = "Increase Thrust", active = true, category = "Thrust Control")]
        public void ContextMenuIncreaseThrust()
        {
            if (canAdjustAtAnytime == false) return;
            
            thrustPercent += percentAdjustmentStep;
            if (thrustPercent > maximumThrustPercent) thrustPercent = maximumThrustPercent;
            if (thrustPercent < minimumThrustPercent) thrustPercent = minimumThrustPercent;
            if (thrustPercent > 1.0f) thrustPercent = 1.0f;
            if (thrustPercent < 0.0f) thrustPercent = 0.0f;

            engine = null;
            BindEngine();
            if (engine != null)
			{
                engine.maxThrust = originalMaxThrust * thrustPercent;
                engine.heatProduction = originalHeatProduction * thrustPercent;
            }
        }
        [KSPEvent(name = "ContextMenuDecreaseThrust", guiActive = true, guiName = "Decrease Thrust", active = true, category = "Thrust Control")]
        public void ContextMenuDecreaseThrust()
        {
            if (canAdjustAtAnytime == false) return;

            thrustPercent -= percentAdjustmentStep;
            if (thrustPercent > maximumThrustPercent) thrustPercent = maximumThrustPercent;
            if (thrustPercent < minimumThrustPercent) thrustPercent = minimumThrustPercent;
            if (thrustPercent > 1.0f) thrustPercent = 1.0f;
            if (thrustPercent < 0.0f) thrustPercent = 0.0f;

            engine = null;
            BindEngine();
            if (engine != null)
			{
				engine.maxThrust = originalMaxThrust * thrustPercent;
                engine.heatProduction = originalHeatProduction * thrustPercent;
            }
        }
        [KSPAction("Increase thrust", actionGroup = KSPActionGroup.None)]
        public void ActionGroupIncreaseThrust(KSPActionParam param)
        {
            ContextMenuIncreaseThrust();
        }
        [KSPAction("Decrease thrust", actionGroup = KSPActionGroup.None)]
        public void ActionGroupDecreaseThrust(KSPActionParam param)
        {
            ContextMenuDecreaseThrust();
        }

        public override string GetInfo()
        {
            string info = "Adjustable thrust.\n  Range: " + minimumThrustPercent.ToString("0%") + " - " + maximumThrustPercent.ToString("0%") + "\n  Step: " + ((int)(percentAdjustmentStep * 100.0f)).ToString() + "%";
            return info;
        }

		public override void OnUpdate()
		{
			if (m_state != StartState.None && m_state != StartState.Editor)
			{
				if (canAdjustOverride == false)
				{
					engine = null;
					BindEngine();
					if (part.findFxGroup("running") != null && engine != null)
					{
						part.findFxGroup("running").SetPower(thrustPercent * vessel.ctrlState.mainThrottle);
					}
				}
			}
		}

		/// <summary>
		/// This function should be called in FixedUpdate() because it will forcibly change the throttle setting which will be reset every frame.
		/// </summary>
		/// <param name="percent">The throttle percentage.</param>
		public void SetPercentage(float percent)
		{
			if (canAdjustAtAnytime == false && canAdjustOverride == false) return;
			thrustPercent = percent;
			if (thrustPercent > maximumThrustPercent) thrustPercent = maximumThrustPercent;
			if (thrustPercent < minimumThrustPercent) thrustPercent = minimumThrustPercent;
			if (thrustPercent > 1.0f) thrustPercent = 1.0f;
			if (thrustPercent < 0.0f) thrustPercent = 0.0f;
			
			engine = null;
			BindEngine();
			if (engine != null)
			{
				if (part.findFxGroup("running") != null)
				{
					Debug.Log("Setting FXGroup to: " + thrustPercent.ToString());
					part.findFxGroup("running").SetPower(thrustPercent);
				}
				engine.currentThrottle = thrustPercent;
				engine.maxThrust = originalMaxThrust;
				engine.heatProduction = originalHeatProduction;
			}
		}
        public void IncreaseInitialThrust()
        {
            initialThrust += percentAdjustmentStep;
            if (initialThrust > maximumThrustPercent) initialThrust = maximumThrustPercent;
            if (initialThrust < minimumThrustPercent) initialThrust = minimumThrustPercent;
            if (initialThrust > 1.0f) initialThrust = 1.0f;
            if (initialThrust < 0.0f) initialThrust = 0.0f;

            engine = null;
            BindEngine();
            if (engine != null)
                engine.maxThrust = originalMaxThrust * initialThrust;
        }

        public void DecreaseInitialThrust()
        {
            initialThrust -= percentAdjustmentStep;
            if (initialThrust > maximumThrustPercent) initialThrust = maximumThrustPercent;
            if (initialThrust < minimumThrustPercent) initialThrust = minimumThrustPercent;
            if (initialThrust > 1.0f) initialThrust = 1.0f;
            if (initialThrust < 0.0f) initialThrust = 0.0f;

            engine = null;
            BindEngine();
            if (engine != null)
                engine.maxThrust = originalMaxThrust * initialThrust;
        }

    }
}

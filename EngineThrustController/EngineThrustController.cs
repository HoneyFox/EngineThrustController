using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace EngineThrustController
{
    public class ModuleEngineThrustController : PartModule
    {
        [KSPField(isPersistant = true)]
        public bool canAdjustAtAnytime = true;
        [KSPField]
        public float percentAdjustmentStep = 0.1f;
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

        ModuleEngines engine = null;

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
                EngineThrustControllerGUIItem item = new EngineThrustControllerGUIItem(EngineThrustControllerGUI.GetInstance(), this);
                return;
            }
            else
            {
                EngineThrustControllerGUI.GetInstance().ClearGUIItem();
            }
            
            Events["ContextMenuIncreaseThrust"].guiName = "Increase Thrust by " + percentAdjustmentStep.ToString("0%");
            Events["ContextMenuDecreaseThrust"].guiName = "Decrease Thrust by " + percentAdjustmentStep.ToString("0%");
            
            base.OnStart(state);
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
        [KSPAction("Increase Thrust", actionGroup = KSPActionGroup.None)]
        public void ActionGroupIncreaseThrust(KSPActionParam param)
        {
            ContextMenuIncreaseThrust();
        }
        [KSPAction("Decrease Thrust", actionGroup = KSPActionGroup.None)]
        public void ActionGroupDecreaseThrust(KSPActionParam param)
        {
            ContextMenuDecreaseThrust();
        }

        public override string GetInfo()
        {
            string info = "Adjustable thrust.\n  Range: " + minimumThrustPercent.ToString("0%") + " - " + maximumThrustPercent.ToString("0%") + "\n  Step: " + ((int)(percentAdjustmentStep * 100.0f)).ToString() + "%";
            return info;
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

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

        /// <summary>
        /// Thrust percentage display. Takes its value directly from the engine thrust limiter
        /// </summary>
        [KSPField(guiActive = true, guiName = "Thrust Percent", guiFormat = "0%")]
		float thrustPercent;

		[KSPField]
		public bool showItemInList = true;

        public ModuleEngines engine = null;
		public ModuleEnginesFX engineFX = null;

	
        /// <summary>
        /// Retrieves the engine module from the part this module is contained in
        /// </summary>
        private void BindEngine()
        {
            if (this.engine == null && this.engineFX == null) {
                foreach (PartModule mod in this.part.Modules) {
                    var module = mod as ModuleEngines;
                    if (module != null) {
                        engine = module;
                        break;
                    } else {
                        var moduleFX = mod as ModuleEnginesFX;
                        if (moduleFX != null) {
                            engineFX = moduleFX;
                            break;
                        }
                    }
                }
            }
        }

        public override void OnStart(StartState state)
        {
            Debug.Log("ModuleEngineThrustController OnStart(" + state.ToString() + ")");
            if (state == StartState.None)
                return;
            
            BindEngine();

            maximumThrustPercent = Mathf.Clamp01(maximumThrustPercent);
            minimumThrustPercent = Mathf.Clamp(minimumThrustPercent, 0, maximumThrustPercent);

			if (canAdjustAtAnytime)
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

        [KSPEvent(name = "ContextMenuIncreaseThrust", guiActive = true, guiName = "Increase Thrust", active = true, category = "Thrust Control")]
        public void ContextMenuIncreaseThrust()
        {
            if (!canAdjustAtAnytime)
                return;
            this.ActionGroupIncreaseThrust(null);
        }

        [KSPEvent(name = "ContextMenuDecreaseThrust", guiActive = true, guiName = "Decrease Thrust", active = true, category = "Thrust Control")]
        public void ContextMenuDecreaseThrust()
        {
            if (canAdjustAtAnytime)
                return;

            this.ActionGroupDecreaseThrust(null);
        }

		[KSPEvent(name = "Group1", guiActive = true, guiName = "Set Group 1", active = true, category = "Grouping")]
		public void Group1 ()
		{
			gp = 1;
		}
		[KSPEvent(name = "Group2", guiActive = true, guiName = "Set Group 2", active = true, category = "Grouping")]
		public void Group2 ()
		{
			gp = 2;
		}

        [KSPAction("Increase thrust limiter", actionGroup = KSPActionGroup.None)]
        public void ActionGroupIncreaseThrust(KSPActionParam param)
        {
            this.AdjustPercentage(this.percentAdjustmentStep);
        }
        [KSPAction("Decrease thrust limiter", actionGroup = KSPActionGroup.None)]
        public void ActionGroupDecreaseThrust(KSPActionParam param)
        {
            this.AdjustPercentage(-this.percentAdjustmentStep);
        }

        /// <summary>
        /// sync internal thrust value with actual value of the engine
        /// </summary>
        public override void OnUpdate() {
            this.thrustPercent = this.GetPercentage();
        }

        /// <summary>
        /// In-Editor Module info box content
        /// </summary>
        /// <returns>The info.</returns>
        public override string GetInfo()
        {
            string info = "Adjustable thrust.\n  Range: " + minimumThrustPercent.ToString("0%") + " - " + maximumThrustPercent.ToString("0%") + "\n  Step: " + ((int)(percentAdjustmentStep * 100.0f)).ToString() + "%";
            return info;
        }

        /// <summary>
        /// Adjusts the engine thrust limiter by the given amount
        /// </summary>
        /// <param name="step">Step.</param>
        public void AdjustPercentage(float step)
        {
            this.SetPercentage(this.GetPercentage() + step);
        }

        /// <summary>
        /// Retrieves the current thrust limiter of the underlying engine.
        /// </summary>
        /// <returns>The percentage.</returns>
        public float GetPercentage() {
            if (this.engine != null)
                return this.engine.thrustPercentage / 100f;
            if (this.engineFX != null)
                return this.engineFX.thrustPercentage / 100f;
            return -1;
        }

		/// <summary>
		/// Sets the underlying engine thrust limiter to the given value.
		/// </summary>
		/// <param name="percent">The throttle percentage.</param>
		public void SetPercentage(float percent)
		{
            if (canAdjustAtAnytime || canAdjustOverride) {
                thrustPercent = Mathf.Clamp01(Mathf.Clamp(percent, minimumThrustPercent, maximumThrustPercent));
			
			    BindEngine();

    			if (engine != null) {
                    engine.thrustPercentage = this.thrustPercent * 100f;
			    } else if (engineFX != null) {
                  engineFX.thrustPercentage = this.thrustPercent * 100f;
			    }
		    }
        }
    }
}

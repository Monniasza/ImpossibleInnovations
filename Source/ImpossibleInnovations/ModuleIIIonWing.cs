﻿/*
	This file is part of Impossible Innovations,
		(C) 2014-2018 : JandCandO https://spacedock.info/profile/jandcando
		(C) 2018-2019 Lisias T : http://lisias.net <support@lisias.net>

	Impossible Innovations is licensed as follows:

		* CC BY-NC-SA 4.0 : https://creativecommons.org/licenses/by-nc-sa/4.0/

	And you are allowed to choose the License that better suit your needs.

	Impossible Innovations is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the GNU General Public License 2.0
	along with Impossible Innovations. If not, see < https://creativecommons.org/>.
*/
using UnityEngine;

namespace ImpossibleInnovations
{
    public class ModuleIIIonWing : PartModule
    {
        #region KSPFields
        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Electric Consumption", isPersistant = false)]
        public double wingElectricConsumption;

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Lift Coefficient", isPersistant = false)]
        public float deflectionLiftCoeff;

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Active", isPersistant = true)]
        public bool wingActive = true;

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Status", isPersistant = false)]
        public string wingStatus = "Active";
        #endregion

        #region Functions
        public void wingOn()
        {
            wingActive = true;
            
            deflectionLiftCoeff = 15f;
            ((ModuleLiftingSurface)part.Modules["ModuleLiftingSurface"]).deflectionLiftCoeff = deflectionLiftCoeff;
        }

        public void wingOff()
        {
            wingActive = false;
            
            wingElectricConsumption = 0;

            deflectionLiftCoeff = 1.75f; //a bit less than a normal wing
            ((ModuleLiftingSurface)part.Modules["ModuleLiftingSurface"]).deflectionLiftCoeff = deflectionLiftCoeff;
        }

        [KSPEvent(active = true, guiActive = true, guiActiveEditor = false, guiName = "Toggle Wing")]
        public void wingToggle()
        {
            if (wingActive)
            {
                wingOff();
                wingStatus = "Inactive";
            }
            else
            {
                wingOn();
                wingStatus = "Active";
            }
        }
        #endregion

        #region Action Groups
        [KSPAction("Wing On")]
        public void actionWingOn(KSPActionParam param)
        {
            wingOn();
            wingStatus = "Active";
        }

        [KSPAction("Wing Off")]
        public void actionWingOff(KSPActionParam param)
        {
            wingOff();
            wingStatus = "Inactive";
        }

        [KSPAction("Toggle Wing")]
        public void actionWingToggle(KSPActionParam param)
        {
            wingToggle();
        }
        #endregion

        public void FixedUpdate()
        {
			if (!HighLogic.LoadedSceneIsFlight) return;

            if (part.Modules.Contains("ModuleLiftingSurface"))
            {               
                if (vessel.srfSpeed < 30 && wingActive) //if vessel speed is below 30 m/s, give full lift at no cost
                {
                    wingOn();
                    wingStatus = "Below min. drain velocity";

                    wingElectricConsumption = 0;
                }
                else if (II_Utilities.GetShipResourceAmount(vessel, "ElectricCharge") >= wingElectricConsumption && wingActive) //if vessel is on and has enough electricity, wing is on
                {
                    wingOn();
                    wingStatus = "Active";

                    wingElectricConsumption = (vessel.srfSpeed * vessel.atmDensity) / 15; //electricCharge drain dependent on airspeed and atmospheric density. if either is 0, no electricCharge is drained
                    part.RequestResource("ElectricCharge", TimeWarp.fixedDeltaTime * wingElectricConsumption);
                }
                else if (wingActive) //ELSE, if wing is active (implying there is no electricCharge), then shut wing off
                {
                    wingOff();
                    wingStatus = "Not enough electricCharge!";
                }
                else //if wingActive == false, then make sure it is off
                {
                    wingOff();
                }
            }
        }
    }
}

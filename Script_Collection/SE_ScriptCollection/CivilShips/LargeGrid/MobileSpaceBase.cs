#region pre-script
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using VRageMath;
using VRage.Game;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Ingame;
using Sandbox.Game.EntityComponents;
using VRage.Game.Components;
using VRage.Collections;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace MobileSpaceBase
{
    public class Program : MyGridProgram
    {
        #endregion
        //To put your code in a PB copy from this comment...

        #region CONSTANTS
        const string GRID_NAME = "None";
        string [] TAGS = { "<Gas>", "<Inventory>", "<Docking>" };
        #endregion

        List<IMyTerminalBlock> allBlocks = null;
        List<IMyTextPanel> G_screens = null;

        int errorFLAG = 0;
        string errorText = null;

        string output = null;

        public Program ()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;  //  UPdates itself every game tick.
        }

        public void Main ( string argument, UpdateType updateType )
        {
            if ( allBlocks == null || errorFLAG == 1 )
            {
                allBlocks = new List<IMyTerminalBlock> ();
                Fetch_All_Blocks ();
            }
            if ( G_screens == null || errorFLAG == 2 )
            {
                if ( errorFLAG != 1 )
                {
                    G_screens = new List<IMyTextPanel> ();
                    Fetch_Screens ();
                }

            }

            if ( errorFLAG == 0 )
            {
                errorText = "None";
            }
        }

        private void Fetch_All_Blocks ()
        {
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock> ();
            GridTerminalSystem.GetBlocks ( blocks );

            if ( blocks.Count != 0 )
            {
                for ( int i = 0; i < blocks.Count; i++ )
                {
                    if ( blocks [i].CustomName.Contains ( GRID_NAME ) )
                    {
                        allBlocks.Add ( blocks [i] );
                    }
                }

                if ( allBlocks.Count == 0 )
                {
                    errorFLAG = 1;
                    errorText = $"No blocks with ({GRID_NAME}) in their name";
                }
                else
                {
                    errorFLAG = 0;
                }
            }
            else
            {
                errorFLAG = 1;
                errorText = "No blocks on grid???...";
            }

        }

        private void Fetch_Screens ()
        {
            /*List<IMyTextPanel> allScreens = new List<IMyTextPanel> ();
            GridTerminalSystem.GetBlocksOfType ( allScreens );*/

            for ( int i = 0; i < allBlocks.Count; i++ )
            {
                if ( allBlocks [i].CustomName.Contains ( GRID_NAME ) )
                {
                    G_screens.Add ( allBlocks [i] as IMyTextPanel );
                }
            }

            if ( G_screens.Count == 0 )
            {
                errorText = $"No LCD's with ({GRID_NAME}) in their custom name";
                errorFLAG = 2;
            }
            else
            {
                errorFLAG = 0;
            }
        }

        private void Inventory_Status ()
        {
            VRage.MyFixedPoint currentVolume = 0;
            VRage.MyFixedPoint maxVolume = 0;

        }

        private class Item
        {
            public string Name { get; private set; }
            public VRage.MyFixedPoint Amount { get; private set; }

            public Item ( string _name, VRage.MyFixedPoint _amount )
            {
                Name = _name;
                Amount = _amount;
            }

            public void Set_Amount ( VRage.MyFixedPoint _amount )
            {
                Amount += _amount;
            }
        }
        //to this comment.
        #region post-script
    }

    class ResourceSearcher
    {
        private Dictionary<string, double> detectedAmounts = new Dictionary<string, double> ();

        public void LookFor ( string type )
        {
            if ( detectedAmounts.ContainsKey ( type ) )
                return;

            detectedAmounts [type] = 0;
        }

        private void InspectInventory ( IMyInventory inv )
        {
            var items = inv.GetItems ();
            for ( var i = 0; i < items.Count; ++i )
            {
                var item = items [i];

                // You can use Content.TypeId.ToString() to get "MyObjectBuilder_Ore" / "MyObjectBuilder_Ingot" and the like.
                // The SubtypeName will be the kind of ore/ingot/whatever.
                var id = item.Content.TypeId.ToString () + "." + item.Content.SubtypeName;

                if ( !detectedAmounts.ContainsKey ( id ) )
                    continue;

                detectedAmounts [id] += (double) item.Amount;
            }
        }

        public double GetAmount ( string type )
        {
            double d;
            if ( detectedAmounts.TryGetValue ( type, out d ) )
                return d;
            return 0;
        }
    }
}
#endregion
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
                for ( int i = 0; i < G_screens.Count; i++ )
                {
                    G_screens [i].WritePublicText ( Inventory_Status () );
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
                    if ( allBlocks [i].CustomName.Contains ( "LCD" ) )
                    {
                        G_screens.Add ( allBlocks [i] as IMyTextPanel );
                    }

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

        private string Inventory_Status ()
        {
            VRage.MyFixedPoint currentVolume = 0;
            VRage.MyFixedPoint maxVolume = 0;

            List<Item> allItems = new List<Item> ();

            for ( int i = 0; i < allBlocks.Count; i++ )
            {
                if ( allBlocks [i].HasInventory )
                {
                    for ( int j = 0; j < allBlocks [i].GetInventory ().GetItems ().Count; j++ )
                    {
                        var item = allBlocks [i].GetInventory ().GetItems () [j];

                        if ( allItems.Exists ( element => element.Type == item.Content.TypeId.ToString () && element.Name == item.Content.SubtypeName ) )
                        {
                            allItems.Find ( element => element.Type == item.Content.TypeId.ToString () && element.Name == item.Content.SubtypeName ).Set_Amount ( item.Amount );
                        }
                        else
                        {
                            allItems.Add ( new Item ( item.Content.TypeId.ToString (), item.Content.SubtypeName, item.Amount ) );
                        }
                    }
                }
            }

            string output = "";

            string [] format = { "<Ore>\n", "<Ingots>\n", "<Components>\n", "<Ammunition>\n", "<Tools>\n" };

            for ( int i = 0; i < allItems.Count; i++ )
            {
                if ( allItems [i].Type.Split ( '_' ) [1] == "Ore" )
                {
                    format [0] += $"        {allItems [i].ToString ()}\n";
                }
                else if ( allItems [i].Type.Split ( '_' ) [1] == "Ingot" )
                {
                    format [1] += $"        {allItems [i].ToString ()}\n";
                }
                else if ( allItems [i].Type.Split ( '_' ) [1] == "Component" )
                {
                    format [2] += $"        {allItems [i].ToString ()}\n";
                }
                else if ( allItems [i].Type.Split ( '_' ) [1] == "AmmoMagazine" )
                {
                    format [3] += $"        {allItems [i].ToString ()}\n";
                }
                else
                {
                    format [4] += $"        {allItems [i].ToString ()}\n";
                }
            }

            output = $"{format [0]}{format [1]}{format [2]}{format [3]}{format [4]}";

            return output;
        }

        private class Item
        {
            public string Type { get; private set; }
            public string Name { get; private set; }
            public VRage.MyFixedPoint Amount { get; private set; }

            public Item ( string _type, string _name, VRage.MyFixedPoint _amount )
            {
                Type = _type;
                Name = _name;
                Amount = _amount;
            }

            public void Set_Amount ( VRage.MyFixedPoint _amount )
            {
                Amount += _amount;
            }

            public override string ToString ()
            {
                decimal amount = (decimal) Amount;
                string type = Type.Split ( '_' ) [1];

                string output = $"{this.Name} {( ( type == "Ore" || type == "Ingot" ) ? ( type ) : ("") )}: {amount.ToString ( "#,##0.##" )}";

                return output;
            }
        }
        //to this comment.
        #region post-script
    }
}

#endregion
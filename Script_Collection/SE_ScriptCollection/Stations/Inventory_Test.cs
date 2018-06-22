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

namespace Inventory_Test
{
    public class Program : MyGridProgram
    {
        #endregion
        //To put your code in a PB copy from this comment...

        #region CONSTANTS
        const string GRID_NAME = "Station";
        #endregion

        List<IMyTerminalBlock> blocks = null;

        IMyTextPanel LCD = null;

        public Program ()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;  //  UPdates itself every game tick.
        }

        public void Save ()
        {

        }

        public void Main ( string argument, UpdateType updateType )
        {
            if ( LCD == null )
            {
                LCD = GridTerminalSystem.GetBlockWithName ( "LCD - Inventory" ) as IMyTextPanel;
            }
            if ( blocks == null )
            {
                blocks = new List<IMyTerminalBlock> ();
                GridTerminalSystem.GetBlocks ( blocks );
            }

            List<Item> items = new List<Item> ();
            if ( blocks != null && blocks.Count != 0 )
            {
                for ( int i = 0; i < blocks.Count; i++ )
                {
                    if ( blocks [i].HasInventory )
                    {
                        for ( int j = 0; j < blocks [i].GetInventory ().GetItems ().Count; j++ )
                        {
                           var item = blocks [i].GetInventory ().GetItems () [j];

                            if ( items.Exists ( element => element.Name ==  item.Content.SubtypeName ) )
                            {
                                items.Find ( element => element.Name == item.Content.SubtypeName ).Amount += item.Amount;
                            }
                            else
                            {
                                items.Add ( new Item ( item.Content.SubtypeName, item.Amount ) );
                            }

                        }
                    }
                }
            }

            string output = String.Empty;

            for ( int i = 0; i < items.Count; i++ )
            {
                output += $"{items[i].ToString()}\n";

            }

            LCD.WritePublicText ( output );

        }

        private class Item
        {
            public string Name { get; private set; }
            public VRage.MyFixedPoint Amount { get; set; }

            public Item ( string _name, VRage.MyFixedPoint _amount )
            {
                Name = _name;
                Amount = _amount;
            }

            public override string ToString ()
            {
                decimal amount = (decimal)Amount;

                string output = $"{this.Name}: {amount.ToString ( "#,##0.##" )}";
                return output;
            }
        }
        //to this comment.
        #region post-script
    }
}
#endregion

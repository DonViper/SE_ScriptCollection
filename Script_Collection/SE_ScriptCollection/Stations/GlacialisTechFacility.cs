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

namespace GlacialisTechFacility
{
    public class Program : MyGridProgram
    {
        #endregion
        //To put your code in a PB copy from this comment...

        #region CONSTANTS
        const string LCD_BLOCK_GROUP_NAME = "LCD's";
        const string SINGLE_LCD_NAME = "LCD";
        readonly string [] LCD_TAGS = { "<Power>", "<Docking>", "<Storage>", "<Inventory>" };
        #endregion
        bool G_useBlockGroup = true;

        List<IMyTextPanel> G_screens;
        IMyTextPanel LCD;

        public Program ()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;  //  UPdates itself every game tick.
        }

        public void Save ()
        {

        }

        public void Main ( string argument, UpdateType updateType )
        {

            G_useBlockGroup = Fecth_LCD_Block_Group ();

            if ( G_useBlockGroup == true )
            {
                for ( int i = 0; i < G_screens.Count; i++ )
                {
                    G_screens [i].WritePublicText ( "<-Terminal->\n", false );
                    G_screens [i].WritePublicText ( "<--------->", true );
                }
            }
            else //  If there is no block group
            {
                LCD = GridTerminalSystem.GetBlockWithName ( SINGLE_LCD_NAME ) as IMyTextPanel;
                if ( LCD != null )
                {
                    Echo ( $"Single LCD mode" );
                    LCD.ShowPublicTextOnScreen ();
                    LCD.WritePublicText ( "<-Terminal->\n", false );
                    LCD.WritePublicText ( "<--------->", true );
                }
                else
                {
                    Echo ( $"No LCD with name ({SINGLE_LCD_NAME}) found. No screens available." );
                }

            }

        }

        /// <summary>
        /// Fetch LCD block group if there is one
        /// </summary>
        /// <returns></returns>
        private bool Fecth_LCD_Block_Group ()
        {

            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock> ();
            if ( GridTerminalSystem.GetBlockGroupWithName ( LCD_BLOCK_GROUP_NAME ) != null )
            {
                GridTerminalSystem.GetBlockGroupWithName ( LCD_BLOCK_GROUP_NAME ).GetBlocks ( blocks );
            }

            if ( blocks.Count > 0 )
            {
                G_screens = new List<IMyTextPanel> ();

                for ( int element = 0; element < blocks.Count; element++ )
                {
                    IMyTextPanel LCD = blocks [element] as IMyTextPanel;
                    LCD.ShowPublicTextOnScreen ();
                    if ( LCD.ShowText != true )
                    {
                        Echo ( $"LCD with name ({LCD.CustomName}) could not be set to show public text on screen!" );
                    }
                    LCD.FontColor = new Color ( 0, 255, 0 );
                    G_screens.Add ( LCD );
                }

                Echo ( $"Using LCD group." );
                return true;
            }

            Echo ( $"LCD group ({LCD_BLOCK_GROUP_NAME}) missing." );

            return false;
        }
        //to this comment.
        #region post-script
    }
}
#endregion
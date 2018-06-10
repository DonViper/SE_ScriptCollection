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
        const string LCD_BLOCK_GROUP_NAME = "LCD's (Glacialis Tech Facility)";
        const string SINGLE_LCD_NAME = "LCD (Glacialis Tech Facilty)";
        readonly string [] LCD_TAGS = { "<Power>", "<Docking>", "<Storage>", "<Inventory>" };
        #endregion

        int G_isRunning = 0;
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
            Echo ( Run_Indicator () );  //  Visual representation of the scrip running

            Fecth_LCD_Block_Group ();

            if ( G_useBlockGroup == true )
            {
                Docking ();
            }
            else if ( G_useBlockGroup == false )  //  If there is no block group
            {
                LCD = GridTerminalSystem.GetBlockWithName ( SINGLE_LCD_NAME ) as IMyTextPanel;
                if ( LCD != null )
                {
                    LCD.WritePublicText ( "<-Terminal->", false );
                    Docking ();
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
        private void Fecth_LCD_Block_Group ()
        {
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock> ();
            GridTerminalSystem.GetBlockGroupWithName ( LCD_BLOCK_GROUP_NAME ).GetBlocks ( blocks );

            if ( blocks != null )
            {
                G_screens = new List<IMyTextPanel> ();

                for ( int element = 0; element < blocks.Count; element++ )
                {
                    IMyTextPanel LCD = blocks [element] as IMyTextPanel;
                    LCD.SetShowOnScreen ( VRage.Game.GUI.TextPanel.ShowTextOnScreenFlag.PUBLIC );
                    if ( LCD.ShowOnScreen != VRage.Game.GUI.TextPanel.ShowTextOnScreenFlag.PUBLIC )
                    {
                        Echo ( $"LCD with name ({LCD.CustomName}) could not be set to show public text on screen!" );
                        G_useBlockGroup = false;
                    }
                    LCD.FontColor = new Color ( 0, 255, 0 );
                    G_screens.Add ( LCD );
                }
            }
            else
            {
                Echo ( $"No LCD block group with the name ({LCD_BLOCK_GROUP_NAME}) found. Switching to single LCD use." );
            }
        }

        /// <summary>
        /// Info on docked ships
        /// </summary>
        private void Docking ()
        {
            if ( G_useBlockGroup == true )  //  If there is a block group
            {
                for ( int i = 0; i < G_screens.Count; i++ )
                {
                    if ( G_screens [i].CustomName.Contains ( LCD_TAGS [1] ) )   //  If there is a docking tag
                    {
                        G_screens [i].WritePublicText ( "<----Docking---->", false );
                        LCD.WritePublicText ( "No advanced info available", true );
                        G_screens [i].WritePublicText ( "<--------------->", true );
                    }
                    else    //  If there is no docking tag
                    {
                        Show_Simple_Docking_Info ();
                    }
                }
            }
            else if ( LCD != null ) //  If there is no bloc group
            {
                Show_Simple_Docking_Info ();
            }

        }

        /// <summary>
        /// Simple version of docking info
        /// </summary>
        private void Show_Simple_Docking_Info ()
        {
            LCD.WritePublicText ( "<----Docking---->", true );
            LCD.WritePublicText ( "No info available", true );
            LCD.WritePublicText ( "<--------------->", true );
        }

        /// <summary>
        /// Animation for indication of the script running
        /// </summary>
        /// <returns></returns>
        private string Run_Indicator ()
        {
            string runText = "";
            if ( G_isRunning == 0 )
            {
                runText = "    Running";
            }
            else if ( G_isRunning == 1 )
            {
                runText = "   =Running=";
            }
            else if ( G_isRunning == 2 )
            {
                runText = "  ==Running==";
            }
            else if ( G_isRunning == 3 )
            {
                runText = " ===Running===";
            }
            else if ( G_isRunning == 4 )
            {
                runText = "|===Running===|";
            }
            else
            {
                return null;
            }

            return runText;
        }
        //to this comment.
        #region post-script
    }
}
#endregion
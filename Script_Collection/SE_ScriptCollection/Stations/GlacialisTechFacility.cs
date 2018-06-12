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
        const string BASE_NAME = "|";
        const string LCD_BLOCK_GROUP_NAME = "LCD's";
        const string SINGLE_LCD_NAME = "LCD";
        readonly string [] LCD_TAGS = { "<Power>", "<Docking>", "<Storage>", "<Inventory>" };
        #endregion
        bool G_useBlockGroup = true;

        List<IMyTextPanel> G_screens;
        IMyTextPanel G_LCD;

        int G_isRunning = 0;
        string runAnimation;
        string debugText;
        bool errorFLAG = false;

        List<IMyShipConnector> connectors;

        public Program ()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;  //  UPdates itself every game tick.
        }

        public void Save ()
        {

        }

        public void Main ( string argument, UpdateType updateType )
        {
            #region Running Indicator
            Run_Indicator ();
            Echo ( runAnimation );
            G_isRunning++;
            #endregion

            G_useBlockGroup = Fecth_LCD_Block_Group ();

            //  If there is no block group
            if ( G_useBlockGroup == false )
            {
                G_LCD = GridTerminalSystem.GetBlockWithName ( SINGLE_LCD_NAME ) as IMyTextPanel;
                if ( G_LCD != null )
                {
                    G_LCD.ShowPublicTextOnScreen ();
                }
                else
                {
                    debugText = ( $"No LCD with name ({SINGLE_LCD_NAME}) found.\n" );
                }

            }
            else if ( G_useBlockGroup == true && errorFLAG == false )
            {
                debugText = "None";
            }

            Docking_Status ();

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
                        debugText = ( $"LCD with name ({LCD.CustomName}) could not be set to show public text on screen!\n" );
                    }
                    LCD.FontColor = new Color ( 0, 255, 0 );
                    G_screens.Add ( LCD );
                }
                G_LCD = null;
                return true;
            }

            debugText = ( $"(Optional) LCD group ({LCD_BLOCK_GROUP_NAME}) missing.\n" );

            return false;
        }

        private void Fecth_Connectors ()
        {
            List<IMyShipConnector> tempList = new List<IMyShipConnector> ();
            GridTerminalSystem.GetBlocksOfType ( tempList );

            //string names = "Connecters:\n";
            //string name = "";

            if ( tempList.Count > 0 )
            {
                connectors = new List<IMyShipConnector> ();
                //names = "";
                for ( int i = 0; i < tempList.Count; i++ )
                {
                    if ( tempList [i].CustomName.Contains ( BASE_NAME ) )
                    {
                        connectors.Add ( tempList [i] );
                        //name = tempList [i].CustomName;
                        //names += $"{name}\n";

                    }
                }
                if ( connectors.Count <= 0 )
                {
                    debugText = $"No connectors with ({BASE_NAME}) in their name";
                    errorFLAG = true;
                }
                else
                {
                    errorFLAG = false;
                }

                tempList = null;
            }
            else
            {
                debugText = "No connectors found!";
                errorFLAG = true;

            }

            //LCD.WritePublicText ( names, false );
        }

        private void Docking_Status ()
        {
            Fecth_Connectors ();

            if ( G_useBlockGroup == true )
            {
                for ( int j = 0; j < G_screens.Count; j++ )
                {
                    G_screens [j].WritePublicText ( "<---Ship Status--->\n", false );
                }

                for ( int i = 0; i < connectors.Count; i++ )
                {
                    if ( connectors [i].Status == MyShipConnectorStatus.Connected )
                    {
                        if ( G_useBlockGroup == true )
                        {
                            for ( int j = 0; j < G_screens.Count; j++ )
                            {
                                G_screens [j].WritePublicText ( $"{connectors [i].CustomName}\n<>\n{connectors [i].OtherConnector.CubeGrid.CustomName}\n\n", true );
                            }
                        }
                    }
                    else if ( connectors [i].Status == MyShipConnectorStatus.Connectable && G_useBlockGroup == true )
                    {
                        for ( int j = 0; j < G_screens.Count; j++ )
                        {
                            G_screens [j].WritePublicText ( $"{connectors [i].CustomName}\n</>\n{connectors [i].OtherConnector.CubeGrid.CustomName}\n\n", true );
                        }
                    }
                    else
                    {
                        if ( G_useBlockGroup == true )
                        {
                            for ( int j = 0; j < G_screens.Count; j++ )
                            {
                                G_screens [j].WritePublicText ( $"{connectors [i].CustomName}\n()\n", true );
                            }
                        }

                    }
                }
            }
            else
            {
                if ( G_LCD != null )
                {
                    G_LCD.WritePublicText ( "<---Ship Status--->\n", false );
                    int lockedCount = 0;

                    for ( int i = 0; i < connectors.Count; i++ )
                    {
                        if ( connectors [i].Status == MyShipConnectorStatus.Connected )
                        {
                            lockedCount++;
                        }
                    }
                    G_LCD.WritePublicText ( $"Connectors: {connectors.Count}\n", true );
                    G_LCD.WritePublicText ( $"Locked: {lockedCount}/{connectors.Count}\n", true );
                }
            }
        }

        /// <summary>
        /// Indication of the scrip running
        /// </summary>
        private void Run_Indicator ()
        {
            if ( G_isRunning == 8 )
            {
                runAnimation = ( $"|======-||-======|\nMode: {( G_useBlockGroup == true ? ( "Group" ) : ( "Single" ) )}\nErrors: {debugText}" );
            }
            if ( G_isRunning == 16 )
            {
                runAnimation = ( $"|=====-=||=-=====|\nMode: {( G_useBlockGroup == true ? ( "Group" ) : ( "Single" ) )}\nErrors: {debugText}" );
            }
            if ( G_isRunning == 24 )
            {
                runAnimation = ( $"|====-==||==-====|\nMode: {( G_useBlockGroup == true ? ( "Group" ) : ( "Single" ) )}\nErrors: {debugText}" );
            }
            if ( G_isRunning == 32 )
            {
                runAnimation = ( $"|==-====||===-===|\nMode: {( G_useBlockGroup == true ? ( "Group" ) : ( "Single" ) )}\nErrors: {debugText}" );
            }
            if ( G_isRunning == 40 )
            {
                runAnimation = ( $"|==-====||====-==|\nMode: {( G_useBlockGroup == true ? ( "Group" ) : ( "Single" ) )}\nErrors: {debugText}" );
            }
            if ( G_isRunning == 48 )
            {
                runAnimation = ( $"|=-=====||=====-=|\nMode: {( G_useBlockGroup == true ? ( "Group" ) : ( "Single" ) )}\nErrors: {debugText}" );
            }
            if ( G_isRunning == 56 )
            {
                runAnimation = ( $"|-======||======-|\nMode: {( G_useBlockGroup == true ? ( "Group" ) : ( "Single" ) )}\nErrors: {debugText}" );
            }
            if ( G_isRunning == 64 )
            {
                runAnimation = ( $"|=-=====||=====-=|\nMode: {( G_useBlockGroup == true ? ( "Group" ) : ( "Single" ) )}\nErrors: {debugText}" );
            }
            if ( G_isRunning == 72 )
            {
                runAnimation = ( $"|==-====||====-==|\nMode: {( G_useBlockGroup == true ? ( "Group" ) : ( "Single" ) )}\nErrors: {debugText}" );
            }
            if ( G_isRunning == 80 )
            {
                runAnimation = ( $"|===-===||===-===|\nMode: {( G_useBlockGroup == true ? ( "Group" ) : ( "Single" ) )}\nErrors: {debugText}" );
            }
            if ( G_isRunning == 88 )
            {
                runAnimation = ( $"|====-==||==-====|\nMode: {( G_useBlockGroup == true ? ( "Group" ) : ( "Single" ) )}\nErrors: {debugText}" );
            }
            if ( G_isRunning == 96 )
            {
                G_isRunning = 0;
                runAnimation = ( $"|=====-=||=-=====|\nMode: {( G_useBlockGroup == true ? ( "Group" ) : ( "Single" ) )}\nErrors: {debugText}" );
            }
        }
        //to this comment.
        #region post-script
    }
}
#endregion
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
        const string MASTER_LCD = "Master";
        readonly string [] LCD_TAGS = { "<Gas>", "<Docking>", "<Damage>" };
        #endregion
        bool G_useBlockGroup = true;

        List<IMyTextPanel> G_screens;
        IMyTextPanel G_LCD;
        IMyTextPanel G_masterLCD;

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
                //  If there is an LCD
                if ( G_LCD != null )
                {
                    G_LCD.ShowPublicTextOnScreen ();
                    G_LCD.WritePublicText ( "<-------------------Status------------------->\n", false );
                    G_LCD.FontSize = 1.195f;
                }
                else
                {
                    debugText = ( $"No LCD with name ({SINGLE_LCD_NAME}) found.\n" );
                }

            }
            if ( G_useBlockGroup == true )
            {
                for ( int i = 0; i < G_screens.Count; i++ )
                {
                    for ( int j = 0; j < LCD_TAGS.Length; j++ )
                    {
                        if ( !G_screens [i].CustomName.Contains ( LCD_TAGS [j] ) )
                        {
                            G_screens [i].WritePublicText ( "<-------------------Status------------------->\n", false );
                            G_screens [i].FontSize = 1.195f;
                        }
                    }

                }
            }
            //  If there are no errors
            if ( G_useBlockGroup == true && errorFLAG == false )
            {
                debugText = "None";
            }

            if ( G_LCD != null || G_useBlockGroup == true )
            {
                if ( argument != null )
                {
                    int key;
                    if ( int.TryParse ( argument, out key ) )
                    {
                        switch ( key )
                        {
                            case 0:
                                G_masterLCD.CustomName = $"{MASTER_LCD}";
                                G_masterLCD.CustomName = $"{G_masterLCD.CustomName} {LCD_TAGS [0]}";
                                break;
                            case 1:
                                G_masterLCD.CustomName = $"{MASTER_LCD}";
                                G_masterLCD.CustomName = $"{G_masterLCD.CustomName} {LCD_TAGS [1]}";
                                break;

                            case 9:
                                G_masterLCD.CustomName = $"{MASTER_LCD}";
                                break;

                            default:
                                G_masterLCD.CustomName = $"{MASTER_LCD}";
                                break;
                        }
                    }
                }
                else
                {
                    G_masterLCD.CustomName = $"{MASTER_LCD}";
                }

                Docking_Status ();

                Tank_Status ();
            }
        }

        /// <summary>
        /// Fetch LCD block group and return true if there is one.
        /// </summary>
        /// <returns></returns>
        private bool Fecth_LCD_Block_Group ()
        {

            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock> ();
            //  Checking for block group
            if ( GridTerminalSystem.GetBlockGroupWithName ( LCD_BLOCK_GROUP_NAME ) != null )
            {
                GridTerminalSystem.GetBlockGroupWithName ( LCD_BLOCK_GROUP_NAME ).GetBlocks ( blocks );
            }

            //  If there is a block group and it's not empty
            if ( blocks.Count > 0 )
            {
                G_screens = new List<IMyTextPanel> ();

                //  Looping trough block group
                for ( int element = 0; element < blocks.Count; element++ )
                {
                    IMyTextPanel LCD = blocks [element] as IMyTextPanel;
                    LCD.ShowPublicTextOnScreen ();  //  Setting screens to display text

                    //  If there is a master screen
                    if ( LCD.CustomName.Contains ( MASTER_LCD ) && G_masterLCD == null )
                    {
                        LCD.CustomName = MASTER_LCD;    //  In case the master screen has a tag reset to default
                        G_masterLCD = LCD;
                    }

                    //  If the screen still does not display text
                    if ( LCD.ShowText != true )
                    {
                        debugText = ( $"LCD with name ({LCD.CustomName}) could not be set to show public text on screen!\n" );
                    }

                    LCD.FontColor = new Color ( 0, 255, 0 );    //  CHange font color of the screen to green
                    G_screens.Add ( LCD );  //  Add the screen to the G_Screens list
                }
                G_LCD = null;   //  If there is a block group set single LCD to null
                return true;
            }

            debugText = ( $"(Optional) LCD group ({LCD_BLOCK_GROUP_NAME}) missing.\n" );

            return false;
        }

        /// <summary>
        /// Fetch connectors on the grid
        /// </summary>
        private void Fecth_Connectors ()
        {
            List<IMyShipConnector> tempList = new List<IMyShipConnector> ();
            GridTerminalSystem.GetBlocksOfType ( tempList );

            //  If there is any connectors on the base
            if ( tempList.Count > 0 )
            {
                connectors = new List<IMyShipConnector> ();

                //  Loop trough temporary list with connectors
                for ( int i = 0; i < tempList.Count; i++ )
                {
                    //  If the connectors have the grid name in their name add it tot the connectors list.
                    if ( tempList [i].CustomName.Contains ( BASE_NAME ) )
                    {
                        connectors.Add ( tempList [i] );
                    }
                }
                //  If no connectors with the grid name in their name was found.
                if ( connectors.Count <= 0 )
                {
                    debugText = $"No connectors with ({BASE_NAME}) in their name";
                    errorFLAG = true;
                }
                else
                {
                    errorFLAG = false;
                }

                tempList = null;    //  Set the temp list to null.
            }
            //  If there is no connectors on the grid
            else
            {
                debugText = "No connectors found!";
                connectors = null;
                errorFLAG = true;

            }

            //LCD.WritePublicText ( names, false );
        }

        /// <summary>
        /// Status for docking on the grid
        /// </summary>
        private void Docking_Status ()
        {
            Fecth_Connectors ();

            //  When using block group
            if ( G_useBlockGroup == true && connectors != null && connectors.Count > 0 )
            {
                Group_Docking ();
            }
            else    //  Using single lcd
            {
                Single_Docking ();
            }
        }

        /// <summary>
        /// Using LCD blockgroup
        /// </summary>
        private void Group_Docking ()
        {
            for ( int j = 0; j < G_screens.Count; j++ )
            {
                if ( !G_screens [j].CustomName.Contains ( LCD_TAGS [0] ) && !G_screens [j].CustomName.Contains ( LCD_TAGS [1] ) )
                {
                    G_screens [j].WritePublicText ( $"<---Ship Status--->\nLocked: {Get_Amount_Connected ()}/{connectors.Count}\n", true );
                }
                else if ( G_screens [j].CustomName.Contains ( LCD_TAGS [1] ) )
                {
                    G_screens [j].WritePublicText ( "<-----------------Ship Status----------------->\n", false );
                    G_screens [j].FontSize = 1.120f;
                }

            }

            // Looping trough connectors
            for ( int i = 0; i < connectors.Count; i++ )
            {
                //  If connectors are connected
                if ( connectors [i].Status == MyShipConnectorStatus.Connected )
                {
                    //  Looping though screens and displaying info
                    for ( int j = 0; j < G_screens.Count; j++ )
                    {
                        //  If a screen has a tag show advanced version
                        if ( G_screens [j].CustomName.Contains ( LCD_TAGS [1] ) && G_screens [j].CustomName.Contains ( LCD_TAGS [0] ) == false )
                        {
                            G_screens [j].WritePublicText ( $"{( ( connectors [i].CustomName.Length >= 16 ) ? ( connectors [i].CustomName.Substring ( 0, 16 ) ) : ( connectors [i].CustomName ) )} |<>| {( ( connectors [i].OtherConnector.CubeGrid.CustomName.Length >= 16 ) ? ( connectors [i].OtherConnector.CubeGrid.CustomName.Substring ( 0, 16 ) ) : ( connectors [i].OtherConnector.CubeGrid.CustomName ) ) }\n", true );
                        }
                    }
                }
                //  If connecters are connectable
                else if ( connectors [i].Status == MyShipConnectorStatus.Connectable && G_useBlockGroup == true )
                {
                    //  Looping though screens and displaying info
                    for ( int j = 0; j < G_screens.Count; j++ )
                    {
                        //  If a screen has a tag
                        if ( G_screens [j].CustomName.Contains ( LCD_TAGS [1] ) && G_screens [j].CustomName.Contains ( LCD_TAGS [0] ) == false )
                        {
                            G_screens [j].WritePublicText ( $"{( ( connectors [i].CustomName.Length >= 16 ) ? ( connectors [i].CustomName.Substring ( 0, 16 ) ) : ( connectors [i].CustomName ) )} |</>| {( ( connectors [i].OtherConnector.CubeGrid.CustomName.Length >= 16 ) ? ( connectors [i].OtherConnector.CubeGrid.CustomName.Substring ( 0, 16 ) ) : ( connectors [i].OtherConnector.CubeGrid.CustomName ) ) }\n", true );
                        }
                    }
                }
                //  If connectors are not connected
                else
                {
                    //  Looping though screens and displaying info
                    for ( int j = 0; j < G_screens.Count; j++ )
                    {
                        //  If a screen has a tag
                        if ( G_screens [j].CustomName.Contains ( LCD_TAGS [1] ) && G_screens [j].CustomName.Contains ( LCD_TAGS [0] ) == false )
                        {
                            G_screens [j].WritePublicText ( $"{( ( connectors [i].CustomName.Length >= 16 ) ? ( connectors [i].CustomName.Substring ( 0, 16 ) ) : ( connectors [i].CustomName ) )} |<|\n", true );
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Get the amount of connecters currently connected
        /// </summary>
        /// <returns></returns>
        private int Get_Amount_Connected ()
        {
            int lockedCount = 0;

            //  Looping trough all connectors
            for ( int i = 0; i < connectors.Count; i++ )
            {
                //  If connectors is connected count them up
                if ( connectors [i].Status == MyShipConnectorStatus.Connected )
                {
                    lockedCount++;
                }
            }

            return lockedCount;
        }

        /// <summary>
        /// Using single LCD mode
        /// </summary>
        private void Single_Docking ()
        {
            if ( G_LCD != null )
            {
                G_LCD.WritePublicText ( $"<---Ship Status--->\nLocked: {Get_Amount_Connected ()}/{connectors.Count}\n", true );
            }
        }

        /// <summary>
        /// Status for Oxygen and hydrogen on the grid
        /// </summary>
        private void Tank_Status ()
        {
            List<IMyGasTank> allTanks = new List<IMyGasTank> ();
            GridTerminalSystem.GetBlocksOfType ( allTanks );

            List<IMyGasTank> hydro = new List<IMyGasTank> ();
            List<IMyGasTank> oxy = new List<IMyGasTank> ();

            double hydroLevel = 0;
            double oxyLevel = 0;

            //  If there is tanks on the grid
            if ( allTanks.Count > 0 )
            {
                for ( int i = 0; i < allTanks.Count; i++ )
                {
                    //  If it's a hydrogen tank
                    if ( allTanks [i].CustomName.Contains ( "Hydrogen" ) && allTanks [i].CustomName.Contains ( BASE_NAME ) )
                    {
                        hydroLevel += allTanks [i].FilledRatio;
                        hydro.Add ( allTanks [i] );
                    }
                    //  If it's a oxygen tank
                    else if ( allTanks [i].CustomName.Contains ( "Oxygen" ) && allTanks [i].CustomName.Contains ( BASE_NAME ) )
                    {
                        oxyLevel += allTanks [i].FilledRatio;
                        oxy.Add ( allTanks [i] );
                    }
                }

                //  If there's no hydrogen tanks on the grid
                if ( hydro.Count <= 0 )
                {
                    debugText = $"No hydrogen tanks with ({BASE_NAME}) in their name!";
                }
                //  If there's no oxygen tanks on the grid
                else if ( oxy.Count <= 0 )
                {
                    debugText = $"No oxygen tanks with ({BASE_NAME}) in their name!";
                }

                hydroLevel = ( hydroLevel / hydro.Count ) * 100;
                oxyLevel = ( oxyLevel / oxy.Count ) * 100;


                if ( G_useBlockGroup == true )
                {
                    for ( int i = 0; i < G_screens.Count; i++ )
                    {
                        //  If the screens name contains the <Gas> tag
                        if ( G_screens [i].CustomName.Contains ( LCD_TAGS [0] ) )
                        {
                            G_screens [i].FontSize = 1.300f;
                            G_screens [i].WritePublicText ( $"<--Oxygen and Hydrogen status-->\nHydro tanks: {hydro.Count} |<->| Oxy Tanks: {oxy.Count}\nHydrogen: {hydroLevel.ToString ( "F" )}%\nOxygen: {oxyLevel.ToString ( "F" )}%\n", false );
                        }
                        //  If the screens name does not contain any tags
                        else if ( !G_screens [i].CustomName.Contains ( LCD_TAGS [1] ) )
                        {
                            G_screens [i].WritePublicText ( $"<--Gas status-->\nHydro: {hydroLevel.ToString("F")}% |<->| Oxy: {oxyLevel.ToString("F")}%\n", true );
                        }
                    }
                }
                //  If single mode is active
                else
                {
                    G_LCD.WritePublicText ( $"<--Gas status-->\nHydro: {hydroLevel.ToString ( "F" )}% |<->| Oxy: {oxyLevel.ToString ( "F" )}%\n", true );
                }
            }
            //  If there is no tanks on the grid
            else
            {
                errorFLAG = true;
                debugText = "No tanks available!";
            }
        }

        private void Damage_Status ()
        {

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
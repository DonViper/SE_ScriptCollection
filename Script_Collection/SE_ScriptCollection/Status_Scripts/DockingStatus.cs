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

namespace DockingStatus
{
    public class Program : MyGridProgram
    {
        #endregion
        //To put your code in a PB copy from this comment...

        #region Comment
        /*---------------------------------------------------------------------------------------HOW TO USE---------------------------------------------------------------------------*/
        /*
                <Setting script>
                    Step 1: Change GRID_NAME under CONSTANS to whatever your grid is called.
                            Example: const string GRID_NAME = "MyStation";.

                    step 2: Change LCD_TAG under CONSTANTS to whatever tag you wanna use. Default is "<Docking>"
                            Example: const string LCD_TAG = "MyTag";.
                    
                    Step 3: Set SCREEN_COLOR under CONSTANTS to whatever color you want the screen to be.
                            Example: Color SCREEN_COLOR = new Color (100, 244, 59);.

                <Setting up LCD's>
                    Step 1: Name your LCD's whatever you want but include your GRID_NAME and LCD_TAG in the name.
                            Example: LCD <Docking> (MyStation)
                    
                    Note: Whenever you add a new LCD to the collection of LCD's used by the script, while the script is running, 
                              you will have to recompile the script.

                <Setting up Connectors>
                    Step 1: Name your Connectors whatever you want but include your GRID_NAME in the name.
                            Example: COnnector - Docking Area (MyStation).

                <Error Handling>
                    The script will indicate if it's running by showing an animation in the PB's message area.
                    If it not animating the script is not running.
                    If this is the case try and do every step again. Something might have gone wrong in the process.

                    Otherwise look at the error message in the PB's message are. It will tell you exactly what the problem is.
                    In the case the error message does not update to "None" even though every mistake has been corrected -> Recompile the script.
        */
        /*------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        #endregion

        #region CONSTANTS
        const string GRID_NAME = "None";
        const string LCD_TAG = "<Docking>";
        Color SCREEN_COLOR = new Color ( 0, 255, 0 );
        #endregion

        /*------------------------------------------------------------------------------------------SCRIPT--------------------------------------------------------------------------------*/
        RunIndicator AnimationRunner = new RunIndicator ();

        List<IMyTextPanel> G_screens;

        public Program ()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;  //  UPdates itself every game tick.
        }

        public void Save ()
        {

        }

        public void Main ( string argument, UpdateType updateType )
        {
            Echo ( AnimationRunner.Run_Animation () );

            if ( G_screens == null || AnimationRunner.ErrorFLAG == 1 )
            {
                Fecth_Screen ();
            }

            if ( AnimationRunner.ErrorFLAG == 0 || AnimationRunner.ErrorFLAG == 2 )
            {
                Write_To_ALl_Screens ();
            }

            if ( AnimationRunner.ErrorFLAG == 0 )
            {
                AnimationRunner.ErrorText = "None";
            }
        }

        /// <summary>
        /// Fecth all screens with GRID_NAME in their name
        /// </summary>
        private void Fecth_Screen ()
        {
            List<IMyTextPanel> allScreen = new List<IMyTextPanel> ();
            GridTerminalSystem.GetBlocksOfType ( allScreen );

            G_screens = new List<IMyTextPanel> ();

            if ( allScreen.Count != 0 )
            {
                for ( int i = 0; i < allScreen.Count; i++ )
                {
                    if ( allScreen [i].CustomName.Contains ( GRID_NAME ) )
                    {
                        if ( allScreen [i].CustomName.Contains ( LCD_TAG ) )
                        {
                            allScreen [i].ShowPublicTextOnScreen ();
                            allScreen [i].FontSize = 1.120f;
                            allScreen [i].FontColor = SCREEN_COLOR;
                            G_screens.Add ( allScreen [i] );
                        }
                    }
                }

                if ( G_screens.Count <= 0 )
                {
                    AnimationRunner.ErrorFLAG = 1;
                    AnimationRunner.ErrorText = $"No screens with ({LCD_TAG}) in their name";
                }
                else
                {
                    AnimationRunner.ErrorFLAG = 0;
                }
            }
            else
            {
                AnimationRunner.ErrorFLAG = 1;
                AnimationRunner.ErrorText = $"No screens on ({GRID_NAME})";
            }

        }

        /// <summary>
        /// Write output to each screen in the list
        /// </summary>
        private void Write_To_ALl_Screens ()
        {
            for ( int i = 0; i < G_screens.Count; i++ )
            {
                G_screens [i].WritePublicText ( Docking_Status () );
            }
        }

        /// <summary>
        /// Using LCD blockgroup
        /// </summary>
        private string Docking_Status ()
        {
            List<IMyShipConnector> allConnectors = new List<IMyShipConnector> ();
            GridTerminalSystem.GetBlocksOfType ( allConnectors );

            List<IMyShipConnector> connectors = new List<IMyShipConnector> ();

            string output = "<-------------Connector Status-------------->\n";

            if ( allConnectors.Count == 0 || AnimationRunner.ErrorFLAG == 2 )
            {
                GridTerminalSystem.GetBlocksOfType ( allConnectors );
            }

            if ( allConnectors.Count != 0 )
            {
                for ( int i = 0; i < allConnectors.Count; i++ )
                {
                    if ( allConnectors [i].CustomName.Contains ( GRID_NAME ) )
                    {
                        connectors.Add ( allConnectors [i] );
                    }
                }

                if ( connectors.Count == 0 )
                {
                    AnimationRunner.ErrorFLAG = 2;
                    AnimationRunner.ErrorText = $"No connectors with ({GRID_NAME}) in their name";
                }
                else
                {
                    AnimationRunner.ErrorFLAG = 0;

                    for ( int i = 0; i < connectors.Count; i++ )
                    {
                        string status = $"{( ( connectors [i].Status == MyShipConnectorStatus.Connected ) ? ( "|<>|" ) : ( ( connectors [i].Status == MyShipConnectorStatus.Connectable ) ? ( "|</>|" ) : ( "|<|" ) ) )}";
                        string otherConnector = $"{( ( connectors [i].Status == MyShipConnectorStatus.Connected || connectors [i].Status == MyShipConnectorStatus.Connectable ) ? ( ( ( connectors [i].OtherConnector.CubeGrid.CustomName.Length >= 16 ) ? ( connectors [i].OtherConnector.CubeGrid.CustomName.Substring ( 0, 16 ) ) : ( connectors [i].OtherConnector.CubeGrid.CustomName ) ) ) : ( "None" ) )}";
                        output += $"{( ( connectors [i].CustomName.Length >= 16 ) ? ( connectors [i].CustomName.Substring ( 0, 16 ) ) : ( connectors [i].CustomName ) )} {status} {otherConnector}\n";

                    }

                    return output;
                }


                return $"{output} No Data...";

            }
            else
            {
                AnimationRunner.ErrorFLAG = 2;
                AnimationRunner.ErrorText = $"No Connectors on ({GRID_NAME})";
            }

            return "Error...";

        }

        /// <summary>
        /// Provides an animation on wether or not the program is running
        /// </summary>
        class RunIndicator
        {
            private int runCount = 0;
            private string animation = string.Empty;
            public string ErrorText { private get; set; }
            public int ErrorFLAG { get; set; }

            public RunIndicator ()
            {
                ErrorText = string.Empty;
                ErrorFLAG = 0;
            }

            /// <summary>
            /// Indication of the scrip running
            /// </summary>
            private void Run_Indicator ()
            {
                if ( runCount == 8 )
                {
                    animation = ( $"|======-||-======|\nErrors: {ErrorText}" );
                }
                if ( runCount == 16 )
                {
                    animation = ( $"|=====-=||=-=====|\nErrors: {ErrorText}" );
                }
                if ( runCount == 24 )
                {
                    animation = ( $"|====-==||==-====|\nErrors: {ErrorText}" );
                }
                if ( runCount == 32 )
                {
                    animation = ( $"|==-====||===-===|\nErrors: {ErrorText}" );
                }
                if ( runCount == 40 )
                {
                    animation = ( $"|==-====||====-==|\nErrors: {ErrorText}" );
                }
                if ( runCount == 48 )
                {
                    animation = ( $"|=-=====||=====-=|\nErrors: {ErrorText}" );
                }
                if ( runCount == 56 )
                {
                    animation = ( $"|-======||======-|\nErrors: {ErrorText}" );
                }
                if ( runCount == 64 )
                {
                    animation = ( $"|=-=====||=====-=|\nErrors: {ErrorText}" );
                }
                if ( runCount == 72 )
                {
                    animation = ( $"|==-====||====-==|\nErrors: {ErrorText}" );
                }
                if ( runCount == 80 )
                {
                    animation = ( $"|===-===||===-===|\nErrors: {ErrorText}" );
                }
                if ( runCount == 88 )
                {
                    animation = ( $"|====-==||==-====|\nErrors: {ErrorText}" );
                }
                if ( runCount == 96 )
                {
                    runCount = 0;
                    animation = ( $"|=====-=||=-=====|\nErrors: {ErrorText}" );
                }
            }

            /// <summary>
            /// Runs animation
            /// </summary>
            /// <returns></returns>
            public string Run_Animation ()
            {
                Run_Indicator ();
                runCount++;
                return animation;
            }
        }
        //to this comment.
        #region post-script
    }
}
#endregion
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

namespace AirLock
{
    public class Program : MyGridProgram
    {
        #endregion
        //To put your code in a PB copy from this comment...
        #region Comment
        /*---------------------------------------------------------------------------------------HOW TO USE----------------------------------------------------------------------------------*/
        /*
                If the program throws an error: Fix the error and recompile.
                The program uses CustomData from the terminal.
                    <Building Airlock>
                        Step 1: Put in the name of your station in GRID_NAME below.
                        Step 2: Place at least 2 doors. 1 at each end of the Airlock.
                        Step 3: Name them with "Inside" and "Outside" and tag it with the name of
                                     your base. Spell it exactly as it is written here without the quotation marks. Eks: Door - Main Airlock Outside MyStationName
                        Step 4: Place 1 sensors at each end of the Airlock.Repeat step 3.
                        Step 5: Place at least one vent inside the airlock. Repeat step 3.

                    <Setting up Custom data>
                        Step 1: Go into each Airlock door and Sensor, and edit "Custom Data".
                                    Write your Airlock name and where the door/sensor is located.Syntax is a followed: [Name of your Airlock]:[Inside/Outside].
                                    F.eks: Main Airlock:Inside<--- For door and sensor that is located inside.
                                               Main Airlock:Outside<--- For door and senors located outside.
                        Step 2: Go into all the Airlock vents and write the name of your Airlock.Syntax is as followed: [Name of your Airlock]
                                     F.eks: Main Airlock

                    <NOTE>
                        It's VERY important that Custom Data and the given Tags (Grid Name, Outside, Inside) is spelled identical.
                        Location tags are fixed and MUST follow the instructions above.Grid Name just has to be spelled equally among all Airlock blocks.

                    < Setting up Sensor actions >
                        Step 1: Put your Airlock vents into the first slot [X] [] ( Entering sensor ) and your Airlock doors into the second slot [] [X] ( Leaving sensor ).
                                    Set Vents pressurization to Off and door tagged with "Inside" to close, for inside sensor.
                                    Set Vents pressurization to On and door tagged with "Outside" to clode, for outside sensor.
                        Step 2: Adjust you sensors range to fit inside your Airlock and don't let their range cross.


        */
        /*------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        #endregion
        #region CONSTANTS
        const string GRID_NAME = "Station";
        #endregion

        #region Indicator
        int G_isRunning = 0;
        string runAnimation;
        string errorText;
        int errorFLAG = 0;
        #endregion

        List<IMyDoor> doors;
        List<IMySensorBlock> sensors;
        List<IMyAirVent> vents;

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

            if ( doors == null || errorFLAG == 1 )
            {
                Fetch_Doors ();
            }

            if ( sensors == null && errorFLAG == 0 || errorFLAG == 2 )
            {
                Fetch_Sensors ();
            }

            if ( vents == null && errorFLAG == 0 || errorFLAG == 3 )
            {
                Fetch_Vents ();
            }

            if ( errorFLAG == 0 )
            {
                errorText = "None";
            }

            if ( errorFLAG == 0 )
            {
                for ( int i = 0; i < vents.Count; i++ )
                {
                    string [] info = vents [i].DetailedInfo.Split ( ':' );

                    if ( info [3] == " Not pressurized" )
                    {
                        //List<IMyDoor> insideDoors = new List<IMyDoor> ();
                        //GridTerminalSystem.GetBlocksOfType<IMyDoor> ( insideDoors );


                        if ( doors.Count != 0 )
                        {
                            for ( int j = 0; j < doors.Count; j++ )
                            {
                                string [] doorData = doors [j].CustomData.Split ( ':' );


                                errorText = ( ( doorData.Length == 2 ) ? ( doorData [1] ) : ( doorData [0] ) );

                                if ( ((doorData.Length == 2) ? (doorData[1] == "Inside") : (doorData[0] == "Inside")) )
                                {
                                    doors [j].CloseDoor ();

                                    if ( doors [j].Status == DoorStatus.Closed )
                                    {
                                        doors [j].GetActionWithName ( "OnOff_Off" ).Apply ( doors [j] );
                                    }
                                }
                                
                            }
                        }
                    }
                }

                If_Activated ();
            }

        }

        /// <summary>
        /// Fetxh all doors
        /// </summary>
        private void Fetch_Doors ()
        {
            doors = new List<IMyDoor> ();

            List<IMyDoor> allDoors = new List<IMyDoor> ();
            GridTerminalSystem.GetBlocksOfType ( allDoors );
            if ( allDoors.Count != 0 )
            {
                for ( int i = 0; i < allDoors.Count; i++ )
                {
                    if ( allDoors [i].CustomName.Contains ( GRID_NAME ) )
                    {
                        if ( allDoors [i].CustomName.Contains ( "Airlock" ) )
                        {
                            doors.Add ( allDoors [i] );
                        }

                    }
                }

                if ( doors.Count == 0 )
                {
                    errorText = $"No airlock doors on ({GRID_NAME})";
                    errorFLAG = 1;
                }
                else if ( ( doors.Count % 2 ) != 0 )
                {
                    errorFLAG = 1;
                    errorText = $"An airlock is not complete {doors.Count}/{( doors.Count + 1 )} doors.";
                }
                else
                {
                    errorFLAG = 0;
                }
            }
            else
            {
                errorText = "No doors on grid";
                errorFLAG = 1;
            }

            allDoors = null;
        }

        /// <summary>
        /// Fetch all sensors
        /// </summary>
        private void Fetch_Sensors ()
        {
            sensors = new List<IMySensorBlock> ();

            List<IMySensorBlock> allSensors = new List<IMySensorBlock> ();
            GridTerminalSystem.GetBlocksOfType ( allSensors );

            if ( allSensors.Count != 0 )
            {
                for ( int i = 0; i < allSensors.Count; i++ )
                {
                    if ( allSensors [i].CustomName.Contains ( GRID_NAME ) )
                    {
                        if ( allSensors [i].CustomName.Contains ( "Airlock" ) )
                        {
                            sensors.Add ( allSensors [i] );
                        }
                    }

                }

                if ( sensors.Count == 0 )
                {
                    errorText = $"No airlock sensors on ({GRID_NAME})";
                    errorFLAG = 2;
                }
                else if ( ( sensors.Count % 2 ) != 0 )
                {
                    errorFLAG = 2;
                    errorText = $"An airlock is not complete {sensors.Count}/{( sensors.Count + 1 )} sensors.";
                }
                else
                {
                    errorFLAG = 0;
                }
            }
            else
            {
                errorText = "No sensors on grid";
                errorFLAG = 2;
            }


            allSensors = null;
        }

        /// <summary>
        /// Fetch all vents
        /// </summary>
        private void Fetch_Vents ()
        {
            vents = new List<IMyAirVent> ();

            List<IMyAirVent> allVents = new List<IMyAirVent> ();
            GridTerminalSystem.GetBlocksOfType ( allVents );

            if ( allVents.Count != 0 )
            {
                for ( int i = 0; i < allVents.Count; i++ )
                {
                    if ( allVents [i].CustomName.Contains ( GRID_NAME ) )
                    {
                        if ( allVents [i].CustomName.Contains ( "Airlock" ) )
                        {
                            vents.Add ( allVents [i] );
                        }
                    }
                }

                if ( vents.Count == 0 )
                {
                    errorText = $"No airlock sensors on ({GRID_NAME})";
                    errorFLAG = 3;
                }
                else
                {
                    errorFLAG = 0;
                }
            }
            else
            {
                errorFLAG = 3;
                errorText = "No vents on grid";
            }

        }

        /// <summary>
        /// If a sensor is activated
        /// </summary>
        private void If_Activated ()
        {
            for ( int i = 0; i < sensors.Count; i++ )
            {
                if ( sensors [i].IsActive )
                {
                    string [] data = sensors [i].CustomData.Split ( ':' );
                    string pressureInfo = Is_Under_Pressure ( data [0] );
                    for ( int j = 0; j < doors.Count; j++ )
                    {
                        if ( sensors [i].CustomData == doors [j].CustomData )
                        {
                            if ( data [1] == "Inside" )
                            {
                                if ( pressureInfo == "100.00%" && pressureInfo != "0.00%" && pressureInfo != "Not pressurized" )
                                {
                                    doors [j].GetActionWithName ( "OnOff_On" ).Apply ( doors [j] );
                                    if ( doors [j].Status != DoorStatus.Opening || doors [j].Status != DoorStatus.Open )
                                    {
                                        doors [j].OpenDoor ();
                                    }

                                }

                            }
                            else
                            {
                                errorText = pressureInfo;
                                if ( pressureInfo == "0.00%" && pressureInfo != "100.00%" || pressureInfo == "Not pressurized" )
                                {
                                    doors [j].GetActionWithName ( "OnOff_On" ).Apply ( doors [j] );
                                    if ( doors [j].Status != DoorStatus.Opening || doors [j].Status != DoorStatus.Open )
                                    {
                                        doors [j].OpenDoor ();
                                    }
                                }
                            }

                        }
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Check if the room is pressurized
        /// </summary>
        /// <param name="_data">The data for the airlock to look for</param>
        /// <returns></returns>
        private string Is_Under_Pressure ( string _data )
        {
            IMyAirVent vent = null;
            //Depressurize_On/Depressurize_Off
            for ( int i = 0; i < vents.Count; i++ )
            {
                if ( vents [i].CustomData == _data )
                {
                    vent = vents [i];
                    break;
                }
            }

            string [] info = vent.DetailedInfo.Split ( ':' );

            return info [3];

        }

        /// <summary>
        /// Indication of the scrip running
        /// </summary>
        private void Run_Indicator ()
        {
            if ( G_isRunning == 8 )
            {
                runAnimation = ( $"|======-||-======|\nErrors: {errorText}" );
            }
            if ( G_isRunning == 16 )
            {
                runAnimation = ( $"|=====-=||=-=====|\nErrors: {errorText}" );
            }
            if ( G_isRunning == 24 )
            {
                runAnimation = ( $"|====-==||==-====|\nErrors: {errorText}" );
            }
            if ( G_isRunning == 32 )
            {
                runAnimation = ( $"|==-====||===-===|\nErrors: {errorText}" );
            }
            if ( G_isRunning == 40 )
            {
                runAnimation = ( $"|==-====||====-==|\nErrors: {errorText}" );
            }
            if ( G_isRunning == 48 )
            {
                runAnimation = ( $"|=-=====||=====-=|\nErrors: {errorText}" );
            }
            if ( G_isRunning == 56 )
            {
                runAnimation = ( $"|-======||======-|\nErrors: {errorText}" );
            }
            if ( G_isRunning == 64 )
            {
                runAnimation = ( $"|=-=====||=====-=|\nErrors: {errorText}" );
            }
            if ( G_isRunning == 72 )
            {
                runAnimation = ( $"|==-====||====-==|\nErrors: {errorText}" );
            }
            if ( G_isRunning == 80 )
            {
                runAnimation = ( $"|===-===||===-===|\nErrors: {errorText}" );
            }
            if ( G_isRunning == 88 )
            {
                runAnimation = ( $"|====-==||==-====|\nErrors: {errorText}" );
            }
            if ( G_isRunning == 96 )
            {
                G_isRunning = 0;
                runAnimation = ( $"|=====-=||=-=====|\nErrors: {errorText}" );
            }
        }
        //to this comment.
        #region post-script
    }
}
#endregion

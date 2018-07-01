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
                The program uses "Custom Data" from the terminal.
                    <Building Airlock>
                        Step 1: Put in the name of your station in GRID_NAME below.
                        Step 2: Place at least 2 doors. 1 at each end of the Airlock.
                        Step 3: Name them with "Inside" and "Outside" and tag it with the name of your base.
                                Spell it exactly as it is written without the quotation marks. Eks: Door - Main Airlock Outside MyStation
                        Step 4: Place 1 sensors at each end of the Airlock.Repeat step 3.
                        Step 5: Place at least one vent inside the airlock. Repeat step 3.

                    <Setting up Custom data>
                        Step 1: Go into each Airlock door and Sensor, and edit "Custom Data".
                                    Write your Airlock name and where the door/sensor is located. Syntax is as followed: [Name of your Airlock]:[Inside/Outside].
                                    F.eks: Main Airlock:Inside <--- For door and sensor that is located inside.
                                           Main Airlock:Outside <--- For door and senors located outside.
                        Step 2: Go into all the Airlock vents and write the name of your Airlock. Syntax is as followed: [Name of your Airlock]
                                     F.eks: Main Airlock <--- FOr all vents associated with, in this case, "Main Airlock".

                    < Setting up Sensor actions >
                        Step 0: (Optional) If you have more than one vent for an Airlock. Group them together and use the group in the following steps.
                        Step 1: Put your Airlock vent into the first slot [X] [] ( Entering sensor ) and your Airlock doors into the second slot [] [X] ( Leaving sensor ).
                                    Set Vent pressurization to Off and the door tagged with "Inside" to close, for inside sensor.
                                    Set Vent pressurization to On and the door tagged with "Outside" to close, for outside sensor.
                        Step 2: Adjust you sensors range to fit inside your Airlock and don't let their range cross.

          (Optional)<Setting up LCD's>
                        In order to make the script output any data on LCD screens you'll have to change USE_LCD = false to USE_LCD = true.
                        Each LCD that should output the Airlock data on screen must have "Airlock" and the grid name in it's name.
                        Eks: LCD -  Airlock MyStation
                        You can change the color of the screen by changing SCREEN_COLOR (Red, Green, Blue) to any value you want.
                            <Signs> 
                                    "|-|": Both Airlock doors are closed
                                    ">-|": Inside Airlock door is open
                                    "|-<": Outside Airlock doors is open
                                    ">-<": Both Airlock doors are open
                                    "|~|": Both Airlock doors are closed but there is a leak

                    <NOTE>
                        It's VERY important that Custom Data and the given Tags (Grid Name, Outside, Inside) is spelled identical.
                        Location tags are fixed and MUST follow the instructions above. Grid Name just has to be spelled equally among all Airlock blocks.
                            Custom data should include the following for;
                            Airlock name = The name of the Airlock this block is accosiated with.
                            Location     = Wether the block is on the outside of the grid or inside the grid
                            Vents  : [Airlock Name]
                            Doors  : [Airlock Name]:[Location]
                            Sensors: [Airlock Name]:[Location]
                    <Removing blocks>
                        If you remove a block you will have to recompile the script.
                        If you don't the script will throw an "Instance of an object is not set to a refrence" error.

        */
        /*------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        #endregion
        #region CONSTANTS
        const string GRID_NAME = "Station";
        const bool USE_LCD = true;
        Color SCREEN_COLOR = new Color ( 0, 255, 0 );
        #endregion

        /*------------------------------------------------------------------------------------------SCRIPT-------------------------------------------------------------------------------------*/
        #region Indicator
        int G_isRunning = 0;
        string runAnimation;
        string errorText;
        int errorFLAG = 0;
        #endregion

        List<IMyDoor> doors = null;
        List<IMySensorBlock> sensors = null;
        List<IMyAirVent> vents = null;
        List<IMyTextPanel> screens = null;

        List<string> airlockNames = new List<string> ();

        int amountOfAirlocks = 0;
        string [] airlockStatus = { "|-|", ">-|", "|-<", ">-<", "|~|" };

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

            if ( screens == null && USE_LCD && errorFLAG == 0 || errorFLAG == 5 )
            {
                Fetch_LCD ();
            }

            if ( errorFLAG == 0 || errorFLAG == 4 )
            {
                If_Activated ();
            }

            if ( errorFLAG == 0 )
            {
                errorText = "None";

                Check_For_Leaks ();

                if ( USE_LCD )
                {
                    Show_On_Screen ();
                }
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
            //  If allDoors list is not empty
            if ( allDoors.Count != 0 )
            {
                int completeAirlock = 0;
                for ( int i = 0; i < allDoors.Count; i++ )
                {
                    if ( allDoors [i].CustomName.Contains ( GRID_NAME ) )   //  Search for doors belonging to this grid
                    {
                        if ( allDoors [i].CustomName.Contains ( "Airlock" ) )   //  Search for doors belonging to an Airlock
                        {
                            doors.Add ( allDoors [i] );

                            if ( ( completeAirlock % 2 ) == 0 )
                            {
                                amountOfAirlocks++;
                            }
                        }

                    }
                }

                //  If doors list is empty
                if ( doors.Count == 0 )
                {
                    errorText = $"No airlock doors on ({GRID_NAME})";
                    errorFLAG = 1;
                }
                else if ( ( doors.Count % 2 ) != 0 )    //  If there's an uneven amount of airlock doors
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

            amountOfAirlocks /= 2;
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

            //  If allSensors list is not empty
            if ( allSensors.Count != 0 )
            {
                for ( int i = 0; i < allSensors.Count; i++ )
                {
                    if ( allSensors [i].CustomName.Contains ( GRID_NAME ) ) //  Search for sensors belonging to this grid
                    {
                        if ( allSensors [i].CustomName.Contains ( "Airlock" ) ) //  Search for sensors belonging to an Airlock
                        {
                            sensors.Add ( allSensors [i] );
                        }
                    }

                }

                //  If sensors list is empty
                if ( sensors.Count == 0 )
                {
                    errorText = $"No airlock sensors on ({GRID_NAME})";
                    errorFLAG = 2;
                }
                else if ( ( sensors.Count % 2 ) != 0 )  //  If there is a n uneven amount of sensors
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

            //  If allVents is not empty
            if ( allVents.Count != 0 )
            {
                for ( int i = 0; i < allVents.Count; i++ )
                {
                    if ( allVents [i].CustomName.Contains ( GRID_NAME ) )   //  Search for vents belonging to this grid
                    {
                        if ( allVents [i].CustomName.Contains ( "Airlock" ) )   //  Search for vents belonging to and Airlock
                        {
                            vents.Add ( allVents [i] );

                            if ( !Airlock_Exists ( allVents [i].CustomData ) )
                            {
                                airlockNames.Add ( allVents [i].CustomData );
                                //outputText += $"{allVents [i].CustomData}\n";
                            }
                        }
                    }
                }
                //  If vents list is empty
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
        /// Fetch all LCD's
        /// </summary>
        private void Fetch_LCD ()
        {
            List<IMyTextPanel> allScreens = new List<IMyTextPanel> ();
            GridTerminalSystem.GetBlocksOfType ( allScreens );

            screens = new List<IMyTextPanel> ();

            if ( allScreens.Count != 0 )
            {
                for ( int i = 0; i < allScreens.Count; i++ )
                {
                    if ( allScreens [i].CustomName.Contains ( GRID_NAME ) )
                    {
                        if ( allScreens [i].CustomName.Contains ( "Airlock" ) )
                        {
                            screens.Add ( allScreens [i] );
                            allScreens [i].ShowPublicTextOnScreen ();
                            allScreens [i].FontColor = SCREEN_COLOR;
                            allScreens [i].FontSize = 1.025f;
                        }

                    }
                }

                if ( screens.Count == 0 )
                {
                    errorFLAG = 5;
                    errorText = $"No Airlock LCD's on ({GRID_NAME})";
                }
                else
                {
                    errorFLAG = 0;
                }
            }
            else
            {
                errorFLAG = 5;
                errorText = $"({GRID_NAME}) does not have any LCD's";
                screens = null;
            }

        }

        /// <summary>
        /// If a sensor is activated
        /// </summary>
        private void If_Activated ()
        {
            IMyDoor door = null;
            for ( int i = 0; i < sensors.Count; i++ )
            {
                if ( sensors [i].IsActive )
                {
                    string [] data = sensors [i].CustomData.Split ( ':' );  //  The custom data string seperated by ':'

                    if ( data.Length <= 1 )
                    {
                        errorFLAG = 4;
                        errorText = $"Custom data on ({sensors [i].CustomName}) is missing custom data";
                        break;
                    }

                    string pressureInfo = Is_Under_Pressure ( data [0] );   //  will see if the room is pressurized or not

                    if ( pressureInfo == "Error" )
                    {
                        errorFLAG = 4;
                        errorText = $"({data [0]}) has no vents.\n Check custom data on each vent\n for the Airlock";
                        break;
                    }

                    for ( int j = 0; j < doors.Count; j++ )
                    {
                        if ( sensors [i].CustomData == doors [j].CustomData )   //  If sensor and door mhas matching custom data
                        {
                            door = doors [j];

                            if ( data [1] == "Inside" ) //  If the sensors custom data includes "Inside"
                            {
                                errorFLAG = 0;

                                if ( pressureInfo == "100.00%" && pressureInfo != "0.00%" && pressureInfo != "Not pressurized" )    //  If the room is 100% pressurized; Open door
                                {
                                    door.GetActionWithName ( "OnOff_On" ).Apply ( door ); //  Turn door on

                                    if ( door.Status != DoorStatus.Opening || door.Status != DoorStatus.Open )    //  If the door is not open or opening; Open door
                                    {
                                        door.OpenDoor ();
                                    }

                                }
                                break;

                            }
                            else if ( data [1] == "Outside" )   //  If the sensors custom data include "Outside"
                            {
                                //errorText = $"(Info) - pressureInfo";

                                errorFLAG = 0;

                                if ( pressureInfo == "0.00%" && pressureInfo != "100.00%" || pressureInfo == "Not pressurized" )    //  If the room not pressuzrized; Open door
                                {
                                    door.GetActionWithName ( "OnOff_On" ).Apply ( door ); //  Turn door on

                                    if ( door.Status != DoorStatus.Opening || door.Status != DoorStatus.Open )    //  If door is not open or opening; Open door
                                    {
                                        door.OpenDoor ();
                                    }
                                }
                                break;
                            }
                            else
                            {
                                errorFLAG = 4;
                                errorText = $"({data [0]}) has erros. Activated sensor does not include \"Inside\" or \"Outside\"";
                                break;
                            }
                        }

                    }

                    if ( door == null )
                    {
                        errorFLAG = 4;
                        errorText = $"No Custom data on airlock doors is matching sensor ({sensors [i].CustomName})";
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
            string [] info = null;
            for ( int i = 0; i < vents.Count; i++ )
            {
                //  If a vent custom data matches passed data
                if ( vents [i].CustomData == _data )
                {
                    vent = vents [i];
                    break;
                }
            }

            if ( vent != null )
            {
                errorFLAG = 0;
                info = vent.DetailedInfo.Split ( ':' );   //  Detailed info sperated by ':'
            }

            return ( ( info == null ) ? ( "Error" ) : ( info [3] ) );    //  Return pressure data

        }

        /// <summary>
        /// Checks each airlock if there is a leak.
        /// </summary>
        private void Check_For_Leaks ()
        {
            //  Check for Airlock leaks.
            for ( int i = 0; i < vents.Count; i++ )
            {
                string [] info = vents [i].DetailedInfo.Split ( ':' );

                //  If the Airlock is not pressurized close all inside airlock doors and turn them of
                if ( info [3] == " Not pressurized" )
                {
                    //List<IMyDoor> insideDoors = new List<IMyDoor> ();
                    //GridTerminalSystem.GetBlocksOfType<IMyDoor> ( insideDoors );

                    // If doors list is not empty
                    if ( doors.Count != 0 )
                    {
                        for ( int j = 0; j < doors.Count; j++ )
                        {
                            string [] doorData = doors [j].CustomData.Split ( ':' );    //  Door custom data seperated by ':'

                            //errorText = ( ( doorData.Length == 2 ) ? ( doorData [1] ) : ( doorData [0] ) );

                            if ( ( ( doorData.Length == 2 ) ? ( doorData [1] == "Inside" ) : ( doorData [0] == "Inside" ) ) ) //  If custom data includes "Inside"; Close door and turn it off
                            {
                                doors [j].CloseDoor ();

                                if ( doors [j].Status == DoorStatus.Closed )    //  If the door is closed turn it off
                                {
                                    doors [j].GetActionWithName ( "OnOff_Off" ).Apply ( doors [j] );
                                }
                            }

                        }
                    }
                }
            }
        }

        /// <summary>
        /// OUtput text on all screens
        /// </summary>
        private void Show_On_Screen ()
        {
            for ( int i = 0; i < screens.Count; i++ )
            {
                screens [i].WritePublicText ( $"<----------------------Airlocks---------------------->\n{Airlock_Status ()}", false );
            }
        }

        /// <summary>
        /// Returns a visualization of airlock doors and leaks
        /// </summary>
        /// <returns></returns>
        private string Airlock_Status ()
        {
            string formatedText = null;
            string status = null;
            for ( int i = 0; i < doors.Count; i++ )
            {
                string [] data = doors [i].CustomData.Split ( ':' );

                if ( Airlock_Exists ( data [0] ) )
                {
                    if ( data [1] == "Inside" )
                    {
                        //  If both airlock doors are closed
                        if ( doors [i].Status == DoorStatus.Closed && doors [doors.FindIndex ( door => door.CustomData == data [0] + ":Outside" )].Status == DoorStatus.Closed )
                        {
                            status = airlockStatus [0];
                        }
                        //  If the inside door is open but outside is closed
                        else if ( doors [i].Status == DoorStatus.Open && doors [doors.FindIndex ( door => door.CustomData == data [0] + ":Outside" )].Status == DoorStatus.Closed )
                        {
                            status = airlockStatus [1];
                        }
                        //  If inside door is closed but outside door is open
                        else if ( doors [i].Status == DoorStatus.Closed && doors [doors.FindIndex ( door => door.CustomData == data [0] + ":Outside" )].Status == DoorStatus.Open )
                        {
                            status = airlockStatus [2];
                        }
                        //  If both doors are open
                        else if ( doors [i].Status == DoorStatus.Open && doors [doors.FindIndex ( door => door.CustomData == data [0] + ":Outside" )].Status == DoorStatus.Open )
                        {
                            status = airlockStatus [3];
                        }
                        //  If the airlock has a leack
                        if ( doors [i].Status == DoorStatus.Closed && doors [doors.FindIndex ( door => door.CustomData == data [0] + ":Outside" )].Status == DoorStatus.Closed && Is_Under_Pressure ( data [0] ) == " Not pressurized" )
                        {
                            status = airlockStatus [4];
                        }
                        formatedText += $"{data [0]}: {status}\n";
                    }

                   
                }

            }
            return formatedText;
        }

        /// <summary>
        /// Checks the airlocksName list to determine if a name is already in the list. WIll return true if it does and false if it does not.
        /// </summary>
        /// <param name="_name"></param>
        /// <returns></returns>
        private bool Airlock_Exists ( string _name )
        {
            if ( airlockNames.Exists ( name => name == _name ) )
            {
                return true;
            }

            return false;
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

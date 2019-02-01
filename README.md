# SE_ScriptCollection
This is my personal collection of my own scripts made for Space engineers.

###Template

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

namespace HydrogenStatus
{
    public class Program : MyGridProgram
    {
        #endregion
        //To put your code in a PB copy from this comment...

        #region Comment
        /*---------------------------------------------------------------------------------------HOW TO USE---------------------------------------------------------------------------*/
        /*
                <>
                    Step 1:

                <>
                    Step 1:

                <>
                    Step 1:

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
        const string LCD_TAG = "<Gas>";
        Color SCREEN_COLOR = new Color ( 0, 255, 0 );
        #endregion

        /*------------------------------------------------------------------------------------------SCRIPT--------------------------------------------------------------------------------*/
        RunIndicator AnimationRunner = new RunIndicator ();

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

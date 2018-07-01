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

namespace RunIndicator
{
    public class Program : MyGridProgram
    {
        #endregion
        //To put your code in a PB copy from this comment...

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

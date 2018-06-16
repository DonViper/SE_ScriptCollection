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

namespace Turtle
{
    public class Program : MyGridProgram
    {
        #endregion
        //To put your code in a PB copy from this comment...
        #region CONSTANTS
        const string GRID_NAME = "Turtle";
        const string STORAGE_KEY = "<>";
        const float VOLUME_LIMIT = 50;
        enum LCD_POS { Left, Middle, Right };
        Color [] ALERT_COLOR = { new Color ( 255, 0, 0 ), new Color ( 0, 255, 0 ) };   //  Color of display when Alerting (Red) or not (Green)
        #endregion
        List<IMyTerminalBlock> AllBlocks = new List<IMyTerminalBlock> ();
        IMyProjector G_projecter;
        IMyTextPanel [] G_LCD = new IMyTextPanel [3];
        bool G_capacityAlert;

        #region Indicator
        int G_isRunning = 0;
        string runAnimation;
        string debugText;
        //bool errorFLAG = false;
        #endregion

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

            if ( AllBlocks.Count <= 0 || G_projecter.IsProjecting || G_projecter == null )
            {
                Fecth_All_Blocks ();

                if ( G_projecter == null )
                {
                    debugText = "Projector missing";
                }
            }

            //  Volume screen
            if ( G_LCD [(int) LCD_POS.Right] != null )
            {
                G_LCD [(int) LCD_POS.Right].ShowPublicTextOnScreen ();
                G_LCD [(int) LCD_POS.Right].FontSize = 2.400f;
                G_LCD [(int) LCD_POS.Right].WritePublicText ( Capacity_Status () );
            }
            else
            {
                debugText = "Right LCD missing";
            }

            //  Damage screen
            if ( G_LCD [(int) LCD_POS.Left] != null )
            {
                G_LCD [(int) LCD_POS.Left].ShowPublicTextOnScreen ();
                G_LCD [(int) LCD_POS.Left].FontColor = ALERT_COLOR [1];
                G_LCD [(int) LCD_POS.Left].FontSize = 2.550f;
                G_LCD [(int) LCD_POS.Left].WritePublicText ( Damage_Status () );
            }
            else
            {
                debugText = "Left LCD missing";
            }

            //  Active screen
            if ( G_LCD [(int) LCD_POS.Middle] != null )
            {
                G_LCD [(int) LCD_POS.Middle].ShowPublicTextOnScreen ();
                G_LCD [(int) LCD_POS.Middle].FontColor = ALERT_COLOR [1];
                G_LCD [(int) LCD_POS.Middle].FontSize = 2.366f;
                G_LCD [(int) LCD_POS.Middle].WritePublicText ( ( runAnimation == null ) ? ( "" ) : ( runAnimation ) );
            }
        }

        private void Fecth_All_Blocks ()
        {
            List<IMyTerminalBlock> temp = new List<IMyTerminalBlock> ();
            GridTerminalSystem.GetBlocks ( temp );
            for ( int i = 0; i < temp.Count; i++ )
            {
                if ( temp [i].CustomName.Contains ( GRID_NAME ) )
                {
                    if ( temp [i].CustomName.Contains ( "Projector" ) )    //  Get Projector
                    {
                        G_projecter = temp [i] as IMyProjector;
                    }
                    if ( temp [i].CustomName.Contains ( "LCD - Left" ) ) //  Get left LCD
                    {
                        G_LCD [0] = temp [i] as IMyTextPanel;
                    }
                    if ( temp [i].CustomName.Contains ( "LCD - Middle" ) )  //  Get Middle LCD
                    {
                        G_LCD [1] = temp [i] as IMyTextPanel;
                    }
                    if ( temp [i].CustomName.Contains ( "LCD - Right" ) )   //  Get Right LCD
                    {
                        G_LCD [2] = temp [i] as IMyTextPanel;
                    }
                    else
                    {
                        AllBlocks.Add ( temp [i] );
                    }
                }
            }

            temp = null;
        }

        private string Damage_Status ()
        {
            int damagedBlocks = 0;
            for ( int i = 0; i < AllBlocks.Count; i++ )
            {
                if ( !AllBlocks [i].IsFunctional )
                {
                    damagedBlocks++;
                }
            }

            return $"Dmg blocks: {damagedBlocks}";
        }

        private string Capacity_Status ()
        {
            VRage.MyFixedPoint maxVolume = 0;
            VRage.MyFixedPoint currentVolume = 0;

            for ( int i = 0; i < AllBlocks.Count; i++ )
            {
                if ( AllBlocks [i].CustomName.Contains ( STORAGE_KEY ) )
                {
                    maxVolume += AllBlocks [i].GetInventory ().MaxVolume;
                    currentVolume += AllBlocks [i].GetInventory ().CurrentVolume;
                }
            }

            //  Formula V / MV * 100 = P
            double percentage = ( ( (double) currentVolume / (double) maxVolume ) * 100 );

            if ( percentage >= VOLUME_LIMIT )
            {
                G_LCD [(int) LCD_POS.Right].FontColor = ALERT_COLOR [0];
                G_capacityAlert = true;
            }
            else if ( percentage < VOLUME_LIMIT && G_capacityAlert )
            {
                G_LCD [(int) LCD_POS.Right].FontColor = ALERT_COLOR [1];
                G_capacityAlert = false;
            }

            return $"Volume: {percentage.ToString ( "F" )}%";
        }

        /// <summary>
        /// Indication of the scrip running
        /// </summary>
        private void Run_Indicator ()
        {
            if ( G_isRunning == 8 )
            {
                runAnimation = ( $"|======-||-======|\nErrors: {debugText}" );
            }
            if ( G_isRunning == 16 )
            {
                runAnimation = ( $"|=====-=||=-=====|\nErrors: {debugText}" );
            }
            if ( G_isRunning == 24 )
            {
                runAnimation = ( $"|====-==||==-====|\nErrors: {debugText}" );
            }
            if ( G_isRunning == 32 )
            {
                runAnimation = ( $"|==-====||===-===|\nErrors: {debugText}" );
            }
            if ( G_isRunning == 40 )
            {
                runAnimation = ( $"|==-====||====-==|\nErrors: {debugText}" );
            }
            if ( G_isRunning == 48 )
            {
                runAnimation = ( $"|=-=====||=====-=|\nErrors: {debugText}" );
            }
            if ( G_isRunning == 56 )
            {
                runAnimation = ( $"|-======||======-|\nErrors: {debugText}" );
            }
            if ( G_isRunning == 64 )
            {
                runAnimation = ( $"|=-=====||=====-=|\nErrors: {debugText}" );
            }
            if ( G_isRunning == 72 )
            {
                runAnimation = ( $"|==-====||====-==|\nErrors: {debugText}" );
            }
            if ( G_isRunning == 80 )
            {
                runAnimation = ( $"|===-===||===-===|\nErrors: {debugText}" );
            }
            if ( G_isRunning == 88 )
            {
                runAnimation = ( $"|====-==||==-====|\nErrors: {debugText}" );
            }
            if ( G_isRunning == 96 )
            {
                G_isRunning = 0;
                runAnimation = ( $"|=====-=||=-=====|\nErrors: {debugText}" );
            }
        }
        //to this comment.
        #region post-script
    }
}
#endregion

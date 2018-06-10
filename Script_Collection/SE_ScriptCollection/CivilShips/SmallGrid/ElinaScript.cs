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
//using SpaceEngineers.Game.ModAPI.Ingame;

namespace Elina
{
    public class Program : MyGridProgram
    {
        #endregion
        //To put your code in a PB copy from this comment...

        public Program ()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        #region Constants
        private readonly string [] LCD_NAME = { "LCD - Left (Elina)", "LCD - Middle (Elina)", "LCD - Right (Elina)" };
        private const string STORAGE_KEY = "<>";
        const string BATTERY_NAME = "Battery (Elina)";
        const string CONNECTOR_NAME = "<> Connector - Bottom (Elina)";
        const string COCKPIT_NAME = "<> Cockpit (Elina)";
        private enum LCDPos { Left, Middle, Right };
        #endregion

        IMyTextPanel [] G_HUD;
        List<IMyTerminalBlock> G_batteries;
        IMyShipConnector G_connector;

        Color [] G_alertColor = { new Color ( 255, 0, 0 ), new Color ( 0, 230, 0 ) };   //  Color of display when Alerting (Red) or not (Green)
        bool G_capacityAlert;
        bool G_fuelalert;

        public void Main ( string argument, UpdateType updateType )
        {
            G_connector = GridTerminalSystem.GetBlockWithName ( CONNECTOR_NAME ) as IMyShipConnector;
            if ( G_connector == null )
            {
                Echo ( "Connector is missing!" );
            }
            #region Screen Text
            G_HUD = Fecth_HUD ();   //  Get all HUD screens

            if ( G_HUD [(int) LCDPos.Left] != null )
            {
                G_HUD [(int) LCDPos.Left].WritePublicText ( "<-----Terminal----->", false );
                Fuel_Status ();
                G_HUD [(int) LCDPos.Left].WritePublicText ( "\n<---------------------->", true );
            }

            if ( G_HUD [(int) LCDPos.Middle] != null )
            {
                G_HUD [(int) LCDPos.Middle].WritePublicText ( "<------------Terminal------------>", false );
                G_HUD [(int) LCDPos.Middle].WritePublicText ( "\n    <This terminal is offline>", true );
                G_HUD [(int) LCDPos.Middle].WritePublicText ( "\n<------------------------------------>", true );
            }


            if ( G_HUD [(int) LCDPos.Right] != null )
            {
                G_HUD [(int) LCDPos.Right].WritePublicText ( "<-------Terminal------->", false );
                Storage_Status ( LCDPos.Right );
                G_HUD [(int) LCDPos.Right].WritePublicText ( "\n<-------------------------->", true );
            }

            #endregion
        }

        public void Save ()
        {

        }

        private IMyTextPanel [] Fecth_HUD ()
        {
            IMyTextPanel [] panels = new IMyTextPanel [3];
            for ( int LCD = 0; LCD < LCD_NAME.Length; LCD++ )
            {
                IMyTextPanel panel = GridTerminalSystem.GetBlockWithName ( LCD_NAME [LCD] ) as IMyTextPanel;
                if ( panel == null )
                {
                    Echo ( "No Panel with name: " + LCD_NAME [LCD] + " Was found" );
                }
                panels [LCD] = panel;
            }

            return panels;
        }

        /// <summary>
        /// Return true if the Remote is under control
        /// </summary>
        /// <returns></returns>
        private bool IsControled
        {
            get
            {
                IMyCockpit remote = GridTerminalSystem.GetBlockWithName ( COCKPIT_NAME ) as IMyCockpit;
                if ( remote != null )
                {
                    if ( remote.IsUnderControl )
                    {
                        Echo ( "Cockpit Is Controled" );
                        return true;
                    }
                    Echo ( "Cockpit Is not Controled" );
                    
                }
                else
                {
                    Echo ( "Cockpit is missing!" );
                }

                return false;
            }
        }

        /// <summary>
        /// Status of stored fuel
        /// </summary>
        private void Fuel_Status ()
        {
            G_batteries = new List<IMyTerminalBlock> ();
            GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock> ( G_batteries );

            float currentStored = 0;
            float maxStored = 0;
            int batteryCount = 0;

            for ( int b = 0; b < G_batteries.Count; b++ )
            {
                if ( G_batteries [b].CustomName == BATTERY_NAME )
                {
                    var battery = G_batteries [b] as IMyBatteryBlock;
                    currentStored += battery.CurrentStoredPower;
                    maxStored += battery.MaxStoredPower;
                    batteryCount++;
                }
            }
            Echo ( "Batteries found: " + batteryCount );

            if ( IsControled )
            {

                double RemainingBatteryPowerPercentage = ( (double) currentStored / (double) maxStored ) * 100;

                if ( RemainingBatteryPowerPercentage <= 10 && G_fuelalert != true )
                {
                    G_HUD [(int) LCDPos.Left].WritePublicText ( "\n<DANGER> <----Battery IS LOW!---->\n", true );
                    G_HUD [(int) LCDPos.Left].SetValue ( "FontColor", G_alertColor [0] );
                    G_fuelalert = true;
                }
                else if ( RemainingBatteryPowerPercentage >= 10.1 && G_fuelalert == true )
                {
                    G_HUD [(int) LCDPos.Left].SetValue ( "FontColor", G_alertColor [1] );
                    G_fuelalert = false;
                }

                G_HUD [(int) LCDPos.Left].WritePublicText ( ( ( G_batteries [0] as IMyBatteryBlock ).IsCharging ? "\n<Power>: Recharging" : "\n<Power>: " + Math.Round ( RemainingBatteryPowerPercentage, 2 ) + "%" ), true );
            }
            else if ( !IsControled && ( G_connector != null ) ? ( G_connector.Status == MyShipConnectorStatus.Connected ) : ( false ) )
            {
                
                for ( int i = 0; i < G_batteries.Count; i++ )
                {
                    if ( G_batteries [i].CustomName == BATTERY_NAME )
                    {
                        G_batteries [i].GetActionWithName ( "Recharge" ).Apply ( G_batteries [i] );
                    }
                }
            }
        }

        /// <summary>
        /// Storage info of the ship. Requers a key, which is writtin into the Custom name of an object, in order to locate each specific Object
        /// </summary>
        private void Storage_Status ( LCDPos _position )
        {
            List<IMyInventory> Storage = new List<IMyInventory> ();
            VRage.MyFixedPoint MaxVolume = 0;
            VRage.MyFixedPoint CurrentVolume = 0;

            #region Collecting associated Inventories
            List<IMyTerminalBlock> AllBlocks = new List<IMyTerminalBlock> ();
            GridTerminalSystem.GetBlocks ( AllBlocks );

            if ( Storage.Count <= 0 )
            {
                for ( int i = 0; i < AllBlocks.Count; i++ )
                {
                    if ( AllBlocks [i].CustomName.Contains ( STORAGE_KEY ) )
                    {
                        Storage.Add ( (IMyInventory) AllBlocks [i].GetInventory () );

                        MaxVolume += AllBlocks [i].GetInventory ().MaxVolume;
                        CurrentVolume += AllBlocks [i].GetInventory ().CurrentVolume;
                    }
                }
            }
            else
            {
                Echo ( "No storage units on this ship" );
            }
            #endregion

            //  Formula V / MV * 100 = P
            double Percentage = ( ( (double) CurrentVolume / (double) MaxVolume ) * 100 );

            if ( Percentage >= 50 && G_capacityAlert != true )
            {
                G_HUD [(int) _position].WritePublicText ( "\n<---VEHICLE IS HEAVY!--->", true ); //  In case the ships Storage Capacity reaches a point where the ship is less manuverable.
                G_HUD [(int) _position].SetValue ( "FontColor", G_alertColor [0] );
                G_capacityAlert = true;
            }
            else if ( Percentage <= 49 && G_capacityAlert == true )
            {
                G_HUD [(int) _position].SetValue ( "FontColor", G_alertColor [1] );
                G_capacityAlert = false;
            }
            G_HUD [(int) _position].WritePublicText ( "\n<Volume>: " + Percentage.ToString ( "F" ) + "%", true );
        }
        //to this comment.
        #region post-script
    }
}
#endregion
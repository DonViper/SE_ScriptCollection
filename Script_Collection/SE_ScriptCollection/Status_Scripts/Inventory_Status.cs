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

namespace Inventory_Status
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

                    step 2: Change LCD_TAG under CONSTANTS to whatever tag you wanna use. Default is "<Inventory>"
                            Example: const string LCD_TAG = "MyTag";.
                    
                    Step 3: Set SCREEN_COLOR under CONSTANTS to whatever color you want the screen to be.
                            Example: Color SCREEN_COLOR = new Color (100, 244, 59);.

                <Setting up LCD's>
                    Step 1: Name your LCD's whatever you want but include your GRID_NAME and LCD_TAG in the name.
                            Example: LCD <Inventory> (MyStation)

                    Step 2: Edit the "Custom Data", in the terminal, of each LCD.
                            Write what the LCD should display
                           <Options>
                                1: "Ore" - Will show all Ores.
                                2: "Ingots" - Will show all Ingots.
                                3: "Components" - Will show all Components.
                                4: "Ammunition" - Will show all Ammunitions.
                                5: "Tools" - Will show all Tools.
                                6: "Capacity" - Will show how many percentage of each container and all containers combined is used.
                    
                    Note: Whenever you add a new LCD to the collection of LCD's used by the script, while the script is running, 
                              you will have to recompile the script.

                <Setting up Containers>
                    Step 1: Name your Connectors whatever you want but include your GRID_NAME in the name.
                            Example: Container <Inventory> (MyStation).
                       
                    Step 2: Edit the "Custom Data", in the terminal, of each container.
                            Write what the container should hold
                            <Options>
                                1: "Ore" - Will pull all Ores on Sort.
                                2: "Ingot" -  Will pull all Ingots on Sort.
                                3: "Component" -  Will pull all Components on Sort.
                                4: "AmmoMagazine" -  Will pull all Ammounitions on Sort.
                                5: "PhysicalGunObject" - Will pull all Tools on Sort.
                            THIS IS VERY IMPORTANT -> If this part is not done correctly you can't sort the containers.
                            
                            <How to sort>
                                Run the program with "Sort" as argument
                                This will sort all containers associated with the script.

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
        const string LCD_TAG = "<Inventory>";
        Color SCREEN_COLOR = new Color ( 0, 255, 0 );
        #endregion

        /*------------------------------------------------------------------------------------------SCRIPT--------------------------------------------------------------------------------*/
        RunIndicator AnimationRunner = new RunIndicator ();

        List<IMyTextPanel> G_screens;
        List<IMyCargoContainer> G_containers;

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
                Write_To_ALl_Screens ( argument );
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
                            allScreen [i].FontSize = 1.010f;
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
        private void Write_To_ALl_Screens ( string _argument = null )
        {
            for ( int i = 0; i < G_screens.Count; i++ )
            {
                if ( Inventory_Status ( _argument ) != null )
                {
                    switch ( G_screens [i].CustomData )
                    {
                        case "Ore":
                            G_screens [i].WritePublicText ( $"<-----------------Inventory Status----------------->\n{ Inventory_Status () [0]}" );
                            break;

                        case "Ingots":
                            G_screens [i].WritePublicText ( $"<-----------------Inventory Status----------------->\n{ Inventory_Status () [1]}" );
                            break;

                        case "Components":
                            G_screens [i].WritePublicText ( $"<-----------------Inventory Status----------------->\n{ Inventory_Status () [2]}" );
                            break;

                        case "Ammunition":
                            G_screens [i].WritePublicText ( $"<-----------------Inventory Status----------------->\n{ Inventory_Status () [3]}" );
                            break;

                        case "Tools":
                            G_screens [i].WritePublicText ( $"<-----------------Inventory Status----------------->\n{ Inventory_Status () [4]}" );
                            break;

                        case "Capacity":
                            G_screens [i].WritePublicText ( $"<-----------------Inventory Status----------------->\n{ Inventory_Status () [5]}" );
                            break;

                        default:
                            G_screens [i].WritePublicText ( "Error..." );
                            break;
                    }
                }
                else
                {
                    G_screens [i].WritePublicText ( "Error..." );
                }


            }
        }

        /// <summary>
        /// Returns a formated string with all items and all containers
        /// </summary>
        /// <param name="_argument">String value</param>
        /// <returns></returns>
        private string [] Inventory_Status ( string _argument = null )
        {
            List<IMyCargoContainer> allInventories = new List<IMyCargoContainer> ();

            if ( G_containers == null || AnimationRunner.ErrorFLAG == 2 )
            {
                GridTerminalSystem.GetBlocksOfType ( allInventories );
                G_containers = new List<IMyCargoContainer> ();
            }

            string [] format = { "<Ore>\n", "<Ingots>\n", "<Components>\n", "<Ammunition>\n", "<Tools>\n", "<Capacity>\n" };

            List<Item> allItems = new List<Item> ();

            if ( allInventories.Count != 0 )
            {
                for ( int i = 0; i < allInventories.Count; i++ )
                {
                    if ( allInventories [i].CustomName.Contains ( GRID_NAME ) )
                    {
                        G_containers.Add ( allInventories [i] );
                    }
                }
            }
            else
            {
                AnimationRunner.ErrorFLAG = 2;
                AnimationRunner.ErrorText = $"No Containers on ({GRID_NAME})";
            }

            if ( G_containers.Count == 0 )
            {
                AnimationRunner.ErrorFLAG = 2;
                AnimationRunner.ErrorText = $"No container with ({GRID_NAME}) in their name";
            }
            else
            {
                AnimationRunner.ErrorFLAG = 0;

                for ( int i = 0; i < G_containers.Count; i++ )
                {
                    if ( G_containers [i].HasInventory )
                    {
                        for ( int j = 0; j < G_containers [i].GetInventory ().GetItems ().Count; j++ )
                        {
                            var item = G_containers [i].GetInventory ().GetItems () [j];

                            if ( _argument == "Sort" )
                            {
                                Sort_Item ( item, j, G_containers, G_containers [i] );
                            }


                            if ( allItems.Exists ( element => element.Type == item.Content.TypeId.ToString () && element.Name == item.Content.SubtypeName ) )
                            {
                                allItems.Find ( element => element.Type == item.Content.TypeId.ToString () && element.Name == item.Content.SubtypeName ).Set_Amount ( item.Amount );
                            }
                            else
                            {
                                allItems.Add ( new Item ( item.Content.TypeId.ToString (), item.Content.SubtypeName, item.Amount ) );
                            }
                        }
                    }
                }

                //string output = string.Empty;

                for ( int i = 0; i < allItems.Count; i++ )
                {
                    if ( allItems [i].Type.Split ( '_' ) [1] == "Ore" )
                    {
                        format [0] += $"        {allItems [i].ToString ()}\n";
                    }
                    else if ( allItems [i].Type.Split ( '_' ) [1] == "Ingot" )
                    {
                        format [1] += $"        {allItems [i].ToString ()}\n";
                    }
                    else if ( allItems [i].Type.Split ( '_' ) [1] == "Component" )
                    {
                        format [2] += $"        {allItems [i].ToString ()}\n";
                    }
                    else if ( allItems [i].Type.Split ( '_' ) [1] == "AmmoMagazine" )
                    {
                        format [3] += $"        {allItems [i].ToString ()}\n";
                    }
                    else
                    {
                        format [4] += $"        {allItems [i].ToString ()}\n";
                    }
                }

                format [5] += ( Capacity_Status ( G_containers ) ?? "No data..." );

                //output = $"{format [0]}{format [1]}{format [2]}{format [3]}{format [4]}";

                return format;
            }

            return null;
        }

        /// <summary>
        /// The capacity of the each container and all containers combined
        /// </summary>
        /// <param name="_containers">List with containers</param>
        /// <returns></returns>
        private string Capacity_Status ( List<IMyCargoContainer> _containers )
        {
            VRage.MyFixedPoint currentVolume = 0;
            VRage.MyFixedPoint maxVolume = 0;

            string output = string.Empty;

            if ( _containers.Count != 0 )
            {
                for ( int i = 0; i < _containers.Count; i++ )
                {
                    VRage.MyFixedPoint max = _containers [i].GetInventory ().MaxVolume;
                    VRage.MyFixedPoint current = _containers [i].GetInventory ().CurrentVolume;

                    currentVolume += current;
                    maxVolume += max;

                    double individualPercentage = ( ( (double) current / (double) max ) * 100 );

                    output += $"        {( ( _containers [i].CustomName.Length >= 18 ) ? ( _containers [i].CustomName.Substring ( 0, 18 ) ) : ( _containers [i].CustomName ) )}: {individualPercentage.ToString ( "F" )}%\n";
                }
            }
            else
            {
                return null;
            }

            //  Formula V / MV * 100 = P
            double percentage = ( ( (double) currentVolume / (double) maxVolume ) * 100 );

            return $"{output}\nTotal:                        {percentage.ToString ( "F" )}%";
        }

        /// <summary>
        /// Sorts all containers
        /// </summary>
        /// <param name="_item">The item to be transfered</param>
        /// <param name="_itemIndex">Index of the item to be transfered</param>
        /// <param name="_containers">The list containing containers</param>
        /// <param name="_transferFrom">The container to transfer from</param>
        private void Sort_Item ( IMyInventoryItem _item, int _itemIndex, List<IMyCargoContainer> _containers, IMyCargoContainer _transferFrom )
        {
            for ( int i = 0; i < _containers.Count; i++ )
            {
                if ( _containers [i].CustomData == _item.Content.TypeId.ToString ().Split ( '_' ) [1] )
                {
                    _containers [i].GetInventory ().TransferItemFrom ( _transferFrom.GetInventory (), _itemIndex, stackIfPossible: true );
                }
            }
        }

        /// <summary>
        /// Represents an Item object
        /// </summary>
        private class Item
        {
            /// <summary>
            /// The type of the item
            /// </summary>
            public string Type { get; private set; }
            /// <summary>
            /// THe name of the item
            /// </summary>
            public string Name { get; private set; }
            /// <summary>
            /// THe amount of this item
            /// </summary>
            public VRage.MyFixedPoint Amount { get; private set; }

            /// <summary>
            /// Constructer
            /// </summary>
            /// <param name="_type">The type of the item</param>
            /// <param name="_name">The name of the item</param>
            /// <param name="_amount">THe amount of the item</param>
            public Item ( string _type, string _name, VRage.MyFixedPoint _amount )
            {
                Type = _type;
                Name = _name;
                Amount = _amount;
            }

            /// <summary>
            /// Sets the amount for this item.
            /// </summary>
            /// <param name="_amount">The amount to add</param>
            public void Set_Amount ( VRage.MyFixedPoint _amount )
            {
                Amount += _amount;
            }

            /// <summary>
            /// Converts the item into a formatted string
            /// </summary>
            /// <returns></returns>
            public override string ToString ()
            {
                decimal amount = (decimal) Amount;
                string type = Type.Split ( '_' ) [1];

                string output = $"{this.Name} {( ( type == "Ore" || type == "Ingot" ) ? ( type ) : ( "" ) )}: {amount.ToString ( "#,##0.##" )}";

                return output;
            }
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
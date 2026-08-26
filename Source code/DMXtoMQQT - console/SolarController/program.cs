using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;



namespace uDMXtoMQTT
{


    public partial class Program
    {
        static sol_objects dmx_collection = new sol_objects();
        //Collection<sol_objects> dmx_collection = new Collection<sol_objects>();
        static Sol_data_class data = new Sol_data_class();
        static dataclassgui data_class = new dataclassgui();
        static controller led_controller;

        public static void Main()
        {

            sol_objects data = new sol_objects();

            dmx_collection.data_object.Add(data_class);



            // checks if default.cfg is present in program dir and reads data, if true. if false writes a cfg file with default data, as starting point.
            if (File.Exists(@"default.cfg") == true)
            {
                read_config_file();
                Console.WriteLine(DateTime.Now.ToString() + " Config file read...\n");
                dmx_collection.data_object[0].textbox1_status_messages += DateTime.Now.ToString() + " Config file read...\n";
            }
            else
            {
                dmx_collection.data_object[0].textbox1_status_messages += DateTime.Now.ToString() + " No config file found\n Default values written to config.cfg\n";
                Console.WriteLine(DateTime.Now.ToString() + " No config file found\n Default values written to config.cfg\n");
                write_config_file();
                read_config_file();
            }      

            // create led controller object, handles data between mqqt and dmx controller.
            // pretends to be an led controller
            led_controller = new controller(dmx_collection.data_object[0]);
            Thread unit_control = new Thread(new ParameterizedThreadStart(led_controller.thread_routine));
            unit_control.Start();


        }


    }




}

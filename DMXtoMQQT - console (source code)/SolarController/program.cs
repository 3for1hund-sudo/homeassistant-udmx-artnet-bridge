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

            // catches HA program termination
            using var sigTermRegistration = PosixSignalRegistration.Create(PosixSignal.SIGTERM, context =>
            {
                Console.WriteLine($"{DateTime.Now} -> SIGTERM (Signal 15) recieved Home Assistant Supervisor!");

                // 1. Stop controller-loopen og kør oprydningen i controller.cs
                led_controller.Stop();

                // 2. Giv tråden op til 3 sekunder til at nå at sende de sidste pakker og lukke MQTT
                unit_control.Join(3000);

                Console.WriteLine($"{DateTime.Now} -> closing program via POSIX handler.");

                // 3. Fortæl operativsystemet, at vi har håndteret signalet succesfuldt
                context.Cancel = true;

                // 4. Afslut appen med kode 0 (Clean exit)
                Environment.Exit(0);
            });
            // --------------------------------------------------

            // Hold hovedtråden i live i samspil med baggrundstråden
            unit_control.Join();
        }


    }




}

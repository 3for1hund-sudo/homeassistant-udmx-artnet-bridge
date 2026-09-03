using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uDMXtoMQTT
{
   public partial class Program
    {

        // read default.cfg and shunts the config into the proper variables
        public static void read_config_file()
        {
            int counter = 0;
            int counter2 = 0;
            int start_of_string = 0;
            int end_of_string = 0;
            string temp_string = "";
            string temp_string2 = "";
            
            string[] split_array = new string[20];

            string data_read = "";

            const string config_file_name = "default.cfg";

            const string xml_setup = "<setup>";
            const string xml_setup_inv = "</setup>";
            const string xml_mqqt_server_ip = "        <mqqt_server_ip>";
            const string xml_mqqt_server_ip_inv = "</mqqt_server_ip>";
            const string xml_mqqt_server_port = "        <mqqt_server_port>";
            const string xml_mqqt_server_port_inv = "</mqqt_server_port>";
            const string xml_mqqt_server_password = "        <mqqt_server_password>";
            const string xml_mqqt_server_password_inv = "</mqqt_server_password>";
            const string xml_mqqt_server_user = "        <mqqt_server_user>";
            const string xml_mqqt_server_user_inv = "</mqqt_server_user>";
            const string xml_number_of_channels = "        <number_of_channels>";
            const string xml_number_of_channels_inv = "</number_of_channels>";
            const string xml_mqqt_device_id = "        <mqqt_device_id>";
            const string xml_mqqt_device_id_inv = "</mqqt_device_id>";
            const string xml_fade_stepsize = "        <fade_stepsize>";
            const string xml_fade_stepsize_inv = "</fade_stepsize>";
            const string xml_colormorph_fade_stepsize = "        <colormorph_fade_stepsize>";
            const string xml_colormorph_fade_stepsize_inv = "</colormorph_fade_stepsize>";


            //const string xml_channel_info = "<channels>\n";
            //const string xml_channel_info_inv = "</channels>\n";
            const string xml_channel_name = "        <channel_name>";
            const string xml_channel_name_inv = "</channel_name>";
            const string xml_subscribe_control_topic = "        <subscribe_control_topic>";
            const string xml_subscribe_control_topic_inv = "</subscribe_control_topic>";
            const string xml_publish_topic = "        <publish_topic>";
            const string xml_publish_topic_inv = "</publish_topic>";
            const string xml_led_type = "        <led_type>";
            const string xml_led_type_inv = "</led_type>";
            const string xml_qos_level = "        <qos_level>";
            const string xml_qos_level_inv = "</qos_level>";
            const string xml_colors_red_file = "        <colormorph_red>";
            const string xml_colors_red_file_inv = "</colormorph_red>";
            const string xml_colors_green_file = "        <colormorph_green>";
            const string xml_colors_green_file_inv = "</colormorph_green>";
            const string xml_colors_blue_file = "        <colormorph_blue>";
            const string xml_colors_blue_file_inv = "</colormorph_blue>";
            const string xml_colors_white_file = "        <colormorph_white>";
            const string xml_colors_white_file_inv = "</colormorph_white>";
            const string xml_colors_white2_file = "        <colormorph_white2>";
            const string xml_colors_white2_file_inv = "</colormorph_white2>";
            const string xml_colormorph_name_file = "        <colors_name_file>";
            const string xml_colormorph_name_file_inv = "</colors_name_file>";
            const string xml_colormorph_random_color_file = "        <colormorph_random_color>";
            const string xml_colormorph_random_color_inv = "</colormorph_random_color>";
            const string xml_colormorph_speed = "        <colormorph_speed>";
            const string xml_colormorph_speed_inv = "</colormorph_speed>";
            const string xml_artnet_target_ip = "        <artnet_target_ip>";
            const string xml_artnet_target_ip_inv = "</artnet_target_ip>";
            const string xml_artnet_enabled = "        <artnet_enabled>";
            const string xml_artnet_enabled_inv = "</artnet_enabled>";
            const string xml_artnet_universe = "        <artnet_universe>";
            const string xml_artnet_universe_inv = "</artnet_universe>";

            string exeDirectory = AppContext.BaseDirectory;

            // Kombiner mappen med dit filnavn, så du får f.eks. "C:\MitProgram\default.cfg" 
            // eller på Pi'en: "/opt/pimigrationservice/default.cfg"
            string fullConfigPath = System.IO.Path.Combine(exeDirectory, config_file_name);

            using (System.IO.StreamReader file = new System.IO.StreamReader(fullConfigPath))
            {
                data_read += file.ReadToEnd();
            }

           // Console.WriteLine(data_read);

            // isolate program setup data , from cfg
            start_of_string = data_read.LastIndexOf(xml_setup);
            end_of_string = data_read.LastIndexOf(xml_setup_inv);
            temp_string = data_read.Substring(start_of_string, (end_of_string - start_of_string));

            //isolate mqqt server ip adress, from setup data
            start_of_string = temp_string.LastIndexOf(xml_mqqt_server_ip);
            end_of_string = temp_string.LastIndexOf(xml_mqqt_server_ip_inv);
            temp_string2 = temp_string.Substring(start_of_string, (end_of_string - start_of_string));
            split_array = temp_string2.Split(">");
            dmx_collection.data_object[0].mqqt_server_ip = split_array[1];

            //isolate mqqt server port, from setup data
            start_of_string = temp_string.LastIndexOf(xml_mqqt_server_port);
            end_of_string = temp_string.LastIndexOf(xml_mqqt_server_port_inv);
            temp_string2 = temp_string.Substring(start_of_string, (end_of_string - start_of_string));
            split_array = temp_string2.Split(">");
            dmx_collection.data_object[0].mqqt_server_port = split_array[1];

            //isolate mqqt server password, from setup data
            start_of_string = temp_string.LastIndexOf(xml_mqqt_server_password);
            end_of_string = temp_string.LastIndexOf(xml_mqqt_server_password_inv);
            temp_string2 = temp_string.Substring(start_of_string, (end_of_string - start_of_string));
            split_array = temp_string2.Split(">");
            dmx_collection.data_object[0].mqqt_server_password = split_array[1];

            //isolate mqqt server username, from setup data
            start_of_string = temp_string.LastIndexOf(xml_mqqt_server_user);
            end_of_string = temp_string.LastIndexOf(xml_mqqt_server_user_inv);
            temp_string2 = temp_string.Substring(start_of_string, (end_of_string - start_of_string));
            split_array = temp_string2.Split(">");
            dmx_collection.data_object[0].mqqt_server_user = split_array[1];

            //isolate number of channels in use, max udmx can handle is 512, from setup data
            start_of_string = temp_string.LastIndexOf(xml_number_of_channels);
            end_of_string = temp_string.LastIndexOf(xml_number_of_channels_inv);
            temp_string2 = temp_string.Substring(start_of_string, (end_of_string - start_of_string));
            split_array = temp_string2.Split(">");
            dmx_collection.data_object[0].number_of_channels = Convert.ToInt32(split_array[1]);

            //isolate device id, setup data
            start_of_string = temp_string.LastIndexOf(xml_mqqt_device_id);
            end_of_string = temp_string.LastIndexOf(xml_mqqt_device_id_inv);
            temp_string2 = temp_string.Substring(start_of_string, (end_of_string - start_of_string));
            split_array = temp_string2.Split(">");
            dmx_collection.data_object[0].mqqt_device_id = split_array[1];

            // effect fade to led values, stepsize for fade
            start_of_string = temp_string.LastIndexOf(xml_fade_stepsize);
            end_of_string = temp_string.LastIndexOf(xml_fade_stepsize_inv);
            temp_string2 = temp_string.Substring(start_of_string, (end_of_string - start_of_string));
            split_array = temp_string2.Split(">");
            dmx_collection.data_object[0].fade_stepsize = Convert.ToInt32(split_array[1]);

            // effect colormorph, stepsise for fade to next color
            start_of_string = temp_string.LastIndexOf(xml_colormorph_fade_stepsize);
            end_of_string = temp_string.LastIndexOf(xml_colormorph_fade_stepsize_inv);
            temp_string2 = temp_string.Substring(start_of_string, (end_of_string - start_of_string));
            split_array = temp_string2.Split(">");
            dmx_collection.data_object[0].colormorph_fade_stepsize = Convert.ToInt32(split_array[1]);

            // Isoler og indlæs Art-Net Target IP
            if (temp_string.Contains(xml_artnet_target_ip))
            {
                start_of_string = temp_string.LastIndexOf(xml_artnet_target_ip);
                end_of_string = temp_string.LastIndexOf(xml_artnet_target_ip_inv);
                temp_string2 = temp_string.Substring(start_of_string, (end_of_string - start_of_string));
                split_array = temp_string2.Split('>');
                dmx_collection.data_object[0].artnet_target_ip = split_array[1].Trim();
            }

            // Isoler og indlæs Art-Net Enabled (True/False)
            if (temp_string.Contains(xml_artnet_enabled))
            {
                start_of_string = temp_string.LastIndexOf(xml_artnet_enabled);
                end_of_string = temp_string.LastIndexOf(xml_artnet_enabled_inv);
                temp_string2 = temp_string.Substring(start_of_string, (end_of_string - start_of_string));
                split_array = temp_string2.Split('>');
                dmx_collection.data_object[0].artnet_enabled = Convert.ToBoolean(split_array[1].Trim());
            }

            // Isoler og indlæs Art-Net Universnummer (0-15)
            if (temp_string.Contains(xml_artnet_universe))
            {
                start_of_string = temp_string.LastIndexOf(xml_artnet_universe);
                end_of_string = temp_string.LastIndexOf(xml_artnet_universe_inv);
                temp_string2 = temp_string.Substring(start_of_string, (end_of_string - start_of_string));
                split_array = temp_string2.Split('>');
                dmx_collection.data_object[0].artnet_universe = Convert.ToByte(split_array[1].Trim());
            }

            for (counter = 0; counter < dmx_collection.data_object[0].number_of_channels; counter++)
            {
                // isolate program setup data , from cfg
                start_of_string = data_read.LastIndexOf("<ch" + counter + ">");
                end_of_string = data_read.LastIndexOf("</ch" + counter + ">");
                temp_string = data_read.Substring(start_of_string, (end_of_string - start_of_string));

                //isolate channel name, from channel
                start_of_string = temp_string.LastIndexOf(xml_channel_name);
                end_of_string = temp_string.LastIndexOf(xml_channel_name_inv);
                temp_string2 = temp_string.Substring(start_of_string, (end_of_string - start_of_string));
                split_array = temp_string2.Split(">");
                dmx_collection.data_object[0].channel_name[counter] = split_array[1];

                //isolate subscribe control topic, from channel
                start_of_string = temp_string.LastIndexOf(xml_subscribe_control_topic);
                end_of_string = temp_string.LastIndexOf(xml_subscribe_control_topic_inv);
                temp_string2 = temp_string.Substring(start_of_string, (end_of_string - start_of_string));
                split_array = temp_string2.Split(">");
                dmx_collection.data_object[0].subscribe_control_topic[counter] = split_array[1];

                //isolate subsrice publish topic, from channel
                start_of_string = temp_string.LastIndexOf(xml_publish_topic);
                end_of_string = temp_string.LastIndexOf(xml_publish_topic_inv);
                temp_string2 = temp_string.Substring(start_of_string, (end_of_string - start_of_string));
                split_array = temp_string2.Split(">");
                dmx_collection.data_object[0].publish_topic[counter] = split_array[1];

                //isolate LED type, from channel
                start_of_string = temp_string.LastIndexOf(xml_led_type);
                end_of_string = temp_string.LastIndexOf(xml_led_type_inv);
                temp_string2 = temp_string.Substring(start_of_string, (end_of_string - start_of_string));
                split_array = temp_string2.Split(">");
                dmx_collection.data_object[0].led_type[counter] = split_array[1];

                //isolate Qos level, from channel
                start_of_string = temp_string.LastIndexOf(xml_qos_level);
                end_of_string = temp_string.LastIndexOf(xml_qos_level_inv);
                temp_string2 = temp_string.Substring(start_of_string, (end_of_string - start_of_string));
                split_array = temp_string2.Split(">");
                dmx_collection.data_object[0].Qos_level[counter] = Convert.ToByte(split_array[1]);



                
                if (!dmx_collection.data_object[0].led_type[counter].Contains("GROUP"))
                {


                    if (dmx_collection.data_object[0].led_type[counter] == "R" || dmx_collection.data_object[0].led_type[counter] == "RGB" || dmx_collection.data_object[0].led_type[counter] == "RGBW" || dmx_collection.data_object[0].led_type[counter] == "RGBWW")
                    {

                        // red colormorph, values into array
                        start_of_string = temp_string.LastIndexOf(xml_colors_red_file);
                        end_of_string = temp_string.LastIndexOf(xml_colors_red_file_inv);
                        temp_string2 = temp_string.Substring(start_of_string, (end_of_string - start_of_string));
                        split_array = temp_string2.Split(">");
                        split_array = split_array[1].Split(",");
                        for (counter2 = 0; counter2 < split_array.Length; counter2++)
                            dmx_collection.data_object[0].colors_red_file[counter, counter2] = Convert.ToByte(split_array[counter2]);
                    }

                    if (dmx_collection.data_object[0].led_type[counter] == "G" || dmx_collection.data_object[0].led_type[counter] == "RGB" || dmx_collection.data_object[0].led_type[counter] == "RGBW" || dmx_collection.data_object[0].led_type[counter] == "RGBWW")
                    {

                        // green colormorph, values into array
                        start_of_string = temp_string.LastIndexOf(xml_colors_green_file);
                        end_of_string = temp_string.LastIndexOf(xml_colors_green_file_inv);
                        temp_string2 = temp_string.Substring(start_of_string, (end_of_string - start_of_string));
                        split_array = temp_string2.Split(">");
                        split_array = split_array[1].Split(",");
                        for (counter2 = 0; counter2 < split_array.Length; counter2++)
                            dmx_collection.data_object[0].colors_green_file[counter, counter2] = Convert.ToByte(split_array[counter2]);
                    }

                    if (dmx_collection.data_object[0].led_type[counter] == "B" || dmx_collection.data_object[0].led_type[counter] == "RGB" || dmx_collection.data_object[0].led_type[counter] == "RGBW" || dmx_collection.data_object[0].led_type[counter] == "RGBWW")
                    {

                        // blue colormorph, values into array
                        start_of_string = temp_string.LastIndexOf(xml_colors_blue_file);
                        end_of_string = temp_string.LastIndexOf(xml_colors_blue_file_inv);
                        temp_string2 = temp_string.Substring(start_of_string, (end_of_string - start_of_string));
                        split_array = temp_string2.Split(">");
                        split_array = split_array[1].Split(",");
                        for (counter2 = 0; counter2 < split_array.Length; counter2++)
                            dmx_collection.data_object[0].colors_blue_file[counter, counter2] = Convert.ToByte(split_array[counter2]);
                    }

                    if (dmx_collection.data_object[0].led_type[counter] == "W" || dmx_collection.data_object[0].led_type[counter] == "RGBW" || dmx_collection.data_object[0].led_type[counter] == "RGBWW")
                    {

                        // white colormorph, values into array
                        start_of_string = temp_string.LastIndexOf(xml_colors_white_file);
                        end_of_string = temp_string.LastIndexOf(xml_colors_white_file_inv);
                        temp_string2 = temp_string.Substring(start_of_string, (end_of_string - start_of_string));
                        split_array = temp_string2.Split(">");
                        split_array = split_array[1].Split(",");
                        for (counter2 = 0; counter2 < split_array.Length; counter2++)
                            dmx_collection.data_object[0].colors_white_file[counter, counter2] = Convert.ToByte(split_array[counter2]);
                    }

                    if ((dmx_collection.data_object[0].led_type[counter] == "RGBWW") && (temp_string.Contains(xml_colors_white2_file)))
                    {

                        // white2 colormorph, values into array
                        start_of_string = temp_string.LastIndexOf(xml_colors_white2_file);
                        end_of_string = temp_string.LastIndexOf(xml_colors_white2_file_inv);
                        temp_string2 = temp_string.Substring(start_of_string, (end_of_string - start_of_string));
                        split_array = temp_string2.Split(">");
                        split_array = split_array[1].Split(",");
                        for (counter2 = 0; counter2 < split_array.Length; counter2++)
                            dmx_collection.data_object[0].colors_white2_file[counter, counter2] = Convert.ToByte(split_array[counter2]);
                    }

                    //load colormap names
                    start_of_string = temp_string.LastIndexOf(xml_colormorph_name_file);
                    end_of_string = temp_string.LastIndexOf(xml_colormorph_name_file_inv);
                    temp_string2 = temp_string.Substring(start_of_string, (end_of_string - start_of_string));
                    split_array = temp_string2.Split(">");
                    split_array = split_array[1].Split(",");
                    for (counter2 = 0; counter2 < split_array.Length; counter2++)
                        dmx_collection.data_object[0].colors_name_file[counter, counter2] = split_array[counter2];

                    //load random bool
                    start_of_string = temp_string.LastIndexOf(xml_colormorph_random_color_file);
                    end_of_string = temp_string.LastIndexOf(xml_colormorph_random_color_inv);
                    temp_string2 = temp_string.Substring(start_of_string, (end_of_string - start_of_string));
                    split_array = temp_string2.Split(">");
                    dmx_collection.data_object[0].colormorph_random_color[counter] = Convert.ToBoolean(split_array[1]);

                    //isolate color morph speed, from channel
                    start_of_string = temp_string.LastIndexOf(xml_colormorph_speed);
                    end_of_string = temp_string.LastIndexOf(xml_colormorph_speed_inv);
                    temp_string2 = temp_string.Substring(start_of_string, (end_of_string - start_of_string));
                    split_array = temp_string2.Split(">");
                    dmx_collection.data_object[0].colormorph_speed[counter] = Convert.ToInt32(split_array[1]);

               
                }

                //fill current LED array with current controller values to 0;
                dmx_collection.data_object[0].current_led_value[counter] = 0;


            }

            //set brightness to max, for first turn on
            for (counter = 0; counter < 512; counter++)
            {
                dmx_collection.data_object[0].brightness_controller[counter] = 255;
            }
        }

        // writes program config file in xml format, if no data is found in arrays, default info is written to file, to get a starting point.
        public static void write_config_file()
        {
            int counter = 0;
            const string config_file_name = "default.cfg";

            const string xml_setup = "<setup>\n";
            const string xml_setup_inv = "</setup>\n";
            const string xml_mqqt_server_ip = "        <mqqt_server_ip>";
            const string xml_mqqt_server_ip_inv = "</mqqt_server_ip>\n\n";
            const string xml_mqqt_server_port = "        <mqqt_server_port>";
            const string xml_mqqt_server_port_inv = "</mqqt_server_port>\n\n";
            const string xml_mqqt_server_password = "        <mqqt_server_password>";
            const string xml_mqqt_server_password_inv = "</mqqt_server_password>\n\n";
            const string xml_mqqt_server_user = "        <mqqt_server_user>";
            const string xml_mqqt_server_user_inv = "</mqqt_server_user>\n\n";
            const string xml_number_of_channels = "        <number_of_channels>";
            const string xml_number_of_channels_inv = "</number_of_channels>\n\n";
            const string xml_mqqt_device_id = "        <mqqt_device_id>";
            const string xml_mqqt_device_id_inv = "</mqqt_device_id>\n\n";
            const string xml_fade_stepsize = "        <fade_stepsize>";
            const string xml_fade_stepsize_inv = "</fade_stepsize>\n\n";
            const string xml_colormorph_fade_stepsize = "        <colormorph_fade_stepsize>";
            const string xml_colormorph_fade_stepsize_inv = "</colormorph_fade_stepsize>\n\n";


            const string xml_channel_info = "<channels>\n";
            const string xml_channel_info_inv = "</channels>\n";
            const string xml_channel_name = "        <channel_name>";
            const string xml_channel_name_inv = "</channel_name>\n";
            const string xml_subscribe_control_topic = "        <subscribe_control_topic>";
            const string xml_subscribe_control_topic_inv = "</subscribe_control_topic>\n";
            const string xml_publish_topic = "        <publish_topic>";
            const string xml_publish_topic_inv = "</publish_topic>\n";
            const string xml_led_type = "        <led_type>";
            const string xml_led_type_inv = "</led_type>\n";
            const string xml_qos_level = "        <qos_level>";
            const string xml_qos_level_inv = "</qos_level>\n";
            const string xml_colors_red_file = "        <colormorph_red>";
            const string xml_colors_red_file_inv = "</colormorph_red>\n";
            const string xml_colors_green_file = "        <colormorph_green>";
            const string xml_colors_green_file_inv = "</colormorph_green>\n";
            const string xml_colors_blue_file = "        <colormorph_blue>";
            const string xml_colors_blue_file_inv = "</colormorph_blue>\n";
            const string xml_colors_white_file = "        <colormorph_white>";
            const string xml_colors_white_file_inv = "</colormorph_white>\n";
            const string xml_colors_white2_file = "        <colormorph_white2>";
            const string xml_colors_white2_file_inv = "</colormorph_white2>\n";
            const string xml_colormorph_name_file = "        <colors_name_file>";
            const string xml_colormorph_name_file_inv = "</colors_name_file>\n";
            const string xml_colormorph_random_color_file = "        <colormorph_random_color>";
            const string xml_colormorph_random_color_inv = "</colormorph_random_color>\n";
            
            const string xml_colormorph_speed = "        <colormorph_speed>";
            const string xml_colormorph_speed_inv = "</colormorph_speed>\n\n";

            const string xml_artnet_target_ip = "        <artnet_target_ip>";
            const string xml_artnet_target_ip_inv = "</artnet_target_ip>\n\n";
            const string xml_artnet_enabled = "        <artnet_enabled>";
            const string xml_artnet_enabled_inv = "</artnet_enabled>\n\n";
            const string xml_artnet_universe = "        <artnet_universe>";
            const string xml_artnet_universe_inv = "</artnet_universe>\n\n";

            int counter3 = 0;
            int number_of_colors = 0;

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(config_file_name, false))
            {
                file.Write(xml_setup);

                //write server ip to cfg.
                file.Write(xml_mqqt_server_ip);
                file.Write(dmx_collection.data_object[0].mqqt_server_ip);
                file.Write(xml_mqqt_server_ip_inv);
                //write port number
                file.Write(xml_mqqt_server_port);
                file.Write(dmx_collection.data_object[0].mqqt_server_port);
                file.Write(xml_mqqt_server_port_inv);
                //write server password
                file.Write(xml_mqqt_server_password);
                file.Write(dmx_collection.data_object[0].mqqt_server_password);
                file.Write(xml_mqqt_server_password_inv);
                //write user name
                file.Write(xml_mqqt_server_user);
                file.Write(dmx_collection.data_object[0].mqqt_server_user);
                file.Write(xml_mqqt_server_user_inv);
                //number of led channels
                file.Write(xml_number_of_channels);
                file.Write(dmx_collection.data_object[0].number_of_channels);
                file.Write(xml_number_of_channels_inv);
                //number of dmx board channels.
                file.Write(xml_mqqt_device_id);
                file.Write(dmx_collection.data_object[0].mqqt_device_id);
                file.Write(xml_mqqt_device_id_inv);

                file.Write(xml_fade_stepsize);
                file.Write(dmx_collection.data_object[0].fade_stepsize);
                file.Write(xml_fade_stepsize_inv);

                file.Write(xml_colormorph_fade_stepsize);
                file.Write(dmx_collection.data_object[0].colormorph_fade_stepsize);
                file.Write(xml_colormorph_fade_stepsize_inv);

                // Skriv Art-Net Target IP til filen
                file.Write(xml_artnet_target_ip);
                file.Write(dmx_collection.data_object[0].artnet_target_ip);
                file.Write(xml_artnet_target_ip_inv);

                // Skriv Art-Net Global Enabled status til filen
                file.Write(xml_artnet_enabled);
                file.Write(dmx_collection.data_object[0].artnet_enabled.ToString());
                file.Write(xml_artnet_enabled_inv);

                // Skriv Art-Net Universnummer til filen
                file.Write(xml_artnet_universe);
                file.Write(dmx_collection.data_object[0].artnet_universe.ToString());
                file.Write(xml_artnet_universe_inv);

                file.Write(xml_setup_inv);

                file.Write("\n" + xml_channel_info);


                // writes channel info to xml file, if array containing data is null, then dummy info is written.
                for (counter = 0; counter < dmx_collection.data_object[0].number_of_channels; counter++)
                {

                    number_of_colors = 16;

                    for(counter3 = 0;counter3 < 16;counter3++)
                    {
                        if(dmx_collection.data_object[0].colors_name_file[counter, counter3] == "")
                        {
                            number_of_colors = counter3;
                            counter3 = 16;
                        }

                    }

                    file.Write("<ch" + Convert.ToString(counter) + ">\n");

                    file.Write(xml_channel_name);

                    //write channel name or dummy info
                    if (dmx_collection.data_object[0].channel_name[counter] == null)
                        file.Write("no_name_" + "ch" + Convert.ToString(counter));
                    else
                        file.Write(dmx_collection.data_object[0].channel_name[counter]);

                    file.Write(xml_channel_name_inv);


                    file.Write(xml_subscribe_control_topic);
                    //write default control topic
                    if (dmx_collection.data_object[0].subscribe_control_topic[counter] == null)
                        file.Write("kitchen/led/floor/");
                    else
                        file.Write(dmx_collection.data_object[0].subscribe_control_topic[counter]);

                    file.Write(xml_subscribe_control_topic_inv);

                    file.Write(xml_publish_topic);
                    //write default publish topic
                    if (dmx_collection.data_object[0].publish_topic[counter] == null)
                        file.Write("kitchen/led/floor/currentvalue/");
                    else
                        file.Write(dmx_collection.data_object[0].publish_topic[counter]);

                    file.Write(xml_publish_topic_inv);

                    file.Write(xml_led_type);
                    //write default control topic
                    if (dmx_collection.data_object[0].led_type[counter] == null)
                        file.Write("W");
                    else
                        file.Write(dmx_collection.data_object[0].led_type[counter]);

                    file.Write(xml_led_type_inv);

                    file.Write(xml_qos_level);
                    //write mqtt Qos level                    
                    file.Write(dmx_collection.data_object[0].Qos_level[counter]);

                    file.Write(xml_qos_level_inv);

                    if (!dmx_collection.data_object[0].led_type[counter].Contains("GROUP"))
                    {
                        //if channel led type is one of the below, write data
                        if (dmx_collection.data_object[0].led_type[counter] == "R" || dmx_collection.data_object[0].led_type[counter] == "RGB" || dmx_collection.data_object[0].led_type[counter] == "RGBW" || dmx_collection.data_object[0].led_type[counter] == "RGBWW")
                        {
                            file.Write(xml_colors_red_file);

                            //write red default colormorph sequence, if file is empty
                            if (dmx_collection.data_object[0].colors_name_file[counter,0] == "")
                            {
                                int counter2;
                                for (counter2 = 0; counter2 < 16; counter2++)
                                {
                                    // if single color, write simple ramp values to color map
                                    if (dmx_collection.data_object[0].led_type[counter] == "R")
                                    {
                                        if (counter2 < 15)
                                            file.Write(dmx_collection.data_object[0].colormorph_single_color[counter2] + ",");
                                        else
                                            file.Write(dmx_collection.data_object[0].colormorph_single_color[counter2]);
                                    }
                                    else
                                    {
                                        if (counter2 < 15)
                                            file.Write(dmx_collection.data_object[0].colors_red[counter2] + ",");
                                        else
                                            file.Write(dmx_collection.data_object[0].colors_red[counter2]);
                                    }
                                   
                                }
                            }
                            else
                            {   //write custom color sequence
                                int counter2;

                                for (counter2 = 0; counter2 < number_of_colors; counter2++)
                                {
                                    if (dmx_collection.data_object[0].colors_name_file[counter, counter2] != "" && counter2 < (number_of_colors - 1))
                                        file.Write(dmx_collection.data_object[0].colors_red_file[counter,counter2] + ",");
                                    else
                                    {
                                        file.Write(dmx_collection.data_object[0].colors_red_file[counter, counter2]);
                                        counter2 = number_of_colors;
                                    }
                                }
                            }
                            file.Write(xml_colors_red_file_inv);
                        }


                        //if channel led type is one of the below, write data
                        if (dmx_collection.data_object[0].led_type[counter] == "G" || dmx_collection.data_object[0].led_type[counter] == "RGB" || dmx_collection.data_object[0].led_type[counter] == "RGBW" || dmx_collection.data_object[0].led_type[counter] == "RGBWW")
                        {
                            file.Write(xml_colors_green_file);

                            //write green default colormorph sequence, if file is empty
                            if (dmx_collection.data_object[0].colors_name_file[counter,0] == "")
                            {
                                int counter2;
                                for (counter2 = 0; counter2 < 16; counter2++)
                                {
                                    // if single color, write simple ramp values to color map
                                    if (dmx_collection.data_object[0].led_type[counter] == "G")
                                    {
                                        if (counter2 < 15)
                                            file.Write(dmx_collection.data_object[0].colormorph_single_color[counter2] + ",");
                                        else
                                            file.Write(dmx_collection.data_object[0].colormorph_single_color[counter2]);
                                    }
                                    else
                                    {
                                        if (counter2 < 15)
                                            file.Write(dmx_collection.data_object[0].colors_green[counter2] + ",");
                                        else
                                            file.Write(dmx_collection.data_object[0].colors_green[counter2]);
                                    }
                                }
                            }
                            else
                            {   //write custom color sequence
                                int counter2;

                                for (counter2 = 0; counter2 < number_of_colors; counter2++)
                                {
                                    if (dmx_collection.data_object[0].colors_name_file[counter, counter2] != "" && counter2 < (number_of_colors - 1))
                                        file.Write(dmx_collection.data_object[0].colors_green_file[counter,counter2] + ",");
                                    else
                                    {
                                        file.Write(dmx_collection.data_object[0].colors_red_file[counter, counter2]);
                                        counter2 = number_of_colors;
                                    }
                                }
                            }
                            file.Write(xml_colors_green_file_inv);
                        }


                        //if channel led type is one of the below, write data
                        if (dmx_collection.data_object[0].led_type[counter] == "B" || dmx_collection.data_object[0].led_type[counter] == "RGB" || dmx_collection.data_object[0].led_type[counter] == "RGBW" || dmx_collection.data_object[0].led_type[counter] == "RGBWW")
                        {
                            file.Write(xml_colors_blue_file);

                            //write blue default colormorph sequence, if file is empty
                            if (dmx_collection.data_object[0].colors_name_file[counter,0] == "")
                            {
                                int counter2;
                                for (counter2 = 0; counter2 < 16; counter2++)
                                {   
                                    // if single color, write simple ramp values to color map
                                    if (dmx_collection.data_object[0].led_type[counter] == "B")
                                    {
                                        if (counter2 < 15)
                                            file.Write(dmx_collection.data_object[0].colormorph_single_color[counter2] + ",");
                                        else
                                            file.Write(dmx_collection.data_object[0].colormorph_single_color[counter2]);
                                    }
                                    else
                                    {
                                        if (counter2 < 15)
                                            file.Write(dmx_collection.data_object[0].colors_blue[counter2] + ",");
                                        else
                                            file.Write(dmx_collection.data_object[0].colors_blue[counter2]);
                                    }
                                }
                            }
                            else
                            {   //write custom color sequence
                                int counter2;

                                for (counter2 = 0; counter2 < number_of_colors; counter2++)
                                {
                                    if (dmx_collection.data_object[0].colors_name_file[counter, counter2] != "" && counter2 < (number_of_colors - 1))
                                        file.Write(dmx_collection.data_object[0].colors_blue_file[counter,counter2] + ",");
                                    else
                                    {
                                        file.Write(dmx_collection.data_object[0].colors_red_file[counter, counter2]);
                                        counter2 =  number_of_colors;
                                    }
                                }
                            }
                            file.Write(xml_colors_blue_file_inv);
                        }


                        //if channel led type is one of the below, write data
                        if (dmx_collection.data_object[0].led_type[counter] == "W" || dmx_collection.data_object[0].led_type[counter] == "RGBW" || dmx_collection.data_object[0].led_type[counter] == "RGBWW")
                        {
                            file.Write(xml_colors_white_file);

                            //write white default colormorph sequence, if file is empty
                            if (dmx_collection.data_object[0].colors_name_file[counter,0] == "")
                            {
                                int counter2;
                                for (counter2 = 0; counter2 < 16; counter2++)
                                {   // if single color, write simple ramp values to color map
                                    if (dmx_collection.data_object[0].led_type[counter] == "W")
                                    {
                                        if (counter2 < 15)
                                            file.Write(dmx_collection.data_object[0].colormorph_single_color[counter2] + ",");
                                        else
                                            file.Write(dmx_collection.data_object[0].colormorph_single_color[counter2]);
                                    }
                                    else
                                    {
                                        if (counter2 < 15)
                                            file.Write(dmx_collection.data_object[0].colors_white[counter2] + ",");
                                        else
                                            file.Write(dmx_collection.data_object[0].colors_white[counter2]);
                                    }
                                }
                            }
                            else
                            {   //write custom color sequence
                                int counter2;

                                for (counter2 = 0; counter2 < number_of_colors; counter2++)
                                {
                                    if (dmx_collection.data_object[0].colors_name_file[counter, counter2] != "" && counter2 < (number_of_colors - 1))
                                        file.Write(dmx_collection.data_object[0].colors_white_file[counter,counter2] + ",");
                                    else
                                    {
                                        file.Write(dmx_collection.data_object[0].colors_red_file[counter, counter2]);
                                        counter2 = number_of_colors;
                                    }
                                }
                            }
                            file.Write(xml_colors_white_file_inv);
                        }

                        //if channel led type is one of the below, write data
                        if (dmx_collection.data_object[0].led_type[counter] == "RGBWW")
                        {
                            file.Write(xml_colors_white2_file);

                            //write white default colormorph sequence, if file is empty
                            if (dmx_collection.data_object[0].colors_name_file[counter, 0] == "")
                            {
                                int counter2;
                                for (counter2 = 0; counter2 < 16; counter2++)
                                {
                                    if (counter2 < 15)
                                        file.Write(dmx_collection.data_object[0].colors_white2[counter2] + ",");
                                    else
                                        file.Write(dmx_collection.data_object[0].colors_white2[counter2]);
                                }
                            }
                            else
                            {   //write custom color sequence
                                int counter2;

                                for (counter2 = 0; counter2 < number_of_colors; counter2++)
                                {
                                    //if end of white values or max amount reached, write name without ,
                                    if (dmx_collection.data_object[0].colors_name_file[counter, counter2] != "" && counter2 < (number_of_colors - 1))
                                        file.Write(dmx_collection.data_object[0].colors_white2_file[counter, counter2] + ",");
                                    else
                                    {
                                        file.Write(dmx_collection.data_object[0].colors_red_file[counter, counter2]);
                                        counter2 = number_of_colors;
                                    }
                                }
                            }
                            file.Write(xml_colors_white2_file_inv);
                        }

                        //if channel led type is one of the below, write data
                       
                            file.Write(xml_colormorph_name_file);

                            //write white default color name sequence, if file is empty
                            if (dmx_collection.data_object[0].colors_name_file[counter, 0] == "")
                            {
                                int counter2;
                                for (counter2 = 0; counter2 < 16; counter2++)
                                {
                                switch (dmx_collection.data_object[0].led_type[counter])
                                {
                                    case "W":
                                        if (counter2 < 15)
                                            file.Write("white" + Convert.ToString(counter2) + ",");
                                        else
                                            file.Write("white" + Convert.ToString(counter2));

                                        break;
                                    case "R":
                                        if (counter2 < 15)
                                            file.Write("red" + Convert.ToString(counter2) + ",");
                                        else
                                            file.Write("red" + Convert.ToString(counter2));

                                        break;
                                    case "G":
                                        if (counter2 < 15)
                                            file.Write("green" + Convert.ToString(counter2) + ",");
                                        else
                                            file.Write("green" + Convert.ToString(counter2));

                                        break;
                                    case "B":
                                        if (counter2 < 15)
                                            file.Write("blue" + Convert.ToString(counter2) + ",");
                                        else
                                            file.Write("blue" + Convert.ToString(counter2));

                                        break;
                                    case "RGB":
                                        if (counter2 < 15)
                                            file.Write(dmx_collection.data_object[0].colors_name[counter2] + ",");
                                        else
                                            file.Write(dmx_collection.data_object[0].colors_name[counter2]);

                                        break;
                                    case "RGBW":
                                        if (counter2 < 15)
                                            file.Write(dmx_collection.data_object[0].colors_name[counter2] + ",");
                                        else
                                            file.Write(dmx_collection.data_object[0].colors_name[counter2]);

                                        break;
                                    case "RGBWW":
                                        if (counter2 < 15)
                                            file.Write(dmx_collection.data_object[0].colors_name[counter2] + ",");
                                        else
                                            file.Write(dmx_collection.data_object[0].colors_name[counter2]);

                                        break;
                                }                                                        
                                }
                            }
                            else
                            {   //write custom name sequence
                                int counter2 = 0;

                                for (counter2 = 0; counter2 < number_of_colors; counter2++)
                                {
                                //if end of names or max amount reached, write name without ,
                                if (dmx_collection.data_object[0].colors_name_file[counter, counter2] != "" && counter2 < (number_of_colors - 1))
                                    file.Write(dmx_collection.data_object[0].colors_name_file[counter, counter2] + ",");
                                else
                                {
                                    file.Write(dmx_collection.data_object[0].colors_name_file[counter, counter2]);
                                    counter2 = number_of_colors;
                                }
                            }
                            }
                            file.Write(xml_colormorph_name_file_inv);

                        //colormorph random
                        file.Write(xml_colormorph_random_color_file);
                        //write speed of effect colormorph
                        
                        file.Write(dmx_collection.data_object[0].colormorph_random_color[counter]);

                        file.Write(xml_colormorph_random_color_inv);

                        //colormorph speed
                        file.Write(xml_colormorph_speed);
                        //write speed of effect colormorph
                        if (dmx_collection.data_object[0].colormorph_speed[counter] == 0)
                            file.Write("100");
                        else
                            file.Write(dmx_collection.data_object[0].colormorph_speed[counter]);

                        file.Write(xml_colormorph_speed_inv);                        

                    }
                    file.Write("</ch" + Convert.ToString(counter) + ">\n\n");
                }

                file.Write(xml_channel_info_inv);
            }


        }

    }


}

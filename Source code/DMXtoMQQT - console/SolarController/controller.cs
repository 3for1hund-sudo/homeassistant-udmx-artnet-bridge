using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt;
using System.Threading;
using System.Web;
using System.Text.Json;
using Newtonsoft.Json;
using System.Drawing;


namespace uDMXtoMQTT
{
    public class controller
    {
        bool thread_endprocess = false;
        sol_objects dmx_collection = new sol_objects();
        MqttClient mqtt_client;
        uDMX dmx = new uDMX();

        bool pause_controller = false;
        

        public controller(dataclassgui sender)
        {
          

            dmx_collection.data_object.Add(sender);
            create_topics_for_mqtt();
            mqqt_server_connect();
            assemble_subscribe_topics();
            if (mqtt_client.IsConnected)
            {
                dmx_collection.data_object[0].textbox1_status_messages += DateTime.Now.ToString() + " Mqtt connected to server\n";
                Console.WriteLine(DateTime.Now.ToString() + " Mqtt connected to server\n");
            }
            else
            {
                Console.WriteLine(DateTime.Now.ToString() + " Mqtt is not connected\n");
                dmx_collection.data_object[0].textbox1_status_messages += DateTime.Now.ToString() + " Mqtt is not connected\n";
            }
            //write_log_to_file();
            // when topics are assembled
            // set label_full_control_topic to full control topic and set label_full_publish_topic to full publish topic
            dmx_collection.data_object[0].gui_full_control_topic = dmx_collection.data_object[0].gui_subscribe_control_topic + dmx_collection.data_object[0].gui_channel_name;
            dmx_collection.data_object[0].gui_full_publish_topic = dmx_collection.data_object[0].gui_publish_topic + dmx_collection.data_object[0].gui_channel_name;

            
        }


        public void thread_routine(object sender)
        {
            int counter = 0;
            int totalChannels = dmx_collection.data_object[0].number_of_channels;

            // DYNAMISK VARIABEL: Juster din ønskede FPS her (f.eks. 25, 30, 43)
            double targetFPS = 43.0;
            double targetFrameTimeMs = 1000.0 / targetFPS;

            // Opret et stopur til at styre den præcise timing per frame
            System.Diagnostics.Stopwatch frameTimer = new System.Diagnostics.Stopwatch();

            // Variable til FPS-beregning og visning i status-textbox
            int fpsUpdateCounter = 0;
            double accumulatedFrameTimeMs = 0;

            int effect_json_packet_timer = 0;

            // Flag til at holde øje med, om der har været USB-aktivitet i løbet af det seneste sekund
            bool lightChangedInCycle = false;

            // HARDWARE AFLASTNING: Tæller til at holde styr på antal USB-kald i træk
            int usbBurstCounter = 0;

            // set all dmx channels to 0 on start.
            if (dmx.IsOpen)
            {
                for (counter = 0; counter < 512; counter++)
                {
                    dmx.SetSingleChannel(Convert.ToInt16(counter), 0);

                    // En lille mikro-pause under selve opstarten så USB-stacken ikke stresses
                    if (counter % 32 == 0) System.Threading.Thread.Sleep(1);
                }
            }
            else
            {
                dmx.Dispose();
            }

            if (dmx.IsOpen)
            {
                Console.WriteLine(DateTime.Now.ToString() + " Udmx controller connected...\n");
                dmx_collection.data_object[0].textbox1_status_messages += DateTime.Now.ToString() + " Udmx controller connected...\n";
            }
            else
            {
                Console.WriteLine(DateTime.Now.ToString() + " Udmx controller not found\n");
                dmx_collection.data_object[0].textbox1_status_messages += DateTime.Now.ToString() + " Udmx controller not found\n";
            }
            // the led controller process, it perform scene functions, set dmx board channels. base on mqtt packets JSON recived.
            while (thread_endprocess == false)
            {
                // Opdater frame-tiden dynamisk i tilfælde af, at targetFPS ændres udefra under kørsel
                targetFrameTimeMs = 1000.0 / targetFPS;

                // DYNAMISK TIMING: Start stopuret i begyndelsen af hver frame-gennemgang
                frameTimer.Restart();

                // Nulstil burst-tælleren før vi scanner kanalerne i denne nye frame
                usbBurstCounter = 0;

                // scan all dmx channels (Håndterer dynamisk alt fra 1 til 512 kanaler)
                for (counter = 0; counter < totalChannels; counter++)
                {
                    // used to pause controller, when mqtt values are being recieved
                    while (pause_controller == true)
                    {
                        System.Threading.Thread.Sleep(5); // Forhindrer 100% CPU crash under MQTT modtagelse
                    }

                    switch (dmx_collection.data_object[0].effect[counter])
                    {
                        case "none":
                            break;

                        case "fade":
                            effect_fade(counter);
                            break;

                        case "pulse":
                            pulse(counter);
                            break;

                        case "cmorph":
                            color_morph(counter);
                            break;
                    }

                    // Hent burst-størrelse ud fra dit led_type array styret af counteren
                    int maxUsbBurst = 1; // Standard fallback (w, r, g, b)

                    if ((dmx_collection.data_object[0].led_type[counter] != null) && (!dmx_collection.data_object[0].led_type[counter].Contains("GROUP")))
                    {
                        string currentLedType = dmx_collection.data_object[0].led_type[counter].ToLower().Trim();

                        switch (currentLedType)
                        {
                            case "rgb": maxUsbBurst = 3; break;
                            case "rgbw": maxUsbBurst = 4; break;
                            case "rgbww": maxUsbBurst = 5; break;
                            default: maxUsbBurst = 1; break; // Enkeltkanaler tager 1
                        }
                    }

                    // if led current target value, differs from dmx channel set value
                    if (dmx_collection.data_object[0].current_led_value[counter] != dmx_collection.data_object[0].current_led_value_new_target[counter])
                    {
                        // if led state is on
                        if (dmx_collection.data_object[0].controller_state[counter] == true)
                        {
                            if (dmx.IsOpen)
                            {
                                // set dmx channel to new led target
                                dmx.SetSingleChannel(Convert.ToInt16(counter), dmx_collection.data_object[0].current_led_value_new_target[counter]);
                                // set current set value to new led target value.
                                dmx_collection.data_object[0].current_led_value[counter] = dmx_collection.data_object[0].current_led_value_new_target[counter];

                                // Marker at lyset har ændret sig i denne frame
                                lightChangedInCycle = true;

                                // DYNAMISK BURST-PAUSE BASERET PÅ AKTUEL KANAL (Bruger SpinWait for at undgå OS-timer lag):
                                usbBurstCounter++;
                                if (usbBurstCounter >= maxUsbBurst)
                                {
                                    System.Threading.Thread.SpinWait(400);
                                    usbBurstCounter = 0;
                                }
                            }
                            else
                            {
                                Console.WriteLine(DateTime.Now.ToString() + " Udmx controller not found\n");
                                dmx_collection.data_object[0].textbox1_status_messages += DateTime.Now.ToString() + " Udmx controller not found\n";
                                dmx.Dispose();
                            }
                        }
                    }

                    // if controller is turned off, set selected channels to 0. 
                    if (dmx_collection.data_object[0].controller_state[counter] == false && dmx_collection.data_object[0].current_led_value[counter] > 0)
                    {
                        if (dmx.IsOpen)
                        {
                            dmx.SetSingleChannel(Convert.ToInt16(counter), 0);
                            // Sættes til 0 med det samme, så denne if-sætning kun udløses én gang pr. slukning
                            dmx_collection.data_object[0].current_led_value[counter] = 0;

                            // Marker at lyset har ændret sig i denne frame (sluk-sekvens)
                            lightChangedInCycle = true;

                            // DYNAMISK BURST-PAUSE VED SLUKNING:
                            usbBurstCounter++;
                            if (usbBurstCounter >= maxUsbBurst)
                            {
                                System.Threading.Thread.SpinWait(400);
                                usbBurstCounter = 0;
                            }
                        }
                        else
                        {
                            Console.WriteLine(DateTime.Now.ToString() + " Udmx controller not found\n");
                            dmx_collection.data_object[0].textbox1_status_messages += DateTime.Now.ToString() + " Udmx controller not found\n";
                            //write_log_to_file();
                            dmx.Dispose();
                        }
                    }
                }

                dmx_collection.data_object[0].controller_tic_counter++;
                if (dmx_collection.data_object[0].controller_tic_counter >= 36000)
                {
                    dmx_collection.data_object[0].controller_tic_counter = 0;
                }

                // ==================== START PÅ NY TIMING-MOTOR ====================

                // 1. Mål den REELLE tid det tog at afvikle dine effekter og sende USB (INDEN pausen)
                double pureEffectTimeMs = frameTimer.Elapsed.TotalMilliseconds;

                // Læg den RÅ effekt-tid i din akkumulator (så matematikken ikke snydes af ventepausen)
                accumulatedFrameTimeMs += pureEffectTimeMs;

                // Beregn hvor meget tid der er tilbage, før vi skal starte næste frame
                double sleepTimeMs = targetFrameTimeMs - pureEffectTimeMs;

                // PRÆCIS TIMING MED LAV CPU-BELASTNING (Giver 1-3% CPU i idle)
                if (sleepTimeMs > 0 && sleepTimeMs < 1000.0)
                {
                    // HYBRID-LØSNING: Hvis vi har god tid (mere end 5 ms), så læg tråden helt til at sove.
                    // Dette frigiver CPU'en fuldstændigt og bringer din Idle CPU i bund!
                    if (sleepTimeMs > 5.0)
                    {
                        System.Threading.Thread.Sleep(Convert.ToInt32(sleepTimeMs - 2.0));
                    }

                    // Brug kun SpinWait til de sidste 2 millisekunder for at sikre de præcise 43 FPS
                    while (frameTimer.Elapsed.TotalMilliseconds < targetFrameTimeMs)
                    {
                        System.Threading.Thread.SpinWait(10);
                    }
                }
                else
                {
                    System.Threading.Thread.Sleep(0);
                }

                // 3. Stop uret helt til sidst efter timingen er overholdt
                frameTimer.Stop();
                fpsUpdateCounter++;
                effect_json_packet_timer++;

                // 4. DIN RECONVERTEDE FPS-LOGNINGS BLOK:
                if (fpsUpdateCounter >= Convert.ToInt32(targetFPS))
                {
                    if (lightChangedInCycle)
                    {
                        // Gennemsnitlig tid per frame baseret på den RÅ afviklingstid
                        double averageFrameTimeMs = accumulatedFrameTimeMs / fpsUpdateCounter;
                        double liveFPS = averageFrameTimeMs > 0 ? (1000.0 / averageFrameTimeMs) : 0;

                        // Sikrer at loggen ikke runder skævt op, hvis koden kørte ekstremt hurtigt
                        if (liveFPS > targetFPS) liveFPS = targetFPS;
                        Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} -> Rendering Effects: {liveFPS:F1} FPS\n");
                        dmx_collection.data_object[0].textbox1_status_messages += $"{DateTime.Now.ToString("HH:mm:ss")} -> Rendering Effects: {liveFPS:F1} FPS\n";
                        lightChangedInCycle = false;
                    }

                    fpsUpdateCounter = 0;
                    accumulatedFrameTimeMs = 0;
                }

                // ==================== SLUT PÅ NY TIMING-MOTOR ====================
            }
        }













        public void pulse(int counter)
        {
            if(!dmx_collection.data_object[0].led_type[counter].Contains("GROUP"))
            {
                Random number = new Random();

                if ((dmx_collection.data_object[0].controller_tic_counter >= dmx_collection.data_object[0].controller_tic_start[counter]) && (dmx_collection.data_object[0].brightness_controller[counter] != 10))
                {
                    dmx_collection.data_object[0].brightness_controller[counter] = 10;
                    brightness_adjust(counter,0);
                    dmx_collection.data_object[0].controller_tic_start[counter] = number.Next(10) + dmx_collection.data_object[0].controller_tic_counter;

                    if (dmx_collection.data_object[0].controller_tic_start[counter] > 36000)
                        dmx_collection.data_object[0].controller_tic_start[counter] = 36000;

                    

                }

                if ((dmx_collection.data_object[0].controller_tic_counter >= dmx_collection.data_object[0].controller_tic_start[counter]) && (dmx_collection.data_object[0].brightness_controller[counter] == 10))
                {
                    byte random_brightness = Convert.ToByte(number.Next(10,255));
                    
                    dmx_collection.data_object[0].brightness_controller[counter] = random_brightness;
                    brightness_adjust(counter,0);
                    dmx_collection.data_object[0].controller_tic_start[counter] = number.Next(10) + dmx_collection.data_object[0].controller_tic_counter;

                    if (dmx_collection.data_object[0].controller_tic_start[counter] > 36000)
                        dmx_collection.data_object[0].controller_tic_start[counter] = 36000;
                   

                }
            }
        }


        // morph between colors, user can chose color selection and time between fades
        public void color_morph(int counter)
        {
            Random number = new Random();
            bool fade_complete_check = true;
            int random_color = 0;

            //only execute if first channel of RGB led
            if (!dmx_collection.data_object[0].led_type[counter].Contains("GROUP"))
            {
                // checks if fade is done
                switch(dmx_collection.data_object[0].led_type[counter])
                {
                    case "W":
                        if (dmx_collection.data_object[0].current_led_value_new_target[counter] != dmx_collection.data_object[0].fade_target[counter])
                            fade_complete_check = false;
                        break;

                    case "R":
                        if (dmx_collection.data_object[0].current_led_value_new_target[counter] != dmx_collection.data_object[0].fade_target[counter])
                            fade_complete_check = false;
                        break;
                    case "G":
                        if (dmx_collection.data_object[0].current_led_value_new_target[counter] != dmx_collection.data_object[0].fade_target[counter])
                            fade_complete_check = false;
                        break;
                    case "B":
                        if (dmx_collection.data_object[0].current_led_value_new_target[counter] != dmx_collection.data_object[0].fade_target[counter])
                            fade_complete_check = false;
                        break;
                    case "RGB":
                        if (dmx_collection.data_object[0].current_led_value_new_target[counter] != dmx_collection.data_object[0].fade_target[counter])
                            fade_complete_check = false;
                        if (dmx_collection.data_object[0].current_led_value_new_target[counter+1] != dmx_collection.data_object[0].fade_target[counter+1])
                            fade_complete_check = false;
                        if (dmx_collection.data_object[0].current_led_value_new_target[counter+2] != dmx_collection.data_object[0].fade_target[counter+2])
                            fade_complete_check = false;
                        break;
                    case "RGBW":
                        if (dmx_collection.data_object[0].current_led_value_new_target[counter] != dmx_collection.data_object[0].fade_target[counter])
                            fade_complete_check = false;
                        if (dmx_collection.data_object[0].current_led_value_new_target[counter + 1] != dmx_collection.data_object[0].fade_target[counter + 1])
                            fade_complete_check = false;
                        if (dmx_collection.data_object[0].current_led_value_new_target[counter + 2] != dmx_collection.data_object[0].fade_target[counter + 2])
                            fade_complete_check = false;
                        if (dmx_collection.data_object[0].current_led_value_new_target[counter + 3] != dmx_collection.data_object[0].fade_target[counter + 3])
                            fade_complete_check = false;
                        break;
                    case "RGBWW":
                        if (dmx_collection.data_object[0].current_led_value_new_target[counter] != dmx_collection.data_object[0].fade_target[counter])
                            fade_complete_check = false;
                        if (dmx_collection.data_object[0].current_led_value_new_target[counter + 1] != dmx_collection.data_object[0].fade_target[counter + 1])
                            fade_complete_check = false;
                        if (dmx_collection.data_object[0].current_led_value_new_target[counter + 2] != dmx_collection.data_object[0].fade_target[counter + 2])
                            fade_complete_check = false;
                        if (dmx_collection.data_object[0].current_led_value_new_target[counter + 3] != dmx_collection.data_object[0].fade_target[counter + 3])
                            fade_complete_check = false;
                        if (dmx_collection.data_object[0].current_led_value_new_target[counter + 4] != dmx_collection.data_object[0].fade_target[counter + 3])
                            fade_complete_check = false;
                        break;
                }

                // if last fade target, had been reached, then proceed
                if (fade_complete_check == true)
                {
                    

                    // controller tic has reached, set time for next colorchange
                    if (dmx_collection.data_object[0].controller_tic_counter >= dmx_collection.data_object[0].tics_to_next_pulse[counter])
                    {

                        

                        // reset json return packet routine, so it returns feedback to homeassist. every time color changes.
                        dmx_collection.data_object[0].led_change_in_progress[counter] = true;

                        if (dmx_collection.data_object[0].colormorph_random_color[counter] == true)
                        {
                            // generate random number, to select next color
                            
                            random_color = number.Next(0, 15);
                            // if random color is the same as previous color, try again
                            while (dmx_collection.data_object[0].last_random_color[counter] == random_color)
                                random_color = number.Next(0, 15);

                            // save color, for next round
                            dmx_collection.data_object[0].last_random_color[counter] = random_color;
                        }
                        else
                        {   //runs through color list, sequential. starts from beginning when list ends
                            int counter3 = 0;
                            // checks length of current color list.
                            while(dmx_collection.data_object[0].colors_name_file[counter, counter3] != "" && counter3 < 15)
                            {
                                counter3++;
                            }
                            // increase sequence counter by 1
                            random_color = dmx_collection.data_object[0].last_random_color[counter] + 1;

                            //if larger than list length, then reset to 0, else save color number, for next run
                            if (random_color > counter3)
                            {
                                random_color = 0;
                                dmx_collection.data_object[0].last_random_color[counter] = 0;
                            }
                            else
                            {
                                dmx_collection.data_object[0].last_random_color[counter] = random_color;
                            }
                        }
                        
                        // sets next colors, controller should fade to, decided by led type                  
                        switch (dmx_collection.data_object[0].led_type[counter])
                        {
                            case "W":

                                dmx_collection.data_object[0].fade_target[counter] = dmx_collection.data_object[0].colors_white_file[counter, random_color];
                                break;

                            case "R":
                                dmx_collection.data_object[0].fade_target[counter] = dmx_collection.data_object[0].colors_red_file[counter, random_color];
                                break;
                            case "G":
                                dmx_collection.data_object[0].fade_target[counter] = dmx_collection.data_object[0].colors_green_file[counter, random_color];
                                break;
                            case "B":
                                dmx_collection.data_object[0].fade_target[counter] = dmx_collection.data_object[0].colors_blue_file[counter, random_color];
                                break;
                            case "RGB":
                                dmx_collection.data_object[0].fade_target[counter] = dmx_collection.data_object[0].colors_red_file[counter, random_color];
                                dmx_collection.data_object[0].fade_target[counter + 1] = dmx_collection.data_object[0].colors_green_file[counter, random_color];
                                dmx_collection.data_object[0].fade_target[counter + 2] = dmx_collection.data_object[0].colors_blue_file[counter, random_color];
                                break;
                            case "RGBW":
                                dmx_collection.data_object[0].fade_target[counter] = dmx_collection.data_object[0].colors_red_file[counter, random_color];
                                dmx_collection.data_object[0].fade_target[counter + 1] = dmx_collection.data_object[0].colors_green_file[counter, random_color];
                                dmx_collection.data_object[0].fade_target[counter + 2] = dmx_collection.data_object[0].colors_blue_file[counter, random_color];
                                dmx_collection.data_object[0].fade_target[counter + 3] = dmx_collection.data_object[0].colors_white_file[counter, random_color];
                                break;

                            case "RGBWW":
                                dmx_collection.data_object[0].fade_target[counter] = dmx_collection.data_object[0].colors_red_file[counter, random_color];
                                dmx_collection.data_object[0].fade_target[counter + 1] = dmx_collection.data_object[0].colors_green_file[counter, random_color];
                                dmx_collection.data_object[0].fade_target[counter + 2] = dmx_collection.data_object[0].colors_blue_file[counter, random_color];
                                dmx_collection.data_object[0].fade_target[counter + 3] = dmx_collection.data_object[0].colors_white_file[counter, random_color];
                                dmx_collection.data_object[0].fade_target[counter + 4] = dmx_collection.data_object[0].colors_white2_file[counter, random_color];
                                break;
                        }
                        if(dmx_collection.data_object[0].brightness_controller[counter] < 255)
                        brightness_adjust_colormorph(counter);

                        // creates the next tic/time to morph, to next color. if time is above reset of controller tics, set start to 0 + time
                        int sum_temp = dmx_collection.data_object[0].controller_tic_counter + dmx_collection.data_object[0].colormorph_speed[counter];
                        if (sum_temp <= 35974)
                            dmx_collection.data_object[0].tics_to_next_pulse[counter] = sum_temp;
                        else
                            dmx_collection.data_object[0].tics_to_next_pulse[counter] = dmx_collection.data_object[0].colormorph_speed[counter];

                        


                    }
                }


            }

            // When brightness is adjusted below 50%, fade time between colors, are noticeble faster. So to minimize this, stepsize are reduced to half, so it takes longer to fade.

            // save normale fade stepsize.
            int temp_fade_stepsize = dmx_collection.data_object[0].fade_stepsize;

            // if above 128 50%. set fade stepsize to normale colormorph steps
            if (dmx_collection.data_object[0].brightness_whitemax_mixed_rgb[counter] > 128)
                dmx_collection.data_object[0].fade_stepsize = dmx_collection.data_object[0].colormorph_fade_stepsize;
            else
                dmx_collection.data_object[0].fade_stepsize = dmx_collection.data_object[0].colormorph_fade_stepsize / 3;

            //if division give 0
            if (dmx_collection.data_object[0].fade_stepsize == 0)
                dmx_collection.data_object[0].fade_stepsize = 1;

                //fade to set color                 
                effect_fade(counter);

            dmx_collection.data_object[0].fade_stepsize = temp_fade_stepsize;
        }

        // every controller round, if effect "fade" is set on channel.
        // increase or decreases toward target, adjusted in small steps. done by manipulating "current_led_value_new_target", which controller code always set controller output to, when channel is scanned.
        public void effect_fade(int counter)
        {
            
            int data_temp = 0;

            // if fade target is larger than current led setting
            if (dmx_collection.data_object[0].fade_target[counter] > dmx_collection.data_object[0].current_led_value[counter])
            {
                //if fade target - current controller output setting, is larger than step size. 
                if (dmx_collection.data_object[0].fade_target[counter] - dmx_collection.data_object[0].current_led_value[counter] > dmx_collection.data_object[0].fade_stepsize)
                {
                    // set current_led_value_new_target to current controlelr value + step size.
                    data_temp = dmx_collection.data_object[0].current_led_value[counter] + dmx_collection.data_object[0].fade_stepsize;
                    dmx_collection.data_object[0].current_led_value_new_target[counter] = Convert.ToByte(data_temp);
                }
                else // is step size + current led settings is larger than target. just make fade target and new led target, the same.
                    dmx_collection.data_object[0].current_led_value_new_target[counter] = dmx_collection.data_object[0].fade_target[counter];

                
            }
            // if fade target is smaller than current led setting
            if (dmx_collection.data_object[0].fade_target[counter] <= dmx_collection.data_object[0].current_led_value[counter])
            {

                if (dmx_collection.data_object[0].current_led_value[counter] - dmx_collection.data_object[0].fade_target[counter] > dmx_collection.data_object[0].fade_stepsize)
                {
                    data_temp = dmx_collection.data_object[0].current_led_value[counter] - dmx_collection.data_object[0].fade_stepsize;
                    dmx_collection.data_object[0].current_led_value_new_target[counter] = Convert.ToByte(data_temp);
                }
                else
                    dmx_collection.data_object[0].current_led_value_new_target[counter] = dmx_collection.data_object[0].fade_target[counter];
            }

            // when fade is completed, make effect trigger null, fade is only run once.
            if (dmx_collection.data_object[0].fade_target[counter] == dmx_collection.data_object[0].current_led_value[counter] && dmx_collection.data_object[0].effect[counter] == "fade")
                dmx_collection.data_object[0].effect[counter] = "";

        }

        public void assemble_subscribe_topics()
        {
            int counter = 0;
            // assemble control topics, with correct array length
            // assemble Qos level, with correct array length
            byte[] assembled_qos_level_temp = new byte[dmx_collection.data_object[0].Number_of_assembled_topics];
            string[] assembled_control_topics_temp = new string[dmx_collection.data_object[0].Number_of_assembled_topics];
            for (counter = 0; counter < dmx_collection.data_object[0].Number_of_assembled_topics; counter++)
            {
                assembled_qos_level_temp[counter] = dmx_collection.data_object[0].assembled_qos_level[counter];
                assembled_control_topics_temp[counter] = dmx_collection.data_object[0].assembled_control_topic[counter];
                
            }
            // subscribe control topics, with server
            mqtt_client.Subscribe(assembled_control_topics_temp, assembled_qos_level_temp);
            json_autodiscovery_packet(0);
        }

        public void mqqt_server_connect()
        {
            
            // create new mqqt client
            mqtt_client = new MqttClient(dmx_collection.data_object[0].mqqt_server_ip, Convert.ToInt32(dmx_collection.data_object[0].mqqt_server_port), false, null, null, MqttSslProtocols.None);

            // assign event, when json packet recieved
            mqtt_client.MqttMsgPublishReceived += Mqtt_client_MqttMsgPublishReceived;
            // connect mqtt client to mqqt server
                    
            mqtt_client.Connect(dmx_collection.data_object[0].mqqt_device_id, dmx_collection.data_object[0].mqqt_server_user, dmx_collection.data_object[0].mqqt_server_password);
             
        }


        // when control topic message event happens, it comes here
        private void Mqtt_client_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {      

            //convert byte message telegram recieved to string
            string mqtt_message = ASCIIEncoding.ASCII.GetString(e.Message);

            string mqtt_topic = e.Topic;
            Console.WriteLine(DateTime.Now.ToString() + " User set channel: " + mqtt_topic + " to: " + mqtt_message + "\n");
            dmx_collection.data_object[0].textbox1_status_messages += DateTime.Now.ToString() + " User set channel: " + mqtt_topic + " to: " + mqtt_message + "\n";
            //write_log_to_file();
            decode_json_packet(mqtt_topic, mqtt_message);
            
        }

        // decode recieved command on control topic
        public void decode_json_packet(string mqtt_topic, string mqtt_message)
        {
            string[] split_array = new string[100];
            string[] split_array2 = new string[100];
            int index_pos = 0;
            string effect = "";
            string state = "";
            string red = "";
            string blue = "";
            string green = "";
            string warm_white = "";
            string cold_white = "";
            int topic_index = 0; // contains real channel number of first led, asosiated with recieved topic
            int topic_array_pos = 0; // contains array place, of topic packet, being processed
            int counter = 0;
            string temp = "";

            // stop controller , while updating
            pause_controller = true;

            // control topic, matches subscribes topics, then decode packet
            if (dmx_collection.data_object[0].assembled_control_topic.Contains(mqtt_topic))
            {
                for (counter = 0; counter < dmx_collection.data_object[0].number_of_channels; counter++)
                {
                    if (dmx_collection.data_object[0].assembled_control_topic[counter] == mqtt_topic)
                    {
                        topic_index = dmx_collection.data_object[0].ch_numbers_assembled_topics[counter];
                        topic_array_pos = counter;
                    }
                        
                }

                split_array = mqtt_message.Split("color");                


                // seperate controller state, from json packet
                if (mqtt_message.Contains("state"))
                {
                   
                    index_pos = mqtt_message.IndexOf("state");

                    state = mqtt_message.Substring(index_pos + 8, 3);

                    // if state changes to ON, set individual flags, to true. This makes the controller turn on the channels, assosiated with the led band. it rams the channels to last set value.
                    if (state.Contains("ON"))
                    {
                        if (dmx_collection.data_object[0].led_type[topic_index].Contains("R"))
                            dmx_collection.data_object[0].controller_state[topic_index] = true;
                        if (dmx_collection.data_object[0].led_type[topic_index].Contains("G"))
                            dmx_collection.data_object[0].controller_state[topic_index + 1] = true;
                        if (dmx_collection.data_object[0].led_type[topic_index].Contains("B"))
                            dmx_collection.data_object[0].controller_state[topic_index + 2] = true;
                        if (dmx_collection.data_object[0].led_type[topic_index].Contains("W"))
                            dmx_collection.data_object[0].controller_state[topic_index + 3] = true;
                        if (dmx_collection.data_object[0].led_type[topic_index].Contains("w"))
                            dmx_collection.data_object[0].controller_state[topic_index + 4] = true;

                    }

                    // sets channel flag ON/OFF to false, this makes the controller code, ramp down the channels to 0
                    if (state.Contains("OFF"))
                    {
                        if (dmx_collection.data_object[0].led_type[topic_index].Contains("R"))
                            dmx_collection.data_object[0].controller_state[topic_index] = false;
                        if (dmx_collection.data_object[0].led_type[topic_index].Contains("G"))
                            dmx_collection.data_object[0].controller_state[topic_index + 1] = false;
                        if (dmx_collection.data_object[0].led_type[topic_index].Contains("B"))
                            dmx_collection.data_object[0].controller_state[topic_index + 2] = false;
                        if (dmx_collection.data_object[0].led_type[topic_index].Contains("W"))
                            dmx_collection.data_object[0].controller_state[topic_index + 3] = false;
                        if (dmx_collection.data_object[0].led_type[topic_index].Contains("w"))
                            dmx_collection.data_object[0].controller_state[topic_index + 4] = false;
                    }
                }

                // if message contains color info, extract color values
                if (mqtt_message.Contains("color"))
                {

                    split_array = mqtt_message.Split("color\":{\"");

                    // if red is present in color message
                    if (split_array[1].Contains("r"))
                    {
                        split_array2 = split_array[1].Split("r\":");

                        //loop data into red, if char is a number
                        for (index_pos = 0; index_pos < 3; index_pos++)
                        {
                            if (char.IsNumber(split_array2[1][index_pos]))
                                red += split_array2[1][index_pos];
                        }
                        //if group is detected, there is an error in config file, dont change
                        if (!dmx_collection.data_object[0].led_type[topic_index].Contains("GROUP"))
                        {
                            if (dmx_collection.data_object[0].led_type[topic_index].Contains("R"))
                            {
                                dmx_collection.data_object[0].current_led_value_new_target[topic_index] = Convert.ToByte(red);
                            }
                        }
                    }

                    // if green is present in color message
                    if (split_array[1].Contains("g"))
                    {
                        split_array2 = split_array[1].Split("g\":");

                        //loop data into green, if char is a number
                        for (index_pos = 0; index_pos < 3; index_pos++)
                        {
                            if (char.IsNumber(split_array2[1][index_pos]))
                                green += split_array2[1][index_pos];
                        }

                        //if group is detected, there is an error in config file, dont change
                        if (!dmx_collection.data_object[0].led_type[topic_index].Contains("GROUP"))
                        {
                            if (dmx_collection.data_object[0].led_type[topic_index].Contains("G"))
                            {
                                dmx_collection.data_object[0].current_led_value_new_target[topic_index + 1] = Convert.ToByte(green);
                            }
                        }
                    }
                    // if blue is present in color message
                    if (split_array[1].Contains("b"))
                    {
                        split_array2 = split_array[1].Split("b\":");

                        //loop data into blue, if char is a number
                        for (index_pos = 0; index_pos < 3; index_pos++)
                        {
                            if (char.IsNumber(split_array2[1][index_pos]))
                                blue += split_array2[1][index_pos];
                        }

                        //if group is detected, there is an error in config file, dont change
                        if (!dmx_collection.data_object[0].led_type[topic_index].Contains("GROUP"))
                        {
                            if (dmx_collection.data_object[0].led_type[topic_index].Contains("B"))
                            {
                                dmx_collection.data_object[0].current_led_value_new_target[topic_index + 2] = Convert.ToByte(blue);
                            }
                        }
                    }

                    // if warm white is present in color message
                    if (split_array[1].Contains("w"))
                    {
                        split_array2 = split_array[1].Split("w\":");

                        //loop data into green, if char is a number
                        for (index_pos = 0; index_pos < 3; index_pos++)
                        {
                            if (char.IsNumber(split_array2[1][index_pos]))
                                warm_white += split_array2[1][index_pos];
                        }

                        //if group is detected, there is an error in config file, dont change
                        if (!dmx_collection.data_object[0].led_type[topic_index].Contains("GROUP"))
                        {
                            if (dmx_collection.data_object[0].led_type[topic_index].Contains("W"))
                            {
                                dmx_collection.data_object[0].current_led_value_new_target[topic_index + 3] = Convert.ToByte(warm_white);
                                
                                //store a copy of original value, for brightness adjust
                                dmx_collection.data_object[0].brightness_whitemax_mixed_rgb[topic_index + 3] = Convert.ToByte(warm_white); 
                            }
                        }
                    }
                    // if cold white is present in color message
                    if (split_array[1].Contains("W"))
                    {
                        split_array2 = split_array[1].Split("W\":");

                        //loop data into cold, if char is a number
                        for (index_pos = 0; index_pos < 3; index_pos++)
                        {
                            if (char.IsNumber(split_array2[1][index_pos]))
                                cold_white += split_array2[1][index_pos];
                        }
                        //if group is detected, there is an error in config file, dont change
                        if (!dmx_collection.data_object[0].led_type[topic_index].Contains("GROUP"))
                        {
                            if (dmx_collection.data_object[0].led_type[topic_index].Contains("w"))
                            {
                                dmx_collection.data_object[0].current_led_value_new_target[topic_index + 4] = Convert.ToByte(cold_white);
                                //store a copy of original value, for brightness adjust
                                dmx_collection.data_object[0].brightness_whitemax_mixed_rgb[topic_index + 4] = Convert.ToByte(cold_white);
                            }
                        }
                    }

                    // when color is detected, turn brightness off
                    //dmx_collection.data_object[0].brightness_controller[topic_index] = 0;
                }

                //    "brightness":
                if (mqtt_message.Contains("brightness"))
                {
                    split_array = mqtt_message.Split("\"brightness\":");
                    temp = "";

                    //loop data into cold, if char is a number
                    for (index_pos = 0; index_pos < split_array[1].Length; index_pos++)
                    {
                        if (char.IsNumber(split_array[1][index_pos]))
                           temp += split_array[1][index_pos];
                    }
                    dmx_collection.data_object[0].brightness_controller[topic_index] = Convert.ToByte(temp);
                    brightness_adjust(topic_index,0);                

                }
          
                if(mqtt_message.Contains("effect"))
                {
                    split_array = mqtt_message.Split("\"effect\":\"");
                    split_array2 = split_array[1].Split("\"");
                    
                    // loop effect type into effect array. which sets controller code, to execute light with effect.
                    // Effect "none", will disable other effects.
                    switch(dmx_collection.data_object[0].led_type[topic_index])
                    {

                        case "W":
                            dmx_collection.data_object[0].effect[topic_index] = split_array2[0];

                            if(mqtt_message.Contains("color"))
                                  dmx_collection.data_object[0].fade_target[topic_index] = Convert.ToByte(warm_white);
                            break;

                        case "R":
                            dmx_collection.data_object[0].effect[topic_index] = split_array2[0];

                            if (mqtt_message.Contains("color"))
                                dmx_collection.data_object[0].fade_target[topic_index] = Convert.ToByte(red);
                            break;

                        case "G":
                            dmx_collection.data_object[0].effect[topic_index] = split_array2[0];

                            if (mqtt_message.Contains("color"))
                                dmx_collection.data_object[0].fade_target[topic_index] = Convert.ToByte(green); ;
                            break;
                        
                        case "B":
                            dmx_collection.data_object[0].effect[topic_index] = split_array2[0];

                            if (mqtt_message.Contains("color"))
                                dmx_collection.data_object[0].fade_target[topic_index] = Convert.ToByte(blue); 
                            break;
                        
                        case "RGB":
                            dmx_collection.data_object[0].effect[topic_index] = split_array2[0];
                            dmx_collection.data_object[0].effect[topic_index+1] = split_array2[0];
                            dmx_collection.data_object[0].effect[topic_index+2] = split_array2[0];

                            if (mqtt_message.Contains("color"))
                            {
                                dmx_collection.data_object[0].fade_target[topic_index] = Convert.ToByte(red);
                                dmx_collection.data_object[0].fade_target[topic_index + 1] = Convert.ToByte(green);
                                dmx_collection.data_object[0].fade_target[topic_index + 2] = Convert.ToByte(blue);
                            }
                            break;

                        case "RGBW":
                            dmx_collection.data_object[0].effect[topic_index] = split_array2[0];
                            dmx_collection.data_object[0].effect[topic_index+1] = split_array2[0];
                            dmx_collection.data_object[0].effect[topic_index+2] = split_array2[0];
                            dmx_collection.data_object[0].effect[topic_index+3] = split_array2[0];

                            if (mqtt_message.Contains("color"))
                            {
                                dmx_collection.data_object[0].fade_target[topic_index] = Convert.ToByte(red);
                                dmx_collection.data_object[0].fade_target[topic_index + 1] = Convert.ToByte(green);
                                dmx_collection.data_object[0].fade_target[topic_index + 2] = Convert.ToByte(blue);
                                dmx_collection.data_object[0].fade_target[topic_index + 3] = Convert.ToByte(warm_white);
                            }
                            break;

                        case "RGBWW":
                            dmx_collection.data_object[0].effect[topic_index] = split_array2[0];
                            dmx_collection.data_object[0].effect[topic_index+1] = split_array2[0];
                            dmx_collection.data_object[0].effect[topic_index+2] = split_array2[0];
                            dmx_collection.data_object[0].effect[topic_index+3] = split_array2[0];

                            if (mqtt_message.Contains("color"))
                            {
                                dmx_collection.data_object[0].fade_target[topic_index] = Convert.ToByte(red);
                                dmx_collection.data_object[0].fade_target[topic_index + 1] = Convert.ToByte(green);
                                dmx_collection.data_object[0].fade_target[topic_index + 2] = Convert.ToByte(blue);
                                dmx_collection.data_object[0].fade_target[topic_index + 3] = Convert.ToByte(warm_white);
                                dmx_collection.data_object[0].fade_target[topic_index + 3] = Convert.ToByte(cold_white);
                            }
                            break;
                    }
                    
                        switch (dmx_collection.data_object[0].effect[topic_index])
                        {
                            case "none":
                            dmx_collection.data_object[0].led_change_in_progress[topic_index] = true;
                            dmx_collection.data_object[0].brightness_controller[topic_index] = 255;
                            // return Json packet on publish topic, to confirm controller has set command.
                            json_return_packet_creation(topic_index, true);

                            break;
                            

                            case "fade":
                                dmx_collection.data_object[0].led_change_in_progress[topic_index] = true;
                                // return Json packet on publish topic, to confirm controller has set command.
                                json_return_packet_creation(topic_index, true); 
                                break;

                            case "pulse":

                            json_return_packet_creation(topic_index, true);
                            Random number = new Random();

                                dmx_collection.data_object[0].brightness_controller[topic_index] = 50;
                                brightness_adjust(counter, 0);
                                dmx_collection.data_object[0].controller_tic_start[topic_index] = number.Next(10) + dmx_collection.data_object[0].controller_tic_counter;
                                if (dmx_collection.data_object[0].controller_tic_start[topic_index] > 36000)
                                    dmx_collection.data_object[0].controller_tic_start[topic_index] = 36000;

                           

                            break;

                            case "cmorph":

                                json_return_packet_creation(topic_index, true);
                                //if brightness is 0, no info has been recieved. set to 100% per default
                                if (dmx_collection.data_object[0].brightness_controller[topic_index] == 0)
                                {
                                    dmx_collection.data_object[0].brightness_controller[topic_index] = 255;
                                }
                                // set first controllertic/time, color should be changed
                                int sum_temp = dmx_collection.data_object[0].controller_tic_counter + dmx_collection.data_object[0].colormorph_speed[topic_index];
                                if (sum_temp < 36000)
                                    dmx_collection.data_object[0].tics_to_next_pulse[topic_index] = sum_temp;
                                else
                                    dmx_collection.data_object[0].tics_to_next_pulse[topic_index] = dmx_collection.data_object[0].colormorph_speed[topic_index];
                                break;

                        }
                }
                
                if (!mqtt_message.Contains("effect"))
                {
                    
                    // return Json packet on publish topic, to confirm controller has set command.
                    json_return_packet_creation(topic_index, true);
                    
                }

                // return Json packet on publish topic, to confirm controller has set command.
                //mqtt_client.Publish(dmx_collection.data_object[0].assembled_publish_topic[topic_array_pos], Encoding.UTF8.GetBytes(mqtt_message));
                
                pause_controller = false;
            }


        }

        public void json_autodiscovery_packet(int counter)
        {
            for (counter = 0; counter < dmx_collection.data_object[0].Number_of_assembled_topics; counter++)
            {
                // 1. Hent rå værdier fra dit dataobjekt
                string rawName = dmx_collection.data_object[0].channel_name[dmx_collection.data_object[0].ch_numbers_assembled_topics[counter]];
                string rawType = dmx_collection.data_object[0].led_type[dmx_collection.data_object[0].ch_numbers_assembled_topics[counter]];
                string stateTopic = dmx_collection.data_object[0].assembled_publish_topic[counter];
                string commandTopic = dmx_collection.data_object[0].assembled_control_topic[counter];

                // 2. Formatér en sikker system-nøgle (Små bogstaver, ingen mellemrum)
                string safeId = rawName.Replace(" ", "_").Replace("æ", "ae").Replace("ø", "oe").Replace("å", "aa").ToLower();

                // 3. Konverter HA-farvetilstand (Skal være små bogstaver, W/Dimmer skal være "brightness")
                string colorMode = rawType.ToLower();
                if (colorMode == "w" || colorMode == "single" || colorMode == "dimmer")
                {
                    colorMode = "brightness";
                }

                // 4. Byg JSON vha. String Interpolation (Meget nemmere at læse/rette)
                string jsonPayload = $@"{{
  ""name"": ""{rawName}"",
  ""unique_id"": ""udmx_{safeId}"",
  ""state_topic"": ""{stateTopic}"",
  ""command_topic"": ""{commandTopic}"",
  ""schema"": ""json"",
  ""color_mode"": true,
  ""supported_color_modes"": [""{colorMode}""],
  ""effect"": true,
  ""effect_list"": [""none"", ""pulse"", ""colormorph"", ""fade""],
  ""device"": {{
    ""identifiers"": [""udmx_gateway_01""],
    ""name"": ""uDMX to MQTT Gateway"",
    ""model"": ""uDMX Gateway v1.1.6"",
    ""manufacturer"": ""Custom Addon""
  }}
}}";

                // 5. Generer det præcise discovery topic (Altid små bogstaver for at undgå HA-fejl)
                string discoveryTopic = $"homeassistant/light/udmx_{safeId}/config";

                // 6. Publicer som QoS 0, Retain = true
                mqtt_client.Publish(discoveryTopic, Encoding.UTF8.GetBytes(jsonPayload), 0, true);
            }
        }

        public void json_return_packet_creation(int current_channel, bool one_shot_packet)
        {
            

                    string json_packet_message = json_packet_creation(current_channel);
            //return Json packet on publish topic, to confirm controller has set command.
            Console.WriteLine(DateTime.Now.ToString() + " Return JSON topic: " + dmx_collection.data_object[0].publish_topic[current_channel] + dmx_collection.data_object[0].channel_name[current_channel] + " to: " + json_packet_message + "\n");
                    dmx_collection.data_object[0].textbox1_status_messages += DateTime.Now.ToString() + " Return JSON topic: " + dmx_collection.data_object[0].publish_topic[current_channel] + dmx_collection.data_object[0].channel_name[current_channel] + " to: " + json_packet_message + "\n";
                    //write_log_to_file();
                     mqtt_client.Publish(dmx_collection.data_object[0].publish_topic[current_channel] + dmx_collection.data_object[0].channel_name[current_channel], Encoding.UTF8.GetBytes(json_packet_message));
                
            

        }


       string json_packet_creation(int current_channel)
        {
            string json_packet_message = "";

            //{"state":"ON","color":{"r":255,"g":185,"b":60,"w":255},"effect":"fade"}



            if (dmx_collection.data_object[0].controller_state[current_channel] == true)
            {
                json_packet_message = "{\"state\":\"ON\",\"color_mode\":";
                

                json_packet_message += "\"" + dmx_collection.data_object[0].led_type[current_channel].ToLower() + "\",";

                // when fade has been run, it sets effect to null, after completion
                if (dmx_collection.data_object[0].effect[current_channel] != "")
                    json_packet_message += "\"effect\":\"" + dmx_collection.data_object[0].effect[current_channel] + "\",";
                //else
                  //json_packet_message += "\"effect\":\"none\"}";

                json_packet_message += "\"brightness\":" + Convert.ToString(dmx_collection.data_object[0].brightness_controller[current_channel]) + ",\"color\":{";



                switch (dmx_collection.data_object[0].led_type[current_channel])
                {
                    case "W":
                        json_packet_message += "\"w\":" + Convert.ToString(dmx_collection.data_object[0].current_led_value_new_target[current_channel]) + "},";

                        break;

                    case "R":
                        json_packet_message += "\"r\":" + Convert.ToString(dmx_collection.data_object[0].current_led_value_new_target[current_channel]) + "},";

                        break;
                    case "G":
                        json_packet_message += "\"g\":" + Convert.ToString(dmx_collection.data_object[0].current_led_value_new_target[current_channel]) + "},";

                        break;
                    case "B":
                        json_packet_message += "\"b\":" + Convert.ToString(dmx_collection.data_object[0].current_led_value_new_target[current_channel]) + "},";

                        break;
                    case "RGB":
                        json_packet_message += "\"r\":" + Convert.ToString(dmx_collection.data_object[0].current_led_value_new_target[current_channel]) + ",\"g\":" +
                           Convert.ToString(dmx_collection.data_object[0].current_led_value_new_target[current_channel + 1]) + ",\"b\":" +
                           Convert.ToString(dmx_collection.data_object[0].current_led_value_new_target[current_channel + 2]) + "}}";

                        break;
                    case "RGBW":
                        json_packet_message += "\"r\":" + Convert.ToString(dmx_collection.data_object[0].current_led_value_new_target[current_channel]) + ",\"g\":" +
                           Convert.ToString(dmx_collection.data_object[0].current_led_value_new_target[current_channel + 1]) + ",\"b\":" +
                           Convert.ToString(dmx_collection.data_object[0].current_led_value_new_target[current_channel + 2]) + ",\"w\":" +
                           Convert.ToString(dmx_collection.data_object[0].current_led_value_new_target[current_channel + 3]) + "}}";

                        break;

                    case "RGBWW":
                        json_packet_message += "\"r\":" + Convert.ToString(dmx_collection.data_object[0].current_led_value_new_target[current_channel]) + ",\"g\":" +
                           Convert.ToString(dmx_collection.data_object[0].current_led_value_new_target[current_channel + 1]) + ",\"b\":" +
                           Convert.ToString(dmx_collection.data_object[0].current_led_value_new_target[current_channel + 2]) + ",\"w\":" +
                           Convert.ToString(dmx_collection.data_object[0].current_led_value_new_target[current_channel + 3]) + ",\"w\":" +
                           Convert.ToString(dmx_collection.data_object[0].current_led_value_new_target[current_channel + 4]) + "}}";
                        break;
                }

            }
            else
             json_packet_message = "{\"state\":\"OFF\"}";

            return json_packet_message;
        }

        // calculates new colors for RGBW, using HSV color scheme. instead of adjusting linear, colors are adjusted to human perception of brighness. should ensure colors dont get distorted at low settings.
        //white is adjusted linear
        public void brightness_adjust(int topic_index, int source_dest)
        {
            Color minFarve;
            double bright_factor = 0;
            double w1 = 0;

            switch (dmx_collection.data_object[0].led_type[topic_index])
            {
                

                case "W":                          
                          bright_factor = (dmx_collection.data_object[0].brightness_controller[topic_index] / 2.55) / 100;
                          //adjusts white diode, as a percentage of the original color value.
                   
                          w1 = dmx_collection.data_object[0].brightness_whitemax_mixed_rgb[topic_index + 3] * bright_factor;
                          dmx_collection.data_object[0].current_led_value_new_target[topic_index + 3] = Convert.ToByte(w1);
                    break; 

                case "R":
                       
                          //create color from channel color settings
                          minFarve = Color.FromArgb(255, dmx_collection.data_object[0].current_led_value_new_target[topic_index], 0, 0);                                    
                          //convert RGB to HSV (HSV is closer to human perception of color, than using linear scaling)                    
                          RgbToHsv(minFarve, out double h3, out double s3, out double v3);
                          //calculate brightness factor, from recieved brighness setting %
                          bright_factor = (dmx_collection.data_object[0].brightness_controller[topic_index] / 2.55) / 100;
                          //adjusts brightness in HSV and calculate the new value back to RGB
                          HsvToRgb(h3, s3, bright_factor, out int r4, out int g4, out int b4);
                          // set new RGB values
                          dmx_collection.data_object[0].current_led_value_new_target[topic_index] = Convert.ToByte(r4);
                          //dmx_collection.data_object[0].current_led_value_new_target[topic_index + 1] = Convert.ToByte(g4);
                          //dmx_collection.data_object[0].current_led_value_new_target[topic_index + 2] = Convert.ToByte(b4);

                    break;

                case "G": //create color from channel color settings
                          minFarve = Color.FromArgb(255, 0, dmx_collection.data_object[0].current_led_value_new_target[topic_index], 0);
                          //convert RGB to HSV (HSV is closer to human perception of color, than using linear scaling)                    
                          RgbToHsv(minFarve, out double h4, out double s4, out double v4);
                          //calculate brightness factor, from recieved brighness setting %
                          bright_factor = (dmx_collection.data_object[0].brightness_controller[topic_index] / 2.55) / 100;
                          //adjusts brightness in HSV and calculate the new value back to RGB
                          HsvToRgb(h4, s4, bright_factor, out int r5, out int g5, out int b5);
                          // set new RGB values
                          dmx_collection.data_object[0].current_led_value_new_target[topic_index] = Convert.ToByte(g5);
                          //dmx_collection.data_object[0].current_led_value_new_target[topic_index + 1] = Convert.ToByte(g5);
                          //dmx_collection.data_object[0].current_led_value_new_target[topic_index + 2] = Convert.ToByte(r5);
                    break;

               case "B": //create color from channel color settings
                          minFarve = Color.FromArgb(255, 0, 0, dmx_collection.data_object[0].current_led_value_new_target[topic_index]);
                          //convert RGB to HSV (HSV is closer to human perception of color, than using linear scaling)                    
                          RgbToHsv(minFarve, out double h5, out double s5, out double v5);
                          //calculate brightness factor, from recieved brighness setting %
                          bright_factor = (dmx_collection.data_object[0].brightness_controller[topic_index] / 2.55) / 100;
                          //adjusts brightness in HSV and calculate the new value back to RGB
                          HsvToRgb(h5, s5, bright_factor, out int r6, out int g6, out int b6);
                          // set new RGB values
                          dmx_collection.data_object[0].current_led_value_new_target[topic_index] = Convert.ToByte(b6);
                          //dmx_collection.data_object[0].current_led_value_new_target[topic_index + 1] = Convert.ToByte(g6);
                          //dmx_collection.data_object[0].current_led_value_new_target[topic_index + 2] = Convert.ToByte(r6);
                    break;

                case "RGB":  //create color from channel color settings
                             minFarve = Color.FromArgb(255, dmx_collection.data_object[0].current_led_value_new_target[topic_index], dmx_collection.data_object[0].current_led_value_new_target[topic_index + 1], dmx_collection.data_object[0].current_led_value_new_target[topic_index + 2]);
                             //convert RGB to HSV (HSV is closer to human perception of color, than using linear scaling)                    
                             RgbToHsv(minFarve, out double h, out double s, out double v);
                             //calculate brightness factor, from recieved brighness setting %
                             bright_factor = (dmx_collection.data_object[0].brightness_controller[topic_index] / 2.55) / 100;
                             //adjusts brightness in HSV and calculate the new value back to RGB
                             HsvToRgb(h, s, bright_factor, out int r1, out int g1, out int b1);
                             // set new RGB values
                             dmx_collection.data_object[0].current_led_value_new_target[topic_index] = Convert.ToByte(r1);
                             dmx_collection.data_object[0].current_led_value_new_target[topic_index + 1] = Convert.ToByte(g1);
                             dmx_collection.data_object[0].current_led_value_new_target[topic_index + 2] = Convert.ToByte(b1);

                    break;

                case "RGBW": 
                             
                             //create color from channel color settings
                             minFarve = Color.FromArgb(255, dmx_collection.data_object[0].current_led_value_new_target[topic_index], dmx_collection.data_object[0].current_led_value_new_target[topic_index + 1], dmx_collection.data_object[0].current_led_value_new_target[topic_index + 2]);
                             //convert RGB to HSV (HSV is closer to human perception of color, than using linear scaling)                    
                             RgbToHsv(minFarve, out double h1, out double s1, out double v1);
                             //calculate brightness factor, from recieved brighness setting %
                             bright_factor = (dmx_collection.data_object[0].brightness_controller[topic_index] / 2.55) / 100;
                             //adjusts brightness in HSV and calculate the new value back to RGB
                             HsvToRgb(h1, s1, bright_factor, out int r2, out int g2, out int b2);
                             // brighness below 1% is seen as 0
                             if (dmx_collection.data_object[0].current_led_value_new_target[topic_index] > 6 || dmx_collection.data_object[0].current_led_value_new_target[topic_index + 1] > 6 || dmx_collection.data_object[0].current_led_value_new_target[topic_index + 2] > 6)
                              {
                               // set new RGB values, if input colors are larger than 10. (due to homeassist, not sending values for pure white)
                               dmx_collection.data_object[0].current_led_value_new_target[topic_index] = Convert.ToByte(r2);
                               dmx_collection.data_object[0].current_led_value_new_target[topic_index + 1] = Convert.ToByte(g2);
                               dmx_collection.data_object[0].current_led_value_new_target[topic_index + 2] = Convert.ToByte(b2);
                              }
                             //adjusts white diode, as a percentage of the original color value.
                             w1 = dmx_collection.data_object[0].brightness_whitemax_mixed_rgb[topic_index + 3] * bright_factor;
                             dmx_collection.data_object[0].current_led_value_new_target[topic_index + 3] = Convert.ToByte(w1);

                    break;

                case "RGBWW": //create color from channel color settings
                              minFarve = Color.FromArgb(255, dmx_collection.data_object[0].current_led_value_new_target[topic_index], dmx_collection.data_object[0].current_led_value_new_target[topic_index + 1], dmx_collection.data_object[0].current_led_value_new_target[topic_index + 2]);
                              //convert RGB to HSV (HSV is closer to human perception of color, than using linear scaling)                    
                              RgbToHsv(minFarve, out double h2, out double s2, out double v2);
                              //calculate brightness factor, from recieved brighness setting %
                              bright_factor = (dmx_collection.data_object[0].brightness_controller[topic_index] / 2.55) / 100;
                              //adjusts brightness in HSV and calculate the new value back to RGB
                              HsvToRgb(h2, s2, bright_factor, out int r3, out int g3, out int b3);
                              if (dmx_collection.data_object[0].current_led_value_new_target[topic_index] > 6 || dmx_collection.data_object[0].current_led_value_new_target[topic_index + 1] > 6 || dmx_collection.data_object[0].current_led_value_new_target[topic_index + 2] > 6)
                               {
                                // set new RGB values, if input colors are larger than 10. (due to homeassist, not sending values for pure white)
                                dmx_collection.data_object[0].current_led_value_new_target[topic_index] = Convert.ToByte(r3);
                                dmx_collection.data_object[0].current_led_value_new_target[topic_index + 1] = Convert.ToByte(g3);
                                dmx_collection.data_object[0].current_led_value_new_target[topic_index + 2] = Convert.ToByte(b3);
                               }

                              //adjusts warm white diode, as a percentage of the original color value.
                              w1 = dmx_collection.data_object[0].brightness_whitemax_mixed_rgb[topic_index + 3] * bright_factor;
                              dmx_collection.data_object[0].current_led_value_new_target[topic_index + 3] = Convert.ToByte(w1);
                             
                              //adjusts white diode, as a percentage of the original color value.
                              w1 = dmx_collection.data_object[0].brightness_whitemax_mixed_rgb[topic_index + 4] * bright_factor;
                              dmx_collection.data_object[0].current_led_value_new_target[topic_index + 4] = Convert.ToByte(w1);

                    break;
            }

        }

        // calculates new colors for RGBW, using HSV color scheme. instead of adjusting linear, colors are adjusted to human perception of brighness. should ensure colors dont get distorted at low settings.
        //white is adjusted linear
        public void brightness_adjust_colormorph(int topic_index)
        {
            Color minFarve;
            double bright_factor = 0;
            double w1 = 0;

            switch (dmx_collection.data_object[0].led_type[topic_index])
            {


                case "W":
                    bright_factor = (dmx_collection.data_object[0].brightness_controller[topic_index] / 2.55) / 100;
                    //adjusts white diode, as a percentage of the original color value.

                    w1 = dmx_collection.data_object[0].brightness_whitemax_mixed_rgb[topic_index + 3] * bright_factor;
                    dmx_collection.data_object[0].current_led_value_new_target[topic_index + 3] = Convert.ToByte(w1);
                    break;

                case "R":

                    //create color from channel color settings
                    minFarve = Color.FromArgb(255, dmx_collection.data_object[0].fade_target[topic_index], 0, 0);
                    //convert RGB to HSV (HSV is closer to human perception of color, than using linear scaling)                    
                    RgbToHsv(minFarve, out double h3, out double s3, out double v3);
                    //calculate brightness factor, from recieved brighness setting %
                    bright_factor = (dmx_collection.data_object[0].brightness_controller[topic_index] / 2.55) / 100;
                    //adjusts brightness in HSV and calculate the new value back to RGB
                    HsvToRgb(h3, s3, bright_factor, out int r4, out int g4, out int b4);
                    // set new RGB values
                    dmx_collection.data_object[0].fade_target[topic_index] = Convert.ToByte(r4);
                    //dmx_collection.data_object[0].current_led_value_new_target[topic_index + 1] = Convert.ToByte(g4);
                    //dmx_collection.data_object[0].current_led_value_new_target[topic_index + 2] = Convert.ToByte(b4);

                    break;

                case "G": //create color from channel color settings
                    minFarve = Color.FromArgb(255, 0, dmx_collection.data_object[0].fade_target[topic_index], 0);
                    //convert RGB to HSV (HSV is closer to human perception of color, than using linear scaling)                    
                    RgbToHsv(minFarve, out double h4, out double s4, out double v4);
                    //calculate brightness factor, from recieved brighness setting %
                    bright_factor = (dmx_collection.data_object[0].brightness_controller[topic_index] / 2.55) / 100;
                    //adjusts brightness in HSV and calculate the new value back to RGB
                    HsvToRgb(h4, s4, bright_factor, out int r5, out int g5, out int b5);
                    // set new RGB values
                    dmx_collection.data_object[0].fade_target[topic_index] = Convert.ToByte(g5);
                    //dmx_collection.data_object[0].current_led_value_new_target[topic_index + 1] = Convert.ToByte(r5);
                    //dmx_collection.data_object[0].current_led_value_new_target[topic_index + 2] = Convert.ToByte(b5);
                    break;

                case "B": //create color from channel color settings
                    minFarve = Color.FromArgb(255, 0, 0, dmx_collection.data_object[0].fade_target[topic_index]);
                    //convert RGB to HSV (HSV is closer to human perception of color, than using linear scaling)                    
                    RgbToHsv(minFarve, out double h5, out double s5, out double v5);
                    //calculate brightness factor, from recieved brighness setting %
                    bright_factor = (dmx_collection.data_object[0].brightness_controller[topic_index] / 2.55) / 100;
                    //adjusts brightness in HSV and calculate the new value back to RGB
                    HsvToRgb(h5, s5, bright_factor, out int r6, out int g6, out int b6);
                    // set new RGB values
                    dmx_collection.data_object[0].fade_target[topic_index] = Convert.ToByte(b6);
                    //dmx_collection.data_object[0].current_led_value_new_target[topic_index + 1] = Convert.ToByte(g6);
                    //dmx_collection.data_object[0].current_led_value_new_target[topic_index + 2] = Convert.ToByte(r6);
                    break;

                case "RGB":  //create color from channel color settings
                    minFarve = Color.FromArgb(255, dmx_collection.data_object[0].fade_target[topic_index], dmx_collection.data_object[0].fade_target[topic_index + 1], dmx_collection.data_object[0].fade_target[topic_index + 2]);
                    //convert RGB to HSV (HSV is closer to human perception of color, than using linear scaling)                    
                    RgbToHsv(minFarve, out double h, out double s, out double v);
                    //calculate brightness factor, from recieved brighness setting %
                    bright_factor = (dmx_collection.data_object[0].brightness_controller[topic_index] / 2.55) / 100;
                    //adjusts brightness in HSV and calculate the new value back to RGB
                    HsvToRgb(h, s, bright_factor, out int r1, out int g1, out int b1);
                    // set new RGB values
                    dmx_collection.data_object[0].current_led_value_new_target[topic_index] = Convert.ToByte(r1);
                    dmx_collection.data_object[0].current_led_value_new_target[topic_index + 1] = Convert.ToByte(g1);
                    dmx_collection.data_object[0].current_led_value_new_target[topic_index + 2] = Convert.ToByte(b1);

                    break;

                case "RGBW":

                    //create color from channel color settings
                    minFarve = Color.FromArgb(255, dmx_collection.data_object[0].fade_target[topic_index], dmx_collection.data_object[0].fade_target[topic_index + 1], dmx_collection.data_object[0].fade_target[topic_index + 2]);
                    //convert RGB to HSV (HSV is closer to human perception of color, than using linear scaling)                    
                    RgbToHsv(minFarve, out double h1, out double s1, out double v1);
                    //calculate brightness factor, from recieved brighness setting %
                    bright_factor = (dmx_collection.data_object[0].brightness_controller[topic_index] / 2.55) / 100;
                    //adjusts brightness in HSV and calculate the new value back to RGB
                    HsvToRgb(h1, s1, bright_factor, out int r2, out int g2, out int b2);
                    // brighness below 1% is seen as 0
                    if (dmx_collection.data_object[0].fade_target[topic_index] > 6 || dmx_collection.data_object[0].fade_target[topic_index + 1] > 6 || dmx_collection.data_object[0].fade_target[topic_index + 2] > 6)
                    {
                        // set new RGB values, if input colors are larger than 10. (due to homeassist, not sending values for pure white)
                        dmx_collection.data_object[0].fade_target[topic_index] = Convert.ToByte(r2);
                        dmx_collection.data_object[0].fade_target[topic_index + 1] = Convert.ToByte(g2);
                        dmx_collection.data_object[0].fade_target[topic_index + 2] = Convert.ToByte(b2);
                    }
                    //adjusts white diode, as a percentage of the original color value.
                    w1 = dmx_collection.data_object[0].brightness_whitemax_mixed_rgb[topic_index + 3] * bright_factor;
                    dmx_collection.data_object[0].fade_target[topic_index + 3] = Convert.ToByte(w1);

                    break;

                case "RGBWW": //create color from channel color settings
                    minFarve = Color.FromArgb(255, dmx_collection.data_object[0].fade_target[topic_index], dmx_collection.data_object[0].fade_target[topic_index + 1], dmx_collection.data_object[0].fade_target[topic_index + 2]);
                    //convert RGB to HSV (HSV is closer to human perception of color, than using linear scaling)                    
                    RgbToHsv(minFarve, out double h2, out double s2, out double v2);
                    //calculate brightness factor, from recieved brighness setting %
                    bright_factor = (dmx_collection.data_object[0].brightness_controller[topic_index] / 2.55) / 100;
                    //adjusts brightness in HSV and calculate the new value back to RGB
                    HsvToRgb(h2, s2, bright_factor, out int r3, out int g3, out int b3);
                    if (dmx_collection.data_object[0].fade_target[topic_index] > 6 || dmx_collection.data_object[0].fade_target[topic_index + 1] > 6 || dmx_collection.data_object[0].fade_target[topic_index + 2] > 6)
                    {
                        // set new RGB values, if input colors are larger than 10. (due to homeassist, not sending values for pure white)
                        dmx_collection.data_object[0].fade_target[topic_index] = Convert.ToByte(r3);
                        dmx_collection.data_object[0].fade_target[topic_index + 1] = Convert.ToByte(g3);
                        dmx_collection.data_object[0].fade_target[topic_index + 2] = Convert.ToByte(b3);
                    }

                    //adjusts warm white diode, as a percentage of the original color value.
                    w1 = dmx_collection.data_object[0].brightness_whitemax_mixed_rgb[topic_index + 3] * bright_factor;
                    dmx_collection.data_object[0].fade_target[topic_index + 3] = Convert.ToByte(w1);

                    //adjusts white diode, as a percentage of the original color value.
                    w1 = dmx_collection.data_object[0].brightness_whitemax_mixed_rgb[topic_index + 4] * bright_factor;
                    dmx_collection.data_object[0].fade_target[topic_index + 4] = Convert.ToByte(w1);

                    break;
            }

        }

        public void write_log_to_file()
        {

            using (System.IO.StreamWriter file = new System.IO.StreamWriter("logfile.txt", false))
            {
                file.Write(dmx_collection.data_object[0].textbox1_status_messages);

            }

            if(dmx_collection.data_object[0].textbox1_status_messages.Length > 1000000)
            {
                dmx_collection.data_object[0].textbox1_status_messages = "";
            }

        }

        public void RGBWtoRGB(byte Ri, byte Gi, byte Bi, byte Wi,byte brightness, out byte Ro, out byte Go, out byte Bo)
        {
            Color minFarve;
            double bright_factor;
            double w1 = 0;
            bool min = true;
            int Ri1;
            int Gi1;
            int Bi1;

            Ri1 = Ri + (Wi);
            Gi1 = Gi + (Wi);
            Bi1 = Bi + (Wi);
            if (Ri1 > 255)
                Ri1 = 255;
            if (Gi1 > 255)
                Gi1 = 255;
            if (Bi1 > 255)
                Bi1 = 255;     
            
                // set new RGB values, if input colors are larger than 10. (due to homeassist, not sending values for pure white)
                Ro = Convert.ToByte(Ri1);
                Go = Convert.ToByte(Gi1);
                Bo = Convert.ToByte(Bi1);
            
        

        }

        public void RgbToHsv(Color c, out double h, out double s, out double v)
        {
            

            // Beregner HSV baseret på RGB
            double r = c.R / 255.0, g = c.G / 255.0, b = c.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            h = c.GetHue();
            s = (max == 0) ? 0 : delta / max;
            v = max;
        }

     
        public void HsvToRgb(double h, double S, double V, out int r, out int g, out int b)
        {
            double H = h;
            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;
            if (V <= 0)
            { R = G = B = 0; }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {

                    // Red is the dominant color

                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }
            r = Clamp((int)(R * 255.0));
            g = Clamp((int)(G * 255.0));
            b = Clamp((int)(B * 255.0));
        }

        /// <summary>
        /// Clamp a value to 0-255
        /// </summary>
        int Clamp(int i)
        {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return i;
        }


        // depending on user config og channels, topics for mqtt are created dynamically, when thread is launched.
        public void create_topics_for_mqtt()
        {
            int counter = 0;
            int counter2 = 0;
            string assembled_topic = "";          

            for (counter = 0;counter < dmx_collection.data_object[0].number_of_channels; counter++)
            {
                switch(dmx_collection.data_object[0].led_type[counter])
                {
                    case "W":
                        dmx_collection.data_object[0].assembled_control_topic[counter2] = dmx_collection.data_object[0].subscribe_control_topic[counter] + dmx_collection.data_object[0].channel_name[counter];
                        dmx_collection.data_object[0].assembled_publish_topic[counter2] = dmx_collection.data_object[0].publish_topic[counter] + dmx_collection.data_object[0].channel_name[counter];
                        dmx_collection.data_object[0].assembled_qos_level[counter2] = dmx_collection.data_object[0].Qos_level[counter];
                        if (counter > 0)
                            dmx_collection.data_object[0].ch_numbers_assembled_topics[counter2] = counter;
                        else
                            dmx_collection.data_object[0].ch_numbers_assembled_topics[counter2] = 0;
                        counter2++;
                        break;

                    case "R":
                        dmx_collection.data_object[0].assembled_control_topic[counter2] = dmx_collection.data_object[0].subscribe_control_topic[counter] + dmx_collection.data_object[0].channel_name[counter];
                        dmx_collection.data_object[0].assembled_publish_topic[counter2] = dmx_collection.data_object[0].publish_topic[counter] + dmx_collection.data_object[0].channel_name[counter];
                        dmx_collection.data_object[0].assembled_qos_level[counter2] = dmx_collection.data_object[0].Qos_level[counter];
                        if (counter > 0)
                            dmx_collection.data_object[0].ch_numbers_assembled_topics[counter2] = counter;
                        else
                            dmx_collection.data_object[0].ch_numbers_assembled_topics[counter2] = 0;
                        counter2++;
                        break;

                    case "G":
                        dmx_collection.data_object[0].assembled_control_topic[counter2] = dmx_collection.data_object[0].subscribe_control_topic[counter] + dmx_collection.data_object[0].channel_name[counter];
                        dmx_collection.data_object[0].assembled_publish_topic[counter2] = dmx_collection.data_object[0].publish_topic[counter] + dmx_collection.data_object[0].channel_name[counter];
                        dmx_collection.data_object[0].assembled_qos_level[counter2] = dmx_collection.data_object[0].Qos_level[counter];
                        if (counter > 0)
                            dmx_collection.data_object[0].ch_numbers_assembled_topics[counter2] = counter;
                        else
                            dmx_collection.data_object[0].ch_numbers_assembled_topics[counter2] = 0;
                        counter2++;
                        break;

                    case "B":
                        dmx_collection.data_object[0].assembled_control_topic[counter2] = dmx_collection.data_object[0].subscribe_control_topic[counter] + dmx_collection.data_object[0].channel_name[counter];
                        dmx_collection.data_object[0].assembled_publish_topic[counter2] = dmx_collection.data_object[0].publish_topic[counter] + dmx_collection.data_object[0].channel_name[counter];
                        dmx_collection.data_object[0].assembled_qos_level[counter2] = dmx_collection.data_object[0].Qos_level[counter];
                        if (counter > 0)
                            dmx_collection.data_object[0].ch_numbers_assembled_topics[counter2] = counter;
                        else
                            dmx_collection.data_object[0].ch_numbers_assembled_topics[counter2] = 0;
                        counter2++;
                        break;

                    case "RGB":
                        dmx_collection.data_object[0].assembled_control_topic[counter2] = dmx_collection.data_object[0].subscribe_control_topic[counter] + dmx_collection.data_object[0].channel_name[counter];
                        dmx_collection.data_object[0].assembled_publish_topic[counter2] = dmx_collection.data_object[0].publish_topic[counter] + dmx_collection.data_object[0].channel_name[counter];
                        dmx_collection.data_object[0].assembled_qos_level[counter2] = dmx_collection.data_object[0].Qos_level[counter];
                        if (counter > 0)
                            dmx_collection.data_object[0].ch_numbers_assembled_topics[counter2] = counter;
                        else
                            dmx_collection.data_object[0].ch_numbers_assembled_topics[counter2] = 0;
                        counter2++;
                        break;

                    case "RGBW":
                        dmx_collection.data_object[0].assembled_control_topic[counter2] = dmx_collection.data_object[0].subscribe_control_topic[counter] + dmx_collection.data_object[0].channel_name[counter];
                        dmx_collection.data_object[0].assembled_publish_topic[counter2] = dmx_collection.data_object[0].publish_topic[counter] + dmx_collection.data_object[0].channel_name[counter];
                        dmx_collection.data_object[0].assembled_qos_level[counter2] = dmx_collection.data_object[0].Qos_level[counter];
                        if (counter > 0)
                            dmx_collection.data_object[0].ch_numbers_assembled_topics[counter2] = counter;
                        else
                            dmx_collection.data_object[0].ch_numbers_assembled_topics[counter2] = 0;
                        counter2++;
                        break;

                    case "RGBWW":
                        dmx_collection.data_object[0].assembled_control_topic[counter2] = dmx_collection.data_object[0].subscribe_control_topic[counter] + dmx_collection.data_object[0].channel_name[counter];
                        dmx_collection.data_object[0].assembled_publish_topic[counter2] = dmx_collection.data_object[0].publish_topic[counter] + dmx_collection.data_object[0].channel_name[counter];
                        dmx_collection.data_object[0].assembled_qos_level[counter2] = dmx_collection.data_object[0].Qos_level[counter];
                        if (counter > 0)
                            dmx_collection.data_object[0].ch_numbers_assembled_topics[counter2] = counter;
                        else
                            dmx_collection.data_object[0].ch_numbers_assembled_topics[counter2] = 0;
                        counter2++;
                        break;
                }

            }
            dmx_collection.data_object[0].Number_of_assembled_topics = counter2;
        }

   
    }
}

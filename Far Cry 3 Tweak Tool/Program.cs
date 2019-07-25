using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;

namespace Far_Cry_3_Tweak_Tool {
    //Far Cry 3 is a hugely popular game, but certain fixes are needlessly complicated.
    //This tool will allow users to tweak Far Cry 3 and apply fixes from within a convenient GUI instead of manually having to fiddle around with the code.
    //
    //Features:
    //Cap framerate to alleviate microstuttering (-RenderProfile_MaxFPS X)
    //Disable depth of field. This only disables depth of field on background and when not aiming.
    /*Before doing anything, set PostFx in-game settings to low and save.
     * Go to the configuration file(s) location.
     * Open GamerProfile.xml with a text editor.
     * Locate PostFxQuality = "x" and change x to false.*/
    //Disable SSAO (bad quality, apparently breaks MSAO)
    /*Open GamerProfile.xml with a text editor.
     * Locate SSAOLevel = "x" and change x to 0.*/

    //Maybe also fix scroll binding problems?

    //Fix negative mouse acceleration.
    /*Download the latest version of the Far Cry 3 Mod tools
     * Extract the file
     * Go into<path-to-game>\data_win32
     * Right click on Common.fat and select Open with.
     * Select browse, then go into the "bin" folder you just extracted and select Gibbed.Dunia2.Unpack.EXE.
     * Open the newly created Common_Unpack folder and go into the actionmaps folder.
     * Open Inputactionmapcommon.xml with a text editor.
     * Find and replace all instances of maxOutput= "10" with maxOutput = "999999".
     * Save the file.
     * Go back to Data_win32 folder.
     * Now open another Window and head over to the bin folder.
     * Drag the common_Unpack folder and place it onto Gibbed.Dunia2.Pack.EXE.
     * Delete Common.dat and Common.fat.
     * Rename Common_Unpack.dat and Common_Unpack.fat to Common.dat and Common.fat respectively.*/

    //Investigate:
    //There is a prompt to switch to controller for one time that cannot be confirmed with a controller.
    //After that, all menus can be used with a controller.


    static class Program {
        //Unsure if these will return slashes or not.
        static String userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        static String applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        //Store global copy of gamerProfile
        private static String gamerProfileFile = userProfilePath + @"\Documents\My Games\Far Cry 3\GamerProfile.xml";
        private static String gamerProfileData = "";

        //TODO INPUT STUFF UNDER APP DATA?
        static String inputUserActionMap = @"\My Games\Far Cry 3\InputUserActionMap.xml";



        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            LoadGamerProfile();
            //MessageBox.Show(gamerProfileData, "Something went wrong", MessageBoxButtons.OK);
            MessageBox.Show(GetVariable("MusicEnabled"), "Something went wrong", MessageBoxButtons.OK);
            //Application.Run(new Form1());


        }


        //Mouse acceleration.
        static void DisableNegativeMouseAcceleration() {
            //This is where we need to start making assumptions. We are going to assume that the tweak tool is installed under Far Cry 3\bin.
            //Files must be unpacked first.

            String file = @"..\data_win32\common_unpack\actionmaps\inputactionmapcommon.xml";
            //We need to establish the Far Cry 3 directory and go from there.
            string text = File.ReadAllText(file);

            //TODO: We only want to match maxOutput="10" for now.
            text = text.Replace("maxOutput=\"10\"", "maxOutput = \"999999\"");
        }

        static void UnpackCommon() {
            //GibbedTools\bin\Gibbed.Dunia2.Unpack.exe

            ProcessStartInfo startInfo = new ProcessStartInfo(@"GibbedTools\bin\Gibbed.Dunia2.Unpack.exe") {
                WindowStyle = ProcessWindowStyle.Normal,

                //This should hopefully cause the program to unpack common.dat        
                Arguments = @"..\data_win32\common.dat"
            };
            Process process = Process.Start(startInfo);
            //We need to display a message saying "processing..." while we ewait.
            process.WaitForExit(); // Waits indefinitely for the process to finish

            //The process is now done. What we need to do is verify that the files we want have been extracted.

            if (File.Exists(@"..\data_win32\common_unpack\actionmaps\inputactionmapcommon.xml")) {

                //The file exists so we can proceed.
            }
            else {
                //Something has gone wrong.
            }

        }

        static void RepackCommon() {

        }



        //%USERPROFILE%\Documents\My Games\Far Cry 3\GamerProfile.xml
        //%LOCALAPPDATA%\My Games\Far Cry 3\InputUserActionMap.xml
        //What about Proton?


        //When the game is freshly installed, these files won't exist.
        //We need to find a way around this.
        static void SetWriteProtect(bool condition) {
            try {
                FileAttributes attrs = File.GetAttributes(gamerProfileFile);
                if (condition) {
                    if (attrs.HasFlag(FileAttributes.ReadOnly) && !condition) {
                        File.SetAttributes(gamerProfileFile, attrs & ~FileAttributes.ReadOnly);
                    }
                }
                else {
                    if (!attrs.HasFlag(FileAttributes.ReadOnly) && condition) {
                        File.SetAttributes(gamerProfileFile, attrs & FileAttributes.ReadOnly);
                    }
                }
            } catch {
                //Do something like display an error box.
            }


        }

        //Enable framerate cap. Capping the game at 60fps reportedly alleviates microstutter.
        static void EnableFramerateCap() {
            if(!gamerProfileData.Contains("MaxFPS")) {
                //If MaxFPS is not present, add it.
                //Match <RenderProfile and replace with <RenderProfile MaxFPS="60">
                gamerProfileData = gamerProfileData.Replace("<RenderProfile", "<RenderProfile MaxFPS=\"60\"");

            }
        }

        //The game will ask you whether you want to use a gamepad. Once you accept, it won't ask again. This could be a problem.
        static void UseGamePad(bool condition) {
            if(condition) {
                gamerProfileData = gamerProfileData.Replace("UseGamePad=\"0\" GamepadAnswered=\"0\"", "UseGamePad =\"1\" GamepadAnswered=\"1\"");
            }
            else {
                //Note. Keep question answered for now.
                gamerProfileData = gamerProfileData.Replace("UseGamePad=\"1\" GamepadAnswered=\"1\"", "UseGamePad =\"0\" GamepadAnswered=\"1\"");
            }
        }

        //The game's SSAO is inexplicably terrible, and in addition to being extremely ugly, it reportedly breaks the MSAA implementation.
        static void DisableSSAO() {
            //For the sake of cleanness, we should only write the file once when saving. Simply do a long if(changed) and then save.
            gamerProfileData = Regex.Replace(gamerProfileData, "SSAOLevel=\"[0-9]\"", "SSAOLevel=\"0\"");
        }

        static void SetResolution(int x, int y) {

            gamerProfileData = Regex.Replace(gamerProfileData, "ResolutionX=\"[0-9*]\"", "ResolutionX=\"" + x + "\"");
            gamerProfileData = Regex.Replace(gamerProfileData, "ResolutionY=\"[0-9*]\"", "ResolutionY=\"" + x + "\"");
        }

        static void SetFullScreen(bool condition) {
            if (condition) {
                gamerProfileData = Regex.Replace(gamerProfileData, "Fullscreen=\"[0-1*]\"", "Fullscreen=\"1\"");
            } else {
                gamerProfileData = Regex.Replace(gamerProfileData, "Fullscreen=\"[0-1*]\"", "Fullscreen=\"0\"");
            }
        }

        static void SetBorderless(bool condition) {
            //Borderless = "1"
            if (condition) {
                gamerProfileData = Regex.Replace(gamerProfileData, "Borderless=\"[0-1*]\"", "Borderless=\"1\"");
            }
            else {
                gamerProfileData = Regex.Replace(gamerProfileData, "Borderless=\"[0-1*]\"", "Borderless=\"0\"");
            }
        }

        static void SetMSAA(int level) {
            //MSAALevel = "0"
        }

        static void LoadGamerProfile() {
            gamerProfileData = File.ReadAllText(gamerProfileFile);
        }

        //I need to scrape all the settings out of the file so the GUI can be populated.

        static int ScrapeBorderless() {
            //How do we extract the data?
            //Parsing this way could be unsafe.
            return Int32.Parse(Regex.Match(gamerProfileData, "Borderless=\"([0-1])\"").ToString());
        }

        static void ScrapeVariables() {

            string borderless = GetVariable("Borderless");
            string resolutionX = GetVariable("ResolutionX");
            string resolutionY = GetVariable("ResolutionY");

            string musicEnabled = GetVariable("MusicEnabled");

        }


        //Far Cry 3 sound variables.
        //<SoundProfile MusicEnabled = "1" MasterVolume="100" MicEnabled="1" IncomingVoiceEnabled="1" Language="english" />


        //To prevent code reuse, we can use a generic regex
        static string GetVariable(String variable) {
            return Regex.Match(gamerProfileData, variable + "=\"([0-9*])\"").Value;
        }

        //Need to somehow set the variable with a new value here.
        static void SetVariable(String variable, String value) {
            gamerProfileData = Regex.Replace(gamerProfileData, variable + "=\"([0-9*])\"", value);
        }



        static void SaveGamerProfile() {
            File.WriteAllText(gamerProfileFile, gamerProfileData);
        }
    }

}
